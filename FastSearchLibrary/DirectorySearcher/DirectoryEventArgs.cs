using System;
using System.Collections.Generic;
using System.IO;

namespace FastSearchLibrary
{
    /// <summary>
    /// Provides data for DirectoriesFound event.
    /// </summary>
    public class DirectoryEventArgs: EventArgs
    {
        /// <summary>
        /// Gets a list of finding directories.
        /// </summary>
        public List<DirectoryInfo> Directories { get; private set;}

        /// <summary>
        /// Initialize a new instance of DirectoryEventArgs class that describes a FilesFound event.
        /// </summary>
        /// <param name="directories">The list of finding directories.</param>
        public DirectoryEventArgs(List<DirectoryInfo> directories)
        {
            Directories = directories;
        }
    }
}
