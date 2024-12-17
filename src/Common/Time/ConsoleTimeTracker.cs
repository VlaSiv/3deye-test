namespace Common.Time;

using System;
using System.Diagnostics;

public class ConsoleTimeTracker : ITimeTracker
{
    private readonly Stopwatch _stopwatch = new();

    public void Start()
    {
        _stopwatch.Restart();
    }

    public void Stop(string operationName)
    {
        _stopwatch.Stop();
        TimeSpan elapsed = _stopwatch.Elapsed;
        Console.WriteLine($"{operationName} completed in {elapsed:hh\\:\\mm\\:ss}");
    }
}
