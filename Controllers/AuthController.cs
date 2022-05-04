using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using secure_api.ViewModels;

namespace secure_api.Controllers
{
  [ApiController]
  [Route("api/auth")]
  public class AuthController : ControllerBase
  {
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;
          private readonly SignInManager<IdentityUser> _signManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthController(IConfiguration config, UserManager<IdentityUser> userManager,
         SignInManager<IdentityUser> signinManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signManager = signinManager;
            _config = config;

        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromQuery]string roleName)
        {
          if (!await _roleManager.RoleExistsAsync(roleName))
          {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
              return BadRequest(result.Errors);
            }
          }
          
          return StatusCode(201);
        }

        [HttpPost("register")]
        public async Task <ActionResult<UserViewModel>> RegisterUser(RegisterUserViewModel model)
        {
          //steg 1: vi skall skapa en ny identityuser typ
          var user = new IdentityUser{
          Email = model.Email!.ToLower(),
          UserName = model.Email.ToLower()

          };
          //steg 2 spara användare till databasen
         var result = await _userManager.CreateAsync(user,model.Password);
         if (result.Succeeded)
         {
           // skapa en claim som heter admin if iadmin is true
           if (model.IsAdmin)
           {
             await _userManager.AddClaimAsync(user, new Claim("Admin", "true"));
             await _userManager.AddToRoleAsync(user, "Administrator");
           }
           await _userManager.AddClaimAsync(user, new Claim ("User","true"));
           await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, user.UserName));
           await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, user.Email));
           await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.NameIdentifier, user.Id));

           
           var userData = new UserViewModel
           {
             UserName = user.UserName,
             Token = await CreateJwtToken(user)
           };
           return StatusCode(201,userData);
         }
         else
         {
           foreach (var error in result.Errors)
           {
             ModelState.AddModelError("User Registration", error.Description);
           }
         }
          return StatusCode(500, ModelState);
        }
        

        [HttpPost("login")]
    public async Task <ActionResult<UserViewModel>> Login(LoginViewModel model)
    {

      // steg1 kontrollera om användare finns i systemet
      var user =await _userManager.FindByNameAsync(model.UserName);
      // om inte kasta ett 401 medelande
      if (user is null)
      {
        return Unauthorized("Felaktigt användarnamn");
      }
      // steg 3 försök att logga in användare 
     var result = await _signManager.CheckPasswordSignInAsync(user, model.Password, false);

     // om inte kasta ett 401 medelande
     if (!result.Succeeded)
     {
       return Unauthorized();
     }
     // steg  5 skapa ett user view modell objekt
     var userData = new UserViewModel{
       UserName = user.UserName,
       Token =  await CreateJwtToken(user)
     };
      return Ok(userData);


    }
    private async Task <string> CreateJwtToken(IdentityUser user)
    {

      // Kommer att hämtas ifrån AppSettings...
      var key = Encoding.ASCII.GetBytes(_config.GetValue<string>("apiKey"));

      // Skapa en lista av Claims som kommer innehålla
      // information som är av värde för behörighetskontroll...
      // var claims = new List<Claim>
      // {
      //     new Claim(ClaimTypes.Name, user.UserName),
      //     new Claim(ClaimTypes.Email, user.Email),
      //     //new Claim("Admin", "true")
          
      // };
    var userClaims = (await _userManager.GetClaimsAsync(user)).ToList();
    var roles = await _userManager.GetRolesAsync(user);
    
    userClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

      // Skapa ett nytt token...
      var jwt = new JwtSecurityToken(
          claims: userClaims,
          // notBefore: Från när skall biljetten/token vara giltig.
          // Vi kan sätta detta till en datum i framtiden om biljetten/token
          // skall skapas men inte vara giltig på en gång...
          notBefore: DateTime.Now,
          // Sätt giltighetstiden på biljetten i detta fallet en vecka.
          expires: DateTime.Now.AddDays(7),
          // Skapa en instans av SigningCredential klassen
          // som används för att skapa en hash och signering av biljetten.
          signingCredentials: new SigningCredentials(
              // Vi använder en SymmetricSecurityKey som tar vår hemlighet
              // som argument och sedan talar vi om vilken algoritm som skall
              // användas för att skapa hash värdet.
              new SymmetricSecurityKey(key),
              SecurityAlgorithms.HmacSha512Signature
          )
      );

      // Vi använder klassen JwtSecurityTokenHandler och dess metod WriteToken för att
      // skapa en sträng av vårt token...
      return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    // private string CreateJwtToken(string userName)
    // {

    //   // Kommer att hämtas ifrån AppSettings...
    //   var key = Encoding.ASCII.GetBytes(_config.GetValue<string>("apiKey"));

    //   // Skapa en lista av Claims som kommer innehålla
    //   // information som är av värde för behörighetskontroll...
    //   var claims = new List<Claim>
    //   {
    //       new Claim(ClaimTypes.Name, userName),
    //       new Claim(ClaimTypes.Email,"kalle@gmail.com"),
    //       new Claim("Admin", "true")
          
    //   };

    //   // Skapa ett nytt token...
    //   var jwt = new JwtSecurityToken(
    //       claims: claims,
    //       // notBefore: Från när skall biljetten/token vara giltig.
    //       // Vi kan sätta detta till en datum i framtiden om biljetten/token
    //       // skall skapas men inte vara giltig på en gång...
    //       notBefore: DateTime.Now,
    //       // Sätt giltighetstiden på biljetten i detta fallet en vecka.
    //       expires: DateTime.Now.AddDays(7),
    //       // Skapa en instans av SigningCredential klassen
    //       // som används för att skapa en hash och signering av biljetten.
    //       signingCredentials: new SigningCredentials(
    //           // Vi använder en SymmetricSecurityKey som tar vår hemlighet
    //           // som argument och sedan talar vi om vilken algoritm som skall
    //           // användas för att skapa hash värdet.
    //           new SymmetricSecurityKey(key),
    //           SecurityAlgorithms.HmacSha512Signature
    //       )
    //   );

    //   // Vi använder klassen JwtSecurityTokenHandler och dess metod WriteToken för att
    //   // skapa en sträng av vårt token...
    //   return new JwtSecurityTokenHandler().WriteToken(jwt);
    // }
  }
}







 // Detta är bara en "dummy" kontroll av användare och lösenord
      // Vi kommer att hantera detta via Identity lite senare...
      // if (model.UserName == "kalle" && model.Password == "Pa$$w0rd")
      // {
      //  Vi skapar ett anonymt objekt här för tillfället...
      //   return Ok(new
      //   {
      //     access_token = ""CreateJwtToken(model.UserName)
      //   });
      // }