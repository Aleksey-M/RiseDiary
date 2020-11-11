using Microsoft.AspNetCore.Http;
using System;

namespace RiseDiary.Model.Services
{
    public class HostAndPortService : IHostAndPortService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HostAndPortService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetHostAndPort() => (_httpContextAccessor.HttpContext?.Request?.Scheme ?? throw new Exception("Request is null")) + @"://"
            + (_httpContextAccessor.HttpContext?.Request?.Host.Host ?? throw new Exception("Host is null")) + ":"
            + (_httpContextAccessor.HttpContext?.Request?.Host.Port ?? throw new Exception("Host is null"));

        public string GetHostAndPortPlaceholder() => "[HOST_AND_PORT]";
    }
}
