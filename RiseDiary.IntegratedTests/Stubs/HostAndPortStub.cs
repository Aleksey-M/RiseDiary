namespace RiseDiary.IntegratedTests.Stubs
{
    internal class HostAndPortStub
    {
        public string GetHostAndPort() => "https://testsite.com:3000";

        public string GetHostAndPortPlaceholder() => "[HOST_AND_PORT]";
    }
}
