using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RiseDiary.UI.Pages
{
    /// <summary>
    /// Interaction logic for AreasPage.xaml
    /// </summary>
    public partial class AreasPage : Page
    {
        private readonly IRepositoriesFactory _factory;
        private ObservableCollection<DiaryRecordTypeJoined> _areasAndTypes;
        private ObservableCollection<DiaryArea> _areas;
        public AreasPage(IRepositoriesFactory factory)
        {
            InitializeComponent();
            DataContext = this;
            _factory = factory;
            _areasAndTypes = new ObservableCollection<DiaryRecordTypeJoined>(_factory.RecordTypesRepository.FetchRecordTypesWithAreas().Result);
            _areas = new ObservableCollection<DiaryArea>(new DiaryArea[] { new DiaryArea { AreaName = "111" } }/*_factory.AreasRepository.FetchAllAreas().Result*/);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {            
            string areaName = cbArea.Text?.Trim();
            if (string.IsNullOrWhiteSpace(areaName)) return;
            string recType = tbRecordType.Text?.Trim();
            if (string.IsNullOrWhiteSpace(recType)) return;

            int? areaId = _areasAndTypes.FirstOrDefault(a => a.AreaName == areaName)?.AreaId;
            if (areaId == null)
            {
                await _factory.AreasRepository.AddArea(areaName);
                _areasAndTypes = new ObservableCollection<DiaryRecordTypeJoined>(await _factory.RecordTypesRepository.FetchRecordTypesWithAreas());
                areaId = _areasAndTypes.FirstOrDefault(a => a.AreaName == areaName)?.AreaId;
            }

            await _factory.RecordTypesRepository.AddRecordType((int)areaId, recType);
        }
    }
}
