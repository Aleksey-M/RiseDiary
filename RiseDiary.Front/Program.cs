using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RiseDiary.Front;
using RiseDiary.Front.AppServices;
using RiseDiary.Shared;
using RiseDiary.Shared.Images;
using RiseDiary.Shared.Records;
using RiseDiary.Shared.Scopes;
using RiseDiary.Shared.Settings;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<MarkdownService>();
builder.Services.AddScoped<JsDependencies>();

// validators
builder.Services.AddScoped<IDtoValidator<ScopeDto>, ScopeValidator>();
builder.Services.AddScoped<ThemeValidator>();
builder.Services.AddScoped<ImportantDaysSettingsValidator>();
builder.Services.AddScoped<ImagesSettingsValidator>();
builder.Services.AddScoped<PagesSizesSettingsValidator>();
builder.Services.AddScoped<UploadImageDtoValidator>();
builder.Services.AddScoped<UpdateImageNameDtoValidator>();
builder.Services.AddScoped<ScaleDownImageDtoValidator>();
builder.Services.AddScoped<CropImageDtoValidator>();
builder.Services.AddScoped<CreateRecordValidator>();
builder.Services.AddScoped<UpdateRecordValidator>();
builder.Services.AddScoped<CreateCogitationValidator>();
builder.Services.AddScoped<UpdateCogitationValidator>();

await builder.Build().RunAsync();
