using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Папка, куда будут копироваться все файлы
        string targetFolder = @"C:\MyFiles";

        // Создаём папку, если её ещё нет (если уже есть - ничего не произойдёт)
        Directory.CreateDirectory(targetFolder);

        Console.WriteLine($"Copying files to the `{targetFolder}` folder");
        Console.WriteLine();

        // Бесконечный цикл: спрашиваем путь к файлу снова и снова
        while (true)
        {
            Console.WriteLine("Please enter the full path to the file:");
            string sourcePath = Console.ReadLine();

            // Если пользователь ничего не ввёл - выходим из программы
            if (string.IsNullOrWhiteSpace(sourcePath))
                break;

            // Проверяем, существует ли такой файл на диске
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine("File not found.");
                continue;
            }

            // Разбиваем путь на имя файла и расширение,
            // например "asd" и ".png"
            string name = Path.GetFileNameWithoutExtension(sourcePath);
            string ext = Path.GetExtension(sourcePath);

            // Собираем путь назначения в целевой папке
            string destPath = Path.Combine(targetFolder, name + ext);

            // Если файл с таким именем уже есть - подбираем свободное имя:
            // сначала name(1).ext, потом name(2).ext и так далее
            int number = 1;
            while (File.Exists(destPath))
            {
                destPath = Path.Combine(targetFolder, $"{name}({number}){ext}");
                number++;
            }

            // Читаем содержимое исходного файла целиком в массив байт (в оперативную память)
            byte[] fileData = File.ReadAllBytes(sourcePath);

            // Кладём эти байты в MemoryStream - поток, который работает с данными прямо в памяти
            using (MemoryStream memoryStream = new MemoryStream(fileData))
            // Создаём поток для записи в новый файл на диске
            using (FileStream outputStream = new FileStream(destPath, FileMode.Create))
            {
                // Переносим данные из памяти (MemoryStream) в файл на диске (FileStream)
                memoryStream.CopyTo(outputStream);
            }

            Console.WriteLine($"Copy completed: `{destPath}`");
            Console.WriteLine();
        }
    }
}