using System;
using System.IO;
using Microsoft.Maui;
using TimeClock.Helpers;


[assembly: Dependency(typeof(FileHelper))]
namespace TimeClock.Helpers
{
    class FileHelper : IFileHelper
    {
        public string GetLocalFilePath(string filename)
        {
            string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

            if (!Directory.Exists(libFolder))
            {
                Directory.CreateDirectory(libFolder);
            }

            return Path.Combine(libFolder, filename);
        }
    }
}

/*
 TODO -consider


Also, the Dependency attribute is used for dependency injection in Xamarin.Forms. .NET MAUI introduces a new service-based dependency injection system which you might want to consider using.
*/