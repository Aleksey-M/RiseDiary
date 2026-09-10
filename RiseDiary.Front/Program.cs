using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RiseDiary.Common;
using RiseDiary.Common.Images;
using RiseDiary.Common.Records;
using RiseDiary.Common.Scopes;
using RiseDiary.Common.Scopes.Client;
using RiseDiary.Common.Settings;
using RiseDiary.Common.Settings.Client;
using RiseDiary.Front;
using RiseDiary.Front.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.RegisterJsonOptions();
builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddSingleton<JsDependencies>();

builder.Services.RegisterCommonSettingsValidators();
builder.Services.AddSingleton<ISettingsClient, SettingsClient>();

builder.Services.RegisterScopesAndThemesValidators();
builder.Services.AddSingleton<IScopesClient, ScopesClient>();


builder.Services.AddSingleton<UploadImageDtoValidator>();
builder.Services.AddSingleton<UpdateImageNameDtoValidator>();
builder.Services.AddSingleton<ScaleDownImageDtoValidator>();
builder.Services.AddSingleton<CropImageDtoValidator>();
builder.Services.AddSingleton<CreateRecordValidator>();
builder.Services.AddSingleton<UpdateRecordValidator>();
builder.Services.AddSingleton<CreateCogitationValidator>();
builder.Services.AddSingleton<UpdateCogitationValidator>();

await builder.Build().RunAsync();
