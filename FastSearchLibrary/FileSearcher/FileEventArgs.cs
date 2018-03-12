using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    public class FileEventArgs: EventArgs
    {
         public List<FileInfo> Files { get; private set; }
         
         public FileEventArgs(List<FileInfo> files)
         {
             Files = files;
         }
    }
}
