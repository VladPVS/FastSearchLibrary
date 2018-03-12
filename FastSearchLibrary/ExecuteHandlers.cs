using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    /// <summary>
    /// Determines where execute event handlers.
    /// </summary>
    public enum ExecuteHandlers
    {
        InCurrentThread = 0,
        InThreadPool = 1
    }
}
