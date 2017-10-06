using RiseDiary.Data.SqliteStorages;
using RiseDiary.Domain.Repositories;
using RiseDiary.UI.Pages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using Serilog.Core;

namespace RiseDiary.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string dataBaseFileName = "DiaryData.db";
        private readonly Logger _log;
        private readonly IRepositoriesFactory _repositoriesFactory;
        private AreasPage _areasPage;

        public AreasPage AreasPage => _areasPage ?? (_areasPage = new AreasPage(_repositoriesFactory));
        public App()
        {
            _log = new LoggerConfiguration().WriteTo.RollingFile("Diary-{Date}.log").CreateLogger();
            _log.Information("Start app");           
            try
            {
                _repositoriesFactory = new RepositoriesFactory(Environment.CurrentDirectory, dataBaseFileName);                
            }
            catch(Exception exc)
            {
                _log.Error(exc.Message);
                throw;
            }
        }
    }
}
