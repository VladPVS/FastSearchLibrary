using System;
using System.Collections.Generic;
using System.IO;

namespace FastSearchLibrary
{
    internal class FileDelegateSearcher: FileSearcherBase
    {

        private Func<FileInfo, bool> isValid = null;

        public FileDelegateSearcher(string folder, Func<FileInfo, bool> isValid, ExecuteHandlers handlerOption): base(folder, handlerOption)
        {
            this.isValid = isValid;
        }


        public FileDelegateSearcher(string folder, Func<FileInfo, bool> isValid): this(folder, isValid, ExecuteHandlers.InCurrentTask)
        {
        }


        public FileDelegateSearcher(string folder): this(folder, (arg) => true, ExecuteHandlers.InCurrentTask)
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
            List<FileInfo> resultFiles = new List<FileInfo>();

            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                if (directories.Length == 0)
                {
                    FileInfo[] files = dirInfo.GetFiles();

                    foreach (var file in files)
                        if (isValid(file))
                            resultFiles.Add(file);

                    if (resultFiles.Count > 0)
                        OnFilesFound(resultFiles);

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
            catch (DirectoryNotFoundException ex)
            {
                return;
            }

            foreach (var d in directories)
            {
                GetFiles(d.FullName);
            }

            try
            {
                var files = dirInfo.GetFiles();

                foreach (var file in files)
                    if (isValid(file))
                        resultFiles.Add(file);

                if (resultFiles.Count > 0)
                    OnFilesFound(resultFiles);
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            catch (PathTooLongException ex)
            {
            }
            catch (DirectoryNotFoundException ex)
            {
            }
        }


        protected override List<DirectoryInfo> GetStartDirectories(string folder)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;
            List<FileInfo> resultFiles = new List<FileInfo>();

            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                FileInfo[] files = dirInfo.GetFiles();

                foreach (var file in files)
                    if (isValid(file))
                        resultFiles.Add(file);

                if (resultFiles.Count > 0)
                    OnFilesFound(resultFiles);

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
            catch (DirectoryNotFoundException ex)
            {
                return new List<DirectoryInfo>();
            }

            return GetStartDirectories(directories[0].FullName);
        }

    }
}
