using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static async Task<int> Main()
    {
        try
        {
            // ===== 1. САҚЛАШ ПАПКАСИ (СЕРВЕР) =====
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

            // ===== 4. КАМЕРАДАН РАСМ ОЛИШ =====
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

            // ===== 5. WATERMARK =====
            string watermark =
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") +
                " | PC: " + Environment.MachineName;

            using (Bitmap bmp = new Bitmap(fullPath))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Font font = new Font("Arial", 22, FontStyle.Bold);
                Brush brush = new SolidBrush(Color.FromArgb(180, Color.White));

                SizeF size = g.MeasureString(watermark, font);
                float x = bmp.Width - size.Width - 20;
                float y = bmp.Height - size.Height - 20;

                g.DrawString(watermark, font, brush, x, y);
                bmp.Save(fullPath, ImageFormat.Jpeg);
            }

            // ===== 6. ФОТО ЙЎЛИНИ 1С УЧУН ЁЗИШ =====
            string infoFile = Path.Combine(folderPath, "last_photo.txt");
            File.WriteAllText(infoFile, fullPath);

            return 0; // ҲАММАСИ ЯХШИ
        }
        catch
        {
            return 10; // ХАТО (КАМЕРА ЁКИ ЙЎЛ)
        }
    }
}
