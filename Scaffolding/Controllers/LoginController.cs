using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Scaffolding.Models;
using Dapper;
using System.Data;
using System.Data.SQLite;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Scaffolding.Controllers
{
	[AllowAnonymous]
	public class LoginController : Controller
	{
		private IConfiguration configuration;
		private readonly IWebHostEnvironment webHost;
		private readonly ILogger<LoginController> _logger;

		public LoginController(IConfiguration _configuration, ILogger<LoginController> logger, IWebHostEnvironment _webHost)
		{
			configuration = _configuration;
			_logger = logger;
			webHost = _webHost;
		}

		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public IActionResult Index(LoginUser user)
		{
			using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
			{
				user = db.Query<LoginUser>("select * from LoginUsers where name=@name and password = @password", user).FirstOrDefault();

				if (user != null)
				{
					Login(user);
					ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
					identity.AddClaim(new Claim(ClaimTypes.Name, user.name));

					HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
					return RedirectToAction("Index", "Home");
				}
				else
				{
					ViewBag.u = "Invalid Username and Password";
					return View();
				}
			}
		}
        private LoginUser AuthenticateUser(LoginUser user)
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                if (user != null)
                {
                    LoginUser loginUser = db.Query<LoginUser>("select * from LoginUsers where name = @name and password = @password", user).FirstOrDefault();
                    if (user.name == loginUser.name && user.password == loginUser.password)
                    {
                        loginUser = new LoginUser { name = user.name, password = user.password };
                    }
                    return loginUser;
                }
                else
                {
                    return user;
                }
            }
        }

        private string GenerateToken(LoginUser user)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var tokenn = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], null,
                expires: DateTime.Now.AddHours(4), signingCredentials: credentials);
			return new JwtSecurityTokenHandler().WriteToken(tokenn);
        }
		[HttpPost]
		public IActionResult Login(LoginUser user)
		{
			using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
			{
				user = db.Query<LoginUser>("select * from LoginUsers where name=@name and password = @password", user).FirstOrDefault();

				IActionResult response = Unauthorized();
				var _user = AuthenticateUser(user);
				if (_user != null)
				{
					var token = GenerateToken(_user);
					response = Ok(new { token = token, Message = "Token Generated" });
                }
                else
                {
                    response = BadRequest(new { Message = "Invalid Username or Password" });
                }
                return response;
            }
		}
		public IActionResult Logout()
		{
			HttpContext.SignOutAsync();
			return RedirectToAction("Index");
		}
	}
}
