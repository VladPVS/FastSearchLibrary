using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    public class DirectoryEventArgs: EventArgs
    {
        public List<DirectoryInfo> Directories { get; private set;}

        public DirectoryEventArgs(List<DirectoryInfo> directories)
        {
            Directories = directories;
        }
    }
}
