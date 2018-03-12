using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchLibrary
{
    public class SearchEventArgs
    {
        public bool IsCanceled { get; private set; }

        public SearchEventArgs(bool isCanceled)
        {
            IsCanceled = isCanceled;
        }
    }
}
