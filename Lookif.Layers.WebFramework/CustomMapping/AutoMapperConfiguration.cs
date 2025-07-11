using System;
using System.Collections.Generic;
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
        try
        {
            IEnumerable<Type> source = assemblies.SelectMany(a => a.ExportedTypes);

            var haveCustomMappings = source
                .Where(type => type.IsClass && !type.IsAbstract && type.GetInterfaces().Contains(typeof(ICustomMapping)))
                .Select(type =>
                {
                    try
                    {
                        return (ICustomMapping)Activator.CreateInstance(type);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to create instance of {type.FullName}: {ex.Message}");
                        throw;
                    }
                })
                .ToList(); // Materialize to catch errors immediately

            CustomMappingProfile profile = new CustomMappingProfile(haveCustomMappings);
            config.AddProfile(profile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error in AddCustomMappingProfiles: {ex}");
            throw;
        }
    }
}
