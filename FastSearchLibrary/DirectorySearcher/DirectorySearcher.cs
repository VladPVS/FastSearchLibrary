using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    /// <summary>
    /// Represents a class for fast directory searching.
    /// </summary>
    public class DirectorySearcher
    {
        #region Instance members

        private DirectoryCancellationSearcherBase searcher;

        private CancellationTokenSource tokenSource;


        /// <summary>
        /// Event fires when next portion of files is founded. Event handlers is not thread safe. 
        /// </summary>
        public event EventHandler<DirectoryEventArgs> DirectoriesFound
        {
            add
            {
                searcher.DirectoriesFound += value;
            }

            remove
            {
                searcher.DirectoriesFound -= value;
            }
        }


        /// <summary>
        /// Event fires when search process is completed or stopped. Event handlers is not thread safe.
        /// </summary>
        public event EventHandler<SearchEventArgs> SearchCompleted
        {
            add
            {
                searcher.SearchCompleted += value;
            }

            remove
            {
                searcher.SearchCompleted -= value;
            }
        }


        #region DirectoryCancellationPaternSearcher constructors

        /// <summary>
        /// Initialize a new instance of DirectorySearch class. 
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Determines where execute event handlers.</param>
        /// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DirectorySearcher(string folder, string pattern, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
        {
            CheckFolder(folder);

            CheckPattern(pattern);

            CheckTokenSource(tokenSource);

            searcher = new DirectoryCancellationPatternSearcher(folder, pattern, tokenSource.Token, handlerOption, suppressOperationCanceledException);
            this.tokenSource = tokenSource;
        }


        /// <summary>
        /// Initialize a new instance of DirectorySearch class. 
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Determines where execute event handlers.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DirectorySearcher(string folder, string pattern, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
            : this (folder, pattern, tokenSource, handlerOption, true)
        {
        }


        /// <summary>
        /// Initialize a new instance of DirectorySearch class. 
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DirectorySearcher(string folder, string pattern, CancellationTokenSource tokenSource)
            : this (folder, pattern, tokenSource, ExecuteHandlers.InCurrentThread, true)
        {
        }


        /// <summary>
        /// Initialize a new instance of DirectorySearch class. 
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DirectorySearcher(string folder, CancellationTokenSource tokenSource)
            : this (folder, "*", tokenSource, ExecuteHandlers.InCurrentThread, true)
        {

        }

        #endregion


        #region DirectoryCancellationDelegateSearcher constructors

        /// <summary>
        /// Initialize a new instance of DirectorySearch class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Determines where execute event handlers.</param>
        /// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DirectorySearcher(string folder, Func<DirectoryInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
        {
            CheckFolder(folder);

            CheckDelegate(isValid);

            CheckTokenSource(tokenSource);

            searcher = new DirectoryCancellationDelegateSearcher(folder, isValid, tokenSource.Token, handlerOption, suppressOperationCanceledException);
            this.tokenSource = tokenSource;
        }


        /// <summary>
        /// Initialize a new instance of DirectorySearch class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Determines where execute event handlers.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DirectorySearcher(string folder, Func<DirectoryInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
            : this(folder, isValid, tokenSource, ExecuteHandlers.InCurrentThread, true)
        {
        }


        /// <summary>
        /// Initialize a new instance of DirectorySearch class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DirectorySearcher(string folder, Func<DirectoryInfo, bool> isValid, CancellationTokenSource tokenSource)
            : this(folder, isValid, tokenSource, ExecuteHandlers.InCurrentThread, true)
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


        private void CheckDelegate(Func<DirectoryInfo, bool> isValid)
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
        /// Starts a directory search operation with realtime reporting using several threads in thread pool.
        /// </summary>
        public void StartSearch()
        {
            searcher.StartSearch();
        }


        /// <summary>
        /// Starts a directory search operation with realtime reporting using several threads in thread pool as an asyncchronous operation.
        /// </summary>
        public Task StartSearchAsync()
        {
            return Task.Run(() =>
            {
                StartSearch();

            }, tokenSource.Token);
        }

        /// <summary>
        /// Stops a directory search operation.
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        public void StopSearch()
        {
            this.tokenSource.Cancel();
        }

        #endregion


        #region Static members

        #region Public members

        /// <summary>
        /// Returns a list of directories that are contained in directory and all subdirectories.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <returns>List of finding directories.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        static public List<DirectoryInfo> GetDirectories(string folder, string pattern = "*")
        {
            List<DirectoryInfo> directories = new List<DirectoryInfo>();
            GetDirectories(folder, directories, pattern);

            return directories;
        }



        /// <summary>
        /// Returns a list of directories that are contained in directory and all subdirectories.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <returns>List of finding directories.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        static public List<DirectoryInfo> GetDirectories(string folder, Func<DirectoryInfo, bool> isValid)
        {
            List<DirectoryInfo> directories = new List<DirectoryInfo>();
            GetDirectories(folder, directories, isValid);

            return directories;
        }



        /// <summary>
        /// Returns a list of directories that are contained in directory and all subdirectories as an asynchronous operation.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        static public Task<List<DirectoryInfo>> GetDirectoriesAsync(string folder, string pattern = "*")
        {
            return Task.Run<List<DirectoryInfo>>(() =>
            {
                return GetDirectories(folder, pattern);
            });
        }



        /// <summary>
        /// Returns a list of directories that are contained in directory and all subdirectories as an asynchronous operation.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        static public Task<List<DirectoryInfo>> GetDirectoriesAsync(string folder, Func<DirectoryInfo, bool> isValid)
        {
            return Task.Run<List<DirectoryInfo>>(() =>
            {
                return GetDirectories(folder, isValid);
            });
        }



        /// <summary>
        /// Returns a list of directories that are contained in directory and all subdirectories using several threads in thread pool.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <returns>List of finding directories.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        static public List<DirectoryInfo> GetDirectoriesFast(string folder, string pattern = "*")
        {
            ConcurrentBag<DirectoryInfo> dirs = new ConcurrentBag<DirectoryInfo>();

            List<DirectoryInfo> startDirs = GetStartDirectories(folder, dirs, pattern);

            startDirs.AsParallel().ForAll((d) =>
            {
                GetStartDirectories(d.FullName, dirs, pattern).AsParallel().ForAll((dir) =>
                {
                    GetDirectories(dir.FullName, pattern).ForEach((r) => dirs.Add(r));
                });
            });

            return dirs.ToList();
        }



        /// <summary>
        /// Returns a list of directories that are contained in directory and all subdirectories using several threads in thread pool.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
        /// <returns>List of finding directories.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        static public List<DirectoryInfo> GetDirectoriesFast(string folder, Func<DirectoryInfo, bool> isValid)
        {
            ConcurrentBag<DirectoryInfo> dirs = new ConcurrentBag<DirectoryInfo>();

            List<DirectoryInfo> startDirs = GetStartDirectories(folder, dirs, isValid);

            startDirs.AsParallel().ForAll((d) =>
            {
                GetStartDirectories(d.FullName, dirs, isValid).AsParallel().ForAll((dir) =>
                {
                    GetDirectories(dir.FullName, isValid).ForEach((r) => dirs.Add(r));
                });
            });

            return dirs.ToList();
        }



        /// <summary>
        /// Returns a list of directories that are contained in directory and all subdirectories using several threads in thread pool as an asynchronous operation.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        static public Task<List<DirectoryInfo>> GetDirectoriesFastAsync(string folder, string pattern = "*")
        {
            return Task.Run<List<DirectoryInfo>>(() =>
            {
                return GetDirectoriesFast(folder, pattern);
            });
        }



        /// <summary>
        /// Returns a list of directories that are contained in directory and all subdirectories using several threads in thread pool as an asynchronous operation.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        static public Task<List<DirectoryInfo>> GetDirectoriesFastAsync(string folder, Func<DirectoryInfo, bool> isValid)
        {
            return Task.Run<List<DirectoryInfo>>(() =>
            {
                return GetDirectoriesFast(folder, isValid);
            });
        }

        #endregion


        #region Private members

        static private void GetDirectories(string folder, List<DirectoryInfo> result, string pattern)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;

            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                if (directories.Length == 0) return;
            }
            catch (UnauthorizedAccessException ex)
            {
                return;
            }
            catch (PathTooLongException ex)
            {
                return;
            }

            Array.ForEach(directories, (d) => GetDirectories(d.FullName, result, pattern));

            try
            {
                Array.ForEach(dirInfo.GetDirectories(pattern), (d) => result.Add(d));
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            catch (PathTooLongException ex)
            {
            }
        }



        static private void GetDirectories(string folder, List<DirectoryInfo> result, Func<DirectoryInfo, bool> isValid)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;

            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                if (directories.Length == 0) return;
            }
            catch (UnauthorizedAccessException ex)
            {
                return;
            }
            catch (PathTooLongException ex)
            {
                return;
            }

            Array.ForEach(directories, (d) => GetDirectories(d.FullName, result, isValid));

            try
            {
                Array.ForEach(dirInfo.GetDirectories(), (d) =>
                {
                    if (isValid(d))
                        result.Add(d);
                });
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            catch (PathTooLongException ex)
            {
            }
        }



        static private List<DirectoryInfo> GetStartDirectories(string folder, ConcurrentBag<DirectoryInfo> dirs, string pattern)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;
            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                if (directories.Length > 1)
                {
                    Array.ForEach(dirInfo.GetDirectories(pattern), (d) => dirs.Add(d));
                    return new List<DirectoryInfo>(directories);
                }

                if (directories.Length == 0)
                    return new List<DirectoryInfo>();

            }
            catch (UnauthorizedAccessException ex)
            {
                return new List<DirectoryInfo>();
            }
            catch (PathTooLongException ex)
            {
                return new List<DirectoryInfo>();
            }

            // if directories.Length == 1
            Array.ForEach(dirInfo.GetDirectories(pattern), (d) => dirs.Add(d));

            return GetStartDirectories(directories[0].FullName, dirs, pattern);
        }



        static private List<DirectoryInfo> GetStartDirectories(string folder, ConcurrentBag<DirectoryInfo> dirs, Func<DirectoryInfo, bool> isValid)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;
            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                if (directories.Length > 1)
                {
                    Array.ForEach(directories, (d) =>
                    {
                        if (isValid(d))
                            dirs.Add(d);
                    });

                    return new List<DirectoryInfo>(directories);
                }

                if (directories.Length == 0)
                    return new List<DirectoryInfo>();

            }
            catch (UnauthorizedAccessException ex)
            {
                return new List<DirectoryInfo>();
            }
            catch (PathTooLongException ex)
            {
                return new List<DirectoryInfo>();
            }

            // if directories.Length == 1
            Array.ForEach(directories, (d) =>
            {
                if (isValid(d))
                    dirs.Add(d);
            });

            return GetStartDirectories(directories[0].FullName, dirs, isValid);
        }


        #endregion

        #endregion
    }
}
