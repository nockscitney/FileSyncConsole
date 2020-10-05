# FileSyncConsole
Sync files between 2 folders, limited to a maximum folder size, with the newest files being synced.

Current Version: 1.1.0.0 - https://github.com/nockscitney/FileSyncConsole/releases/tag/1.1.0.0

### About
This was a project I worked on for a friend, who wanted a way to automatically sync the latest x GB of data from a source folder, to a removable drive.  They also wanted only the lastest items and while they could do this manually, they asked if I could come up with an automatic method.  File Sync Console does just that.

### Requirements
- Windows Operating System
- Microsoft .NET Framework 4.7.2 (or Greater)

### Setup
In order for the application to function, there are some settings which need to be configured.  These settings can be found in Settings.json

- SourceDir: Folder path to folder where the source files reside
- DestinationDir: Folder path to the folder where the files need to be synced
- MaxDestinationSize: Maximum amount of data we want to send to the destination folder
- FileSizeUnit: Specifies the file unit used in the MaxDestinationSize setting. Allowed values are 'B', 'KB' 'MB' and 'GB'
- LogFileName: Name of the log file which will record all the actions during the sync operation