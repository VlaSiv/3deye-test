namespace FileCreator;

using Common.Time;

public class Program
{
    private static void Main(string[] args)
    {
        var timeTracker = new ConsoleTimeTracker();

        string generatedFile = args[0];
        int fileSizeMB = int.Parse(args[1]);

        var generator = new FileGenerator(timeTracker);
        generator.GenerateFile(generatedFile, fileSizeMB);
    }
}