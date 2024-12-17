namespace FileCreator;

using System;
using System.IO;
using System.Text;
using Common;
using Common.Time;

public class FileGenerator
{
    private readonly ITimeTracker _timeTracker;
    private static readonly Random Random = new();
    private readonly int BatchSize;

    public FileGenerator(ITimeTracker timeTracker)
    {
        _timeTracker = timeTracker;
        long availableMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        BatchSize = (int)(availableMemory / 10 / 100); // ~10% of available memory
        BatchSize = Math.Min(BatchSize, 100_000); // Minimum batch size
    }

    /// <summary>
    /// Generates a large test file with specified size in MB.
    /// </summary>
    public void GenerateFile(string outputPath, int fileSizeInMB)
    {
        _timeTracker.Start();
        Console.WriteLine($"Generating file: {outputPath} with size {fileSizeInMB}MB");

        var targetBytes = fileSizeInMB * 1024L * 1024L;
        var writtenBytes = 0;

        using StreamWriter writer = new(outputPath, false, Encoding.UTF8, 1024 * 1024);

        while (writtenBytes < targetBytes)
        {
            var batch = new StringBuilder();

            Parallel.For(0, BatchSize, _ =>
            {
                var line = GenerateLine();
                lock (batch)
                {
                    batch.AppendLine(line);
                }
            });

            var batchContent = batch.ToString();
            var batchSize = Encoding.UTF8.GetByteCount(batchContent);

            if (writtenBytes + batchSize > targetBytes)
            {
                var remainingBytes = (int)(targetBytes - writtenBytes);
                batchContent = batchContent.Substring(0, remainingBytes);
                batchSize = Encoding.UTF8.GetByteCount(batchContent);
            }

            writer.Write(batchContent);
            writtenBytes += batchSize;
        }

        Console.WriteLine("File generation complete.");
        _timeTracker.Stop("File Generation");
    }

    /// <summary>
    /// Generates a single line in the format "Number. String".
    /// </summary>
    private static string GenerateLine()
    {
        var number = Random.Next(1, 100_000);
        var randomString = Constants.SampleStrings[Random.Next(Constants.SampleStrings.Length)];

        return $"{number}. {randomString}";
    }
}
