using AutoMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace Acorn.Infrastructure.MappingProfiles;
public class EquipmentMappingProfile : Profile
{
    public EquipmentMappingProfile()
    {
        CreateMap<EquipmentPaperdoll, EquipmentWelcome>();
    }
}
