using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

class Program
{
    static async Task<int> Main()
    {
        try
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
                    CreationCollisionOption.ReplaceExisting
                );

            // ===== 4. КАМЕРА =====
            var capture = new MediaCapture();
            await capture.InitializeAsync();

            using (var stream =
                await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await capture.CapturePhotoToStreamAsync(
                    ImageEncodingProperties.CreateJpeg(),
                    stream
                );
            }

            capture.Dispose();

            // ===== 5. WATERMARK (ҚОРА ФОН + ОҚ ЁЗУВ) =====
            string watermarkText =
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") +
                " | PC: " + Environment.MachineName;

            using (Bitmap bmp = new Bitmap(fullPath))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                Font font = new Font(FontFamily.GenericSansSerif, 22, FontStyle.Bold);
                SizeF textSize = g.MeasureString(watermarkText, font);

                int padding = 10;
                float x = bmp.Width - textSize.Width - padding * 2;
                float y = bmp.Height - textSize.Height - padding * 2;

                // ҚОРА ФОН
                using (Brush bg = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
                {
                    g.FillRectangle(
                        bg,
                        x - padding,
                        y - padding,
                        textSize.Width + padding * 2,
                        textSize.Height + padding * 2
                    );
                }

                // ОҚ ЁЗУВ
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(watermarkText, font, textBrush, x, y);
                }

                bmp.Save(fullPath, ImageFormat.Jpeg);
            }

            // ===== 6. 1С УЧУН ФОТО ЙЎЛИ =====
            string infoFile = Path.Combine(folderPath, "last_photo.txt");
            File.WriteAllText(infoFile, fullPath);

            return 0; // ҲАММАСИ ЯХШИ
        }
        catch
        {
            return 10; // ХАТО
        }
    }
}
