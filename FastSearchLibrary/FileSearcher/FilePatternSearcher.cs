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
    internal class FilePatternSearcher : FileSearcherBase
    {

        private string pattern;

        public FilePatternSearcher(string folder, string pattern, ExecuteHandlers handlerOption): base(folder, handlerOption)
        {
            this.pattern = pattern;
        }
        

        public FilePatternSearcher(string folder, string pattern): this(folder, pattern, ExecuteHandlers.InCurrentThread)
        {
        }


        public FilePatternSearcher(string folder): this(folder, "*", ExecuteHandlers.InCurrentThread)
        {
        }



        /// <summary>
        /// Starts a file search operation with realtime reporting using several threads in thread pool.
        /// </summary>
        public override void StartSearch()
        {
             GetFilesFast();
        }



        protected override void GetFiles(string folder)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;

            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                if (directories.Length == 0)
                {
                    var resFiles = dirInfo.GetFiles(pattern);
                    if (resFiles.Length > 0)
                        OnFilesFound(resFiles.ToList());
                    return;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return;
            }
            catch (PathTooLongException ex)
            {
                return;
            }

            foreach (var d in directories)
            {
                GetFiles(d.FullName);
            }

            try
            {
                var resFiles = dirInfo.GetFiles(pattern);
                if (resFiles.Length > 0)
                    OnFilesFound(resFiles.ToList());
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            catch (PathTooLongException ex)
            {
            }
        }



        protected override List<DirectoryInfo> GetStartDirectories(string folder)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;
            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                var resFiles = dirInfo.GetFiles(pattern);
                if (resFiles.Length > 0)
                    OnFilesFound(resFiles.ToList());

                if (directories.Length > 1)
                    return new List<DirectoryInfo>(directories);

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

            return GetStartDirectories(directories[0].FullName);
        }


    }
}
