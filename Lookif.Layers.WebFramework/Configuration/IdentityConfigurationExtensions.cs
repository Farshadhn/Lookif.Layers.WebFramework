using Lookif.Layers.Core.MainCore.Identities;
using Lookif.Library.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lookif.Layers.WebFramework.Configuration;

public static class IdentityConfigurationExtensions
{
     

    public static void AddCustomIdentity<TContext,TUser,TRole>(
        this IServiceCollection services,
        IdentitySettings settings,
        Action<IdentityOptions> identityOptions = null
        )
        where TRole : IdentityRole<Guid>
        where TUser : IdentityUser<Guid>
     where TContext : IdentityDbContext<TUser, TRole, Guid>
    {
        if (identityOptions is not null)
        {
            services.AddIdentity<TUser, TRole>(identityOptions)
                .AddEntityFrameworkStores<TContext>()
                .AddDefaultTokenProviders();
        }
        else
        {
            services.AddIdentity<TUser, TRole>(identityOptions =>
            {
                //Password Settings
                identityOptions.Password.RequireDigit = settings.PasswordRequireDigit;
                identityOptions.Password.RequiredLength = settings.PasswordRequiredLength;
                identityOptions.Password.RequireNonAlphanumeric = settings.PasswordRequireNonAlphanumic; //#@!
                identityOptions.Password.RequireUppercase = settings.PasswordRequireUppercase;
                identityOptions.Password.RequireLowercase = settings.PasswordRequireLowercase;

                //UserName Settings
                identityOptions.User.RequireUniqueEmail = settings.RequireUniqueEmail;

            }).AddEntityFrameworkStores<TContext>().AddDefaultTokenProviders();
        }
    }
}
