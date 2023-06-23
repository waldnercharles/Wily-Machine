using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Godot;
using JetBrains.Annotations;
using Environment = System.Environment;

namespace Spaghetti;

public static class Log
{
    public enum LogLevel
    {
        System = 0,
        Assert = 1,
        Error = 2,
        Warning = 3,
        Info = 4,
        Debug = 5,
        Trace = 6
    }

    private const int m_StdOutputHandle = -11;
    private const uint m_EnableVirtualTerminalProcessing = 0x0004;

    private static readonly StringBuilder s_Log;
    private static readonly StringBuilder s_Line;
    private static readonly LogAttribute[] s_LogAttributes;
    private static readonly bool s_ColorEnabled;

    public static LogLevel Verbosity = LogLevel.Trace;

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    static Log()
    {
        s_Log = new StringBuilder();
        s_Line = new StringBuilder();

        s_LogAttributes = new[]
        {
            new LogAttribute
            {
                Name = "SYSTEM",
                Color = LogColor.Orange
            },
            new LogAttribute
            {
                Name = "ASSERT",
                Color = LogColor.Magenta
            },
            new LogAttribute
            {
                Name = "ERROR ",
                Color = LogColor.Red
            },
            new LogAttribute
            {
                Name = "WARN  ",
                Color = LogColor.Yellow
            },
            new LogAttribute
            {
                Name = "INFO  ",
                Color = LogColor.Green
            },
            new LogAttribute
            {
                Name = "DEBUG ",
                Color = LogColor.Cyan
            },
            new LogAttribute
            {
                Name = "TRACE ",
                Color = LogColor.White
            }
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var stdOut = GetStdHandle(m_StdOutputHandle);

            s_ColorEnabled = GetConsoleMode(stdOut, out var outConsoleMode) &&
                             SetConsoleMode(stdOut,
                                 outConsoleMode | m_EnableVirtualTerminalProcessing);
        }
        else
        {
            s_ColorEnabled = false;
        }

        System("Logging Enabled ({0})", Enum.GetName(Verbosity));
    }

    private static bool LogInternalIf(
        bool condition,
        LogLevel logLevel,
        string message,
        object? arg0,
        object? arg1,
        object? arg2,
        string callerFilePath,
        int callerLineNumber)
    {
        if (condition)
        {
            LogInternal(logLevel, message, arg0, arg1, arg2, callerFilePath, callerLineNumber);
            return true;
        }

        return false;
    }

    private static void LogInternal(
        LogLevel logLevel,
        string message,
        object? arg0,
        object? arg1,
        object? arg2,
        string? callerFilePath,
        int callerLineNumber)
    {
        if (Verbosity < logLevel)
        {
            return;
        }

        s_Line.Clear();
        var logAttribute = s_LogAttributes[(int)logLevel];

        s_Line.Append(DateTime.Now.ToString("HH:mm:ss"));
        s_Line.Append(' ');

        s_Line.Append($"[color={logAttribute.Color}]");
        s_Line.Append(logAttribute.Name);
        s_Line.Append(' ');
        s_Line.Append("[/color]");

        var callerInfoIndex = s_Line.Length;
        s_Line.Append("[u]");
        s_Line.Append(Path.GetFileName(callerFilePath));
        s_Line.Append(':');
        s_Line.Append(callerLineNumber);
        s_Line.Append("[/u]");
        s_Line.Append(' ', Math.Max(1, 32 - (s_Line.Length - callerInfoIndex)));

        s_Line.Append($"[color={LogColor.White}]");
        s_Line.AppendFormat(message, arg0, arg1, arg2);
        s_Line.Append("[/color]");

        s_Log.Append(s_Line);
        s_Log.AppendLine();

        GD.PrintRich(s_Line);

        if (logLevel == LogLevel.Assert)
        {
            Debugger.Break();
        }
    }

    public static bool TraceIf(
        bool condition,
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        return LogInternalIf(condition, LogLevel.Trace, message, arg0, arg1, arg2, callerFilePath,
            callerLineNumber);
    }

    public static void Trace(
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(LogLevel.Trace, message, arg0, arg1, arg2, callerFilePath, callerLineNumber);
    }

    public static bool DebugIf(
        bool condition,
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        return LogInternalIf(condition, LogLevel.Debug, message, arg0, arg1, arg2, callerFilePath,
            callerLineNumber);
    }

    public static void Debug(
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(LogLevel.Debug, message, arg0, arg1, arg2, callerFilePath, callerLineNumber);
    }

    public static bool InfoIf(
        bool condition,
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        return LogInternalIf(condition, LogLevel.Info, message, arg0, arg1, arg2, callerFilePath,
            callerLineNumber);
    }

    public static void Info(
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(LogLevel.Info, message, arg0, arg1, arg2, callerFilePath, callerLineNumber);
    }

    public static bool WarningIf(
        bool condition,
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        return LogInternalIf(condition, LogLevel.Warning, message, arg0, arg1, arg2, callerFilePath,
            callerLineNumber);
    }

    public static void Warning(
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(LogLevel.Warning, message, arg0, arg1, arg2, callerFilePath, callerLineNumber);
    }

    [AssertionMethod]
    public static bool ErrorIf(
        [DoesNotReturnIf(true)] [AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition,
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        if (condition)
        {
            LogInternal(LogLevel.Error, message, arg0, arg1, arg2, callerFilePath,
                callerLineNumber);

            Console.ReadLine();
            throw new Exception(message);
        }

        return false;
    }

    [DoesNotReturn]
    [ContractAnnotation("=> halt")]
    public static void Error(
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(LogLevel.Error, message, arg0, arg1, arg2, callerFilePath, callerLineNumber);
        Console.ReadLine();
        throw new Exception(message);
    }

    [Conditional("DEBUG")]
    public static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        string? message = null, // "Assertion failed.",
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        if (!condition)
        {
            message = string.IsNullOrEmpty(message) ? "Assertion Failed" : $"Assertion Failed: {message}";
            message = string.Format(message, arg0, arg1, arg2);

            LogInternal(LogLevel.Assert, message, arg0, arg1, arg2, callerFilePath, callerLineNumber);
            Environment.Exit(1);
            throw new Exception(message);
        }
    }

    public static void System(
        string message,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(LogLevel.System, message, arg0, arg1, arg2, callerFilePath, callerLineNumber);
    }

    private static class LogColor
    {
        public const string Black = "black";
        public const string Gray = "gray";
        public const string Blue = "blue";
        public const string Orange = "orange";
        public const string Green = "green";
        public const string Cyan = "cyan";
        public const string Red = "red";
        public const string Magenta = "magenta";
        public const string Yellow = "yellow";
        public const string White = "white";
    }

    private struct LogAttribute
    {
        public string Name;
        public string Color;
    }
}
