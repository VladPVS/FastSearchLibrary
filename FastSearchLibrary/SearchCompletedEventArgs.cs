using System;

namespace FastSearchLibrary
{
    /// <summary>
    /// Provides data for SearchCompleted event.
    /// </summary>
    public class SearchCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets whether this search process has completed due cancellation.
        /// </summary>
        public bool IsCanceled { get; private set; }

        /// <summary>
        /// Initialize a new instance of SearchCompletedEventArgs class that describes a SearchCompleted event.
        /// </summary>
        /// <param name="isCanceled">Determines whether search process canceled.</param>
        public SearchCompletedEventArgs(bool isCanceled)
        {
            IsCanceled = isCanceled;
        }
    }
}
