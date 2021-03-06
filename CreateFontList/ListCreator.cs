using CreateFontList.Classes;
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
                    if (font.getFontName().Length > 50)
                        sw.WriteLine(font.getFontName().Trim().Substring(0, 50));
                    else
                        sw.WriteLine(font.getFontName().Trim());
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
                foreach(var font in distinctFontList)
                {
                    sw.WriteLine($"{font.getDirectoryLocation().Replace("C:/temp/FontFolder", "G:/Apps/Fonts").Trim()},{font.getFontName()},{font.getBitMap()}");
                }

                /*foreach (var font in fontList)
                {
                    if (font.Item2.Length > 50)
                        sw.WriteLine($"{font.Item1.Replace("C:/temp/FontFolder", "G:/Apps/Fonts").Trim()},{font.Item2.Trim().Substring(0, 50)}");
                    else
                        sw.WriteLine($"{font.Item1.Replace("C:/temp/FontFolder", "G:/Apps/Fonts").Trim()},{font.Item2.Trim()}");
                }*/
            }

            if (Directory.Exists(targetDirectory))
                SharedFunctionality.Shared_Functions.DeleteDirectory(targetDirectory);
        }
        private static string RemoveBadCharacters(string fileName)
        {
            var badCharacters = new List<string> { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" };
            
            foreach(var character in badCharacters)
            {
                fileName = fileName.Replace(character, string.Empty);
            }
            return fileName;
        }

        private static IEnumerable<FontItem> GetFont(List<Process> initialProcessList, List<string> directoryList,
           ref List<Tuple<string, string>> font, string targetDirectory = @"C:\temp\FontFolder\")
        {
            List<string> listResult = new List<string>();
            List<FontItem> fontItemList = new List<FontItem>();
            ProcessStartInfo objProcess = new ProcessStartInfo();
            objProcess.UseShellExecute = true;
            objProcess.RedirectStandardOutput = false;

            var rect = new User32.Rect();

            var i = 0;
            var counter = 0;

            var fileName = "";

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
                }

                if (process.MainWindowTitle.Contains("(TrueType)"))
                {
                    if (process.MainWindowTitle == "Arial Unicode MS (TrueType)" || process.MainWindowTitle == "Arial Black (TrueType)")
                        Console.WriteLine();

                    font.Add(Tuple.Create(directoryList[j], process.MainWindowTitle.Replace("(TrueType)", "")));
                    listResult.Add(process.MainWindowTitle.Replace("(TrueType)", ""));

                    fileName = RemoveBadCharacters(process.MainWindowTitle.Replace("(TrueType)", ""));

                    if (fileName.Length > 20)
                    {
                        if (File.Exists($@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}.jpeg"))
                        {
                            //save new version of the font
                            do
                            {
                                counter++;
                            } while (File.Exists($@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}[{counter}]"));
                           
                            bmp.Save($@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}[{counter}].jpeg",
                                ImageFormat.Jpeg);

                            var item = new FontItem(directoryList[j], fileName,
                                $@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}[{counter}].jpeg");

                            counter = 0;    //reset counter
                        }
                        else
                        {
                            bmp.Save($@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}.jpeg",
                                ImageFormat.Jpeg);

                            var item = new FontItem(directoryList[j], fileName,
                                $@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}.jpeg");
                            fontItemList.Add(item);
                        }
                    }
                    else
                    {
                        if (File.Exists($@"C:\Users\Erik May\Documents\Images\{fileName}.jpeg"))
                        {
                            do
                            {
                                counter++;
                            } while (File.Exists($@"C:\Users\Erik May\Documents\Images\{fileName}[{counter}].jpeg"));
                            
                            bmp.Save($@"C:\Users\Erik May\Documents\Images\{fileName}[{counter}].jpeg");

                            counter = 0;

                            var item = new FontItem(directoryList[j], fileName,
                                $@"C:\Users\Erik May\Documents\Images\{fileName}[{counter}].jpeg");
                            fontItemList.Add(item);
                        }
                        else
                        {
                            bmp.Save($@"C:\Users\Erik May\Documents\Images\{fileName}.jpeg",
                                ImageFormat.Jpeg);

                            var item = new FontItem(directoryList[j], fileName,
                                $@"C:\Users\Erik May\Documents\Images\{fileName}.jpeg");
                            fontItemList.Add(item);
                        }
                    }
                }
                else if (process.MainWindowTitle.Contains("(OpenType)"))
                {
                    font.Add(Tuple.Create(directoryList[j], process.MainWindowTitle.Replace("(OpenType)", "")));
                    listResult.Add(process.MainWindowTitle.Replace("(OpenType)", ""));

                    fileName = RemoveBadCharacters(process.MainWindowTitle.Replace("(OpenType)", ""));

                    if (fileName.Length > 20)
                    {
                        if (File.Exists($@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}.jpeg"))
                        {
                            do
                            {
                                counter++;
                            } while (File.Exists($@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}[{counter}].jpeg"));

                            var item = new FontItem(directoryList[j], fileName,
                                $@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}[{counter}].jpeg");
                            fontItemList.Add(item);

                            bmp.Save($@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}[{counter}].jpeg");

                            counter = 0;
                        }
                        else
                        {
                            bmp.Save($@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}.jpeg",
                                ImageFormat.Jpeg);

                            var item = new FontItem(directoryList[j], fileName,
                                $@"C:\Users\Erik May\Documents\Images\{fileName.Substring(0, 20)}.jpeg");
                            fontItemList.Add(item);
                        }
                    }
                    else
                    {
                        bmp.Save($@"C:\Users\Erik May\Documents\Images\{fileName}.jpeg",
                           ImageFormat.Jpeg);
                        var item = new FontItem(directoryList[j], fileName,
                                $@"C:\Users\Erik May\Documents\Images\{fileName}.jpeg");
                        fontItemList.Add(item);
                    }
                }
                else
                {
                    if (process.MainWindowTitle.StartsWith(@"C:\Users\Erik May")) continue;
                    font.Add(Tuple.Create($"{directoryList[j].Replace(targetDirectory, @"G:\Apps\Fonts\")}", process.MainWindowTitle));
                    listResult.Add(process.MainWindowTitle);

                    fileName = RemoveBadCharacters(process.MainWindowTitle);

                    var item = new FontItem(directoryList[j].Replace(targetDirectory, @"G:\Apps\Fonts\"), fileName,
                        $@"C:\Users\Erik May\Documents\Images\{process.MainWindowTitle}.jpeg");
                    fontItemList.Add(item);
                    bmp.Save($@"C:\Users\Erik May\Documents\Images\{process.MainWindowTitle}.jpeg", ImageFormat.Jpeg);
                }

                process.Kill();
            }
            var thing = fontItemList;
            var result = fontItemList.Distinct();
            //var result = listResult.Distinct();
            return result;
        }
    }//1469469

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
