//  System
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//  Logic
using NickScotney.FileSync.Logic.Helpers;
using NickScotney.FileSync.Logic.Models;

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
            var destinationFolders = new DirectoryInfo(destinationDir)
                 .GetDirectories()
                 .OrderByDescending(fi => fi.LastWriteTime)
                 .ToList();
            //  Get a list of source files, ordered by modified descending.
            var sourceFiles = new DirectoryInfo(sourceDir)
                .GetFiles()
                .OrderByDescending(fi => fi.LastWriteTime)
                .ToList();
            //  Get a list of source folders, ordered by modified descending
            var sourceFolders = new DirectoryInfo(sourceDir)
                .GetDirectories()
                .OrderByDescending(fi => fi.LastWriteTime)
                .ToList();

            //  List of combined files and folders (Destination)
            List<SourceObject> destinationObjects = CreateSourceList(destinationFolders, destinationFiles);

            //  List of combined files and folders (source)
            List<SourceObject> sourceObjects = CreateSourceList(sourceFolders, sourceFiles);

            long runningDirectorySize = 0;

            //  Do we have any files?
            if (destinationObjects.Count() == 0)
            {
                LogToConsole("Destination is empty - Begining File Copy");

                //  Loop the source files heres
                foreach (var sourceObject in sourceObjects)
                {
                    //  If adding the current file will go over our space limit, quit
                    if ((runningDirectorySize + sourceObject.SourceObjectLength) >= maxDirSize)
                        break;

                    if (sourceObject.SourceObjectType == SourceObjectType.File)
                        //  Copy File here
                        File.Copy(sourceObject.SourceObjectPath, Path.Combine(destinationDir, sourceObject.SourceObjectName));
                    else
                    {
                        //  Create Folder
                        Directory.CreateDirectory(Path.Combine(destinationDir, sourceObject.SourceObjectName));

                        //  Copy folder contents
                        foreach (var file in new DirectoryInfo(sourceObject.SourceObjectPath).GetFiles())
                            File.Copy(file.FullName, Path.Combine(Path.Combine(destinationDir, sourceObject.SourceObjectName), file.Name));
                    }
                    runningDirectorySize += sourceObject.SourceObjectLength;

                    LogToConsole($"    Copied {(sourceObject.SourceObjectType == SourceObjectType.File ? "File" : "Folder")} {sourceObject.SourceObjectName}");
                }

                return;
            }

            //  Loop source files, to get a list to fit the file size requirements
            List<SourceObject> filteredSourceFiles = new List<SourceObject>();

            LogToConsole("Generating Latest Files List: ");

            foreach (var sourceFile in sourceObjects)
            {
                //  If adding the current file will go over our space limit, quit
                if ((runningDirectorySize + sourceFile.SourceObjectLength) >= maxDirSize)
                    break;

                //  Add file to the filtered list
                filteredSourceFiles.Add(sourceFile);
                runningDirectorySize += sourceFile.SourceObjectLength;

                LogToConsole($"   File Included: {sourceFile.SourceObjectName}");
            }

            LogToConsole("Looking for Files / Folders to Delete: ");

            foreach (var destinationFile in destinationObjects)
            {
                //  If it doesn't exist in the filtered list, delete it
                if (filteredSourceFiles.Count(fi => fi.SourceObjectName == destinationFile.SourceObjectName) == 0)
                {
                    if (destinationFile.SourceObjectType == SourceObjectType.File)
                        File.Delete(destinationFile.SourceObjectPath);
                    else
                    {
                        //  Delete Files
                        foreach (var file in new DirectoryInfo(destinationFile.SourceObjectPath).GetFiles())
                            File.Delete(file.FullName);

                        //  Delete Folder
                        Directory.Delete(destinationFile.SourceObjectPath);
                    }

                    LogToConsole($"    {(destinationFile.SourceObjectType == SourceObjectType.File ? "File" : "Folder")} Deleted from Destination: {destinationFile.SourceObjectName}");
                }
            }

            LogToConsole("Begin Copy Process: ");

            //  Copy over any files which aren't in destination
            foreach (var sourceFile in filteredSourceFiles)
            {
                //  Does the file exist in the destination?
                if (destinationObjects.Count(fi => fi.SourceObjectName == sourceFile.SourceObjectName) == 0)
                {
                    if (sourceFile.SourceObjectType == SourceObjectType.File)
                        File.Copy(sourceFile.SourceObjectPath, Path.Combine(destinationDir, sourceFile.SourceObjectName));
                    else
                    {
                        //  Create Folder
                        Directory.CreateDirectory(Path.Combine(destinationDir, sourceFile.SourceObjectName));

                        //  Copy folder contents
                        foreach (var file in new DirectoryInfo(sourceFile.SourceObjectPath).GetFiles())
                            File.Copy(file.FullName, Path.Combine(Path.Combine(destinationDir, sourceFile.SourceObjectName), file.Name));
                    }

                    LogToConsole($"    Copied {(sourceFile.SourceObjectType == SourceObjectType.File ? "File" : "Folder")} {sourceFile.SourceObjectName}");
                }
            }
        }


        List<SourceObject> CreateSourceList(List<DirectoryInfo> folderList, List<FileInfo> fileList)
        {
            List<SourceObject> sourceObjects = new List<SourceObject>();

            if ((fileList != null) && fileList.Count > 0)
                foreach (var sourceFile in fileList)
                    sourceObjects.Add(new SourceObject(sourceFile));

            if ((folderList != null) && folderList.Count > 0)
                foreach (var sourceFolder in folderList)
                    sourceObjects.Add(new SourceObject(sourceFolder));

            if (sourceObjects.Count > 0)
                sourceObjects = sourceObjects
                    .OrderByDescending(so => so.SourceModifiedDate)
                    .ToList();

            return sourceObjects;
        }

        void LogToConsole(string logLine)
        {
            Console.WriteLine($"{DateTime.Now}: {logLine}");
            FileHelper.WriteToLogFile(!String.IsNullOrEmpty(logLine) ? $"{DateTime.Now}: {logLine}" : logLine);
        }
    }
}
