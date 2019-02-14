using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhotosDataBase.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhotosDataBase.Hubs
{
    public class PhotosImportHub : Hub
    {
        private readonly PhotosDbContext _context;
        private readonly IConfiguration _configuration;
        private static string _diarySQLConnection;
        
        public PhotosImportHub(PhotosDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;            
        }

        private DiaryDbContext CreateDiaryContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DiaryDbContext>();
            if (_diarySQLConnection == null)
            {
                _diarySQLConnection = _configuration.GetConnectionString("DiaryConnectionString");
            }
            optionsBuilder.UseNpgsql(_diarySQLConnection);
            return new DiaryDbContext(optionsBuilder.Options);
        }

        private static CancellationTokenSource _cts = null;
        private static IDisposable _timer = null;
        private static int _recordsToProcess = 0;
        private static int _processedRecords = 0;


    }
}
