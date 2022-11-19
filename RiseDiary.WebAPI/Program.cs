using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.Model;
using RiseDiary.Model.Services;
using RiseDiary.Shared;
using RiseDiary.Shared.Scopes;
using RiseDiary.Shared.Settings;
using RiseDiary.WebAPI.Config;
using RiseDiary.WebAPI.Data;
using RiseDiary.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.SetLocalhostPort(args);

// set db context
builder.Services.Configure<SqliteOptions>(builder.Configuration.GetSection(SqliteOptions.SectionName));

var dbFileName = builder.Configuration
    .GetSection(SqliteOptions.SectionName)
    .GetValue<string>(nameof(SqliteOptions.FileName)) ?? throw new Exception("Database file name is not set");

builder.Services.AddDbContext<DiaryDbContext>(options => options.UseSqlite(
    $"Data Source={dbFileName};", o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

await DataSeed.CheckData(dbFileName);

builder.Services.AddScoped<IScopesService, ScopesService>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.AddScoped<IRecordsThemesService, RecordsThemesService>();
builder.Services.AddScoped<IImagesService, ImagesService>();
builder.Services.AddScoped<IRecordsImagesService, RecordsImagesService>();
builder.Services.AddScoped<IImagesEditService, ImagesEditService>();
builder.Services.AddScoped<ICropImageService, CropImageService>();
builder.Services.AddScoped<IRecordsService, RecordsService>();
builder.Services.AddScoped<ICogitationsService, CogitationsService>();
builder.Services.AddScoped<IRecordsSearchService, RecordsSearchService>();
builder.Services.AddScoped<IRecordsSearchTextService, RecordsSearchTextService>();
builder.Services.AddScoped<IDatesService, DatesService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<ISqliteDatabase, SqliteDatabase>();


builder.Services.AddScoped<IDtoValidator<ScopeDto>, ScopeValidator>();
builder.Services.AddScoped<ThemeValidator>();
builder.Services.AddScoped<ImportantDaysSettingsValidator>();
builder.Services.AddScoped<ImagesSettingsValidator>();
builder.Services.AddScoped<PagesSizesSettingsValidator>();


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

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();