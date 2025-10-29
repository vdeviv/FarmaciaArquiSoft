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
using ServiceLot.Infrastructure;
using ServiceUser.Application.Services;
using ServiceUser.Domain;
using ServiceUser.Domain.Validators;
using ServiceUser.Infraestructure.Persistence;
using System.Globalization;
using ClientEntity = ServiceClient.Domain.Client;

// USINGS AÑADIDOS DE LA RAMA ReportesS
using ServiceReports.Application;
using ServiceReports.Infrastructure;
using ServiceReports.Infrastructure.Repositories;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Application.Services;
using ServiceReports.Infrastructure.Reports;

using LotEntity = ServiceLot.Domain.Lot;


var builder = WebApplication.CreateBuilder(args);

DatabaseConnection.Initialize(builder.Configuration);

builder.Services.AddRazorPages()
                .AddViewOptions(o =>
                {
                    // Desactivar validación del lado del cliente (HTML5/jQuery)
                    o.HtmlHelperOptions.ClientValidationEnabled = false;
                })
                .AddMvcOptions(options =>
                {
                    // Desactivar DataAnnotations: solo validaciones personalizadas
                    options.ModelMetadataDetailsProviders.Clear();
                    options.ModelValidatorProviders.Clear();
                });

builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

// =========================================================================
// REGISTROS DE SERVICIOS EXISTENTES (CLIENTES Y LOTES)
// =========================================================================
builder.Services.AddScoped<IRepository<ClientEntity>, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddScoped<IRepository<LotEntity>, LotRepository>();
builder.Services.AddScoped<LotService>();

// =========================================================================
// REGISTROS DE SERVICIOS DE USUARIO Y AUTENTICACIÓN (RAMA MAIN)
// =========================================================================
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IValidator<User>, UserValidator>();

var supportedCultures = new[] { new CultureInfo("es-BO"), new CultureInfo("es") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-BO");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailSender, EmailSender>();

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
// REGISTROS DE SERVICIOS DE REPORTES (RAMA ReportesS)
// =========================================================================
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<IClientFidelityReportBuilder, PdfClientFidelityReportBuilder>(); // Esta línea estaba causando conflicto
builder.Services.AddScoped<IClientFidelityReportService, ClientFidelityReportService>();


// =========================
// (Opcional) OTROS SERVICIOS futuros
// =========================
// Ejemplo:
// builder.Services.AddScoped<IRepository<ProviderEntity>, ProviderRepository>();
// builder.Services.AddScoped<ProviderService>();

// =========================
// ConstrucciÃ³n de la app
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

var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
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