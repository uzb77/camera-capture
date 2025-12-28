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
        if (args.Length == 0)
            return 1;

        string path = args[0];

        // 📌 FILE NAME + FOLDER
        string folderPath = System.IO.Path.GetDirectoryName(path);
        string fileName = System.IO.Path.GetFileName(path);

        // 📌 CREATE FILE IF NOT EXISTS
        StorageFolder folder =
            await StorageFolder.GetFolderFromPathAsync(folderPath);

        StorageFile file =
            await folder.CreateFileAsync(
                fileName,
                CreationCollisionOption.ReplaceExisting
            );

        // 📌 CAMERA
        var capture = new MediaCapture();
        await capture.InitializeAsync();

        using var stream =
            await file.OpenAsync(FileAccessMode.ReadWrite);

        await capture.CapturePhotoToStreamAsync(
            ImageEncodingProperties.CreateJpeg(),
            stream
        );

        capture.Dispose();
        return 0;
    }
}
