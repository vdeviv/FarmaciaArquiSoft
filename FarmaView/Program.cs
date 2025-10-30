using Microsoft.AspNetCore.Authentication.Cookies; // 🚀 Re-añadido
using Microsoft.AspNetCore.Localization; // 🚀 Re-añadido
using Microsoft.Extensions.Options; // 🚀 Re-añadido
using ServiceClient.Application;
using ServiceClient.Infrastructure;
using ServiceCommon.Application;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Data;
using ServiceCommon.Infrastructure.Persistence; // 🚀 Re-añadido (necesario para persistencia)
using ServiceLot.Application;
using ServiceLot.Infrastructure;
using ServiceReports.Application;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Application.Services;
using ServiceReports.Infrastructure;
using ServiceReports.Infrastructure.Reports;
using ServiceReports.Infrastructure.Repositories;
using ServiceUser.Application.Services;
using ServiceUser.Domain;
using ServiceUser.Domain.Validators; // 🚀 Re-añadido
using ServiceUser.Infraestructure.Persistence; // 🚀 Re-añadido
using System.Globalization; // 🚀 Re-añadido
using ClientEntity = ServiceClient.Domain.Client;
using LotEntity = ServiceLot.Domain.Lot;

var builder = WebApplication.CreateBuilder(args);


DatabaseConnection.Initialize(builder.Configuration);

builder.Services.AddRazorPages()

        .AddViewOptions(o =>
        {
            o.HtmlHelperOptions.ClientValidationEnabled = false;
        })
        .AddMvcOptions(options =>
        {
            options.ModelMetadataDetailsProviders.Clear();
            options.ModelValidatorProviders.Clear();
        });
// --------------------------------------------------------------------------------

builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

builder.Services.AddScoped<IRepository<ClientEntity>, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddScoped<IRepository<LotEntity>, LotRepository>();
builder.Services.AddScoped<LotService>();

// =========================================================================
// SERVICIOS DE USUARIO
// =========================================================================
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IValidator<User>, UserValidator>();

// CONFIGURACIÓN DE CULTURA
var supportedCultures = new[] { new CultureInfo("es-BO"), new CultureInfo("es") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-BO");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// CONFIGURACIÓN DE EMAIL
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailSender, EmailSender>();


// =========================================================================
// AUTENTICACIÓN
// =========================================================================
builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.AccessDeniedPath = "/Auth/Denied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });

builder.Services.AddAuthorization();

// =========================================================================
// SERVICIOS DE REPORTES (CONSOLIDADO)
// =========================================================================
builder.Services.AddScoped<ReportRepository>();

// Registramos dos implementaciones para la misma interfaz
builder.Services.AddScoped<IClientFidelityReportBuilder, PdfClientFidelityReportBuilder>();
builder.Services.AddScoped<IClientFidelityReportBuilder, ExcelClientFidelityReportBuilder>();

// El sistema inyectará la colección de Builders arriba registrados aquí
builder.Services.AddScoped<IClientFidelityReportService, ClientFidelityReportService>();


// =========================
// Construcción de la app
// =========================
var app = builder.Build();

// =========================
// Middleware del pipeline
// =========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// MIDDLEWARE DE LOCALIZACIÓN
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MIDDLEWARE DE AUTENTICACIÓN (CRUCIAL)
app.UseAuthentication();

// MIDDLEWARE PERSONALIZADO PARA FORZAR EL CAMBIO DE CONTRASEÑA
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value?.ToLowerInvariant() ?? "";

    bool allow =
      path.StartsWith("/auth/login") ||
      path.StartsWith("/auth/changepassword") ||
      path.StartsWith("/auth/logout") ||
      path.StartsWith("/css") || path.StartsWith("/js") ||
      path.StartsWith("/lib") || path.StartsWith("/images");

    var authed = ctx.User?.Identity?.IsAuthenticated == true;
    if (authed)
    {
        var changed = ctx.User.FindFirst("HasChangedPassword")?.Value == "true";
        if (!changed && !allow)
        {
            ctx.Response.Redirect("/Auth/ChangePassword");
            return;
        }
    }

    await next();
});

app.UseAuthorization(); // Ahora en el orden correcto (después de UseAuthentication)

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();