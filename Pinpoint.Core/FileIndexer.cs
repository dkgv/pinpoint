using System.Collections.Generic;
using System.IO;

namespace Pinpoint.Core
{
    public static class FileIndexer
    {
        public static IEnumerable<FileInfo> Index(string path, string extension)
        {
            static void Recurse(string path, string extension, List<FileInfo> results)
            {
                var directory = new DirectoryInfo(path);
                var files = directory.GetFiles();

                foreach (var file in files)
                {
                    if (File.GetAttributes(file.FullName).HasFlag(FileAttributes.Directory))
                    {
                        Recurse(file.FullName, extension, results);
                    }
                    else
                    {
                        if (Path.GetExtension(file.FullName).Equals(extension))
                        {
                            results.Add(file);
                        }
                    }
                }
            }

            var results = new List<FileInfo>();
            Recurse(path, extension, results);
            return results;
        }
    }
}
