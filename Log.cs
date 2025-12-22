using System;

namespace Bloqbit
{
    public static class Log
    {
        private static readonly int level = int.TryParse(Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "0", out var v) ? v : 0;

        public static void Debug(string message) { if (level <= 0) Write(message, "DEBUG", ConsoleColor.Gray); }
        public static void Info(string message) { if (level <= 1) Write(message, "INFO", ConsoleColor.Blue); }
        public static void Warn(string message) { if (level <= 2) Write(message, "WARN", ConsoleColor.Yellow); }
        public static void Error(string message) { if (level <= 3) Write(message, "ERROR", ConsoleColor.Red); }
        public static void Critical(string message) { if (level <= 4) Write(message, "CRITICAL", ConsoleColor.Magenta); }
        public static void Success(string message) { if (level <= 5) Write(message, "DONE", ConsoleColor.Green); }
        public static void Print(string message) { if (level <= 6) Write(message, "LOG", ConsoleColor.White); }

        private static void Write(string message, string tag, ConsoleColor color)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            Console.ForegroundColor = color;
            Console.WriteLine($" | {tag} | {message}");
            Console.ResetColor();
        }
    }
}