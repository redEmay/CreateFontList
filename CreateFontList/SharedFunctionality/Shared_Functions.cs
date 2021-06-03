using System;
using System.Collections.Generic;
using System.IO;

namespace CreateFontList.SharedFunctionality
{
    class Shared_Functions
    {
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine($"Copying {target.FullName}\\{fi.Name}");
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static void GetDirectoryList(ref List<string> files, string[] directory)
        {
            foreach (var file in directory)
            {
                if (file.Contains(".")) files.Add(file);
                else GetDirectoryList(ref files, Directory.GetFiles(file));
            }
        }
    }
}
