﻿using System;

namespace RiseDiary.Domain.Model
{
    public class DiaryArea
    {
        public int AreaId { get; set; }
        public string AreaName { get; set; }
    }

    public class DiaryRecordType
    {
        public int RecordTypeId { get; set; }
        public int AreaId { get; set; }
        public string RecordTypeName { get; set; }
    }

    public class DiaryRecordTypeJoined
    {
        public int RecordTypeId { get; set; }
        public int? AreaId { get; set; }
        public string RecordTypeName { get; set; }        
        public string AreaName { get; set; }
    }

    public class DiaryImage
    {
        public int ImageId { get; set; }
        public string ImageName { get; set; }
        public DateTime CreateDate { get; set; }        
    }
    
    public class DiaryRecord
    {
        public int RecordId { get; set; }
        public DateTime RecordDate { get; set; }
        public DateTime RecordCreateDate { get; set; }
        public DateTime RecordModifyDate { get; set; }
        public string RecordName { get; set; }
        public string RecordText { get; set; }
    }

    public class Cogitation
    {
        public int CognitationId { get; set; }
        public int RecordId { get; set; }
        public DateTime CognitationDate { get; set; }
        public string Text { get; set; }
    }
}