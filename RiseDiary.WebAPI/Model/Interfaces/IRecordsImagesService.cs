namespace RiseDiary.Model;

public interface IRecordsImagesService
{
    Task<RecordImageEntity> AddRecordImage(Guid recordId, Guid imageId, int? order = null);
    Task RemoveRecordImage(Guid recordId, Guid imageId);
    Task<List<RecordImageEntity>> GetLinkedImagesList(Guid recordId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, string>> GetLinkedRecordsInfo(Guid imageId, CancellationToken cancellationToken = default);
    Task<List<RecordImageEntity>> ChangeRecordImageOrder(Guid recordId, Guid imageId, int newOrder);
}
