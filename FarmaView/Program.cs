using ServiceClient.Application;
using ServiceClient.Infrastructure;
using ServiceCommon.Application;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Data;
using ServiceUser.Application.Services;
using ServiceUser.Domain;
using ServiceLot.Application;
using ServiceLot.Infrastructure;
using LotEntity = ServiceLot.Domain.Lot;
using ClientEntity = ServiceClient.Domain.Client;
using ServiceReports.Application;          
using ServiceReports.Infrastructure;      
using ServiceReports.Infrastructure.Repositories; 
using ServiceReports.Application.DTOs;

var builder = WebApplication.CreateBuilder(args);

DatabaseConnection.Initialize(builder.Configuration);

builder.Services.AddRazorPages();

builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

builder.Services.AddScoped<IRepository<ClientEntity>, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddScoped<IRepository<LotEntity>, LotRepository>();
builder.Services.AddScoped<LotService>();

builder.Services.AddScoped<ReportRepository>();  // <- Usa tu capa infra para leer DB


// =========================
// (Opcional) OTROS SERVICIOS futuros
// =========================
// Ejemplo:
// builder.Services.AddScoped<IRepository<ProviderEntity>, ProviderRepository>();
// builder.Services.AddScoped<ProviderService>();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
