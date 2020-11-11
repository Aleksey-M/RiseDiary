using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    public class RecordsImagesService : IRecordsImagesService
    {
        private readonly DiaryDbContext _context;
        public RecordsImagesService(DiaryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddRecordImage(Guid recordId, Guid imageId)
        {
            var ri = await _context.RecordImages
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(ri => ri.RecordId == recordId && ri.ImageId == imageId)
                .ConfigureAwait(false);

            if (ri != null)
            {
                ri.Deleted = false;
            }
            else
            {
                var recExists = await _context.Records.AsNoTracking().AnyAsync(r => r.Id == recordId).ConfigureAwait(false);
                var imgExists = await _context.Images.AsNoTracking().AnyAsync(i => i.Id == imageId).ConfigureAwait(false);
                var errorMessage = (recExists, imgExists) switch
                {
                    (true, true) => "",
                    (false, true) => $"Record with id '{recordId}' does not exists",
                    (true, false) => $"Image with id '{imageId}' does not exists",
                    (false, false) => $"Record with id '{recordId}' does not exists, and image with id '{imageId}' does not exists"
                };

                if (errorMessage != "") throw new ArgumentException(errorMessage);

                await _context.RecordImages.AddAsync(new DiaryRecordImage { ImageId = imageId, RecordId = recordId }).ConfigureAwait(false);
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<List<Guid>> GetLinkedImagesIdList(Guid recordId) => await _context.RecordImages
            .AsNoTracking()
            .Where(ri => ri.RecordId == recordId)
            .Select(ri => ri.ImageId)
            .ToListAsync()
            .ConfigureAwait(false);

        public async Task<Dictionary<Guid, string>> GetLinkedRecordsInfo(Guid imageId) => await _context.RecordImages
            .Include(ri => ri.Record)
            .Where(ri => ri.ImageId == imageId)
            .ToDictionaryAsync(ri => ri.RecordId, ri => ri.Record?.Name ?? throw new Exception($"Record with id = {ri.RecordId} is not exists in db"));

        public async Task RemoveRecordImage(Guid recordId, Guid imageId)
        {
            var recordImage = await _context.RecordImages
                .SingleOrDefaultAsync(ri => ri.RecordId == recordId && ri.ImageId == imageId)
                .ConfigureAwait(false);

            if (recordImage != null)
            {
                _context.RecordImages.Remove(recordImage);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
