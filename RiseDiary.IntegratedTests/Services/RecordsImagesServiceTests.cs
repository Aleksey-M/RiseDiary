using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Services
{
    internal class RecordsImagesServiceTests : TestedServices
    {
        private async Task<(Guid recId, Guid imgId, IRecordsImagesService recImgSvc, IImagesService imgSvc, DiaryDbContext context)> CreateRecordWithLinkedImage()
        {
            var context = CreateContext();
            var recImgSvc = GetRecordsImagesService(context);
            var imageSvc = GetImagesService(context);
            var imageId = await imageSvc.AddImage(TestFile);
            var recordId = Create_Record(context);
            await recImgSvc.AddRecordImage(recordId, imageId);

            return (recordId, imageId, recImgSvc, imageSvc, context);
        }

        [Test]
        public async Task BindRecord_RecordImage_DeleteImage_ShouldMarkAsDeleted()
        {
            var (recId, imgId, recImgSvc, imgSvc, context) = await CreateRecordWithLinkedImage();

            var bindEntity = await context.RecordImages.SingleOrDefaultAsync(br => br.RecordId == recId && br.ImageId == imgId);
            bindEntity.Should().NotBeNull();

            await imgSvc.DeleteImage(imgId);

            bindEntity = await context.RecordImages.SingleOrDefaultAsync(br => br.RecordId == recId && br.ImageId == imgId);
            bindEntity.Should().BeNull();
        }

        [Test]
        public async Task BindRecord_RecordImage_DeleteRecord_ShouldMarkAsDeleted()
        {
            var (recId, imgId, recImgSvc, imgSvc, context) = await CreateRecordWithLinkedImage();
            var recSvc = GetRecordsService(context);

            var bindEntity = await context.RecordImages.SingleOrDefaultAsync(br => br.RecordId == recId && br.ImageId == imgId);
            bindEntity.Should().NotBeNull();

            await recSvc.DeleteRecord(recId);

            bindEntity = await context.RecordImages.SingleOrDefaultAsync(br => br.RecordId == recId && br.ImageId == imgId);
            bindEntity.Should().BeNull();
        }

        [Test]
        public async Task AddImageForRecord_AfterDeletingTheSame_ShouldUnmarkAsDeleted()
        {
            var (recId, imgId, recImgSvc, imgSvc, context) = await CreateRecordWithLinkedImage();
            context.SoftDeleting = true;

            await recImgSvc.RemoveRecordImage(recId, imgId);
            await recImgSvc.AddRecordImage(recId, imgId);

            int bindedRecordCount = await context.RecordImages.CountAsync(br => br.RecordId == recId && br.ImageId == imgId);
            bindedRecordCount.Should().Be(1);
        }

        [Test]
        public async Task GetLinkedImagesIdList_AfterUnlinking_ShouldReturnEmptyList()
        {
            var (recId, imgId, recImgSvc, _, _) = await CreateRecordWithLinkedImage();
            await recImgSvc.RemoveRecordImage(recId, imgId);

            var imagesIdList = await recImgSvc.GetLinkedImagesIdList(recId);

            imagesIdList.Should().NotBeNull();
            imagesIdList.Should().BeEmpty();
        }

        [Test]
        public async Task GetLinkedImagesIdList_ShouldReturnEmptyList()
        {
            var (_, _, recImgSvc, _, context) = await CreateRecordWithLinkedImage(); // not empty db
            var recordId = Create_Record(context);

            var imagesIdList = await recImgSvc.GetLinkedImagesIdList(recordId);

            imagesIdList.Should().NotBeNull();
            imagesIdList.Should().BeEmpty();
        }

        [Test]
        public async Task GetLinkedImagesIdList_ShouldReturn1ImageId()
        {
            var (recId, imgId, recImgSvc, _, _) = await CreateRecordWithLinkedImage();

            var imagesIdList = await recImgSvc.GetLinkedImagesIdList(recId);

            imagesIdList.Should().NotBeNull();
            imagesIdList.Should().HaveCount(1);
            imagesIdList.Should().Contain(imgId);
        }

        [Test]
        public async Task GetLinkedRecordsInfo_ShouldReturnEmptyDictionary()
        {
            var (_, _, recImgSvc, _, context) = await CreateRecordWithLinkedImage(); // not empty db
            var imageId = Create_Image(context);

            var recordsDict = await recImgSvc.GetLinkedRecordsInfo(imageId);

            recordsDict.Should().NotBeNull();
            recordsDict.Should().BeEmpty();
        }

        [Test]
        public async Task GetLinkedRecordsInfo_ShouldReturn1RecordIdAndName()
        {
            var (recId, imgId, recImgSvc, _, context) = await CreateRecordWithLinkedImage();
            var recName = context.Records.SingleOrDefault(r => r.Id == recId)?.Name;

            var recordsDict = await recImgSvc.GetLinkedRecordsInfo(imgId);

            recordsDict.Should().NotBeNull();
            recordsDict.Should().HaveCount(1);
            recordsDict.Keys.Should().Contain(recId);
            recordsDict[recId].Should().Be(recName);
        }

        private async Task<(Guid recordId, List<Guid> imagesIds, DiaryDbContext context)> CreateRecordAnd3Images()
        {
            var context = CreateContext();
            var imageSvc = GetImagesService(context);

            var imagesIds = new List<Guid>
            {
                await imageSvc.AddImage(TestFile),
                await imageSvc.AddImage(TestFile),
                await imageSvc.AddImage(TestFile),
                await imageSvc.AddImage(TestFile)
            };

            var recordId = Create_Record(context);

            return (recordId, imagesIds, context);
        }

        [Test]
        public async Task AddRecordImage_WithoutOrder_ShouldSaveMaxOrder()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);

            await recordsImagesService.AddRecordImage(recordId, imagesIds[0]);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1]);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2]);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(2);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(3);
        }

        [Test]
        public async Task AddRecordImage_WithBiggestOrder_ShouldAddToEnd()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0]);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1]);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2]);

            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 78);

            int order = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order.Should().Be(4);
        }

        [Test]
        public async Task AddRecordImage_WithNegativeOrder_ShouldAddToStart()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0]);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1]);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2]);

            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], -1);

            int order = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order.Should().Be(1);
            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(2);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(3);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(4);
        }

        [Test]
        public async Task AddRecordImage_WithoutExistedOrder_ShouldSaveOrder()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);

            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 10);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 15);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 50);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(2);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(3);
        }

        [Test]
        public async Task AddRecordImage_WithSoftDeletion_ShouldSaveMaxOrder()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            context.SoftDeleting = true;

            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 5);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 7);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 11);
            await recordsImagesService.RemoveRecordImage(recordId, imagesIds[1]);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3]);

            await recordsImagesService.AddRecordImage(recordId, imagesIds[1]);


            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(5);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(3);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(4);
        }

        [Test]
        public async Task AddRecordImage_WithSoftDeletionAndExistedOrder_ShouldSaveOrderAndShiftExisted()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            context.SoftDeleting = true;

            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 5);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 7);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 11);
            await recordsImagesService.RemoveRecordImage(recordId, imagesIds[1]);


            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 1);


            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(2);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(1);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(3);
        }

        [Test]
        public async Task AddRecordImage_WithExistedOrder_ShouldSaveOrderAndShiftExisted()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();

            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 6);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 9);


            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 2);


            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(3);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(4);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(2);
        }

        [Test]
        public async Task ChangeRecordImageOrder_ToRight1_ShouldUpdateOrders()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 1);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 2);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 4);

            var result = await recordsImagesService.ChangeRecordImageOrder(recordId, imagesIds[2], 4);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(2);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(4);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(3);

            result.Select(x => (imageId: x.ImageId, order: x.Order)).Should().Equal(new List<(Guid, int)>
            {
                (imagesIds[0], 1),
                (imagesIds[1], 2),
                (imagesIds[3], 3),
                (imagesIds[2], 4)
            });
        }

        [Test]
        public async Task ChangeRecordImageOrder_ToRight2_ShouldUpdateOrders()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 1);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 2);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 4);

            var result = await recordsImagesService.ChangeRecordImageOrder(recordId, imagesIds[0], 3);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(3);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(1);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(2);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(4);

            result.Select(x => (imageId: x.ImageId, order: x.Order)).Should().Equal(new List<(Guid, int)>
            {
                (imagesIds[1], 1),
                (imagesIds[2], 2),
                (imagesIds[0], 3),
                (imagesIds[3], 4)
            });
        }

        public async Task ChangeRecordImageOrder_ToLeft1_ShouldUpdateOrders()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 1);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 2);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 4);

            var result = await recordsImagesService.ChangeRecordImageOrder(recordId, imagesIds[2], 2);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(3);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(2);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(4);

            result.Select(x => (imageId: x.ImageId, order: x.Order)).Should().Equal(new List<(Guid, int)>
            {
                (imagesIds[0], 1),
                (imagesIds[2], 2),
                (imagesIds[1], 3),
                (imagesIds[3], 4)
            });
        }

        [Test]
        public async Task ChangeRecordImageOrder_ToLeft2_ShouldUpdateOrders()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 1);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 2);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 4);

            var result = await recordsImagesService.ChangeRecordImageOrder(recordId, imagesIds[3], 2);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(3);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(4);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(2);

            result.Select(x => (imageId: x.ImageId, order: x.Order)).Should().Equal(new List<(Guid, int)>
            {
                (imagesIds[0], 1),
                (imagesIds[3], 2),
                (imagesIds[1], 3),
                (imagesIds[2], 4)
            });
        }

        [Test]
        public async Task ChangeRecordImageOrder_WithSameOrder_ShouldNotUpdateOrders()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 1);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 2);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 4);

            var result = await recordsImagesService.ChangeRecordImageOrder(recordId, imagesIds[2], 3);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(2);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(3);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(4);

            result.Select(x => (imageId: x.ImageId, order: x.Order)).Should().Equal(new List<(Guid, int)>
            {
                (imagesIds[0], 1),
                (imagesIds[1], 2),
                (imagesIds[2], 3),
                (imagesIds[3], 4)
            });
        }

        [Test]
        public async Task ChangeRecordImageOrder_WithNegativeOrder_ShouldShiftOrders()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 1);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 2);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 4);

            var result = await recordsImagesService.ChangeRecordImageOrder(recordId, imagesIds[2], -3);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(2);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(3);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(1);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(4);

            result.Select(x => (imageId: x.ImageId, order: x.Order)).Should().Equal(new List<(Guid, int)>
            {
                (imagesIds[2], 1),
                (imagesIds[0], 2),
                (imagesIds[1], 3),
                (imagesIds[3], 4)
            });
        }

        [Test]
        public async Task ChangeRecordImageOrder_WithBigPositiveOrder_ShouldShiftOrders()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 1);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 2);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 4);

            var result = await recordsImagesService.ChangeRecordImageOrder(recordId, imagesIds[1], 21);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order2 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[1])).Order;
            order2.Should().Be(4);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(2);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(3);

            result.Select(x => (imageId: x.ImageId, order: x.Order)).Should().Equal(new List<(Guid, int)>
            {
                (imagesIds[0], 1),
                (imagesIds[2], 2),
                (imagesIds[3], 3),
                (imagesIds[1], 4)
            });
        }

        [Test]
        public async Task RemoveRecordImage_NextImagesOrderShouldBeDecreased()
        {
            var (recordId, imagesIds, context) = await CreateRecordAnd3Images();
            var recordsImagesService = GetRecordsImagesService(context);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[0], 1);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[1], 2);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[2], 3);
            await recordsImagesService.AddRecordImage(recordId, imagesIds[3], 4);

            await recordsImagesService.RemoveRecordImage(recordId, imagesIds[1]);

            int order1 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[0])).Order;
            order1.Should().Be(1);
            int order3 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[2])).Order;
            order3.Should().Be(2);
            int order4 = (await context.RecordImages.SingleAsync(x => x.ImageId == imagesIds[3])).Order;
            order4.Should().Be(3);
        }
    }
}
