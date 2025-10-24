
using ServiceClient.Application;
using ServiceClient.Infrastructure;
using ServiceCommon.Application;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Data;
using ServiceUser.Application.Services;
using ServiceUser.Domain;
using ClientEntity = ServiceClient.Domain.Client;

var builder = WebApplication.CreateBuilder(args);

DatabaseConnection.Initialize(builder.Configuration);

builder.Services.AddRazorPages();

builder.Services.AddSingleton<IEncryptionService, ServiceCommon.Application.EncryptionService>();




builder.Services.AddScoped<IRepository<ClientEntity>, ClientRepository>();


// 2. Registro del Servicio de Cliente (IClientService <-- ClientService)
builder.Services.AddScoped<IClientService, ClientService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();