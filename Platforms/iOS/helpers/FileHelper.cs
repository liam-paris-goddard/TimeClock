using Goddard.Clock.Helpers;


[assembly: Dependency(typeof(FileHelper))]
namespace Goddard.Clock.Helpers;
class FileHelper : IFileHelper
{
    public string GetLocalFilePath(string filename)
    {
        string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

        if (!Directory.Exists(libFolder))
        {
            _ = Directory.CreateDirectory(libFolder);
        }

        return Path.Combine(libFolder, filename);
    }
}