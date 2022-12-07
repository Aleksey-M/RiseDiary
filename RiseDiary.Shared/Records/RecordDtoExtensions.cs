namespace RiseDiary.Shared.Records;

public static class DiaryRecordExtensions
{
    public static string GetRecordNameDisplay(this RecordDto rec) => string.IsNullOrWhiteSpace(rec.Name) ? "[ПУСТО]" : rec.Name;

    public static string GetRecordTextShort(this RecordDto rec)
    {
        if (string.IsNullOrEmpty(rec.Text)) return "[ПУСТО]";
        return rec.Text.Length < 35 ? rec.Text : string.Concat(rec.Text.AsSpan(0, 35), "[...]");
    }
}
