using Lookif.Layers.Core.Infrastructure.Base;
using Lookif.Layers.Core.Infrastructure.Base.DataInitializer;
using Lookif.Library.Common.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace Lookif.Layers.WebFramework.Configuration;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHsts(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
            app.UseHsts();

        return app;
    }

    public static async Task<IApplicationBuilder> IntializeDatabase(this IApplicationBuilder app, bool useMigration = true)
    {
        
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();


        var dataInitializers = scope.ServiceProvider.GetServices<IDataInitializer>();

        var databaseRelatedService = scope.ServiceProvider.GetRequiredService<IDataBaseRelatedService>();
        await databaseRelatedService.RefreshDatabaseAsync(dataInitializers.ToList(), useMigration);
        return app;
    }
}
