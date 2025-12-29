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
        // ===== 1. СЕРВЕР ПАПКАСИ =====
        string folderPath = @"\\server\Camera";

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // ===== 2. ФАЙЛ НОМИ =====
        string fileName =
            "photo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

        string fullPath = Path.Combine(folderPath, fileName);

        // ===== 3. ФАЙЛ ЯРАТИШ =====
        StorageFolder folder =
            await StorageFolder.GetFolderFromPathAsync(folderPath);

        StorageFile file =
            await folder.CreateFileAsync(
                fileName,
                CreationCollisionOption.ReplaceExisting);

        // ===== 4. КАМЕРА =====
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

        // ===== 5. 1С УЧУН ФОТО ЙЎЛИ =====
        string infoFile = Path.Combine(folderPath, "last_photo.txt");
        File.WriteAllText(infoFile, fullPath);

        // ===== 6. WATERMARK =====
        string watermarkText =
            DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") +
            " | PC: " + Environment.MachineName;

        using (Bitmap bmp = new Bitmap(fullPath))
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
            using (Brush bg = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
            {
                g.FillRectangle(
                    bg,
                    x - padding,
                    y - padding,
                    textSize.Width + padding * 2,
                    textSize.Height + padding * 2);
            }

            // ОҚ ЁЗУВ + СОЯ
            using (Brush shadow = new SolidBrush(Color.Black))
            using (Brush text = new SolidBrush(Color.White))
            {
                g.DrawString(watermarkText, font, shadow, x + 2, y + 2);
                g.DrawString(watermarkText, font, text, x, y);
            }

            bmp.Save(fullPath, ImageFormat.Jpeg);
        }
    }
}
