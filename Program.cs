using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

class Program
{
    [STAThread]
    static int Main()
    {
        try
        {
            RunAsync().GetAwaiter().GetResult();
            return 0;
        }
        catch
        {
            return 10;
        }
    }

    static async Task RunAsync()
    {
        // ===== 1. СЕРВЕРДА РАСМ САҚЛАНАДИ =====
        string serverFolder = @"\\server\Camera";

        if (!Directory.Exists(serverFolder))
            Directory.CreateDirectory(serverFolder);

        string fileName =
            "photo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

        string fullPhotoPath = Path.Combine(serverFolder, fileName);

        // ===== 2. ФАЙЛ ЯРАТИШ =====
        StorageFolder folder =
            await StorageFolder.GetFolderFromPathAsync(serverFolder);

        StorageFile file =
            await folder.CreateFileAsync(
                fileName,
                CreationCollisionOption.ReplaceExisting);

        // ===== 3. КАМЕРА =====
        var capture = new MediaCapture();
        await capture.InitializeAsync();

        using (var stream =
            await file.OpenAsync(FileAccessMode.ReadWrite))
        {
            await capture.CapturePhotoToStreamAsync(
                ImageEncodingProperties.CreateJpeg(),
                stream);
        }

        capture.Dispose();

        // ===== 4. ЛОКАЛДА ФОТО ЙЎЛИНИ САҚЛАЙМИЗ =====
        string localTemp = @"C:\Temp";
        Directory.CreateDirectory(localTemp);

        string localInfoFile = Path.Combine(localTemp, "last_photo.txt");
        File.WriteAllText(localInfoFile, fullPhotoPath);

        // ===== 5. WATERMARK (ҚИЗИЛ ЁЗУВ + ҚОРА ФОН) =====
        string watermarkText =
            DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") +
            " | PC: " + Environment.MachineName;

        using (Bitmap bmp = new Bitmap(fullPhotoPath))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            Font font = new Font(FontFamily.GenericSansSerif, 28, FontStyle.Bold);
            SizeF textSize = g.MeasureString(watermarkText, font);

            int padding = 20;

            float x = (bmp.Width - textSize.Width) / 2;
            float y = bmp.Height - textSize.Height - padding * 2;

            // ҚОРА ФОН
            using (Brush bg = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
            {
                g.FillRectangle(
                    bg,
                    x - padding,
                    y - padding,
                    textSize.Width + padding * 2,
                    textSize.Height + padding * 2);
            }

            // ҚИЗИЛ ЁЗУВ + СОЯ
            using (Brush shadow = new SolidBrush(Color.Black))
            using (Brush text = new SolidBrush(Color.Red))
            {
                g.DrawString(watermarkText, font, shadow, x + 2, y + 2);
                g.DrawString(watermarkText, font, text, x, y);
            }

            bmp.Save(fullPhotoPath, ImageFormat.Jpeg);
        }
    }
}
