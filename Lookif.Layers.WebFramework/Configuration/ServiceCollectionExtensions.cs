using ElmahCore.Mvc;
using ElmahCore.Sql;
using Lookif.Layers.Core.Infrastructure.Base.Repositories;
using Lookif.Layers.Core.MainCore.Identities;
using Lookif.Library.Common;
using Lookif.Library.Common.Exceptions;
using Lookif.Library.Common.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.Layers.WebFramework.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDbContext<T>(this IServiceCollection services, IConfiguration configuration) where T : IdentityDbContext<User, Role, Guid>
        {
            services.AddDbContextFactory<T>(options =>
            {
                options
                    .UseSqlServer(configuration.GetConnectionString("SqlServer"));
            });
        }

        public static IMvcBuilder AddMinimalMvc(this IServiceCollection services)
        {
            //https://github.com/aspnet/AspNetCore/blob/0303c9e90b5b48b309a78c2ec9911db1812e6bf3/src/Mvc/Mvc/src/MvcServiceCollectionExtensions.cs
            services.AddSwaggerGenNewtonsoftSupport();
            return services.AddControllers(options =>
            {
                options.Filters.Add(new AuthorizeFilter()); //Apply AuthorizeFilter as global filter to all actions

                //Like [ValidateAntiforgeryToken] attribute but dose not validatie for GET and HEAD http method
                //You can ingore validate by using [IgnoreAntiforgeryToken] attribute
                //Use this filter when use cookie 
                //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

                //options.UseYeKeModelBinder();
            }).AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.Converters.Add(new StringEnumConverter());
                option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                option.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                option.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });



        }

        public static void AddElmahCore(this IServiceCollection services, IConfiguration configuration, SiteSettings siteSetting)
        {
            services.AddElmah<SqlErrorLog>(options =>
            {
                options.Path = siteSetting.ElmahPath;
                options.ConnectionString = configuration.GetConnectionString("Elmah");
                //options.CheckPermissionAction = httpContext => httpContext.User.Identity.IsAuthenticated;
            });
        }

        public static async Task<AuthenticationBuilder> AddJwtAuthentication(
            this IServiceCollection services,
            JwtSettings jwtSettings,
            Action<AuthenticationOptions> authenticationOption = null,
            Action<JwtBearerOptions> jwtBearerOptions = null
            )
        {

            AuthenticationBuilder authenticationBuilder = default;
            (Action<AuthenticationOptions> defaultAuthenticationOption, Action<JwtBearerOptions> defaultJWTBearerOptions) = await FillDefaultValuesAsync();
            authenticationBuilder = authenticationOption switch
            {

                not null => authenticationBuilder = services.AddAuthentication(authenticationOption),
                _ => services.AddAuthentication(defaultAuthenticationOption)

            };

            authenticationBuilder = jwtBearerOptions switch
            {

                not null => authenticationBuilder.AddJwtBearer(jwtBearerOptions),
                _ => authenticationBuilder.AddJwtBearer(defaultJWTBearerOptions)
            };
            return authenticationBuilder;


            async Task<(Action<AuthenticationOptions> defaultAuthenticationOption, Action<JwtBearerOptions> defaultJWTBearerOptions)> FillDefaultValuesAsync()
            {
                Action<AuthenticationOptions> defaultAuthenticationOption = options =>
               {
                   options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                   options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                   options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
               };
                Action<JwtBearerOptions> defaultJWTBearerOptions = options =>
               {
                   var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
                   //ToDo Make it Secure
                   //var encryptionKey = Encoding.UTF8.GetBytes(jwtSettings.Encryptkey);

                   var validationParameters = new TokenValidationParameters
                   {
                       ClockSkew = TimeSpan.Zero, // default: 5 min
                       RequireSignedTokens = true,

                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                       RequireExpirationTime = true,
                       ValidateLifetime = true,

                       ValidateAudience = true, //default : false
                       ValidAudience = jwtSettings.Audience,

                       ValidateIssuer = true, //default : false
                       ValidIssuer = jwtSettings.Issuer,

                       //TokenDecryptionKey = new SymmetricSecurityKey(encryptionKey)
                   };

                   options.RequireHttpsMetadata = false;
                   options.SaveToken = true;
                   options.TokenValidationParameters = validationParameters;
                   options.Events = new JwtBearerEvents
                   {
                       OnAuthenticationFailed = context =>
                       {
                           //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                           //logger.LogError("Authentication failed.", context.Exception);

                           if (context.Exception != null)
                               throw new AppException(ApiResultStatusCode.UnAuthorized, "Authentication failed.", HttpStatusCode.Unauthorized, context.Exception, null);

                           return Task.CompletedTask;
                       },
                       OnTokenValidated = async context =>
                       {
                           var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
                           var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRelated>();

                           var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                           if (claimsIdentity.Claims?.Any() != true)
                               context.Fail("This token has no claims.");

                           var securityStamp = claimsIdentity.FindFirstValue(new ClaimsIdentityOptions().SecurityStampClaimType);
                           if (!securityStamp.HasValue())
                               context.Fail("This token has no security stamp");

                           //Find user and token from database and perform your custom validation
                           // string or int!! make your choice
                           var Id = claimsIdentity.GetUserId<string>();
                           Guid.TryParse(Id, out Guid userId);
                           //userRepository.
                           var user = await userRepository.GetById(userId, context.HttpContext.RequestAborted);

                           if (Guid.TryParse(securityStamp, out _) && user.SecurityStamp != securityStamp)
                               context.Fail("Token security stamp is not valid.");

                           var validatedUser = await signInManager.ValidateSecurityStampAsync(context.Principal);
                           if (validatedUser == null)
                               context.Fail("Token security stamp is not valid. - ValidateSecurityStampAsync");

                           if (!user.IsActive)
                               context.Fail("User is not active.");

                       },
                       OnChallenge = context =>
                       {
                           //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                           //logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);

                           if (context.AuthenticateFailure != null)
                               throw new AppException(ApiResultStatusCode.UnAuthorized, "Authenticate failure.", HttpStatusCode.Unauthorized, context.AuthenticateFailure, null);
                           throw new AppException(ApiResultStatusCode.UnAuthorized, "You are unauthorized to access this resource.", HttpStatusCode.Unauthorized);

                           //return Task.CompletedTask;
                       }
                   };
               };
                return (defaultAuthenticationOption, defaultJWTBearerOptions);
            }

        }








        public static void AddCustomApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                //url segment => {version}
                options.AssumeDefaultVersionWhenUnspecified = true; //default => false;
                options.DefaultApiVersion = new ApiVersion(1, 0); //v1.0 == v1
                options.ReportApiVersions = true;


            });
        }
    }
}
