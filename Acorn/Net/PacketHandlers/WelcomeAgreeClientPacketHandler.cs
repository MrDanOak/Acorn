﻿using Acorn.Data.Repository;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers;
public class WelcomeAgreeClientPacketHandler : IPacketHandler<WelcomeAgreeClientPacket>
{
    private readonly IDataRepository _dataRepository;

    public WelcomeAgreeClientPacketHandler(
        IDataRepository dataRepository
    )
    {
        _dataRepository = dataRepository;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WelcomeAgreeClientPacket packet)
    {
        var eoWriter = new EoWriter();

        Action serialise = packet.FileType switch
        {
            FileType.Eif => () => _dataRepository.Eif.Serialize(eoWriter),
            FileType.Esf => () => _dataRepository.Esf.Serialize(eoWriter),
            FileType.Enf => () => _dataRepository.Enf.Serialize(eoWriter),
            FileType.Ecf => () => _dataRepository.Ecf.Serialize(eoWriter),
            FileType.Emf => () => {
                var map = _dataRepository.Maps.FirstOrDefault(map => map.Id == playerConnection.Character.Map)?.Map ?? 
                    throw new ArgumentOutOfRangeException($"Could not find map {playerConnection.Character.Map} for character {playerConnection.Character.Name}");

                map.Serialize(eoWriter);
            },
            _ => throw new InvalidOperationException($"Unknown file type {packet.FileType}")
        };
        serialise();

        var bytes = eoWriter.ToByteArray();

        await playerConnection.Send(new InitInitServerPacket
        {
            ReplyCode = packet.FileType switch
            {
                FileType.Eif => InitReply.FileEif,
                FileType.Esf => InitReply.FileEsf,
                FileType.Enf => InitReply.FileEnf,
                FileType.Emf => InitReply.FileEmf,
                FileType.Ecf => InitReply.FileEcf,
                _ => throw new InvalidOperationException($"Unknown file type {packet.FileType}")
            },
            ReplyCodeData = packet.FileType switch
            {
                FileType.Enf => new InitInitServerPacket.ReplyCodeDataFileEnf
                {
                    PubFile = new PubFile
                    {
                        FileId = 1,
                        Content = bytes
                    }
                },
                FileType.Emf => new InitInitServerPacket.ReplyCodeDataFileEmf
                {
                    MapFile = new MapFile
                    {
                        Content = bytes
                    }
                },
                FileType.Ecf => new InitInitServerPacket.ReplyCodeDataFileEcf
                {
                    PubFile = new PubFile
                    {
                        FileId = 1,
                        Content = bytes
                    }
                },
                FileType.Eif => new InitInitServerPacket.ReplyCodeDataFileEif
                {
                    PubFile = new PubFile
                    {
                        FileId = 1,
                        Content = bytes
                    }
                },
                FileType.Esf => new InitInitServerPacket.ReplyCodeDataFileEsf
                {
                    PubFile = new PubFile
                    {
                        FileId = 1,
                        Content = bytes
                    }
                },
            }
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WelcomeAgreeClientPacket)packet);
}