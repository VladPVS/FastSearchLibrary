using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    /// <summary>
    /// Determines where event handlers execute.
    /// </summary>
    public enum ExecuteHandlers
    {
        /// <summary>
        /// To execute event handlers in current thread. 
        /// </summary>
        InCurrentThread = 0,

        /// <summary>
        /// To execute event handlers in some thread from thread pool.
        /// </summary>
        InThreadPool = 1
    }
}
