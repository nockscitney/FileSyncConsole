//  System
using System;
using System.IO;
using System.Linq;

namespace NickScotney.FileSync.Logic.Models
{
    public class SourceObject
    {
        public DateTime SourceModifiedDate { get; set; }
        public long SourceObjectLength { get; set; }
        public SourceObjectType SourceObjectType { get; set; }
        public string SourceObjectName { get; set; }
        public string SourceObjectPath { get; set; }

        public SourceObject()
            : base()
        {
            SourceModifiedDate = DateTime.MinValue;
            SourceObjectLength = 0;
            SourceObjectType = SourceObjectType.None;
        }

        public SourceObject(FileInfo fileInfo)
            : this()
        {
            FromFileInfo(fileInfo);
        }

        public SourceObject(DirectoryInfo folderInfo)
            : this()
        {
            FromFolderInfo(folderInfo);
        }


        void FromFileInfo(FileInfo fileInfo)
        {
            SourceObjectLength = fileInfo.Length;
            SourceObjectName = fileInfo.Name;
            SourceObjectPath = fileInfo.FullName;
            SourceModifiedDate = fileInfo.LastWriteTime;
            SourceObjectType = SourceObjectType.File;
        }

        void FromFolderInfo(DirectoryInfo folderInfo)
        {
            SourceObjectLength = folderInfo
                .GetFiles()
                .ToList()
                .Sum(fi => fi.Length);
            SourceObjectName = folderInfo.Name;
            SourceObjectPath = folderInfo.FullName;
            SourceModifiedDate = folderInfo.LastWriteTime;
            SourceObjectType = SourceObjectType.Folder;
        }
    }

    public enum SourceObjectType
    {
        None,
        File,
        Folder
    }
}
