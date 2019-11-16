using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests
{
    [TestFixture]
    internal class AppSettingsTests : TestFixtureBase
    {
        [Test]
        public async Task UpdateSettings_WithCorrectKeyValue_ShouldNotThrowException()
        {
            var context = CreateContext();
            string key = "Timeout";
            string value = "Infinity";

            await context.UpdateAppSetting(key, value);

            Assert.IsNotNull(context.AppSettings.FirstOrDefault(s => s.Key == key && s.Value == value));
        }

        [Test]
        public void UpdateSettings_WithNullKeyValue_ShouldThrowArgumentException()
        {
            var context = CreateContext();
            string key = null;
            string value = "Infinity";

            Assert.ThrowsAsync<ArgumentException>(async () => await context.UpdateAppSetting(key, value));

            key = string.Empty;
            Assert.ThrowsAsync<ArgumentException>(async () => await context.UpdateAppSetting(key, value));

            key = "         ";
            Assert.ThrowsAsync<ArgumentException>(async () => await context.UpdateAppSetting(key, value));
        }

        [Test]
        public async Task UpdateSettings_WithCorrectKeyValue_ShouldWriteModifiedDate()
        {
            var context = CreateContext();
            string key = "Timeout";
            string value = "Infinity";

            await context.UpdateAppSetting(key, value);

            var mDate = context.AppSettings.FirstOrDefault(s => s.Key == key)?.ModifiedDate;
            Assert.AreEqual(DateTime.Now.Date, mDate?.Date);
        }

        [Test]
        public async Task UpdateSettings_ForExistingKey_ShouldUpdateModifiedDate()
        {
            var context = CreateContext();
            string key = "Timeout";
            string value = "Infinity";
            await context.AppSettings.AddAsync(new AppSetting
            {
                Key = key,
                Value = value,
                ModifiedDate = DateTime.Now.AddDays(-2)
            });
            await context.SaveChangesAsync();

            await context.UpdateAppSetting(key, "New Value");

            var mDate = context.AppSettings.FirstOrDefault(s => s.Key == key)?.ModifiedDate;
            Assert.AreEqual(DateTime.Now.Date, mDate?.Date);
        }

        [Test]
        public async Task UpdateSettings_ForExistingKey_ShouldUpdateValue()
        {
            var context = CreateContext();
            string key = "Timeout";
            string value = "Infinity";
            string newValue = "30 seconds";

            await context.UpdateAppSetting(key, value);
            await context.UpdateAppSetting(key, newValue);

            var appSetting = context.AppSettings.FirstOrDefault(s => s.Key == key);
            Assert.NotNull(appSetting);
            Assert.AreEqual(newValue, appSetting.Value);
        }

        [Test]
        public async Task GetSettings_ForExistingKey_ShouldReturnValue()
        {
            var context = CreateContext();
            string key = "Timeout";
            string value = "Infinity";
            await context.AppSettings.AddAsync(new AppSetting
            {
                Key = key,
                Value = value,
                ModifiedDate = DateTime.Now.AddDays(-2)
            });
            await context.SaveChangesAsync();

            var appSetting = await context.GetAppSetting(key);

            Assert.NotNull(appSetting);
            Assert.AreEqual(value, appSetting);

        }

        [Test]
        public async Task GetSettings_ForNotExistingKey_ShouldReturnEmptyString()
        {
            var context = CreateContext();

            var appSetting = await context.GetAppSetting("NotExisting");

            Assert.IsNull(appSetting);
        }
    }
}
