using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Auths
{
    public class JobsAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string HeaderKey { get; set; }
        public string U { get; set; }
        public string P { get; set; }
    }

    public class JobsAuthenticationHandler : AuthenticationHandler<JobsAuthenticationSchemeOptions>
    {
        public JobsAuthenticationHandler(IOptionsMonitor<JobsAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            await Task.CompletedTask;

            if (!Request.Headers.ContainsKey(Options.HeaderKey))
            {
                return AuthenticateResult.NoResult();
            }

            var up = Request.Headers[Options.HeaderKey];

            if (string.IsNullOrEmpty(up) || string.IsNullOrWhiteSpace(up))
                return AuthenticateResult.Fail("非法调用jobs");

            if (!string.Equals(Options.U + "," + Options.P, up, StringComparison.CurrentCultureIgnoreCase))
                return AuthenticateResult.Fail("非法调用jobs");

            var cp = new ClaimsPrincipal();
            var i = new ClaimsIdentity("jobs", "name", null);
            i.AddClaim(new Claim("name", Options.U));
            i.AddClaim(new Claim("for-jobs", "1"));
            cp.AddIdentity(i);
            return AuthenticateResult.Success(new AuthenticationTicket(cp, null, "jobs"));
        }
    }

    public class JobsAuthorizationRequirement : IAuthorizationRequirement
    {        
    }

    public class JobsAuthorizationHandler : AuthorizationHandler<JobsAuthorizationRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, JobsAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == "for-jobs" && c.Value == "1"))
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
            await Task.CompletedTask;
        }
    }
}
