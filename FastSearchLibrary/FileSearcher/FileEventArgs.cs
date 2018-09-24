using System;
using System.Collections.Generic;
using System.IO;

namespace FastSearchLibrary
{
    /// <summary>
    /// Provides data for FilesFound event.
    /// </summary>
    public class FileEventArgs: EventArgs
    {
         /// <summary>
         /// Gets a list of finding files.
         /// </summary>
         public List<FileInfo> Files { get; private set; }

         /// <summary>
         /// Initialize a new instance of FileEventArgs class that describes a FilesFound event.
         /// </summary>
         /// <param name="files">The list of finding files.</param>
         public FileEventArgs(List<FileInfo> files)
         {
             Files = files;
         }
    }
}
