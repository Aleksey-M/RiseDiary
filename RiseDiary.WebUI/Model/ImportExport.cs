//using Microsoft.EntityFrameworkCore;
//using RiseDiary.WebUI.Data;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Serialization;

//namespace RiseDiary.Model.ImportExport
//{
//    // 'S' prefix means 'Serializable'
//    [Serializable]
//    public class SDiaryRecord
//    {
//        public string Id { get; set; }
//        public DateTime Date { get; set; }
//        public DateTime CreateDate { get; set; }
//        public DateTime ModifyDate { get; set; }
//        [XmlIgnore]
//        public string Name;
//        [XmlElement("Name")]
//        public XmlCDataSection NameCData
//        {
//            get => new XmlDocument().CreateCDataSection(Name);
//            set => Name = value?.Data;
//        }
//        [XmlIgnore]
//        public string Text { get; set; }
//        [XmlElement("Text")]
//        public XmlCDataSection TextCData
//        {
//            get => new XmlDocument().CreateCDataSection(Text);
//            set => Text = value?.Data;
//        }
//        public List<SCogitation> Cogitations { get; set; }

//        public List<string> IdentsOfThemes { get; set; }
//        public List<string> IdentsOfImages { get; set; }

//        public static SDiaryRecord CreateFromEntity(DiaryRecord diaryRecord)
//        {
//            return new SDiaryRecord
//            {
//                Id = diaryRecord.Id.ToString(),
//                Date = diaryRecord.Date,
//                CreateDate = diaryRecord.CreateDate,
//                ModifyDate = diaryRecord.ModifyDate,
//                Name = diaryRecord.Name,
//                Text = diaryRecord.Text,
//                Cogitations = diaryRecord.Cogitations.Select(c => new SCogitation
//                {
//                    Id = c.Id.ToString(),
//                    Date = c.Date,
//                    Text = c.Text
//                }).ToList(),
//                IdentsOfThemes = diaryRecord.ThemesRefs?.Select(tr => tr.Theme.Id.ToString()).ToList() ?? new List<string>(),
//                IdentsOfImages = diaryRecord.ImagesRefs?.Select(ir => ir.Image.Id.ToString()).ToList() ?? new List<string>()
//            };
//        }
//    }

//    [Serializable]
//    public class SCogitation
//    {
//        public string Id { get; set; }
//        public DateTime Date { get; set; }
//        [XmlIgnore]
//        public string Text { get; set; }
//        [XmlElement("Text")]
//        public XmlCDataSection TextCData
//        {
//            get => new XmlDocument().CreateCDataSection(Text);
//            set => Text = value?.Data;
//        }
//    }

//    [Serializable]
//    public class SDiaryScope
//    {
//        public string Id { get; set; }
//        [XmlIgnore]
//        public string ScopeName { get; set; }
//        [XmlElement("ScopeName")]
//        public XmlCDataSection ScopeNameCData
//        {
//            get => new XmlDocument().CreateCDataSection(ScopeName);
//            set => ScopeName = value?.Data;
//        }
//        public List<SDiaryTheme> Themes { get; set; }
//    }

//    [Serializable]
//    public class SDiaryTheme
//    {
//        public string Id { get; set; }
//        [XmlIgnore]
//        public string ThemeName { get; set; }
//        [XmlElement("ThemeName")]
//        public XmlCDataSection ThemeNameCData
//        {
//            get => new XmlDocument().CreateCDataSection(ThemeName);
//            set => ThemeName = value?.Data;
//        }
//    }

//    [Serializable]
//    public class SDiaryImage
//    {
//        public string Id { get; set; }
//        [XmlIgnore]
//        public string ImageName { get; set; }
//        [XmlElement("Name")]
//        public XmlCDataSection ImageNameCData
//        {
//            get => new XmlDocument().CreateCDataSection(ImageName);
//            set => ImageName = value?.Data;
//        }
//        public DateTime CreateDate { get; set; }
//        public DateTime ModifyDate { get; set; }
//        public int Width { get; set; }
//        public int Height { get; set; }
//        public int SizeByte { get; set; }
//        [XmlIgnore]
//        public string ImageData { get; set; }
//        [XmlElement("ImageData")]
//        public XmlCDataSection ImageCData
//        {
//            get => new XmlDocument().CreateCDataSection(ImageData);
//            set => ImageData = value?.Data;
//        }
//    }

//    [Serializable]
//    public class ExportedData
//    {
//        public DateTime ExportDate { get; set; }
//        public static int CurrentVersion => 1;
//        public int Version { get; set; } = CurrentVersion;
//        [XmlIgnore]
//        public string BaseHostWithPort { get; set; }
//        [XmlElement("BaseHostWithPort")]
//        public XmlCDataSection BaseHostWithPortCData
//        {
//            get => new XmlDocument().CreateCDataSection(BaseHostWithPort);
//            set => BaseHostWithPort = value?.Data;
//        }
//        public List<SDiaryRecord> Records { get; set; }
//        public List<SDiaryScope> Scopes { get; set; }
//        public List<SDiaryImage> Images { get; set; }
//    }

