using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace CreateFontList
{
    public class ListCreator
    {
        public static void CreateList()
        {
            var appSettings = new AppSettings();

            List<string> fileList = new List<string>();
            List<Tuple<string, string>> fontList = new List<Tuple<string, string>>();

            string sourceDirectory = appSettings.Location.SourceDirectory;
            string targetDirectory = appSettings.Location.TargetDirectory;

            if (Directory.Exists(targetDirectory))
                SharedFunctionality.Shared_Functions.DeleteDirectory(targetDirectory);

            DirectoryInfo sourceFolder = new DirectoryInfo(sourceDirectory);
            DirectoryInfo targetFolder = new DirectoryInfo(targetDirectory);

            SharedFunctionality.Shared_Functions.CopyAll(sourceFolder, targetFolder);

            var files = Directory.GetFiles(targetDirectory);

            SharedFunctionality.Shared_Functions.GetDirectoryList(ref fileList, files);

            var initialProcessesList = Process.GetProcesses().ToList();

            var distinctFontList = GetFont(initialProcessesList, fileList, ref fontList);

            var fileToMove = $"{appSettings.Location.OldFontListFileLocation}List-{File.GetLastWriteTime(appSettings.Location.ShareFontListFileLocation).ToString("yyyy-MM-dd H-mm")}.txt";

            if (File.Exists(appSettings.Location.ShareFontListFileLocation))
            {
                File.Copy(appSettings.Location.ShareFontListFileLocation, fileToMove);
                File.Delete(appSettings.Location.ShareFontListFileLocation);
            }

            using (StreamWriter sw = new StreamWriter(appSettings.Location.ShareFontListFileLocation))
            {
                foreach (var font in distinctFontList)
                {
                    if (font.Length > 50)
                        sw.WriteLine(font.Trim().Substring(0, 50));
                    else
                        sw.WriteLine(font.Trim());
                }
            }

            if (File.Exists(appSettings.Location.MISFontListFileLocation))
            {
                fileList = File.ReadAllLines(appSettings.Location.MISFontListFileLocation).ToList();

                File.Copy(appSettings.Location.MISFontListFileLocation,
                    $"{appSettings.Location.MISOldFontList}{File.GetLastWriteTime(appSettings.Location.MISFontListFileLocation).ToString("yyyy-MM-dd H-mm")}.csv");

                File.Delete(appSettings.Location.MISFontListFileLocation);
            }

            using (StreamWriter sw = new StreamWriter(appSettings.Location.MISFontListFileLocation))
            {
                foreach (var font in fontList)
                {
                    sw.WriteLine($"{font.Item1.Replace("C:/temp/FontFolder", "G:/Apps/Fonts").Trim()},{font.Item2.Trim()}");
                }
            }

            if (Directory.Exists(targetDirectory))
                SharedFunctionality.Shared_Functions.DeleteDirectory(targetDirectory);
        }

        private static IEnumerable<string> GetFont(List<Process> initialProcessList, List<string> directoryList,
           ref List<Tuple<string, string>> font, string targetDirectory = @"C:\temp\FontFolder\")
        {
            List<string> listResult = new List<string>();
            ProcessStartInfo objProcess = new ProcessStartInfo();
            objProcess.UseShellExecute = true;
            objProcess.RedirectStandardOutput = false;
            var i = 0;
            for (int j = 0; j < directoryList.Count; j++)
            {
                i++;
                Console.WriteLine($"{i} out of {directoryList.Count}.");
                objProcess.FileName = directoryList[j];
                Process.Start(objProcess);

                Thread.Sleep(500);

                var process = Process.GetProcesses().Where(p => p.MainWindowTitle != string.Empty).
                    Except(initialProcessList.Where(p => p.MainWindowTitle != string.Empty)).
                    OrderBy(p => p.StartTime).Last();

                if (process.MainWindowTitle.Contains("(TrueType)"))
                {
                    if (process.MainWindowTitle == "Arial Unicode MS (TrueType)" || process.MainWindowTitle == "Arial Black (TrueType)")
                        Console.WriteLine();

                    font.Add(Tuple.Create(directoryList[j], process.MainWindowTitle.Replace("(TrueType)", "")));
                    listResult.Add(process.MainWindowTitle.Replace("(TrueType)", ""));                    
                }
                else if (process.MainWindowTitle.Contains("(OpenType)"))
                {
                    font.Add(Tuple.Create(directoryList[j], process.MainWindowTitle.Replace("(OpenType)", "")));
                    listResult.Add(process.MainWindowTitle.Replace("(OpenType)", ""));
                }
                else
                {
                    font.Add(Tuple.Create($"{directoryList[j].Replace(targetDirectory, @"G:\Apps\Fonts\")}", process.MainWindowTitle));
                    listResult.Add(process.MainWindowTitle);
                }

                process.Kill();

                Thread.Sleep(500);
            }
            var result = listResult.Distinct();
            return result;
        }
    }
}
