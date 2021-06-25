using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace CreateFontList
{
    public class ListCreator
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MAXIMIZE = 3;

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
                    if (font.Item2.Length > 50)
                        sw.WriteLine($"{font.Item1.Replace("C:/temp/FontFolder", "G:/Apps/Fonts").Trim()},{font.Item2.Trim().Substring(0, 50)}");
                    else
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

            var rect = new User32.Rect();

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
                    OrderBy(p => p.StartTime).Last();   //get process 

                ShowWindow(process.MainWindowHandle, SW_MAXIMIZE);  //maximizes window
                Thread.Sleep(1000);

                User32.GetWindowRect(process.MainWindowHandle, ref rect);   //This will change the values in rect object

                var bmp = new Bitmap(rect.right - rect.left, rect.bottom - rect.top, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(bmp))
                {
                    graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(rect.right - rect.left, rect.bottom - rect.top), CopyPixelOperation.SourceCopy);
                    //graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(1000, 1000), CopyPixelOperation.SourceCopy);
                    //graphics.CopyFromScreen(rect.left, rect.top, 200, 200, new Size(rect.right - rect.left, rect.bottom - rect.top), CopyPixelOperation.SourceCopy);
                }

                if (process.MainWindowTitle.Contains("(TrueType)"))
                {
                    if (process.MainWindowTitle == "Arial Unicode MS (TrueType)" || process.MainWindowTitle == "Arial Black (TrueType)")
                        Console.WriteLine();

                    font.Add(Tuple.Create(directoryList[j], process.MainWindowTitle.Replace("(TrueType)", "")));
                    listResult.Add(process.MainWindowTitle.Replace("(TrueType)", ""));

                    if(process.MainWindowTitle.Length>20)
                        bmp.Save($@"C:\Users\Erik May\Documents\Images\{process.MainWindowTitle.Replace("(TrueType)", "").Substring(0, 10)}[{j}].jpeg",
                            ImageFormat.Jpeg);
                    else
                        bmp.Save($@"C:\Users\Erik May\Documents\Images\{process.MainWindowTitle.Replace("(TrueType)", "")}[{j}].jpeg",
                            ImageFormat.Jpeg);
                }
                else if (process.MainWindowTitle.Contains("(OpenType)"))
                {
                    font.Add(Tuple.Create(directoryList[j], process.MainWindowTitle.Replace("(OpenType)", "")));
                    listResult.Add(process.MainWindowTitle.Replace("(OpenType)", ""));

                    if(process.MainWindowTitle.Length>20)
                        bmp.Save($@"C:\Users\Erik May\Documents\Images\{process.MainWindowTitle.Replace("(OpenType)", "").Substring(0, 10)}.jpeg",
                            ImageFormat.Jpeg);
                    else
                        bmp.Save($@"C:\Users\Erik May\Documents\Images\{process.MainWindowTitle.Replace("(OpenType)", "")}.jpeg",
                           ImageFormat.Jpeg);
                }
                else
                {
                    font.Add(Tuple.Create($"{directoryList[j].Replace(targetDirectory, @"G:\Apps\Fonts\")}", process.MainWindowTitle));
                    listResult.Add(process.MainWindowTitle);

                    bmp.Save($@"C:\Users\Erik May\Documents\Images\{process.MainWindowTitle}.jpeg", ImageFormat.Jpeg);
                }

                process.Kill();
            }
            var result = listResult.Distinct();
            return result;
        }
    }

    public static class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
    }
}
