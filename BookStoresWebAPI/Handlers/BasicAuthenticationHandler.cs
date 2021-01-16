using Microsoft.AspNetCore.Authentication;
using System.Linq;

namespace BookStoresWebAPI.Handlers
{
    public class BasicAuthenticationHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
    {
        private readonly Models.BookStoresDBContext _context;
        public BasicAuthenticationHandler
            (Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder,
            ISystemClock clock,
            Models.BookStoresDBContext context) 
            : base(options, logger, encoder, clock)
        {
            _context = context;
        }
          

        protected override async System.Threading.Tasks.Task
            <Microsoft.AspNetCore.Authentication.AuthenticateResult>
            HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Microsoft.AspNetCore.Authentication.AuthenticateResult.Fail("Authorization Not Found!");
            }

            try
            {
                var authenticationHeaderValue =
                        System.Net.Http.Headers.AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

                byte[] bytes =
                    System.Convert.FromBase64String(authenticationHeaderValue.Parameter);

                string[] credentials = System.Text.Encoding.UTF8.GetString(bytes).Split(":");
                string emailAddress = credentials[0];
                string password = credentials[1];

                Models.User user =
                    _context.Users
                    .Where(user => user.EmailAddress == emailAddress && user.Password == password)
                    .FirstOrDefault();

                if(user == null)
                {
                    Microsoft.AspNetCore.Authentication.AuthenticateResult.Fail("Invalid username or password");
                }
                else
                {
                    var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.EmailAddress) };
                    var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
                    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
            }
            catch(System.Exception ex)
            {
                Microsoft.AspNetCore.Authentication.AuthenticateResult.Fail("Error has occured!");
            }

            return AuthenticateResult.Fail("");
        }
    }
}
