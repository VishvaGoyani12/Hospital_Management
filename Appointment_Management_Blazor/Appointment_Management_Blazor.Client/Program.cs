using Appointment_Management_Blazor.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


// Base HTTP client
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Register services
builder.Services.AddScoped<IAccountClientService, AccountClientService>();

// Add authorization core for Blazor WebAssembly
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();