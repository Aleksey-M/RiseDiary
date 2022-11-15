using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RiseDiary.Front;
using RiseDiary.Front.AppServices;
using RiseDiary.Shared.Scopes;
using RiseDiary.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<MarkdownService>();

builder.Services.AddScoped<IDtoValidator<ScopeDto>, ScopeValidator>();
builder.Services.AddScoped<ThemeValidator>();


await builder.Build().RunAsync();
