using KlaxIO.Log;
using KlaxShared.Attributes;
using System;

public enum ELogVerbosity
{
    Release,
    Debug,
    Editor,
    Always
}

public static class LogUtility
{
    public static event Action<CLogger, string, string> Logged;
    
    [ConsoleCommand]
    public static void Log(string text)
    {
        if (Logger != null)
        {
            Logger.Log(text);
        }
    }

    public static void Log(string text, params object[] arguments)
    {
        if (Logger != null)
        {
            Logger.Log(text, arguments);
        }
    }

    public static void Log(string text, ELogVerbosity verbosity, params object[] arguments)
    {
        if (Logger != null)
        {
            Logger.Log(text, verbosity, arguments);
        }
    }

    private static CLogger m_logger;
    public static CLogger Logger
    {
        get { return m_logger; }
        set
        {
            if (value != null)
            {
                value.Logged -= TriggerLoggedEvent;
            }
            m_logger = value;
            if (m_logger != null)
            {
                value.Logged += TriggerLoggedEvent;
            }
        }
    }

    private static void TriggerLoggedEvent(string rawText, string loggedText)
    {
        Logged?.Invoke(Logger, rawText, loggedText);
    }
}
