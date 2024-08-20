using Acorn.Data.Models;
using Acorn.Data.Repository;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers;
public class LoginRequestClientPacketHandler(
    ILogger<LoginRequestClientPacketHandler> logger,
    IRepository<Account> repository
) : IPacketHandler<LoginRequestClientPacket>
{
    private readonly ILogger<LoginRequestClientPacketHandler> _logger = logger;
    private readonly IRepository<Account> _repository = repository;

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, LoginRequestClientPacket packet)
    {
        var responsePacket = (await _repository.GetByKey(packet.Username))
            .Match(success => new LoginReplyServerPacket
            {
                ReplyCode = LoginReply.Ok,
                ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataOk()
                {
                    Characters = []
                    //{
                    //    new CharacterSelectionListEntry()
                    //    {
                    //        Name = "Dan",
                    //        Level = 200,
                    //        Admin = Moffat.EndlessOnline.SDK.Protocol.AdminLevel.HighGameMaster,
                    //        Equipment = new EquipmentCharacterSelect()
                    //        {
                    //            Armor = 4,
                    //            Boots = 4,
                    //            Shield = 1,
                    //            Hat = 4,
                    //            Weapon = 200
                    //        },
                    //        Gender = Moffat.EndlessOnline.SDK.Protocol.Gender.Male,
                    //        Skin = 4,
                    //        HairColor = 1,
                    //        HairStyle = 2
                    //    }
                    //}.ToList()
                }
            },
            notFound => new LoginReplyServerPacket
            {
                ReplyCode = LoginReply.WrongUser,
                ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataWrongUser()
            },
            error => new LoginReplyServerPacket
            {
                ReplyCode = LoginReply.Busy,
                ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataBusy()
            }
        );

        await playerConnection.Send(responsePacket);
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet) => HandleAsync(playerConnection, (LoginRequestClientPacket)packet);
}
