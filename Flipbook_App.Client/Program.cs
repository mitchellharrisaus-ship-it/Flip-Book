using Flipbook_App.Client;
using Flipbook_App.Client.Services;
using FlipBook_Library.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<IDrawShapeService, DrawShapeService>();
builder.Services.AddScoped<SkiaDrawingService>();
builder.Services.AddScoped<AnimationApiService>();

await builder.Build().RunAsync();
