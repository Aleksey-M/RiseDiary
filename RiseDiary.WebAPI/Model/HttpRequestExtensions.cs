namespace RiseDiary.WebUI.Model;

public static class HttpRequestExtensions
{
    public static string GetAppBaseUrl(this HttpRequest reques)
    {
        var port = reques.Host.Port?.ToString();
        var host = reques.Scheme + @"://" + reques.Host.Host;

        if (port is not { Length: 0 }) host += ":" + port;

        if (host.EndsWith(":")) host = host.Remove(host.Length - 1);

        return host;
    }

}
