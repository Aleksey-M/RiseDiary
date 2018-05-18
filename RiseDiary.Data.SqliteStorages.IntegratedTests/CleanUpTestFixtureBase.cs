using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RiseDiary.WebUI.Data;
using System;
using System.IO;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    public class CleanUpTestFixtureBase
    {
        [OneTimeTearDown]
        public void CleanUp()
        {
            TestHelper.RemoveTmpDbFiles();
        }
    }
}
