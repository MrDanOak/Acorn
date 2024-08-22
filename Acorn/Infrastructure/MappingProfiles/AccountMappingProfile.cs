using Acorn.Data;
using AutoMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace Acorn.Infrastructure.MappingProfiles;
internal class AccountMappingProfile : Profile
{
    private UtcNowDelegate _nowProvider;

    public AccountMappingProfile(UtcNowDelegate nowProvider)
    {
        _nowProvider = nowProvider;
    }

    public AccountMappingProfile()
    {
        CreateMap<AccountCreateClientPacket, Account>()
            .ForMember(x => x.Country, opts => opts.MapFrom(acc => acc.Location))
            .ForMember(x => x.Created, opts => opts.MapFrom(_ => _nowProvider()));
    }
}