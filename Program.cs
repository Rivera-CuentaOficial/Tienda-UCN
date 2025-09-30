using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.API.Middlewares;
using TiendaUCN.Application.Infrastructure.Repositories.Implements;
using TiendaUCN.Application.Infrastructure.Repositories.Interfaces;
using TiendaUCN.Application.Services.Implements;
using TiendaUCN.Application.Services.Interfaces;
using TiendaUCN.Domain.Models;
using TiendaUCN.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

#region Logging Configuration
builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services)
);
#endregion

#region Identity Configuration
Log.Information("Configuring Identity...");
builder
    .Services.AddIdentity<User, Role>(options =>
    {
        //Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        //Email settings
        options.User.RequireUniqueEmail = true;

        //Username settings
        options.User.AllowedUserNameCharacters =
            builder
                .Configuration.GetSection("IdentityConfiguration:AllowedUserNameCharacters")
                .Value
            ?? throw new InvalidOperationException("AllowedUserNameCharacters not configured");
    })
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
#endregion

#region Database Configuration
Log.Information("Configuring SQLite database...");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetSection("ConnectionStrings:SqliteDatabase").Value)
);
#endregion

var app = builder.Build();

#region Database Migrations
Log.Information("Migrating and seeding database...");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(app.Services);
}
#endregion

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapControllers();
app.Run();