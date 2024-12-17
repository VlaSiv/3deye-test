namespace Common.Time;

public interface ITimeTracker
{
    void Start();

    void Stop(string operationName);
}
