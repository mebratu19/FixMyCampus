using FixMyCampus.Api.Data;
using FixMyCampus.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using FixMyCampus.Api.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<TicketStatusService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});
var app = builder.Build();


// Create Roles and Admin User
// Create Roles, Admin User, and Technician User
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<int>>>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<User>>();

    string[] roles =
    {
        "Reporter",
        "Admin",
        "Technician"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(
                new IdentityRole<int>(role));
        }
    }

    // Create Admin
    var adminEmail = "admin@fixmycampus.com";

    var admin = await userManager.FindByEmailAsync(
        adminEmail);

    if (admin == null)
    {
        admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "FixMyCampus Admin",
            Role = "Admin",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            admin,
            "Admin@12345");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(
                admin,
                "Admin");
        }
    }

    // Create Technician
    var technicianEmail =
        "technician@fixmycampus.com";

    var technician = await userManager.FindByEmailAsync(
        technicianEmail);

    if (technician == null)
    {
        technician = new User
        {
            UserName = technicianEmail,
            Email = technicianEmail,
            FullName = "Campus Technician",
            Role = "Technician",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            technician,
            "Technician@12345");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(
                technician,
                "Technician");
        }
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();