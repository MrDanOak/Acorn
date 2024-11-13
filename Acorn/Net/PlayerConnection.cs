﻿using Acorn.Data;
using Acorn.Data.Models;
using Acorn.Database.Models;
using Acorn.Extensions;
using Acorn.Infrastructure;
using Acorn.Net.Models;
using Acorn.Net.PacketHandlers;
using Acorn.Net.PacketHandlers.Player.Warp;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Net.Sockets;

namespace Acorn.Net;
public class PlayerConnection : IDisposable
{
    private ILogger<PlayerConnection> _logger;
    private readonly PacketResolver _resolver = new("Moffat.EndlessOnline.SDK.Protocol.Net.Client");
    private readonly PingSequenceStart _upcomingSequence;
    private readonly Action<PlayerConnection> _onDispose;
    private readonly IServiceProvider _serviceProvider;

    public Random Rnd { get; } = new();

    public ClientState ClientState { get; set; } = ClientState.Uninitialized;
    public bool NeedPong { get; set; } = false;
    public int ClientEncryptionMulti { get; set; } = 0;
    public int ServerEncryptionMulti { get; set; } = 0;
    public PacketSequencer PacketSequencer { get; set; } = new(ZeroSequence.Instance);
    public InitSequenceStart StartSequence { get; set; }
    public TcpClient TcpClient { get; }
    public Account? Account { get; set; }
    public bool IsListeningToGlobal { get; set; }

    public int SessionId { get; set; }
    public WarpSession? WarpSession { get; set; }

    public Character Character { get; set; }

    public PlayerConnection(
        IServiceProvider services,
        TcpClient tcpClient,
        ILogger<PlayerConnection> logger,
        Action<PlayerConnection> onDispose,
        ISessionGenerator sessionGenerator
    )
    {
        TcpClient = tcpClient;
        _logger = logger;
        _upcomingSequence = PingSequenceStart.Generate(Rnd);
        _logger.LogInformation("New client connected from {Location}", TcpClient.Client.RemoteEndPoint);
        _onDispose = onDispose;
        _serviceProvider = services;

        SessionId = sessionGenerator.Generate();
        StartSequence = InitSequenceStart.Generate(Rnd);
        Task.Run(Listen);
    }

    public async Task Listen()
    {
        while (true)
        {
            try
            {
                var stream = TcpClient.GetStream();

                var len1 = (byte)stream.ReadByte();
                var len2 = (byte)stream.ReadByte();

                var decodedLength = NumberEncoder.DecodeNumber([len1, len2]);
                _logger.LogDebug("Len1 {Len1}, Len2 {Len2}, Decoded length {DecodedLength}", len1, len2, decodedLength);
                var bytes = new byte[decodedLength];
                await stream.ReadAsync(bytes.AsMemory(0, decodedLength));

                var decodedBytes = ClientEncryptionMulti switch
                {
                    0 => bytes,
                    _ => DataEncrypter.SwapMultiples(DataEncrypter.Deinterleave(DataEncrypter.FlipMSB(bytes)), ClientEncryptionMulti)
                };

                var reader = new EoReader(decodedBytes);
                var action = (PacketAction)reader.GetByte();
                var family = (PacketFamily)reader.GetByte();

                HandleSequence(family, action, ref reader);

                var dataReader = reader.Slice();

                var packet = _resolver.Create(family, action);
                packet.Deserialize(dataReader);
                _logger.LogDebug("[Client] {Packet}", packet.ToString());

                var handlerType = typeof(IPacketHandler<>).MakeGenericType(packet.GetType());
                if (_serviceProvider.GetService(handlerType) is not IHandler handler)
                {
                    _logger.LogError("Handler not registered for packet of type {PacketType} Exiting...", packet.GetType());
                    break;
                }

                var error = false;

                (await handler.HandleAsync(this, packet)).Switch(success => { }, fail => error = true);
                if (error)
                    break;
            }
            catch (Exception e)
            {
                _logger.LogError("Caught exception \"{Message}\" terminating...", e.Message);
                break;
            }
        }
        Dispose();
    }

    private void HandleSequence(PacketFamily family, PacketAction action, ref EoReader reader)
    {
        if (family == PacketFamily.Init && action == PacketAction.Init)
        {
            PacketSequencer.NextSequence();
            return;
        }

        if (family == PacketFamily.Connection && action == PacketAction.Ping)
        {
            PacketSequencer = PacketSequencer.WithSequenceStart(_upcomingSequence);
        }

        var serverSequence = PacketSequencer.NextSequence();
        var clientSequence = serverSequence switch
        {
            >= (int)EoNumericLimits.CHAR_MAX => reader.GetShort(),
            _ => reader.GetChar()
        };

        if (serverSequence != clientSequence)
        {
            _logger.LogError("Expected sequence {Expected} got {Actual}", serverSequence, clientSequence);
        }
    }

    public async Task Send(IPacket packet)
    {
        _logger.LogDebug("[Server] {Packet}", packet.ToString());
        var writer = new EoWriter();
        writer.AddByte((int)packet.Action);
        writer.AddByte((int)packet.Family);
        packet.Serialize(writer);


        byte[] bytes = packet switch
        {
            InitInitServerPacket _ => writer.ToByteArray(),
            _ => DataEncrypter.FlipMSB(DataEncrypter.Interleave(DataEncrypter.SwapMultiples(writer.ToByteArray(), ServerEncryptionMulti)))
        };

        var encodedLength = NumberEncoder.EncodeNumber(bytes.Length);
        var fullBytes = encodedLength[..2].Concat(bytes);
        await TcpClient.GetStream().WriteAsync(fullBytes.AsReadOnly(), new CancellationToken());
    }

    public void Dispose()
    {
        _onDispose(this);
        TcpClient.Close();
    }
}