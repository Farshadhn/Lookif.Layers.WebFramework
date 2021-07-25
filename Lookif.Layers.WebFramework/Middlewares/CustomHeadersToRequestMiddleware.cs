using Lookif.Library.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.Layers.WebFramework.Middlewares
{
    public class CustomHeadersToRequestMiddleware
    {


        private readonly RequestDelegate _next;

        public CustomHeadersToRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers.Add("Time", DateTime.Now.ToString());
            //context.Request.Headers.Add("User", context.User.Identity.GetUserId());

            await _next(context);
            context.Request.Headers.Remove("Time");
            //context.Request.Headers.Remove("User");
        }


    }



    public static class UseCustomHeaderstMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomHeaders(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomHeadersToRequestMiddleware>();
        }
    }
}
