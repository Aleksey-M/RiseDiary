using RiseDiary.Domain.Repositories;
using System;

namespace RiseDiary.Data.SqliteStorages
{
    public class RepositoriesFactory : IRepositoriesFactory
    {
        private readonly DataBaseManager manager;
        private AreasRepository _areasRepository;
        private RecordTypesRepository _recordTypesRepository;
        private RecordsRepository _recordsRepository;
        private CogitationRepository _cogitationRepository;
        private DiaryImagesRepository _diaryImagesRepository;

        public RepositoriesFactory(string pathToDbFile, string dbFileName)
        {
            if (string.IsNullOrWhiteSpace(pathToDbFile)) throw new ArgumentException(nameof(pathToDbFile));
            if (string.IsNullOrWhiteSpace(dbFileName)) throw new ArgumentException(nameof(dbFileName));
            manager = new DataBaseManager(pathToDbFile, dbFileName);
        }

        public IAreasRepository AreasRepository => _areasRepository ?? (_areasRepository = new AreasRepository(manager));

        public IRecordTypesRepository RecordTypesRepository => _recordTypesRepository ?? (_recordTypesRepository = new RecordTypesRepository(manager));

        public IRecordsRepository RecordsRepository => _recordsRepository ?? (_recordsRepository = new RecordsRepository(manager));

        public ICogitationRepository CogitationRepository => _cogitationRepository ?? (_cogitationRepository = new CogitationRepository(manager));

        public IDiaryImagesRepository DiaryImagesRepository => _diaryImagesRepository ?? (_diaryImagesRepository = new DiaryImagesRepository(manager));
    }
}
