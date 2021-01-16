using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BookStoresWebAPI.Controllers
{
    //[Microsoft.AspNetCore.Authorization.Authorize]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [Microsoft.AspNetCore.Mvc.ApiController]
    public class UsersController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly Models.BookStoresDBContext _context;
        private readonly Models.JWTSettings _jWTSettings;

        public UsersController(Models.BookStoresDBContext context , 
            Microsoft.Extensions.Options.IOptions<Models.JWTSettings> jwtSettings)
        {
            _context = context;
            _jWTSettings = jwtSettings.Value;
        }

        // GET: api/Users
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async System.Threading.Tasks.Task
            <Microsoft.AspNetCore.Mvc.ActionResult
            <System.Collections.Generic.IEnumerable<Models.User>>>
                GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        public async System.Threading.Tasks.Task
            <Microsoft.AspNetCore.Mvc.ActionResult<Models.User>> 
                GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }


        [Microsoft.AspNetCore.Mvc.HttpGet("Login")]
        public async System.Threading.Tasks.Task
            <Microsoft.AspNetCore.Mvc.ActionResult<Models.UserWithToken>>
                    Login([Microsoft.AspNetCore.Mvc.FromBody] Models.User user)
        {
            user = await _context.Users
                .Where(u => u.EmailAddress == user.EmailAddress &&
                u.Password == user.Password).FirstOrDefaultAsync();

            Models.UserWithToken userWithToken = new Models.UserWithToken(user);

            if(userWithToken == null)
            {
                return NotFound();
            }

            //sign your token here
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_jWTSettings.SecretKey);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity
                (new System.Security.Claims.Claim[]
                    {
                        new System.Security.Claims.Claim
                            (System.Security.Claims.ClaimTypes.Name,
                            user.EmailAddress)
                    }),
                    Expires = System.DateTime.UtcNow.AddMonths(6),
                    SigningCredentials = 
                        new Microsoft.IdentityModel.Tokens.SigningCredentials
                            (new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            userWithToken.AccessToken = tokenHandler.WriteToken(token);


            return userWithToken;
        }

        //[Microsoft.AspNetCore.Mvc.HttpGet("GetUser")]
        //public async System.Threading.Tasks.Task
        //    <Microsoft.AspNetCore.Mvc.ActionResult<Models.User>> GetUser()
        //{
        //    string emailAdress = HttpContext.User.Identity.Name;

        //    var user = 
        //        await _context.Users
        //        .Where(user => user.EmailAddress == emailAdress)
        //        .FirstOrDefaultAsync();

        //    return user;
        //}

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Microsoft.AspNetCore.Mvc.HttpPut("{id}")]
        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> PutUser(int id, Models.User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Models.User>> PostUser(Models.User user)
        {
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.UserId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [Microsoft.AspNetCore.Mvc.HttpDelete("{id}")]
        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Models.User>> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
