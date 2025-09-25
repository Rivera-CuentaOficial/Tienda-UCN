using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.MapOpenApi();

app.Run();