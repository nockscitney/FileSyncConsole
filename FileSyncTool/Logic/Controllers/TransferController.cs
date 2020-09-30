//  System
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//  Logic
using NickScotney.FileSync.Logic.Helpers;

namespace NickScotney.FileSync.Logic.Controllers
{
    public class TransferController
    {
        //  Variables
        long maxDirSize;
        string destinationDir, sourceDir;

        //  Properties
        public long MaxDirSize
        {
            set { maxDirSize = value; }
        }
        public string DestinationDir
        {
            set { destinationDir = value; }
        }
        public string SourceDir
        {
            set { sourceDir = value; }
        }

        //  Constructors
        public TransferController()
            : base()
        {
            maxDirSize = 0;
        }

        public TransferController(long maxDirSize,string destinationDir, string sourceDir)
            : this()
        {
            this.destinationDir = destinationDir;
            this.maxDirSize = maxDirSize;
            this.sourceDir = sourceDir;
        }


        //  Methods
        public void SyncFolders()
        {
            //  One or both of the directories don't exist
            if ((!Directory.Exists(sourceDir))
                || !Directory.Exists(destinationDir))
                return;
            
            //  Get a list of destination files
            var destinationFiles = new DirectoryInfo(destinationDir)
                .GetFiles()
                .ToList();
            //  Get a list of source files, ordered by modified descending.
            var sourceFiles = new DirectoryInfo(sourceDir)
                .GetFiles()
                .OrderByDescending(fi => fi.LastWriteTime)
                .ToList();
            
            long runningDirectorySize = 0;

            //  Do we have any files?
            if ((destinationFiles == null)
                || destinationFiles.Count() == 0)
            {
                LogToConsole("Destination is empty - Begining File Copy");

                //  Loop the source files heres
                foreach (var sourceFile in sourceFiles)
                {
                    //  If adding the current file will go over our space limit, quit
                    if ((runningDirectorySize + sourceFile.Length) >= maxDirSize)
                        break;

                    //  Copy File here
                    File.Copy(sourceFile.FullName, Path.Combine(destinationDir, sourceFile.Name));
                    runningDirectorySize += sourceFile.Length;

                    LogToConsole($"    Copied File {sourceFile.Name}");
                }

                return;
            }

            destinationFiles.OrderByDescending(fi => fi.LastWriteTime)
                .ToList();

            //  Loop source files, to get a list to fit the file size requirements
            List<FileInfo> filteredSourceFiles = new List<FileInfo>();

            LogToConsole("Generating Latest Files List: ");

            foreach (var sourceFile in sourceFiles)
            {
                //  If adding the current file will go over our space limit, quit
                if ((runningDirectorySize + sourceFile.Length) >= maxDirSize)
                    break;

                //  Add file to the filtered list
                filteredSourceFiles.Add(sourceFile);
                runningDirectorySize += sourceFile.Length;

                LogToConsole($"   File Included: {sourceFile.Name}");
            }

            LogToConsole("Looking for Files to Delete: ");

            //  Check for any files which are in the destination, but not in the source
            foreach (var destinationFile in destinationFiles)
            {
                //  If it doesn't exist in the filtered list, delete it
                if (filteredSourceFiles.Count(fi => fi.Name == destinationFile.Name) == 0)
                {
                    File.Delete(destinationFile.FullName);
                    LogToConsole($"    File Deleted from Destination: {destinationFile.Name}");
                }
            }

            LogToConsole("Begin Copy Process: ");

            //  Copy over any files which aren't in destination
            foreach (var sourceFile in filteredSourceFiles)
            {
                //  Does the file exist in the destination?
                if (destinationFiles.Count(fi => fi.Name == sourceFile.Name) == 0)
                {
                    File.Copy(sourceFile.FullName, Path.Combine(destinationDir, sourceFile.Name));
                    LogToConsole($"    Copied File {sourceFile.Name}");
                }
            }
        }

        void LogToConsole(string logLine)
        {
            Console.WriteLine($"{DateTime.Now}: {logLine}");
            FileHelper.WriteToLogFile(!String.IsNullOrEmpty(logLine) ? $"{DateTime.Now}: {logLine}" : logLine);
        }
    }
}
