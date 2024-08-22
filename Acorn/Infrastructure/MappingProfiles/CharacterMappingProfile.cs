using Acorn.Data;
using AutoMapper;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace Acorn.Infrastructure.MappingProfiles;
public class CharacterMappingProfile : Profile
{
    public CharacterMappingProfile()
    {
        CreateMap<Character, CharacterSelectionListEntry>()
            .ForMember(x => x.Admin, opts => opts.MapFrom(c => (AdminLevel)c.Admin))
            .ForMember(x => x.Equipment, opts => opts.MapFrom(_ => new EquipmentCharacterSelect()))
            .ForMember(x => x.Skin, opts => opts.MapFrom(c => c.Race));
    }
}