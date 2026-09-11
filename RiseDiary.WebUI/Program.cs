using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RiseDiary.Common;
using RiseDiary.Common.Images;
using RiseDiary.Common.Records;
using RiseDiary.WebUI.Services;
using RiseDiary.WebUI;
using RiseDiary.WebUI.Clients;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.RegisterJsonOptions();
builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddSingleton<JsDependencies>();

builder.Services.RegisterDtoValidators();
builder.Services.RegisterWebClients();

builder.Services.AddSingleton<UploadImageDtoValidator>();
builder.Services.AddSingleton<UpdateImageNameDtoValidator>();
builder.Services.AddSingleton<ScaleDownImageDtoValidator>();
builder.Services.AddSingleton<CropImageDtoValidator>();
builder.Services.AddSingleton<CreateRecordValidator>();
builder.Services.AddSingleton<UpdateRecordValidator>();
builder.Services.AddSingleton<CreateCogitationValidator>();
builder.Services.AddSingleton<UpdateCogitationValidator>();

await builder.Build().RunAsync();
