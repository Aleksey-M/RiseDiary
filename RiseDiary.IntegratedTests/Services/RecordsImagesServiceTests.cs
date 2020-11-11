using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
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

            await recImgSvc.AddRecordImage(recId, imgId);
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

    }
}
