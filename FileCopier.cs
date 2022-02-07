using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class FileCopier
{
    private string _copiedFileNamesFilePath { get; }
    private string _sourceDirectory { get; }
    private string _destDirectory { get; }
    private int _time { get; }
    private List<string> _skipFilesList { get; }
    private Stopwatch stopwatch { get; set; }
    private List<FileInfo> copiedFiles { get; set; }
    private List<string> prevCopiedFileNames { get; set; }
    private int totalFilesCopied { get; set; }
    private string currentCopyDir { get; set; }

    public FileCopier(string sourceDirectory, string destDirectory, int time, string copiedFileNamesFilePath, List<string> skipFilesList)
    {
        _sourceDirectory = sourceDirectory;
        if (!Directory.Exists(_sourceDirectory))
            throw new Exception($"{nameof(_sourceDirectory)} does not exist. Please enter valid directory. ref: {_sourceDirectory}");
        _destDirectory = destDirectory;
        if (!Directory.Exists(_destDirectory))
            throw new Exception($"{nameof(_destDirectory)} does not exist. Please enter valid directory. ref: {_destDirectory}");
        _time = time;
        _copiedFileNamesFilePath = copiedFileNamesFilePath;
        _skipFilesList = skipFilesList;
        stopwatch = new Stopwatch();
        copiedFiles = new List<FileInfo>();
        prevCopiedFileNames = new List<string>();
    }

    private void AppendCopied(string filePath, List<FileInfo> appendFilesList)
    {
        var copiedFileNames = appendFilesList.Select(fi => fi.Name);
        File.AppendAllLines(_copiedFileNamesFilePath, copiedFileNames);
    }

    public void CopyFiles()
    {
        prevCopiedFileNames = GetPreviouslyCopied(_copiedFileNamesFilePath);
        stopwatch.Start();
        var srcDirectoryInfo = new DirectoryInfo(_sourceDirectory);

        try
        {
            RecursiveCopy(srcDirectoryInfo, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            AppendCopied(_copiedFileNamesFilePath, copiedFiles);
        }
    }

    private List<string> GetPreviouslyCopied(string filePath)
    {
        if (!File.Exists(filePath))
        {
            using (File.Create(filePath))
            {

            }
        }

        return File.ReadAllLines(filePath).ToList();
    }

    private void RecursiveCopy(DirectoryInfo topDirectoryInfo, bool isTopLevelDir)
    {
        var files = topDirectoryInfo.EnumerateFiles();
        var fullDestPath = isTopLevelDir ? _destDirectory : Path.Combine(_destDirectory, topDirectoryInfo.Name);
        Directory.CreateDirectory(fullDestPath);
        foreach (var file in files)
        {
            if (stopwatch.ElapsedMilliseconds >= _time)
                return;

            var fileWasPrevCopied = prevCopiedFileNames.Contains(file.Name);
            if (fileWasPrevCopied)
                continue;

            var destFilePath = Path.Combine(fullDestPath, file.Name);
            if (!File.Exists(destFilePath) && !_skipFilesList.Contains(file.Extension))
            {
                currentCopyDir = topDirectoryInfo.FullName;
                file.CopyTo(destFilePath);
                totalFilesCopied++;
            }

            ReportProgress();
            copiedFiles.Add(file);
        }

        foreach (var directoryInfo in topDirectoryInfo.GetDirectories())
        {
            if (stopwatch.ElapsedMilliseconds >= _time)
            {
                return;
            }

            RecursiveCopy(directoryInfo, false);
        }
    }

    private void ReportProgress()
    {
        Console.Clear();
        if (!String.IsNullOrWhiteSpace(currentCopyDir))
        {
            Console.WriteLine($"copying files from {currentCopyDir}...");
        }
        Console.WriteLine($"Files Copied: {totalFilesCopied}");
        Console.WriteLine($"Time Elapsed: {stopwatch.ElapsedMilliseconds/1000} sec /{_time/60000} min -- {stopwatch.ElapsedMilliseconds / _time}%");
    }
}
