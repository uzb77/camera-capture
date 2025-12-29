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
        // ===== CONFIG =====
        string uncFolder = @"\\server\Camera"; // <-- СИЗ СЎРАГАН ЖОЙ
        string fileName = "photo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
        string fullPath = Path.Combine(uncFolder, fileName);

        try
        {
            // ===== ENSURE FOLDER =====
            if (!Directory.Exists(uncFolder))
            {
                Directory.CreateDirectory(uncFolder);
            }

            // ===== CREATE FILE VIA WINRT =====
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(uncFolder);
            StorageFile file = await folder.CreateFileAsync(
                fileName,
                CreationCollisionOption.ReplaceExisting
            );

            // ===== CAMERA =====
            var capture = new MediaCapture();
            await capture.InitializeAsync();

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await capture.CapturePhotoToStreamAsync(
                    ImageEncodingProperties.CreateJpeg(),
                    stream
                );
            }

            capture.Dispose();

            // ===== WATERMARK =====
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

            return 0; // OK
        }
        catch (UnauthorizedAccessException)
        {
            return 20; // ACCESS DENIED TO \\server
        }
        catch (DirectoryNotFoundException)
        {
            return 21; // PATH NOT FOUND
        }
        catch
        {
            return 10; // CAMERA OR OTHER ERROR
        }
    }
}
