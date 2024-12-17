namespace FileSorter;

using System.Collections.Concurrent;
using Common;
using Common.Time;

public class FileSorter
{
    private readonly ITimeTracker _timeTracker;
    private readonly int BatchSize;

    public FileSorter(ITimeTracker timeTracker)
    {
        _timeTracker = timeTracker;
        long availableMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        BatchSize = (int)(availableMemory / 10 / 100); // ~10% of available memory
        BatchSize = Math.Max(BatchSize, 100_000); // Minimum batch size
    }

    /// <summary>
    /// Sorts a large text file in batches and merges them to avoid memory issues.
    /// </summary>
    public void SortLargeFile(string inputPath, string outputPath, string tempDirectory)
    {
        _timeTracker.Start();
        Console.WriteLine("Starting file sorting process...");

        List<string> batchFiles = SplitAndSortBatches(inputPath, tempDirectory);
        MergeSortedBatches(batchFiles, outputPath);

        Console.WriteLine("Sorting process complete.");
        _timeTracker.Stop("File Sorting");
    }

    /// <summary>
    /// Splits the input file into smaller batches, sorts each batch, and saves them as temporary files.
    /// </summary>
    private List<string> SplitAndSortBatches(string inputPath, string tempDirectory)
    {
        var tempFiles = new ConcurrentBag<string>();
        Directory.CreateDirectory(tempDirectory);

        using var reader = new StreamReader(inputPath);
        Parallel.ForEach(PartitionLines(reader), batchLines =>
        {
            var sortedBatch = batchLines
                .AsParallel()
                .OrderBy(line => SplitLine(line).Item2)
                .ThenBy(line => SplitLine(line).Item1)
                .ToList();
            string tempFile = Path.Combine(tempDirectory, $"batch_{Guid.NewGuid()}.txt");
            File.WriteAllLines(tempFile, sortedBatch);
            tempFiles.Add(tempFile);
        });

        return [.. tempFiles];
    }

    /// <summary>
    /// Partitions the lines from the input file into batches of a specified size.
    /// </summary>
    /// <param name="reader">The StreamReader to read lines from the input file.</param>
    /// <returns>An IEnumerable of List of strings, where each List represents a batch of lines.</returns>
    private IEnumerable<List<string>> PartitionLines(StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            var batch = new List<string>(BatchSize);
            for (int i = 0; i < BatchSize && !reader.EndOfStream; i++)
            {
                batch.Add(reader.ReadLine());
            }

            yield return batch;
        }
    }

    /// <summary>
    /// Merges multiple sorted batch files into a single sorted output file.
    /// </summary>
    private static void MergeSortedBatches(List<string> batchFiles, string outputPath)
    {
        using var writer = new StreamWriter(outputPath);
        var readers = batchFiles.Select(f => new StreamReader(f)).ToList();
        var minHeap = new PriorityQueue<(string Line, int FileIndex), string>();

        try
        {
            for (int i = 0; i < readers.Count; i++)
            {
                if (!readers[i].EndOfStream)
                {
                    string line = readers[i].ReadLine();
                    minHeap.Enqueue((line, i), line);
                }
            }

            while (minHeap.Count > 0)
            {
                var (smallestLine, fileIndex) = minHeap.Dequeue();
                writer.WriteLine(smallestLine);

                if (!readers[fileIndex].EndOfStream)
                {
                    string nextLine = readers[fileIndex].ReadLine();
                    minHeap.Enqueue((nextLine, fileIndex), nextLine);
                }
            }
        }
        finally
        {
            readers.ForEach(r => r.Close());
        }
    }

    private static (int, string) SplitLine(string line)
    {
        var dotIndex = line.IndexOf(Constants.Dot);
        var number = int.Parse(line[..dotIndex]);
        var strPart = line[(dotIndex + 2)..];

        return (number, strPart);
    }
}
