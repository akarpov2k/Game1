using System;
using System.IO;
using AnomalyPath.Tests;

namespace AnomalyPath.ConsoleTests
{
    /// <summary>Консольный запуск того же набора тестов, что и в Unity EditMode.</summary>
    public static class Program
    {
        /// <summary>Точка входа: путь к данным можно передать аргументом.</summary>
        /// <param name="args">Необязательный путь к папке данных.</param>
        /// <returns>0 — все проверки прошли, 1 — есть падения.</returns>
        public static int Main(string[] args)
        {
            var dataRoot = args.Length > 0 ? Path.GetFullPath(args[0]) : FindDataRoot();
            var result = TestSuite.Run(dataRoot, Console.WriteLine);
            return result.Success ? 0 : 1;
        }

        private static string FindDataRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "Assets", "Data");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Не найдена папка Assets/Data: передайте путь аргументом");
        }
    }
}