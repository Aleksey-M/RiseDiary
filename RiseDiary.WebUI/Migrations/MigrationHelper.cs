using RiseDiary.WebUI.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RiseDiary.WebUI.Migrations
{
    public static class MigrationHelper
    {
        public static void UpdateThumbnailsFromFullImagesTable(this DiaryDbContext context)
        {
            var pairs = context.Images.Join(
                context.FullSizeImages,
                i => i.Id,
                fi => fi.Id,
                (i, fi) => new { Image = i, FullImage = fi });

            foreach(var pair in pairs)
            {
                pair.Image.Thumbnail = ImageHelper.ScaleImage(pair.FullImage.Data);
                pair.Image.SizeByte = pair.FullImage.Data.Length;
                (pair.Image.Width, pair.Image.Height) = ImageHelper.ImageSize(pair.FullImage.Data);
                context.Attach(pair.Image);
            }
            context.SaveChanges();
            context.Vacuum();
        }

        public static string PlainTextToHtml(string text)
        {
            return HttpUtility.HtmlEncode(text)
                .Replace("\r\n", "\r")
                .Replace("\n", "\r")
                .Replace("\r", "<br>\r\n")
                .Replace("  ", " &nbsp;");
        }

        public static void UpdateRecordsAndCogitationsPlainTextToHtml(this DiaryDbContext context)
        {
            context.Cogitations.ToList().ForEach(c => c.Text = PlainTextToHtml(c.Text));
            context.SaveChanges();
            context.Records.ToList().ForEach(r => r.Text = PlainTextToHtml(r.Text));
            context.SaveChanges();
        } 
    }
}
