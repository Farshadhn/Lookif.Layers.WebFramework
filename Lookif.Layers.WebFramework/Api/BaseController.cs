using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Lookif.Library.Common;
using Lookif.Library.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Lookif.Layers.WebFramework.Filters;

namespace Lookif.Layers.WebFramework.Api;

[ApiController]
[ApiResultFilter]
[Route("api/v{version:apiVersion}/[controller]/[action]")]// api/v1/[controller]
public class BaseController<TService> : ControllerBase
{
    public TService Service => HttpContext.RequestServices.GetRequiredService<TService>();
    public Guid UserId =>
        HttpContext.User switch
        {
            not null and ClaimsPrincipal user when user.Identity is not null && user.Identity.IsAuthenticated => Guid.Parse(user.Identity?.GetUserId()),
            _ => Guid.Empty
        }; 


    public string UserName => ((ClaimsIdentity)HttpContext.User.Identity).Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value;

    public DateTime Time => HttpContext.Request.Headers["Time"].ToString().ToDateTime();
    public IEnumerable<string> roles => ((ClaimsIdentity)HttpContext.User.Identity).Claims
          .Where(c => c.Type == ClaimTypes.Role)
          .Select(c => c.Value);
}
[ApiController]
[ApiResultFilter]
[Route("api/v{version:apiVersion}/[controller]/[action]")]// api/v1/[controller]
public class BaseController : ControllerBase
{
    public Guid UserId => Guid.Parse(HttpContext.User.Identity.GetUserId());

    public DateTime Time => HttpContext.Request.Headers["Time"].ToString().ToDateTime();
    public IEnumerable<string> roles => ((ClaimsIdentity)HttpContext.User.Identity).Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value);
}
