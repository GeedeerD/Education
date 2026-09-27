using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.IO;

namespace FileCopyConsole
{
    class Program
    {
        static readonly string DestinationFolder = @"C:\MyFiles";

        static void Main(string[] args)
        {
            Directory.CreateDirectory(DestinationFolder);

            Console.WriteLine($"Копирование файлов в папку `{DestinationFolder}`");

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Введите полный путь к файлу:");
                string sourcePath = Console.ReadLine()?.Trim().Trim('`', '"');

                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    Console.WriteLine("Путь не может быть пустым. Попробуйте снова или нажмите Ctrl+C для выхода.");
                    continue;
                }

                if (!File.Exists(sourcePath))
                {
                    Console.WriteLine($"Файл не найден: `{sourcePath}`");
                    continue;
                }

                try
                {
                    string destinationPath = CopyFileViaMemoryStream(sourcePath, DestinationFolder);
                    Console.WriteLine($"Копирование завершено: `{destinationPath}`");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при копировании файла: {ex.Message}");
                }
            }
        }

        static string CopyFileViaMemoryStream(string sourceFilePath, string destinationFolder)
        {
            string fileName = Path.GetFileName(sourceFilePath);
            string destinationPath = GetAvailableFilePath(destinationFolder, fileName);

            using (var memoryStream = new MemoryStream())
            {
                using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                {
                    sourceStream.CopyTo(memoryStream);
                }

                memoryStream.Position = 0;

                using (var destinationStream = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write))
                {
                    memoryStream.CopyTo(destinationStream);
                }
            }

            return destinationPath;
        }

        static string GetAvailableFilePath(string folder, string fileName)
        {
            string fullPath = Path.Combine(folder, fileName);

            if (!File.Exists(fullPath))
                return fullPath;

            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            int counter = 1;
            string newFullPath;

            do
            {
                string newFileName = $"{nameWithoutExt}({counter}){extension}";
                newFullPath = Path.Combine(folder, newFileName);
                counter++;
            }
            while (File.Exists(newFullPath));

            return newFullPath;
        }
    }
}