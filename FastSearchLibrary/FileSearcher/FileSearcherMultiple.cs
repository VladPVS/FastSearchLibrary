using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    /// <summary>
    /// Represents a class for fast file search in multiple directories.
    /// </summary>
    public class FileSearcherMultiple
    {
        private List<FileSearcherBase> searchers;

        private CancellationTokenSource tokenSource;

        private bool suppressOperationCanceledException;


        /// <summary>
        /// Event fires when next portion of files is found. Event handlers is not thread safe. 
        /// </summary>
        public event EventHandler<FileEventArgs> FilesFound
        {
            add
            {
                searchers.ForEach((s) => s.FilesFound += value);
            }

            remove
            {
                searchers.ForEach((s) => s.FilesFound -= value);
            }
        }


        /// <summary>
        /// Event fires when search process is completed or stopped. Event handlers is not thread safe.
        /// </summary>
        public event EventHandler<SearchCompletedEventArgs> SearchCompleted;



        /// <summary>
        /// Calls a SearchCompleted event.
        /// </summary>
        /// <param name="isCanceled">Determines whether search process canceled.</param>
        protected virtual void OnSearchCompleted(bool isCanceled)
        {
            EventHandler<SearchCompletedEventArgs> handler = SearchCompleted;

            if (handler != null)
            {
                var arg = new SearchCompletedEventArgs(isCanceled);

                SearchCompleted(this, arg);
            }
        }


        #region FileCancellationDelegateSearcher constructors

        /// <summary>
        /// Initializes a new instance of FileSearcherMultiple class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
        /// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcherMultiple(List<string> folders, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
        { 
            foreach (var folder in folders)
                CheckFolder(folder);

            CheckDelegate(isValid);

            CheckTokenSource(tokenSource);

            searchers = new List<FileSearcherBase>();

            this.suppressOperationCanceledException = suppressOperationCanceledException;

            foreach (var folder in folders)
            {
                searchers.Add(new FileCancellationDelegateSearcher(folder, isValid, tokenSource.Token, handlerOption, false));
            }
            
            this.tokenSource = tokenSource;
        }


        /// <summary>
        /// Initializes a new instance of FileSearcherMultiple class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcherMultiple(List<string> folders, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
            : this(folders, isValid, tokenSource, handlerOption, true)
        {
        }


        /// <summary>
        /// Initializes a new instance of FileSearcherMultiple class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcherMultiple(List<string> folders, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource)
            : this(folders, isValid, tokenSource, ExecuteHandlers.InCurrentTask, true)
        {
        }

        #endregion


        #region Checking methods

        private void CheckFolder(string folder)
        {
            if (folder == null)
                throw new ArgumentNullException("Argument \"folder\" is null.");

            if (folder == String.Empty)
                throw new ArgumentException("Argument \"folder\" is not valid.");

            DirectoryInfo dir = new DirectoryInfo(folder);

            if (!dir.Exists)
                throw new ArgumentException("Argument \"folder\" does not represent an existing directory.");
        }


        private void CheckPattern(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException("Argument \"pattern\" is null.");

            if (pattern == String.Empty)
                throw new ArgumentException("Argument \"pattern\" is not valid.");
        }


        private void CheckDelegate(Func<FileInfo, bool> isValid)
        {
            if (isValid == null)
                throw new ArgumentNullException("Argument \"isValid\" is null.");
        }


        private void CheckTokenSource(CancellationTokenSource tokenSource)
        {
            if (tokenSource == null)
                throw new ArgumentNullException("Argument \"tokenSource\" is null.");
        }


        #endregion


        /// <summary>
        /// Starts a file search operation with realtime reporting using several threads in thread pool.
        /// </summary>
        public void StartSearch()
        {
            try
            {
                searchers.ForEach(s =>
                {
                    s.StartSearch();
                });
            }
            catch(OperationCanceledException ex)
            {
                OnSearchCompleted(true);
                if (!suppressOperationCanceledException)
                    throw;
                return;
            }

            OnSearchCompleted(false);
        }


        /// <summary>
        /// Starts a file search operation with realtime reporting using several threads in thread pool as an asynchronous operation.
        /// </summary>
        public Task StartSearchAsync()
        {
             return Task.Run(() =>
             {
                  StartSearch();

             }, tokenSource.Token);      
        }


        /// <summary>
        /// Stops a file search operation.
        /// </summary>
        public void StopSearch()
        {
            tokenSource.Cancel();
        }

    }
}
