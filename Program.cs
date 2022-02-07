using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IncrementalFileCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            // string[] args = new string[] { "-s", "/run/user/1000/gvfs/gphoto2:host=OnePlus_SM8150-MTP__SN%3AC8917861_8b73d18e", "-d", "/media/danieljolley/Samsung_T5/Daniel Phone Pictures", "-t", "1" };
            var argsList = args.ToList();
            var sourcePath = argsList.ElementAt(argsList.IndexOf("-s") + 1);
            var destPath = argsList.ElementAt(argsList.IndexOf("-d") + 1);
            var duration = Int32.Parse(argsList.ElementAt(argsList.IndexOf("-t") + 1));
            var skipFilesList = new List<string>();
            if (argsList.Contains("-f"))
            {
                var skipFiles = args.ElementAtOrDefault(argsList.IndexOf("-f") + 1);
                skipFilesList = skipFiles.Split(" ").ToList();
                skipFilesList.ForEach(f => f.Replace(" ", ""));
            }

            var fileCopier = new FileCopier(sourcePath, destPath, 60000 * duration, Path.Combine(AppContext.BaseDirectory, "copiedfiles.txt"), skipFilesList);
            fileCopier.CopyFiles();
        }
    }
}
