using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using ServiceClient.Application;
using ServiceClient.Infrastructure;
using ServiceCommon.Application;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Data;
using ServiceCommon.Infrastructure.Persistence;
using ServiceLot.Application;
using ServiceLot.Domain.Validators;           // ✅ para LotValidator
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
using ServiceUser.Domain.Validators;
using ServiceUser.Infraestructure.Persistence;
using ServiceClient.Domain;                    // para tipos de Client
using ServiceClient.Domain.Validators;         // para ClientValidator
using System.Globalization;

using ClientEntity = ServiceClient.Domain.Client;
<<<<<<< HEAD
using ServiceProvider.Application;
using ServiceProvider.Infraestructure;
using ProviderEntity = ServiceProvider.Domain.Provider;

// USINGS AÑADIDOS DE LA RAMA Reportes
using ServiceReports.Application;
using ServiceReports.Infrastructure;
using ServiceReports.Infrastructure.Repositories;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Application.Services;
using ServiceReports.Infrastructure.Reports;

=======
>>>>>>> main
using LotEntity = ServiceLot.Domain.Lot;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 🔧 Inicialización de base de datos
// =========================================================
DatabaseConnection.Initialize(builder.Configuration);

// =========================================================
// 🔧 Razor Pages + Validaciones personalizadas
// =========================================================
builder.Services
    .AddRazorPages()
    .AddViewOptions(o => { o.HtmlHelperOptions.ClientValidationEnabled = false; })
    .AddMvcOptions(options =>
    {
        options.ModelMetadataDetailsProviders.Clear();
        options.ModelValidatorProviders.Clear();
    });

// =========================================================
// 🔒 Servicios comunes
// =========================================================
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

<<<<<<< HEAD

// =========================================================================
// REGISTROS DE SERVICIOS EXISTENTES (CLIENTES, LOTES Y PROVEEDORES)
// =========================================================================
=======
// =========================================================
// 🧍 Clientes
// =========================================================
>>>>>>> main
builder.Services.AddScoped<IRepository<ClientEntity>, ClientRepository>();
builder.Services.AddScoped<IValidator<ClientEntity>, ClientValidator>();
builder.Services.AddScoped<IClientService, ClientService>();

// =========================================================
// 📦 Lotes
// =========================================================
builder.Services.AddScoped<IRepository<LotEntity>, LotRepository>();
builder.Services.AddScoped<IValidator<LotEntity>, LotValidator>();  // ✅ nuevo validador
builder.Services.AddScoped<LotService>();

<<<<<<< HEAD
// >>> NUEVO: Provider
builder.Services.AddScoped<IRepository<ProviderEntity>, ProviderRepository>();
builder.Services.AddScoped<IProviderService, ProviderService>();

// =========================================================================
// REGISTROS DE SERVICIOS DE USUARIO Y AUTENTICACIÓN (RAMA MAIN)
// =========================================================================
=======
// =========================================================
// 👤 Usuarios
// =========================================================
>>>>>>> main
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IValidator<User>, UserValidator>();

// =========================================================
// 🌎 Configuración de cultura
// =========================================================
var supportedCultures = new[] { new CultureInfo("es-BO"), new CultureInfo("es") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-BO");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// =========================================================
// ✉️ Servicio de correo
// =========================================================
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailSender, EmailSender>();

// =========================================================
// 🔐 Autenticación y autorización
// =========================================================
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

// =========================================================
// 📊 Reportes
// =========================================================
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<IClientFidelityReportBuilder, PdfClientFidelityReportBuilder>();
builder.Services.AddScoped<IClientFidelityReportBuilder, ExcelClientFidelityReportBuilder>();
builder.Services.AddScoped<IClientFidelityReportService, ClientFidelityReportService>();

// =========================================================
// 🚀 Construcción de la app
// =========================================================
var app = builder.Build();

// =========================================================
// ⚙️ Middleware pipeline
// =========================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 🌍 Localización
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// 🔒 Middleware: forzar cambio de contraseña
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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
