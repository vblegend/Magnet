using Magnet.Core;
using System;
using System.Runtime.CompilerServices;

public static class DebuggerExtends
{

    public static void DEBUG(this AbstractScript script, string message, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string? callMethod = null)
    {
        script.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {message}");
    }

    public static void DEBUG(this AbstractScript script, string format, object[] args, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string? callMethod = null)
    {
        script.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {String.Format(format, args)}");
    }

    public static void PRINT(this AbstractScript script, string message)
    {
        script.Output(MessageType.Debug, message);
    }
    public static void PRINT(this AbstractScript script, Object message)
    {
        script.Output(MessageType.Debug, message.ToString());
    }

    public static void PRINT(this AbstractScript script, string format, params object?[] args)
    {
        script.PRINT(message: String.Format(format, args));
    }

}