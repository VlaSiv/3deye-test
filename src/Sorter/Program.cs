namespace FileSorter;

using Common.Time;

public class Program
{
    public static void Main(string[] args)
    {
        var timeTracker = new ConsoleTimeTracker();

        string inputFile = args[0];
        string outputFile = "sorted_output.txt";
        string tempDir = "temp_batches";

        var sorter = new FileSorter(timeTracker);
        sorter.SortLargeFile(inputFile, outputFile, tempDir);
    }
}