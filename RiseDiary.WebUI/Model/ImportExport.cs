using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RiseDiary.Model.ImportExport
{
    [Serializable]
    public class SDiaryRecord
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        [XmlIgnore]
        public string Name;
        [XmlElement("Name")]
        public XmlCDataSection NameCData
        {
            get => new XmlDocument().CreateCDataSection(Name);            
            set => Name = value?.Data;
        }
        [XmlIgnore]
        public string Text { get; set; }
        [XmlElement("Text")]
        public XmlCDataSection TextCData
        {
            get => new XmlDocument().CreateCDataSection(Text);
            set => Text = value?.Data;
        }
        public List<SCogitation> Cogitations { get; set; }
    }

    [Serializable]
    public class SCogitation
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        [XmlIgnore]
        public string Text { get; set; }
        [XmlElement("Text")]
        public XmlCDataSection TextCData
        {
            get => new XmlDocument().CreateCDataSection(Text);
            set => Text = value?.Data;
        }
    }

    [Serializable]
    public class SDiaryScope
    {
        public string Code { get; set; }
        public string ScopeName { get; set; }
        public List<SDiaryTheme> Themes { get; set; }
    }

    [Serializable]
    public class SDiaryTheme
    {
        public string Code { get; set; }
        public string ThemeName { get; set; }
    }

    [Serializable]
    public class SDiaryImage
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SizeByte { get; set; }
        public string Base64StringImageData { get; set; }
    }

    [Serializable]
    public class ExportedData
    {
        public DateTime ExportDate { get; set; }
        [XmlIgnore]
        public string BaseHostWithPort { get; set; }
        public XmlCDataSection BaseHostWithPortCData
        {
            get => new XmlDocument().CreateCDataSection(BaseHostWithPort);
            set => BaseHostWithPort = value?.Data;
        }
        public List<SDiaryRecord> Records { get; set; }

    }

    public static class DiarySerializer
    {
        public static async Task<string> SerializeRecords(List<DiaryRecord> recordsToSerialize, string hostWithPort)
        {
            var xmlRoot = new ExportedData
            {
                BaseHostWithPort = hostWithPort,
                ExportDate = DateTime.Now,
                Records = recordsToSerialize.Select(r => new SDiaryRecord
                {
                    Code = r.Code,
                    Date = r.Date,
                    CreateDate = r.CreateDate,
                    ModifyDate = r.ModifyDate,
                    Name = r.Name,
                    Text = r.Text,
                    Cogitations = r.Cogitations.Select(c => new SCogitation
                    {
                        Code = c.Code,
                        Date = c.Date,
                        Text = c.Text
                    }).ToList()
                }).ToList()
            };

            string result = string.Empty;

            using (var destStream = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(ExportedData), new Type[] { typeof(SDiaryRecord), typeof(SCogitation) });
                serializer.Serialize(destStream, xmlRoot);
                await destStream.FlushAsync();
                destStream.Position = 0;

                var sr = new StreamReader(destStream);
                result = await sr.ReadToEndAsync();                
            }
            return result;
        }
    }
}