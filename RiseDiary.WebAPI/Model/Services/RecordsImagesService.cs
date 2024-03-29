﻿using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.Shared.Images;

namespace RiseDiary.Model.Services;

internal sealed class RecordsImagesService : IRecordsImagesService
{
    private readonly DiaryDbContext _context;

    public RecordsImagesService(DiaryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DiaryRecordImage> AddRecordImage(Guid recordId, Guid imageId, int? order = null)
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


        var recordImage = await _context.RecordImages
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(ri => ri.RecordId == recordId && ri.ImageId == imageId)
            .ConfigureAwait(false);

        if (recordImage != null && !recordImage.Deleted)
        {
            return recordImage;
        }

        recordImage ??= new DiaryRecordImage
        {
            ImageId = imageId,
            RecordId = recordId,
            Order = 0
        };

        recordImage.Order = await _context.RecordImages
            .Where(x => x.RecordId == recordId)
            .Select(x => x.Order)
            .DefaultIfEmpty()
            .MaxAsync()
            .ConfigureAwait(false) + 1;

        if (recordImage.Deleted)
        {
            recordImage.Deleted = false;
        }
        else
        {
            await _context.RecordImages.AddAsync(recordImage).ConfigureAwait(false);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);


        if (order.HasValue && order.Value < recordImage.Order)
        {
            var imageWithOrderExists = await _context.RecordImages
                .AnyAsync(x => x.RecordId == recordId && x.Order == order.Value)
                .ConfigureAwait(false);

            var list = await _context.RecordImages
                .Where(x => x.RecordId == recordId)
                .ToListAsync()
                .ConfigureAwait(false);

            if (imageWithOrderExists)
            {
                ImagesSorter.ShiftOrders(list, imageId, order.Value);
            }

            if (order.Value <= 0)
            {
                ImagesSorter.ShiftOrders(list, imageId, 1);
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        return recordImage;
    }

    public async Task<List<DiaryRecordImage>> ChangeRecordImageOrder(Guid recordId, Guid imageId, int newOrder)
    {
        var list = await _context.RecordImages
               .Include(x => x.Image)
               .Where(x => x.RecordId == recordId)
               .ToListAsync()
               .ConfigureAwait(false);

        int oldOrder = list.Single(x => x.RecordId == recordId && x.ImageId == imageId).Order;

        ImagesSorter.ShiftOrders(list, imageId, newOrder, oldOrder);

        await _context.SaveChangesAsync();

        return list.OrderBy(x => x.Order).ToList();
    }

    public async Task<List<DiaryRecordImage>> GetLinkedImagesList(Guid recordId, CancellationToken cancellationToken) =>
        await _context.RecordImages
            .AsNoTracking()
            .Include(ri => ri.Image)
            .Where(ri => ri.RecordId == recordId)
            .OrderBy(ri => ri.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Dictionary<Guid, string>> GetLinkedRecordsInfo(Guid imageId, CancellationToken cancellationToken) =>
        await _context.RecordImages
            .AsNoTracking()
            .Include(ri => ri.Record)
            .Where(ri => ri.ImageId == imageId)
            .ToDictionaryAsync(ri => ri.RecordId, ri => ri.Record?.Name
                ?? throw new Exception($"Record with id = {ri.RecordId} is not exists in db"), cancellationToken);

    public async Task RemoveRecordImage(Guid recordId, Guid imageId)
    {
        var recordImage = await _context.RecordImages
            .SingleOrDefaultAsync(ri => ri.RecordId == recordId && ri.ImageId == imageId)
            .ConfigureAwait(false);

        if (recordImage != null)
        {
            _context.RecordImages.Remove(recordImage);

            var nextRecordImages = await _context.RecordImages
                .Where(x => x.RecordId == recordId && x.Order >= recordImage.Order)
                .ToListAsync()
                .ConfigureAwait(false);

            ImagesSorter.UpdateOrdersSequence(nextRecordImages, recordImage.Order);

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
