using Autofac;
using Lookif.Layers.Core.Infrastructure.Base;
using Lookif.Layers.Core.Infrastructure.Base.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lookif.Layers.WebFramework.Configuration
{
    public static class AutofacConfigurationExtensions
    {
        public static void AddServices(
            this ContainerBuilder containerBuilder,
            Assembly entitiesAssembly,
            Assembly dataAssembly,
            Assembly servicesAssembly,
            Type Repository,
            Type RepositoryWithKey
            )
        {



            var Lcore = Assembly.Load("Lookif.Layers.Core");
            var Lservice = Assembly.Load("Lookif.Layers.Service");
            var CommonAssembly = Assembly.Load("Lookif.Library.Common");

            //RegisterType > As > Liftetime
            containerBuilder.RegisterGeneric(RepositoryWithKey).As(typeof(IRepository<,>)).InstancePerLifetimeScope();
            containerBuilder.RegisterGeneric(Repository).As(typeof(IRepository<>)).InstancePerLifetimeScope();



         
            var jwtServiceType = Lservice.GetExportedTypes()
                      .FirstOrDefault(t => t.IsGenericTypeDefinition && t.Name == "JwtService`1");
            containerBuilder.RegisterGeneric(jwtServiceType).As(typeof(IJwtService<>)).InstancePerLifetimeScope();


            containerBuilder.RegisterAssemblyTypes(CommonAssembly, entitiesAssembly, dataAssembly, servicesAssembly, Lcore, Lservice)
               .AssignableTo<IScopedDependency>()
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

            containerBuilder.RegisterAssemblyTypes(CommonAssembly, entitiesAssembly, dataAssembly, servicesAssembly, Lcore, Lservice)
                .AssignableTo<ITransientDependency>()
                .AsImplementedInterfaces()
                .InstancePerDependency();

            containerBuilder.RegisterAssemblyTypes(CommonAssembly, entitiesAssembly, dataAssembly, servicesAssembly, Lcore, Lservice)
                .AssignableTo<ISingletonDependency>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }


    }
}
