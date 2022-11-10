namespace RiseDiary.WebAPI.Config;

internal static class WebApplicationBuilderExtensions
{
    internal static void SetLocalhostPort(this WebApplicationBuilder builder, string[] args)
    {
        var port = GetCustomPort(args);
        if (port != null)
        {
            builder.WebHost.UseUrls("http://localhost:" + port + "/");
        }
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
}
