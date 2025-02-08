using System;
using System.IO;
using System.Linq;

namespace Logging
{
    public class Program
    {
        private static void Main()
        {
            ILogger[] loggers = new ILogger[] 
            { 
                LoggerCreator.CreateLoggerToFile(), 
                LoggerCreator.CreateLoggerToConsole(), 
                LoggerCreator.CreateSecureLogWritter(LoggerCreator.CreateLoggerToFile()), 
                LoggerCreator.CreateSecureLogWritter(LoggerCreator.CreateLoggerToConsole()), 
                LoggerCreator.CreateCompositeLogWritter(
                    LoggerCreator.CreateLoggerToConsole(),
                    LoggerCreator.CreateSecureLogWritter(LoggerCreator.CreateLoggerToFile())) 
            };

            Pathfinder[] pathfinders = new Pathfinder[loggers.Length];

            for (int i = 0; i < loggers.Length; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write($"{i}) ");
                pathfinders[i] = new Pathfinder(loggers[i]);
                pathfinders[i].Find($"{i} Hellow world!");
            }

            Console.WriteLine();
        }
    }

    public class LoggerCreator
    {
        public static FileLogWritter CreateLoggerToFile() 
        {
            return new FileLogWritter(); 
        }

        public static ConsoleLogWritter CreateLoggerToConsole() 
        {
            return new ConsoleLogWritter(); 
        }

        public static SecureLogWritter CreateSecureLogWritter(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            return new SecureLogWritter(logger);
        }

        public static CompositeLogWritter CreateCompositeLogWritter(params ILogger[] loggers)
        {
            if (loggers == null)
                throw new ArgumentNullException(nameof(loggers));

            if (loggers.Any(logger => logger == null))
                throw new ArgumentNullException($"The {nameof(loggers)} contains an element equal to null.");

            return new CompositeLogWritter(loggers);
        }
    }

    public class Pathfinder
    {
        private readonly ILogger _logger;

        public Pathfinder(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Find(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _logger.WriteError(message);
        }
    }

    public interface ILogger
    {
        void WriteError(string message);
    }

    public class ConsoleLogWritter : ILogger
    {
        public void WriteError(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"{this} {message}");
        }
    }

    public class FileLogWritter : ILogger
    {
        public void WriteError(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            File.WriteAllText("log.txt", message);
            Console.WriteLine($"{this} {message}");
        }
    }

    public class SecureLogWritter : ILogger
    {
        private readonly ILogger _logger;

        public SecureLogWritter(ILogger logger)
        {
            _logger = logger;
        }

        public void WriteError(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                _logger.WriteError($"{this} {message}");
        }
    }

    public class CompositeLogWritter : ILogger
    {
        private readonly ILogger[] _loggers;

        public CompositeLogWritter(params ILogger[] loggers)
        {
            if (loggers == null)
                throw new ArgumentNullException(nameof(loggers));

            if (loggers.Any(logger => logger == null))
                throw new ArgumentNullException($"The {nameof(loggers)} contains an element equal to null.");

            _loggers = loggers;
        }

        public void WriteError(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            for (int i = 0; i < _loggers.Length; i++)
                _loggers[i].WriteError(message);
        }
    }
}
