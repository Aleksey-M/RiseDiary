using RiseDiary.Model;

namespace RiseDiary.IntegratedTests.Stubs
{
    internal class HostAndPortStub : IHostAndPortService
    {
        public string GetHostAndPort() => "https://testsite.com:3000";

        public string GetHostAndPortPlaceholder() => "[HOST_AND_PORT]";
    }
}
