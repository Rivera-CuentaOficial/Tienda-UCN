using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Resend;
using Serilog;
using TiendaUCN.src.API.Middlewares;
using TiendaUCN.src.Application.Jobs.Implements;
using TiendaUCN.src.Application.Jobs.Interfaces;
using TiendaUCN.src.Application.Mappers;
using TiendaUCN.src.Application.Services.Implements;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Implements;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("SqliteDatabase")
    ?? throw new InvalidOperationException("Connection string SqliteDatabase no configurado");

builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<ProductMapper>();
builder.Services.AddScoped<CartMapper>();
builder.Services.AddScoped<OrderMapper>();
builder.Services.AddScoped<AuditMapper>();
builder.Services.AddScoped<CategoryAndBrandMapper>();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserJob, UserJob>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IBlacklistedTokensRepository, BlacklistedTokensRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();


#region Email Service Configuration
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken =
        builder.Configuration.GetValue<string>("ResendAPIKey")
        ?? throw new InvalidOperationException("ResendAPIKey no esta configurada");
});
builder.Services.AddTransient<IResend, ResendClient>();
#endregion

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
    .AddRoles<Role>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
#endregion

#region Authentication Configuration
Log.Information("Configurando autenticación JWT");
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        string jwtSecret =
            builder.Configuration["JWTSecret"]
            ?? throw new InvalidOperationException("La clave secreta JWT no está configurada.");
        options.TokenValidationParameters =
            new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtSecret)
                ),
                ValidateLifetime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero, //Sin tolerencia a tokens expirados
            };
    });
#endregion

#region Database Configuration
Log.Information("Configuring SQLite database...");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetSection("ConnectionStrings:SqliteDatabase").Value)
);
#endregion

#region Hangfire Configuration
Log.Information("Configurando los trabajos en segundo plano de Hangfire");
var cronExpression =
    builder.Configuration["Jobs:CronJobDeleteUnconfirmedUsers"]
    ?? throw new InvalidOperationException(
        "La expresión cron para eliminar usuarios no confirmados no está configurada."
    );
var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
    builder.Configuration["Jobs:TimeZone"]
        ?? throw new InvalidOperationException(
            "La zona horaria para los trabajos no está configurada."
        )
);
builder.Services.AddHangfire(configuration =>
{
    var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);
    var databasePath = connectionStringBuilder.DataSource;

    configuration.UseSQLiteStorage(databasePath);
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    configuration.UseSimpleAssemblyNameTypeSerializer();
    configuration.UseRecommendedSerializerSettings();
});
builder.Services.AddHangfireServer();

#endregion
var app = builder.Build();
app.UseHangfireDashboard(
    builder.Configuration["HangfireDashboard:DashboardPath"]
        ?? throw new InvalidOperationException("La ruta de hangfire no ha sido declarada"),
    new DashboardOptions
    {
        StatsPollingInterval =
            builder.Configuration.GetValue<int?>("HangfireDashboard:StatsPollingInterval")
            ?? throw new InvalidOperationException(
                "El intervalo de actualización de estadísticas del panel de control de Hangfire no está configurado."
            ),
        DashboardTitle =
            builder.Configuration["HangfireDashboard:DashboardTitle"]
            ?? throw new InvalidOperationException(
                "El título del panel de control de Hangfire no está configurado."
            ),
        DisplayStorageConnectionString =
            builder.Configuration.GetValue<bool?>(
                "HangfireDashboard:DisplayStorageConnectionString"
            )
            ?? throw new InvalidOperationException(
                "La configuración 'HangfireDashboard:DisplayStorageConnectionString' no está definida."
            ),
    }
);

#region Database Migrations and jobs Configuration
Log.Information("Migrating and seeding database...");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(app.Services);
    var jobId = nameof(UserJob.DeleteUnconfirmedAsync);
    RecurringJob.AddOrUpdate<UserJob>(
        jobId,
        job => job.DeleteUnconfirmedAsync(),
        cronExpression,
        new RecurringJobOptions { TimeZone = timeZone }
    );
    Log.Information(
        $"Job recurrente '{jobId}' configurado con cron: {cronExpression} en zona horaria: {timeZone.Id}"
    );

    MapperExtensions.ConfigureMapster(scope.ServiceProvider);
}
#endregion

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CartMiddleware>();
//app.UseMiddleware<TokenBlacklistMiddleware>();

app.MapOpenApi();
app.MapControllers();
app.Run();