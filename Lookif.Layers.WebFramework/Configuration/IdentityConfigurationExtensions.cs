using Lookif.Library.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using Lookif.Layers.Core.MainCore.Identities;

namespace Lookif.Layers.WebFramework.Configuration
{
    public static class IdentityConfigurationExtensions
    {
        public static void AddCustomIdentity<T>(this IServiceCollection services, IdentitySettings settings)  
         where T : IdentityDbContext<User, Role, Guid>
        {
            services.AddIdentity<User, Role>(identityOptions =>
            {
                //Password Settings
                identityOptions.Password.RequireDigit = settings.PasswordRequireDigit;
                identityOptions.Password.RequiredLength = settings.PasswordRequiredLength;
                identityOptions.Password.RequireNonAlphanumeric = settings.PasswordRequireNonAlphanumic; //#@!
                identityOptions.Password.RequireUppercase = settings.PasswordRequireUppercase;
                identityOptions.Password.RequireLowercase = settings.PasswordRequireLowercase;

                //UserName Settings
                identityOptions.User.RequireUniqueEmail = settings.RequireUniqueEmail;

                //Singin Settings
                //identityOptions.SignIn.RequireConfirmedEmail = false;
                //identityOptions.SignIn.RequireConfirmedPhoneNumber = false;

                //Lockout Settings
                //identityOptions.Lockout.MaxFailedAccLookif.LayersAttempts = 5;
                //identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                //identityOptions.Lockout.AllowedForNewUsers = false;
            })
            .AddEntityFrameworkStores<T>()
            .AddDefaultTokenProviders();
        }
    }
}
