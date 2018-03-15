using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    internal abstract class FileSearcherBase
    {
        /// <summary>
        /// Determines where execute FilesFound event handlers.
        /// </summary>
        protected ExecuteHandlers handlerOption { get; set; }


        protected string folder;


        protected ConcurrentBag<Task> taskHandlers;


        public FileSearcherBase(string folder, ExecuteHandlers handlerOption)
        {
            this.folder = folder;
            this.handlerOption = handlerOption;
            taskHandlers = new ConcurrentBag<Task>();
        }


        public event EventHandler<FileEventArgs> FilesFound;

        public event EventHandler<SearchCompletedEventArgs> SearchCompleted;



        protected virtual void GetFilesFast()
        {
            List<DirectoryInfo> startDirs = GetStartDirectories(folder);

            startDirs.AsParallel().ForAll((d) =>
            {
                GetStartDirectories(d.FullName).AsParallel().ForAll((dir) =>
                {
                    GetFiles(dir.FullName);
                });
            });

            OnSearchCompleted(false);
        }



        protected virtual void OnFilesFound(List<FileInfo> files)
        {
            if (handlerOption == ExecuteHandlers.InNewTask)
                taskHandlers.Add(Task.Run(() => CallFilesFound(files)));
            else
                CallFilesFound(files);
        }



        protected virtual void CallFilesFound(List<FileInfo> files)
        {
            EventHandler<FileEventArgs> handler = FilesFound;

            if (handler != null)
            {
                var arg = new FileEventArgs(files);
                handler(this, arg);
            }
        }



        protected virtual void OnSearchCompleted(bool isCanceled)
        {
            if (handlerOption == ExecuteHandlers.InNewTask)
            {
                 Task.WaitAll(taskHandlers.ToArray());   
            }

            CallSearchCompleted(isCanceled);
        }


        protected virtual void CallSearchCompleted(bool isCanceled)
        {
            EventHandler<SearchCompletedEventArgs> handler = SearchCompleted;

            if (handler != null)
            {
                var arg = new SearchCompletedEventArgs(isCanceled);

                SearchCompleted(this, arg);
            }
        }


        protected abstract void GetFiles(string folder);



        protected abstract List<DirectoryInfo> GetStartDirectories(string folder);



        public abstract void StartSearch();
    }
}
