using System;

namespace RiseDiary.Model
{
    public class AppSetting
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public DateTime ModifiedDate { get; set; }
    }
}