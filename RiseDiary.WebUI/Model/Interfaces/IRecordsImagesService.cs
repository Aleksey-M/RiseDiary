using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IRecordsImagesService
    {
        Task AddRecordImage(Guid recordId, Guid imageId, int? order = null);

        Task RemoveRecordImage(Guid recordId, Guid imageId);

        Task<List<Guid>> GetLinkedImagesIdList(Guid recordId);

        Task<Dictionary<Guid, string>> GetLinkedRecordsInfo(Guid imageId);

        Task<List<DiaryRecordImage>> ChangeRecordImageOrder(Guid recordId, Guid imageId, int newOrder);
    }
}
