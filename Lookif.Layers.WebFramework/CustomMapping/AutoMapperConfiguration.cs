﻿using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Lookif.Layers.WebFramework.CustomMapping;

public static class AutoMapperConfiguration
{
    public static void InitializeAutoMapper(this IServiceCollection services, params Assembly[] assemblies)
    {
        //With AutoMapper Instance, you need to call AddAutoMapper services and pass assemblies that contains automapper Profile class
        //services.AddAutoMapper(assembly1, assembly2, assembly3);
        //See http://docs.automapper.org/en/stable/Configuration.html
        //And https://code-maze.com/automapper-net-core/

        services.AddAutoMapper(config =>
        {
            config.AddCustomMappingProfile(assemblies);
        }, assemblies);

    }

    public static void AddCustomMappingProfile(this IMapperConfigurationExpression config)
    {
        config.AddCustomMappingProfile(Assembly.GetEntryAssembly());
    }

    public static void AddCustomMappingProfile(this IMapperConfigurationExpression config, params Assembly[] assemblies)
    {
        var allTypes = assemblies.SelectMany(a => a.ExportedTypes);

        var list = allTypes.Where(type => type.IsClass && !type.IsAbstract &&
            type.GetInterfaces().Contains(typeof(ICustomMapping)))
            .Select(type => (ICustomMapping)Activator.CreateInstance(type));

        var profile = new CustomMappingProfile(list);

        config.AddProfile(profile);
    }
}
