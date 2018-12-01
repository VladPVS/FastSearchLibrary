# FastSearchLibrary
The multithreading .NET library that provides opportunity to fast find files or directories using different search criteria.

[The MIF](https://github.com/VladPVS/The-MIF "The MIF search tool") file search tool is based on this library. You can [try](https://github.com/VladPVS/The-MIF/releases "Download The MIF") it if you want to estimate speed of work right now.
#### Works really fast. Check it yourself!
![Downloads](https://img.shields.io/github/downloads/VladPVS/FastSearchLibrary/total.svg)

## ADVANTAGES
* Library uses recursive search algorithm that is splitted on subtasks executing in thread pool
* **UnauthorizedAccessException** is never thrown while search is executed
* It's possible to choose different search criteria
* It's possible to stop search process when it is necessary
* It's possible to set different search paths at the same time

## INSTALLATION
1. Download archive with last [release](https://github.com/VladPVS/FastSearchLibrary/releases "Last release") if you use .NET 4.6.2 or higher otherwise download v1.1.6.1
2. Extract content to some directory.
3. Copy .dll and .xml files in directory of your project.
4. Add library to your project: Solution Explorer -> Reference -> item AddReference in context menu -> Browse
5. Add appropriate namespace: `using FastSearchLibrary;`
6. Set target .NET version at least as `4.5.1` if you use v1.1.6.1 of library or `4.6.2` if you use at least v1.1.7.2: Project -> <YourProjectName> Properties -> Target framework

## CONTENT

Next classes provide search functionality:
* FileSearcher
* DirectorySearcher
* FileSearcherMultiple
* DirectorySearcherMultiple

## USE PRINCIPLES
### Basic opportunities
  * Classes `FileSearcher` and `DirectorySearcher` contain static methods that allow to execute search by different criteria.
  These methods return result only when they fully complete execution.
  * Methods that have "Fast" ending divide task on several 
  subtasks that execute simultaneously in thread pool.
  * Methods that have "Async" ending return Task and don't block the called thread.
  * First group of methods accepts 2 parameters: 
    * `string folder` - start search directory
    * `string pattern` - the search string to match against the names of files in path.
    This parameter can contain a combination of valid literal path and wildcard (* and ?)
    characters, but doesn't support regular expressions.
    
  Examples:
  
    List<FileInfo> files = FileSearcher.GetFiles(@"C:\Users", "*.txt");
   Finds all `*.txt` files in `C:\Users` using one thread method.
   
    List<FileInfo> files = FileSearcher.GetFilesFast(@"C:\Users", "*SomePattern*.txt");
   Finds all files that match appropriate pattern using several threads in thread pool.
   
    Task<List<FileInfo>> task = FileSearcher.GetFilesFastAsync(@"C:\", "a?.txt");
   Finds all files that match appropriate pattern using several threads in thread pool as
   an asynchronous operation.
   
   * Second group of methods accepts 2 parameters:
     * `string folder` - start search directory
     * `Func<FileInfo, bool> isValid` - delegate that determines algorithm of file selection.
     
   Examples:
   
    Task<List<FileInfo>> task = FileSearcher.GetFilesFastAsync(@"D:\", (f) =>
    {
         return (f.Name.Contains("Pattern") || f.Name.Contains("Pattern2")) &&
                 f.LastAccessTime >= new DateTime(2018, 3, 1) && f.Length > 1073741824;
    });
   Finds all files that match appropriate conditions using several threads in thread pool as
   an asynchronous operation.
   
   You also can use regular expressions:
    
    Task<List<FileInfo>> task = FileSearcher.GetFilesFastAsync(@"D:\", (f) =>
    {
         return (f) => Regex.IsMatch(f.Name, @".*Imagine[\s_-]Dragons.*.mp3$");
    }); 
    
   Finds all files that match appropriate regular expression using several thread in thread pool as
   an asynchronous operation.
   
 ### Advanced opportunities
   If you want to execute some complicated search with realtime result getting you should use instance of `FileSearcher` class,
   that has various constructor overloads.
   `FileSearcher` class includes next events:
   * `event EventHandler<FileEventArgs> FilesFound` - fires when next portion of files is found.
     Event includes `List<FileInfo> Files { get; }` property that contains list of finding files.
   * `event EventHandler<SearchCompleted> SearchCompleted` - fires when search process is completed or stopped. 
     Event includes `bool IsCanceled { get; }` property that contains value that defines whether search process stopped by calling
     `StopSearch()` method. 
    To get stop search process possibility one has to use constructor that accepts CancellationTokenSource parameter.
    
   Example:
    
    class Searcher
    {
        private static object locker = new object(); // locker object

        private FileSearcher searcher;

        List<FileInfo> files;

        public Searcher()
        {
            files = new List<FileInfo>(); // create list that will contain search result
        }

        public void StartSearch()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            // create tokenSource to get stop search process possibility

            searcher = new FileSearcher(@"C:\", (f) =>
            {
               return Regex.IsMatch(f.Name, @".*[iI]magine[\s_-][dD]ragons.*.mp3$"); 
            }, tokenSource);  // give tokenSource in constructor
 

            searcher.FilesFound += (sender, arg) => // subscribe on FilesFound event
            {
                lock (locker) // using a lock is obligatorily
                {
                    arg.Files.ForEach((f) =>
                    {
                        files.Add(f); // add the next part of the received files to the results list
                        Console.WriteLine($"File location: {f.FullName}, \nCreation.Time: {f.CreationTime}");
                    });

                    if (files.Count >= 10) // one can choose any stopping condition
                       searcher.StopSearch();
                }
            };

            searcher.SearchCompleted += (sender, arg) => // subscribe on SearchCompleted event
            {
                if (arg.IsCanceled) // check whether StopSearch() called
                    Console.WriteLine("Search stopped.");
                else
                    Console.WriteLine("Search completed.");

                Console.WriteLine($"Quantity of files: {files.Count}"); // show amount of finding files
            };

            searcher.StartSearchAsync();
            // start search process as an asynchronous operation that doesn't block the called thread
        }
    }
 Note that all `FilesFound` event handlers are not thread safe so to prevent result loosing one should use
 `lock` keyword as you can see in example above or use thread safe collection from `System.Collections.Concurrent` namespace.
 
 ### Extended opportunities
   There are 2 additional parameters that one can set. These are `handlerOption` and `suppressOperationCanceledException`.
   `ExecuteHandlers handlerOption` parameter represents instance of `ExecuteHandlers` enumeration that specifies where
   FilesFound event handlers are executed:  
   * `InCurrentTask` value means that `FileFound` event handlers will be executed in that task where files were found. 
   * `InNewTask` value means that `FilesFound` event handlers will be executed in new task.
    Default value is `InCurrentTask`. It is more preferably in most cases. `InNewTask` value one should use only if handlers execute
    very sophisticated work that takes a lot of time, e.g. parsing of each found file.
    
   `bool suppressOperationCanceledException` parameter determines whether necessary to suppress 
   OperationCanceledException.
   If `suppressOperationCanceledException` parameter has value `false` and StopSearch() method is called the `OperationCanceledException` 
   will be thrown. In this case you have to process the exception manually.
   If `suppressOperationCanceledException` parameter has value `true` and StopSearch() method is called the `OperationCanceledException` 
   is processed automatically and you don't need to catch it. 
   Default value is `true`.
   
   Example:
            
    CancellationTokenSource tokenSource = new CancellationTokenSource();

    FileSearcher searcher = new FileSearcher(@"D:\Program Files", (f) =>
    {
       return Regex.IsMatch(f.Name, @".{1,5}[Ss]ome[Pp]attern.txt$") && (f.Length >= 8192); // 8192b == 8Kb 
    }, tokenSource, ExecuteHandlers.InNewTask, true); // suppressOperationCanceledException == true
    
   ### MULTIPLE SEARCH
   `FileSearcher` and `DirectorySearcher` classes can search only in one directory (and in all subdirectories surely) 
   but what if you want to perform search in several directories at the same time?     
   Of course, you can create some instances of `FileSearcher` (or `DirectorySearcher`) class and launch them simultaneously, 
   but `FilesFound` (or `DirectoriesFound`) events will occur for each instance you create. As a rule, it's inconveniently.
   Classes `FileSearcherMultiple` and `DirectorySearcherMultiple` are intended to solve this problem. 
   They are similar to `FileSearcher` and `DirectorySearcher` but can execute search in several directories.
   The difference between `FileSearcher` and `FileSearcheMultiple` is that constructor of `Multiple` class accepts list of 
   directories instead one directory.
   
   Example:
   
    List<string> folders = new List<string>
    {
      @"C:\Users\Public",
      @"C:\Windows\System32",
      @"D:\Program Files",
      @"D:\Program Files (x86)"
    }; // list of search directories

    List<string> keywords = new List<string> { "word1", "word2", "word3" }; // list of search keywords

    FileSearcherMultiple multipleSearcher = new FileSearcherMultiple(folders, (f) =>
    {
       if (f.CreationTime >= new DateTime(2015, 3, 15) &&
          (f.Extension == ".cs" || f.Extension == ".sln"))
          {
             foreach (var keyword in keywords)
               if (f.Name.Contains(keyword))
                 return true;
          }
          
       return false;
    }, tokenSource, ExecuteHandlers.InCurrentTask, true);       

   ### NOTES
   #### Using "await" keyword
   It is highly recommend to use "await" keyword when you use any asynchronous method. It allows to get possible
   exceptions from method for following processing, that is demonstrated next code example. Error processing in previous 
   examples had been missed for simplicity.

  Example:

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using FastSearchLibrary;

    namespace SearchWithAwait
    {
        class Program
        {
           private static object locker = new object();

           private static List<FileInfo> files;

           private static Stopwatch stopWatch;


           static void Main(string[] args)
           {
              string searchPattern = @"\.mp4$";

              StartSearch(searchPattern);

              Console.ReadKey(true);
           }


           private static async void StartSearch(string pattern)
           {
              stopWatch = new Stopwatch();

              stopWatch.Start();

              Console.WriteLine("Search had been started.\n");

              files = new List<FileInfo>();

              List<string> searchDirectories = new List<string>
              {
                   @"C:\",
                   @"D:\"
              }; 

              FileSearcherMultiple searcher = new FileSearcherMultiple(searchDirectories, (f) =>
              {
                  return Regex.IsMatch(f.Name, pattern);
              }, new CancellationTokenSource());

              searcher.FilesFound += Searcher_FilesFound;
              searcher.SearchCompleted += Searcher_SearchCompleted;

              try
              {
                 await searcher.StartSearchAsync();
              }
              catch (AggregateException ex)
              {
                 Console.WriteLine($"Error occurred: {ex.InnerException.Message}");
              }
              catch (Exception ex)
              {
                 Console.WriteLine($"Error occurred: {ex.Message}");
              }
              finally
              {
                 Console.Write("\nPress any key to continue...");
              }
           } 


           private static void Searcher_FilesFound(object sender, FileEventArgs arg)
           {
              lock (locker) // using a lock is obligatorily
              {
                 arg.Files.ForEach((f) =>
                 {
                    files.Add(f); // add the next part of the received files to the results list
                    Console.WriteLine($"File location: {f.FullName}\nCreation.Time: {f.CreationTime}\n");
                 });
              }
           }


           private static void Searcher_SearchCompleted(object sender, SearchCompletedEventArgs arg)
           {
              stopWatch.Stop();

              if (arg.IsCanceled) // check whether StopSearch() called
                Console.WriteLine("Search stopped.");
              else
                Console.WriteLine("Search completed.");

              Console.WriteLine($"Quantity of files: {files.Count}"); // show amount of finding files

              Console.WriteLine($"Spent time: {stopWatch.Elapsed.Minutes} min {stopWatch.Elapsed.Seconds} s {stopWatch.Elapsed.Milliseconds} ms");
           }
        }
    }
#### Long paths Windows limitation
There is a 260 symbols Windows limitation on full name of files. In common case library will ignore such "long" paths. But if you want to circumvent this limitation you should follow next steps:
1. Use Windows 10 (assembly 1607 or higher).
2. Download the last [release](https://github.com/VladPVS/FastSearchLibrary/releases "Last release") of this library.
3. Use Visual Studio 2017.
4. Set the version of .NET Framework at least 4.6.2
5. Add the manifest file to your project. 
Select `<Project name>` in Solution explorer, click right button of mouse -> `Add` -> `New item` -> `Application manifest file`. Then add content of [this](https://github.com/VladPVS/FastSearchLibrary/files/2267462/manifest.txt) file to the manifest before the last closed tag.
6. A registry key allows to enable or disable the new long path behavior in Windows.  To enable long path behavior open registry editor and follow next path `HKLM\SYSTEM\CurrentControlSet\Control\FileSystem` Then create parameter `LongPathsEnabled` (type REG_DWORD) with `1` value.
7. Reboot your computer.
    
### SPEED OF WORK
It depends on your computer performance, current loading, but usually `Fast` methods and instance method `StartSearch()` are
performed at least in 2 times faster than simple one-thread recursive algorithm if you use modern multicore processor of course.
