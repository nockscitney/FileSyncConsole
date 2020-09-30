//  System
using System;
using System.Collections.Generic;
using System.IO;
//  Logic
using NickScotney.FileSync.Logic.Models;
//  Newtonsoft
using Newtonsoft.Json;

namespace NickScotney.FileSync.Logic.Helpers
{
    public class FileHelper
    {
        public static string FolderPath { private get; set; }
        public static string  LogFileName 
        {   private get;
            set;
        }


        public static long CalculateFolderSize(string folderPath)
        {
            //  Does the folder path exist?
            if (!Directory.Exists(folderPath))
                return long.MinValue;

            //  Declare working variables
            var directoryFiles = Directory.GetFiles(folderPath);
            var runningFileSize = (long)0;

            //  If there are child files
            if ((directoryFiles != null)
                && directoryFiles.Length >= 0)
                //  Loop to get total file size
                foreach (var fileName in directoryFiles)
                {
                    runningFileSize += new FileInfo(fileName).Length;
                    //Console.WriteLine($"{fileName} | Size: {new FileInfo(fileName).Length}");
                }

            //  Return the calculated value
            return runningFileSize;
        }

        public static long ConvertFromByte(long totalFileSize, string unit)
        {
            switch (unit)
            {
                case "KB":
                    return (totalFileSize / 1024);

                case "MB":
                    return ((totalFileSize / 1024) / 1024);

                case "GB":
                    return (((totalFileSize / 1024) / 1024) / 1024);

                default:
                    return totalFileSize;
            }
        }

        public static List<Setting> LoadSettings(string fileName)
        {
            //  Variable to hold full file path
            var fullFilePath = String.Empty;

            //  Perform file sanity checks
            if (String.IsNullOrEmpty((fullFilePath = FileChecks(fileName))))
                return null;

            //  Define our settings list
            var settingsList = new List<Setting>();

            //  Does the file exist
            if (!File.Exists(fullFilePath))
                File.Create(fullFilePath);

            //  Read the json to a string
            using (StreamReader sr = new StreamReader(fullFilePath))
            {
                var fileContents = sr.ReadToEnd();

                //  Deserialize the string to the object
                if (!String.IsNullOrEmpty(fileContents))
                    settingsList = JsonConvert.DeserializeObject<List<Setting>>(fileContents);

                sr.Close();
            }

            return settingsList;
        }

        public static void SaveSettings(List<Setting> settingsList, string fileName)
        {
            //  Variable to hold full file path
            var fullFilePath = String.Empty;

            //  Perform file sanity checks
            if (String.IsNullOrEmpty((fullFilePath = FileChecks(fileName))))
                return;

            //  Write the json to the file
            using (StreamWriter sw = new StreamWriter(fullFilePath, false))
            {
                sw.Write(JsonConvert.SerializeObject(settingsList, Formatting.Indented));
                sw.Close();
            }
        }

        public static void WriteToLogFile(string logFileLine, bool clearFile = false)
        {
            //  Variable to hold full file path
            var fullFilePath = String.Empty;

            //  Perform file sanity checks
            if (String.IsNullOrEmpty((fullFilePath = FileChecks(LogFileName, false))))
                return;

            try
            {
                if(clearFile)
                {
                    File.Delete(fullFilePath);
                    return;
                }

                //  Open the file in Write only mode
                using (StreamWriter sw = new StreamWriter(File.Open(fullFilePath, FileMode.Append, FileAccess.Write)))
                {
                    //  Write each line to the file                    
                    sw.WriteLine(logFileLine);

                    //  Close the file
                    sw.Close();
                }
            }
            catch { }
        }


        static string FileChecks(string fileName, bool createNew = true)
        {
            //  Check if we have file name
            if (String.IsNullOrEmpty(fileName))
                return String.Empty;

            //  Do we have a folder path
            if (String.IsNullOrEmpty(FolderPath))
                return String.Empty;

            //  Does the folder path exist
            if (!Directory.Exists(FolderPath))
                return String.Empty;

            var fullFilePath = Path.Combine(FolderPath, fileName);
            
            //  Does the file exist
            if(createNew)
                if (!File.Exists(fullFilePath))
                    File.Create(fullFilePath);

            return fullFilePath;
        }
    }
}
