using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Drawing;
using System.Drawing.Imaging;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
            return 1;

        string path = args[0];
        string watermarkText = args[1];

        try
        {
            // ===== CREATE FILE =====
            string folderPath = System.IO.Path.GetDirectoryName(path);
            string fileName = System.IO.Path.GetFileName(path);

            StorageFolder folder =
                await StorageFolder.GetFolderFromPathAsync(folderPath);

            StorageFile file =
                await folder.CreateFileAsync(
                    fileName,
                    CreationCollisionOption.ReplaceExisting
                );

            // ===== CAMERA INIT =====
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

            // ===== WATERMARK =====
            using (Bitmap bmp = new Bitmap(path))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Font font = new Font("Arial", 24, FontStyle.Bold);
                Brush brush = new SolidBrush(Color.FromArgb(180, Color.White));

                SizeF size = g.MeasureString(watermarkText, font);
                float x = bmp.Width - size.Width - 20;
                float y = bmp.Height - size.Height - 20;

                g.DrawString(watermarkText, font, brush, x, y);
                bmp.Save(path, ImageFormat.Jpeg);
            }

            return 0; // OK
        }
        catch (Exception)
        {
            return 10; // CAMERA NOT AVAILABLE
        }
    }
}
