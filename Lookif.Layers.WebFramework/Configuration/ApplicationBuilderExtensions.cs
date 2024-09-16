using Lookif.Layers.Core.Infrastructure.Base;
using Lookif.Layers.Core.Infrastructure.Base.DataInitializer;
using Lookif.Library.Common.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Lookif.Layers.WebFramework.Configuration;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHsts(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        Assert.NotNull(app, nameof(app));
        Assert.NotNull(env, nameof(env));

        if (!env.IsDevelopment())
            app.UseHsts();

        return app;
    }

    public static IApplicationBuilder IntializeDatabase(this IApplicationBuilder app, bool Do_Not_Use_Migration = false)
    {
        Assert.NotNull(app, nameof(app));

        //Use C# 8 using variables
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();


        var dataInitializers = scope.ServiceProvider.GetServices<IDataInitializer>();

        var databaseRelatedService = scope.ServiceProvider.GetRequiredService<IDataBaseRelatedService>();
        databaseRelatedService.RefreshDatabase(dataInitializers.ToList(), Do_Not_Use_Migration);
        return app;
    }
}
