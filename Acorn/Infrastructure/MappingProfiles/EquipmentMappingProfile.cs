using AutoMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acorn.Infrastructure.MappingProfiles;
public class EquipmentMappingProfile : Profile
{
    public EquipmentMappingProfile()
    {
        CreateMap<EquipmentPaperdoll, EquipmentWelcome>();
    }
}
