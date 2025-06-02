using Appointment_Management_Blazor.Client.Helper;
using Appointment_Management_Blazor.Client.Services.Implementations;
using Appointment_Management_Blazor.Client.Services.Interfaces;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IAccountClientService, AccountClientService>();
builder.Services.AddScoped<IDoctorClientService, DoctorClientService>();
builder.Services.AddScoped<IPatientClientService, PatientClientService>();
builder.Services.AddScoped<IPatientAppointmentClientService, PatientAppointmentClientService>();

builder.Services.AddScoped<AuthHeaderHandler>();

builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();