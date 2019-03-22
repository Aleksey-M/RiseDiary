using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.IE
{
    [TestFixture]
    internal class FetchByCodeTests : TestFixtureBase
    {
        [Test]
        public async Task GetRecordByCode_ShouldReturnRecord()
        {
            var context = CreateContext();
            var rec = GetTestRecord();

            await context.AddRecord(rec);
            string code = rec.Code;
            var loadedRec = await context.FetchRecordByCode(code);

            Assert.NotNull(loadedRec);
            Assert.AreEqual(rec.CreateDate, loadedRec.CreateDate);
            Assert.AreEqual(rec.Date, loadedRec.Date);
            Assert.AreEqual(rec.ModifyDate, loadedRec.ModifyDate);
            Assert.AreEqual(rec.Name, loadedRec.Name);
            Assert.AreEqual(rec.Text, loadedRec.Text);
        }

        [Test]
        public async Task GetCogitationByCode_ShouldReturnCogitation()
        {
            var context = CreateContext();
            var (recId, cogCode) = Create_3Records_1Cogitation_WithCode(context);

            var cog = await context.FetchCogitationByCode(cogCode);

            Assert.IsNotNull(cog);
            Assert.AreEqual(recId, cog.RecordId);
        }

        [Test]
        public async Task GetImageByCode_ShouldReturnImage()
        {
            var context = CreateContext();
            var img = GetTestImage();
            string code = "CODE01";

            await context.AddImage(img.Name, img.Thumbnail, code);
            var imgSaved = await context.FetchImageByCode(code);

            Assert.IsNotNull(imgSaved);
            Assert.AreEqual(img.Name, imgSaved.Name);
            Assert.AreEqual(img.CreateDate.Date, imgSaved.CreateDate.Date);
        }

        [Test]
        public async Task GetScopeByCode_ShouldReturnScope()
        {
            var context = CreateContext();
            await context.AddScope("Scope 1", "SCOPE_CODE");

            var s = await context.FetchScopeByCode("SCOPE_CODE");

            Assert.IsNotNull(s);
            Assert.AreEqual("SCOPE_CODE", s.Code);
            Assert.AreEqual("Scope 1", s.ScopeName);
        }

        [Test]
        public async Task GetThemeByCode_ShouldReturnTheme()
        {
            var context = CreateContext();
            string themeName = @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй";
            int themeId = Create_Theme(context, themeName, null, "THEME_CODE");

            var theme = await context.FetchThemeByCode("THEME_CODE");

            Assert.IsNotNull(theme);
            Assert.AreEqual(themeName, theme.ThemeName);
        }
    }
}
