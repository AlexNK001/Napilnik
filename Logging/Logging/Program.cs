using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Защищённый логер даёт функционал, что логер пишется только по пятницам (такая условность).

//Представьте класс Pathfinder у которого есть зависимость от условного ILogger,
//в процессе своей работы он что-то пишет в лог. Что не принципиально.
//Сделайте в нём один метод Find который только пишет в лог через своего логера.

//Перепроектируйте систему логирования так, что бы у меня было 5 объектов класса Pathfinder.
//1) Пишет лог в файл.
//2) Пишет лог в консоль.
//3) Пишет лог в файл по пятницам.
//4) Пишет лог в консоль по пятницам.
//5) Пишет лог в консоль а по пятницам ещё и в файл.
namespace Logging
{
    public class Program
    {
        private static void Main()
        {
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
        //public virtual void WriteError(string message)
        public void WriteError(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine(message);
        }
    }

    public class FileLogWritter : ILogger
    {
        //public virtual void WriteError(string message)
        public void WriteError(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            File.WriteAllText("log.txt", message);
        }
    }

    //public class SecureConsoleLogWritter : ConsoleLogWritter
    public class SecureConsoleLogWritter : ILogger //: ConsoleLogWritter
    {
        private readonly ILogger _logger;
        //public override void WriteError(string message)
        //{
        //    if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
        //    {
        //        base.WriteError(message);
        //    }
        //}
        public void WriteError(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
            {
                _logger.WriteError(message);
            }
        }
    }

    public class CompositLogWritter : ILogger
    {
        private readonly ILogger[] _loggers;

        public CompositLogWritter(params ILogger[] loggers)
        {
            if (loggers == null)
                throw new ArgumentNullException(nameof(loggers));

            if (loggers.Any(logger => logger == null))
            {
                string errorMessage = $"The {nameof(loggers)} contains an element equal to null.";
                throw new ArgumentNullException(errorMessage);
            }

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
