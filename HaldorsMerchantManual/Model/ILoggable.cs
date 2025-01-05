namespace App1.Model;

public interface ILoggable
{
    void LogData(ILogData logData);
}

public interface ILogData
{
    void Data(string message);
}