//    public static class DiarySerializer
//    {
//        public static async Task<string> SerializeDiaryRecords(this DiaryDbContext context, IEnumerable<Guid> idsOfRecords, string hostWithPort, bool withThemes, bool withImages)
//        {
//            IEnumerable<DiaryRecord> allRecordsData = null;

//            if (!withThemes && !withImages)
//            {
//                allRecordsData = await context.Records
//                     .Include(r => r.Cogitations)
//                     .Where(r => idsOfRecords.Contains(r.Id))
//                     .ToListAsync()
//                     .ConfigureAwait(false);
//            }
//            else if (withThemes && !withImages)
//            {
//                allRecordsData = await context.Records
//                     .Include(r => r.Cogitations)
//                     .Include(r => r.ThemesRefs)
//                     .ThenInclude(rt => rt.Theme)
//                     .ThenInclude(t => t.Scope)
//                     .Where(r => idsOfRecords.Contains(r.Id))
//                     .ToListAsync()
//                     .ConfigureAwait(false);
//            }
//            else if (!withThemes && withImages)
//            {
//                allRecordsData = await context.Records
//                     .Include(r => r.Cogitations)
//                     .Include(r => r.ImagesRefs)
//                     .ThenInclude(ri => ri.Image)
//                     .ThenInclude(i => i.FullImage)
//                     .Where(r => idsOfRecords.Contains(r.Id))
//                     .ToListAsync()
//                     .ConfigureAwait(false);
//            }
//            else
//            {
//                allRecordsData = await context.Records
//                     .Include(r => r.Cogitations)
//                     .Include(r => r.ThemesRefs)
//                     .ThenInclude(rt => rt.Theme)
//                     .ThenInclude(t => t.Scope)
//                     .Include(r => r.ImagesRefs)
//                     .ThenInclude(ri => ri.Image)
//                     .ThenInclude(i => i.FullImage)
//                     .Where(r => idsOfRecords.Contains(r.Id))
//                     .ToListAsync()
//                     .ConfigureAwait(false);
//            }

//            var allData = CreateSerializedEntities(allRecordsData, hostWithPort);

//            return await SerializeToString(allData).ConfigureAwait(false);
//        }

//        private static ExportedData CreateSerializedEntities(IEnumerable<DiaryRecord> recordsToSerialization, string hostWithPort)
//        {
//            if (recordsToSerialization == null) throw new ArgumentNullException(nameof(recordsToSerialization));

//            var allData = new ExportedData
//            {
//                BaseHostWithPort = hostWithPort,
//                ExportDate = DateTime.Now,
//                Records = recordsToSerialization?.Select(r => SDiaryRecord.CreateFromEntity(r)).ToList() ?? new List<SDiaryRecord>()
//            };


//            var allUsedThemes = recordsToSerialization.SelectMany(r => r.ThemesRefs ?? new List<DiaryRecordTheme>()).Select(tr => tr.Theme).ToList();
//            var allUsedScopes = allUsedThemes.Select(t => t.Scope).Distinct().ToList();

//            allData.Scopes = allUsedScopes.Select(s => new SDiaryScope
//            {
//                Id = s.Id.ToString(),
//                ScopeName = s.ScopeName,
//                Themes = s.Themes.Where(t => allUsedThemes.Contains(t)).Select(t => new SDiaryTheme
//                {
//                    Id = t.Id.ToString(),
//                    ThemeName = t.ThemeName
//                }).ToList()
//            }).ToList();


//            allData.Images = recordsToSerialization
//                .SelectMany(r => r.ImagesRefs ?? new List<DiaryRecordImage>())
//                .Select(ri => ri.Image)
//                .Distinct()
//                .Select(img => new SDiaryImage
//                {
//                    Id = img.Id.ToString(),
//                    CreateDate = img.CreateDate,
//                    ModifyDate = img.ModifyDate,
//                    ImageName = img.Name,
//                    Height = img.Height,
//                    Width = img.Width,
//                    SizeByte = img.SizeByte,
//                    ImageData = Convert.ToBase64String(img.FullImage.Data)
//                }).ToList();


//            return allData;
//        }
//        private static async Task<string> SerializeToString(ExportedData diaryRecordsData)
//        {
//            string result = string.Empty;

//            using (var destStream = new MemoryStream())
//            {
//                var serializer = new XmlSerializer(
//                    typeof(ExportedData),
//                    new Type[] { typeof(SDiaryTheme), typeof(SDiaryScope), typeof(SDiaryRecord), typeof(SCogitation), typeof(SDiaryImage) });

//                serializer.Serialize(destStream, diaryRecordsData);
//                await destStream.FlushAsync().ConfigureAwait(false);
//                destStream.Position = 0;

//                var sr = new StreamReader(destStream);
//                result = await sr.ReadToEndAsync().ConfigureAwait(false);
//            }

//            return result;
//        }
//    }
//}