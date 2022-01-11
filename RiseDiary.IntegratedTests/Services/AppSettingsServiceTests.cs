using FluentAssertions;
using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Services
{
    internal class AppSettingsServiceTests : TestedServices
    {
        [Test]
        public async Task UpdateSettings_WithCorrectKeyValue_ShouldNotThrowException()
        {
            var context = CreateContext();
            var cfgSvc = GetAppSettingsService(context);
            string value = "80";

            await cfgSvc.UpdateAppSetting(AppSettingsKey.ImageQuality, value);

            context.AppSettings.FirstOrDefault(s => s.Key == AppSettingsKey.ImageQuality.ToString() && s.Value == value)
                .Should().NotBeNull();
        }

        [Test]
        public async Task UpdateSettings_WithCorrectKeyValue_ShouldWriteModifiedDate()
        {
            var context = CreateContext();
            var cfgSvc = GetAppSettingsService(context);
            string value = "70";

            await cfgSvc.UpdateAppSetting(AppSettingsKey.ImageQuality, value);

            var mDate = context.AppSettings.FirstOrDefault(s => s.Key == AppSettingsKey.ImageQuality.ToString())?.ModifiedDate;

            mDate.Should().NotBeNull()
                .And.BeSameDateAs(DateTime.UtcNow.Date)
                .And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task UpdateSettings_ForExistingKey_ShouldUpdateModifiedDate()
        {
            var context = CreateContext();
            var cfgSvc = GetAppSettingsService(context);
            string value = "75";
            await context.AppSettings.AddAsync(new AppSetting
            {
                Key = AppSettingsKey.ImageQuality.ToString(),
                Value = value,
                ModifiedDate = DateTime.UtcNow.AddDays(-2)
            });
            await context.SaveChangesAsync();

            await cfgSvc.UpdateAppSetting(AppSettingsKey.ImageQuality, "66");

            var mDate = context.AppSettings.FirstOrDefault(s => s.Key == AppSettingsKey.ImageQuality.ToString())?.ModifiedDate;
            mDate.Should().NotBeNull()
                .And.BeSameDateAs(DateTime.UtcNow.Date)
                .And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task UpdateSettings_ForExistingKey_ShouldUpdateValue()
        {
            var context = CreateContext();
            var cfgSvc = GetAppSettingsService(context);
            string value = "55";
            string newValue = "30";

            await cfgSvc.UpdateAppSetting(AppSettingsKey.ImageQuality, value);
            await cfgSvc.UpdateAppSetting(AppSettingsKey.ImageQuality, newValue);

            var appSetting = context.AppSettings.FirstOrDefault(s => s.Key == AppSettingsKey.ImageQuality.ToString());
            appSetting.Should().NotBeNull();
            appSetting?.Value.Should().Be(newValue);
        }

        [Test]
        public async Task GetSettings_ForExistingKey_ShouldReturnValue()
        {
            var context = CreateContext();
            var cfgSvc = GetAppSettingsService(context);
            string value = "77";
            await context.AppSettings.AddAsync(new AppSetting
            {
                Key = AppSettingsKey.ImageQuality.ToString(),
                Value = value,
                ModifiedDate = DateTime.UtcNow.AddDays(-2)
            });
            await context.SaveChangesAsync();

            var (appSetting, modifiedDate) = await cfgSvc.GetAppSetting(AppSettingsKey.ImageQuality);

            appSetting.Should().NotBeNull();
            appSetting.Should().Be(value);
            modifiedDate.Should().NotBe(default);
        }

        [Test]
        public async Task GetSettings_ForUnknownKey_ShouldThrowArgumentException()
        {
            var context = CreateContext();
            var cfgSvc = GetAppSettingsService(context);

            Func<Task> action = async () => await cfgSvc.GetAppSetting(AppSettingsKey.Unknown);

            await action.Should().ThrowAsync<ArgumentException>();
        }
    }
}
