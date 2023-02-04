namespace RiseDiary.Shared;

public static class InternalLinksHelper
{
    public const string HostAndPortPlaceholder = "[HOST_AND_PORT]";

    /// <summary>
    /// Замена базового url во внуренних ссылках на плейсхолдер
    /// </summary>
    /// <param name="text">Текст, содержащий ссылки</param>
    /// <param name="currentHostAndPort">Базовый URL приложения</param>
    public static string SetHostAndPortPlaceholder(string? text, string currentHostAndPort) =>
        text?.Replace(currentHostAndPort, HostAndPortPlaceholder, StringComparison.OrdinalIgnoreCase) ?? "";

    /// <summary>
    /// Формирование внутренних ссылок с теккущим базовым URL приложения
    /// </summary>
    /// <param name="text">текст, содержащий ссылки</param>
    /// <param name="currentHostAndPort">Базовый URL приложения</param>
    public static string SetBaseUri(string? text, string currentHostAndPort) =>
        text?.Replace(HostAndPortPlaceholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase) ?? "";
}
