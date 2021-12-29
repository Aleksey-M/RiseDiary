using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IRecordsImagesService
    {
        Task AddRecordImage(Guid recordId, Guid imageId);

        Task RemoveRecordImage(Guid recordId, Guid imageId);

        Task<List<Guid>> GetLinkedImagesIdList(Guid recordId);

        Task<Dictionary<Guid, string>> GetLinkedRecordsInfo(Guid imageId);
    }
}
