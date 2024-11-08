using Acorn.Data.Models;
using Acorn.Database.Models;
using Acorn.Services.Security;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using System.Text;

namespace Acorn.Extensions;
public static class AccountCreateClientPacketExtensions
{
    public static Account AsNewAccount(this AccountCreateClientPacket packet, DateTime created)
    {
        var password = Hash.HashPassword(packet.Username, packet.Password, out var salt);
        return new()
        {
            Characters = new List<Character>(),
            Country = packet.Location,
            Created = created,
            Email = packet.Email,
            FullName = packet.FullName,
            LastUsed = created,
            Location = packet.Location,
            Password = password,
            Salt = Encoding.UTF8.GetString(salt),
            Username = packet.Username
        };
    }
}
