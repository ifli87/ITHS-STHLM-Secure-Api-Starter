using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using secure_api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//steg1 konfigurera vårat datacontext till att använda sqlite
builder.Services.AddDbContext<ApplicationContext>(options =>{
    options.UseSqlite(builder.Configuration.GetConnectionString("sqlite"));
});

// sätta upp identity hantering och ange vilket datacontext som skall användas för att lagra anvädare roller och claim, vi kan dessutom sätta upp regler för ex lösenord och utlåsningsprinciper
builder.Services.AddIdentity<IdentityUser, IdentityRole>(
    options => {
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    }
)
.AddEntityFrameworkStores<ApplicationContext>();

// konfigurera authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters{
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("apiKey"))
        ),
        ValidateLifetime = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        ClockSkew = TimeSpan.Zero
    };
});

//Konfigurera och skapa "policies"
builder.Services.AddAuthorization(options => {
options.AddPolicy("Admins", policy => policy.RequireClaim("Admin"));
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
/*Pipeline definierar ordningen på metodernas motagning och utskick*/
/* Placeras Middleware*/
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
