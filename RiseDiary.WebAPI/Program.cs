var builder = WebApplication.CreateBuilder(args);

var port = GetCustomPort(args);
if (port != null)
{
    builder.WebHost.UseUrls("http://localhost:" + port + "/");
}


// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();



static string? GetCustomPort(string[] args)
{
    try
    {
        if (args.Length == 1 && args[0].StartsWith("port="))
        {
            var port = args[0].Split("=")[1].Trim();
            if (int.TryParse(port, out int p))
            {
                return p.ToString();
            }
        }

        return null;
    }
    catch
    {
        return null;
    }
}