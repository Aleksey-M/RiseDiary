namespace RiseDiary.Shared.Images;

public static class ImagesSorter
{
    public static void ShiftOrders<T>(List<T> diaryRecordImages, Guid insertedImageId, int newOrder, int? oldOrder = null) where T : IImageWithOrder
    {
        var recordImage = diaryRecordImages.Single(x => x.ImageId == insertedImageId);

        if (oldOrder.HasValue)
        {
            if (oldOrder.Value < newOrder)
            {
                foreach (var image in diaryRecordImages)
                {
                    if (image.Order > oldOrder && image.Order <= newOrder)
                    {
                        image.Order--;
                    }
                }
            }
            else if (oldOrder.Value > newOrder)
            {
                foreach (var image in diaryRecordImages)
                {
                    if (image.Order < oldOrder && image.Order >= newOrder)
                    {
                        image.Order++;
                    }
                }
            }
        }
        else
        {
            foreach (var image in diaryRecordImages)
            {
                if (image.Order >= newOrder)
                {
                    image.Order++;
                }
            }
        }

        recordImage.Order = newOrder;

        foreach (var (image, index) in diaryRecordImages.OrderBy(x => x.Order).Select((x, i) => (x, i)))
        {
            image.Order = index + 1;
        }
    }

    public static void UpdateOrdersSequence<T>(List<T> diaryRecordImages, int removedOrderValue) where T : IImageWithOrder
    {
        var nextRecordImages = diaryRecordImages.Where(x => x.Order > removedOrderValue).ToList();

        if (nextRecordImages.Count > 0)
        {
            nextRecordImages.ForEach(x => x.Order--);
        }
    }
}
