namespace Huragok.Application.Logging;

internal enum LoggerNewlineFormat : byte {
    CreateNewline,
    DontCreateNewline,
    ReplaceLast
}

internal enum LoggingLevel : byte {
    Debug,
    Info,
    Warning,
    Error
}

internal static class Logger {
    internal static void Message(string message, LoggerNewlineFormat newlineFormat = LoggerNewlineFormat.CreateNewline, bool writeHeader = true) {
        if (MainProgram.globalLogLevel > LoggingLevel.Info) return;

        const string header = "inf";
        const ConsoleColor color = ConsoleColor.Green;

        switch (newlineFormat) {
            case LoggerNewlineFormat.CreateNewline:
                if (writeHeader) WriteHeader(header, color);
                Console.WriteLine(message);
                break;

            case LoggerNewlineFormat.DontCreateNewline:
                if (writeHeader) WriteHeader(header, color);
                Console.Write(message);
                break;

            case LoggerNewlineFormat.ReplaceLast:
                Console.Write("\r");

                if (writeHeader) WriteHeader(header, color);

                int headerLength = header.Length + 5;
                int width = writeHeader ? Console.WindowWidth - headerLength : Console.WindowWidth;

                Console.Write(message.PadRight(width));
                break;
        }
    }

    internal static void Warning(string message, LoggerNewlineFormat newlineFormat = LoggerNewlineFormat.CreateNewline, bool writeHeader = true) {
        if (MainProgram.globalLogLevel > LoggingLevel.Warning) return;

        const string header = "wrn";
        const ConsoleColor color = ConsoleColor.Yellow;

        switch (newlineFormat) {
            case LoggerNewlineFormat.CreateNewline:
                if (writeHeader) WriteHeader(header, color);
                Console.Error.WriteLine(message);
                break;

            case LoggerNewlineFormat.DontCreateNewline:
                if (writeHeader) WriteHeader(header, color);
                Console.Error.Write(message);
                break;

            case LoggerNewlineFormat.ReplaceLast:
                Console.Write("\r");

                if (writeHeader) WriteHeader(header, color);

                int headerLength = header.Length + 5;
                int width = writeHeader ? Console.WindowWidth - headerLength : Console.WindowWidth;

                Console.Write(message.PadRight(width));
                break;
        }
    }


    internal static void Error(string message, LoggerNewlineFormat newlineFormat = LoggerNewlineFormat.CreateNewline, bool fatal = false, bool writeHeader = true) {
        if (MainProgram.globalLogLevel > LoggingLevel.Error) return;

        string header = fatal ? "!!!" : "err";
        var color = fatal ? ConsoleColor.DarkRed : ConsoleColor.Red;

        switch (newlineFormat) {
            case LoggerNewlineFormat.CreateNewline:
                if (writeHeader) WriteHeader(header, color);
                Console.Error.WriteLine(message);
                break;

            case LoggerNewlineFormat.DontCreateNewline:
                if (writeHeader) WriteHeader(header, color);
                Console.Error.Write(message);
                break;

            case LoggerNewlineFormat.ReplaceLast:
                Console.Write("\r");

                if (writeHeader) WriteHeader(header, color);

                int headerLength = header.Length + 5;
                int width = writeHeader ? Console.WindowWidth - headerLength : Console.WindowWidth;

                Console.Write(message.PadRight(width));
                break;
        }
    }

    internal static void Debug(string message, LoggerNewlineFormat newlineFormat = LoggerNewlineFormat.CreateNewline, bool writeHeader = true) {
        if (MainProgram.globalLogLevel > LoggingLevel.Debug) return;

        const string header = "dbg";
        const ConsoleColor color = ConsoleColor.Gray;
        switch (newlineFormat) {
            case LoggerNewlineFormat.CreateNewline:
                if (writeHeader) WriteHeader(header, color);
                Console.WriteLine(message);
                break;

            case LoggerNewlineFormat.DontCreateNewline:
                if (writeHeader) WriteHeader(header, color);
                Console.Write(message);
                break;

            case LoggerNewlineFormat.ReplaceLast:
                Console.Write("\r");

                if (writeHeader) WriteHeader(header, color);

                int headerLength = header.Length + 5;
                int width = writeHeader ? Console.WindowWidth - headerLength : Console.WindowWidth;

                Console.Write(message.PadRight(width));
                break;
        }
    }

    private static void WriteHeader(string header, ConsoleColor color) {
        Console.Write("[ ");
        Console.ForegroundColor = color;
        Console.Write(header);
        Console.ResetColor();
        Console.Write(" ] ");
    }
}