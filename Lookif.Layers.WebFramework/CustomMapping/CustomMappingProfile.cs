using System.Collections.Generic;
using AutoMapper;

namespace Lookif.Layers.WebFramework.CustomMapping;

public class CustomMappingProfile : Profile
{
    public CustomMappingProfile(IEnumerable<ICustomMapping> haveCustomMappings)
    {
        foreach (var item in haveCustomMappings)
            item.CreateMappings(this);
    }
}
