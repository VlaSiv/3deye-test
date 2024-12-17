namespace FileCreatorSorter.Tests;

using System.IO;
using Common.Time;
using FileCreator;
using Xunit;
using Sorter = FileSorter;

public class FileUtilityTests
{
    private const int FileSizeInMB = 10;
    private readonly string _testDirectory = "TestFiles";
    private readonly ITimeTracker _timeTracker;

    public FileUtilityTests()
    {
        if (!Directory.Exists(_testDirectory))
            Directory.CreateDirectory(_testDirectory);

        _timeTracker = new ConsoleTimeTracker();
    }

    [Fact]
    public void TestFileGenerator_CreatesCorrectFileSize()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test_generated_file.txt");
        int fileSizeMB = FileSizeInMB;
        var generator = new FileGenerator(_timeTracker);

        // Act
        generator.GenerateFile(filePath, fileSizeMB);

        // Assert
        var fileInfo = new FileInfo(filePath);
        Assert.True(fileInfo.Exists, "Generated file does not exist.");
        Assert.InRange(
            fileInfo.Length,
            fileSizeMB * 1024 * 1024 * 0.9,
            fileSizeMB * 1024 * 1024 * 1.1); // FileSizeMB +/- 10%

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void TestFileSorter_SortsCorrectly()
    {
        // Arrange
        var inputFile = Path.Combine(_testDirectory, "unsorted_file.txt");
        var outputFile = Path.Combine(_testDirectory, "sorted_file.txt");
        var tempDir = Path.Combine(_testDirectory, "temp_batches");

        var generator = new FileGenerator(_timeTracker);
        var sorter = new Sorter.FileSorter(_timeTracker);

        generator.GenerateFile(inputFile, FileSizeInMB);

        // Act
        sorter.SortLargeFile(inputFile, outputFile, tempDir);

        // Assert
        Assert.True(File.Exists(outputFile), "Sorted file does not exist.");

        var lines = File.ReadAllLines(outputFile);
        Assert.True(IsSorted(lines), "The output file is not sorted correctly.");

        // Cleanup
        File.Delete(inputFile);
        File.Delete(outputFile);
        Directory.Delete(tempDir, true);
    }

    private static bool IsSorted(string[] lines)
    {
        for (int i = 1; i < lines.Length; i++)
        {
            if (CompareLines(lines[i - 1], lines[i]) > 0)
                return false;
        }
        return true;
    }

    private static int CompareLines(string x, string y)
    {
        var xParts = SplitLine(x);
        var yParts = SplitLine(y);

        int stringComparison = string.Compare(xParts.Item2, yParts.Item2, StringComparison.Ordinal);
        return stringComparison != 0 ? stringComparison : xParts.Item1.CompareTo(yParts.Item1);
    }

    private static (int, string) SplitLine(string line)
    {
        int dotIndex = line.IndexOf('.');
        int number = int.Parse(line[..dotIndex]);
        string strPart = line[(dotIndex + 2)..];
        return (number, strPart);
    }
}
