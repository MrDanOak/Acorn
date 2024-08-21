using AutoMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acorn.MappingProfiles;
public class EquipmentMapper : Profile
{
    public EquipmentMapper()
    {
        CreateMap<EquipmentPaperdoll, EquipmentWelcome>();
    }
}
