using Microsoft.EntityFrameworkCore;
using RiseDiary.Common.Images;
using RiseDiary.Common.Records;
using RiseDiary.Common.Scopes;
using RiseDiary.Common.Settings;
using RiseDiary.Data;
using RiseDiary.Model;
using RiseDiary.Model.Services;
using RiseDiary.WebAPI.Config;
using RiseDiary.WebAPI.Middleware;
using RiseDiary.WebAPI.Services;
using RiseDiary.WebAPI.Settings;
using Serilog;
using RiseDiary.Common;
using RiseDiary.WebAPI.Scopes.Services;

var builder = WebApplication.CreateBuilder(args);

builder.SetLocalhostPort(args);

// set db context
builder.Services.Configure<SqliteOptions>(builder.Configuration.GetSection(SqliteOptions.SectionName));

var dbFileName = builder.Configuration
    .GetSection(SqliteOptions.SectionName)
    .GetValue<string>(nameof(SqliteOptions.FileName)) ?? throw new Exception("Database file name is not set");

builder.Services.AddDbContext<DiaryDbContext>(options => options.UseSqlite(
    $"Data Source={dbFileName};", o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
Console.WriteLine($"Database file: {dbFileName}");


// logging
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.AddSerilog(logger);

builder.Services.RegisterJsonOptions();

builder.Services.AddMemoryCache();

// app services
builder.Services.RegisterCommonSettingsValidators();
builder.Services.RegisterSettingsServices();

builder.Services.RegisterScopesAndThemesValidators();
builder.Services.AddScoped<IScopesService, ScopesService>();

builder.Services.AddScoped<IRecordsThemesService, RecordsThemesService>();
builder.Services.AddScoped<IImagesService, ImagesService>();
builder.Services.AddScoped<IRecordsImagesService, RecordsImagesService>();
builder.Services.AddScoped<IImagesEditService, ImagesEditService>();
builder.Services.AddScoped<IRecordsService, RecordsService>();
builder.Services.AddScoped<ICogitationsService, CogitationsService>();
builder.Services.AddScoped<IRecordsSearchService, RecordsSearchService>();
builder.Services.AddScoped<IRecordsSearchTextService, RecordsSearchTextService>();
builder.Services.AddScoped<IDatesService, DatesService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<ISqliteDatabase, SqliteDatabase>();

// validators
builder.Services.AddScoped<ScopeDtoValidator>();
builder.Services.AddScoped<ThemeDtoValidator>();
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

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();