using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0) return 1;

        string path = args[0];

        var capture = new MediaCapture();
        await capture.InitializeAsync();

        StorageFile file = await StorageFile.GetFileFromPathAsync(path);
        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);

        await capture.CapturePhotoToStreamAsync(
            ImageEncodingProperties.CreateJpeg(),
            stream
        );

        capture.Dispose();
        return 0;
    }
}

