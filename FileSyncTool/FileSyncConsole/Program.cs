//  System
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//  Logic
using NickScotney.FileSync.Logic.Controllers;
using NickScotney.FileSync.Logic.Helpers;
using NickScotney.FileSync.Logic.Models;

namespace NickScotney.FileSync.MovieSyncConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //  Set the folder path for the application
            FileHelper.FolderPath = Environment.CurrentDirectory;

            //  Load Our Settings File
            var settingsList = FileHelper.LoadSettings("Settings.json");

            //  ERROR - No settings were loaded from the settings file
            if ((settingsList == null) 
                || settingsList.Count == 0)
            {
                LogToConsole("ERROR: Unable to load settings - Does the file Exist?");
                Finish();
                return;
            }

            //  Set program variables here
            var destinationDir = LoadSettingValue<string>(settingsList, "DestinationDir");
            var maxDirSize = LoadSettingValue<long>(settingsList, "MaxDestinationSize");
            var sourceDir = LoadSettingValue<string>(settingsList, "SourceDir");
            //  Get the current folder sizes of Source and Destination
            var destinationSize = FileHelper.CalculateFolderSize(destinationDir);
            var sourceSize = FileHelper.CalculateFolderSize(sourceDir);

            //  Set the log file name
            FileHelper.LogFileName = LoadSettingValue<string>(settingsList, "LogFileName");

            //  Clear the log file
            FileHelper.WriteToLogFile(String.Empty, true);

            //  Output Debug Values to Console
            LogToConsole($"Max Dir Size: {FileHelper.ConvertFromByte(maxDirSize, "MB")} MB");
            LogToConsole(String.Empty);
            LogToConsole($"Destination Dir: {destinationDir}");
            LogToConsole($"Destination Dir Size: {FileHelper.ConvertFromByte(destinationSize, "MB")} MB");
            LogToConsole(String.Empty);
            LogToConsole($"Source Dir: {sourceDir}");
            LogToConsole($"Source Dir Size: {FileHelper.ConvertFromByte(sourceSize, "MB")} MB");

            Thread.Sleep(5000);

            LogToConsole(String.Empty);
            LogToConsole("Begin File Sync Operation");
            LogToConsole(String.Empty);

            TransferController transferController = new TransferController(maxDirSize, destinationDir, sourceDir);
            transferController.SyncFolders();

            LogToConsole(String.Empty);

            //  We're done here, finish up
            Finish();
        }


        static void Finish()
        { 
            //  Finish up Here
            Console.WriteLine(String.Empty);
            Console.WriteLine("Press any key . . . ");
            Console.ReadKey();            
        }

        static T LoadSettingValue<T>(List<Setting> settingList, string settingName)
        {
            var setting = settingList
                .First(set => set.SettingName == settingName);

            if (setting == null)
                return default(T);

            return (T)Convert.ChangeType(setting.SettingValue, typeof(T));
        }

        static void LogToConsole(string logLine)
        { 
            Console.WriteLine($"{DateTime.Now}: {logLine}");
            FileHelper.WriteToLogFile(!String.IsNullOrEmpty(logLine) ? $"{DateTime.Now}: {logLine}" : logLine);
        }
    }
}