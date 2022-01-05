using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace RiseDiary.WebUI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static string? GetCustomPort(string[] args)
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    
                    var port = GetCustomPort(args);
                    if (port != null)
                    {
                        webBuilder.UseUrls("http://localhost:" + port + "/");
                    }                    
                });
    }
}
