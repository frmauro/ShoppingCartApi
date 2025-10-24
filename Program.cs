using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShoppingCartApi.Data;
using ShoppingCartApi.Models;
using ShoppingCartApi.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

 var connectionString = configuration["MySqlConnection:MySqlConnectionString"];
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(5, 7)),
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));


// // -- Identity --
// builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
// {
//     options.Password.RequireDigit = false;
//     options.Password.RequireNonAlphanumeric = false;
//     options.Password.RequiredLength = 6;
// })
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders();



//-- JWT Authentication --
var jwtSection = configuration.GetSection("Jwt");
var key = jwtSection.GetValue<string>("Key")!;
var issuer = jwtSection.GetValue<string>("Issuer");
var audience = jwtSection.GetValue<string>("Audience");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    //options.RequireHttpsMetadata = false;
    //options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                //valores validos
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
                ClockSkew = TimeSpan.Zero
            };
});



// -- App services --
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<ProductService>();

// -- Controllers & Swagger --
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShoppingCart API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } }
    });
});

var app = builder.Build();

// Apply migrations automatically in development (opcional)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShoppingCart API v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

