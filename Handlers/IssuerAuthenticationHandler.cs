using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using McNativePayment.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace McNativePayment.Handlers
{

    public class IssuerAuthenticationHandlerOptions
        : AuthenticationSchemeOptions
    { }

    public class IssuerAuthenticationHandler : AuthenticationHandler<IssuerAuthenticationHandlerOptions>
    {

        public const string AUTHENTICATION_SCHEMA = "Issuer Token Authentication";

        private readonly PaymentContext _context;

        public IssuerAuthenticationHandler(IOptionsMonitor<IssuerAuthenticationHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, PaymentContext context) 
            : base(options, logger, encoder, clock)
        {
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            if (!Request.Headers.ContainsKey("API-Token"))
            {
                return AuthenticateResult.Fail("API token is missing");
            }

            var token = Request.Headers["API-Token"].ToString();

            Issuer issuer = await _context.Issuers.FirstOrDefaultAsync(i => i.Token == token);

            if (issuer == null)
            {
                return AuthenticateResult.Fail("Invalid API token");
            }

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, issuer.Id),
                new Claim(ClaimTypes.Name, issuer.Name) };

            var claimsIdentity = new ClaimsIdentity(claims, nameof(IssuerAuthenticationHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
