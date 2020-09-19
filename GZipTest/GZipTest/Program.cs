using System;
using System.IO;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int result = 1;

                if (IsArgumentsValid(args))
                {
                    if (File.Exists(args[2]))
                        File.Delete(args[2]);

                    switch (args[0])
                    {
                        case "compress":
                            var compressor = new Compressor();
                            result = compressor.Compute(args[1], args[2]);
                            break;

                        case "decompress":
                            var decompressor = new Decompressor();
                            result = decompressor.Compute(args[1], args[2]);
                            break;

                        default:
                            throw new Exception("Нераспознан режим работы. Выберете compress или decompress");
                    }
                }

                Console.WriteLine(result);
            }
            catch(Exception ex)
            {
                WriteError (ex.Message);
                Console.WriteLine(1);
            }
        }

        private static bool IsArgumentsValid(string[] args)
        {
            if (args.Length != 3)
            {
                WriteError("Неверное количество аргументов");
                return false;
            }

            string mode = args[0].ToLower();
            if (string.IsNullOrEmpty(mode) || (mode != "compress" && mode != "decompress"))
            {
                WriteError("Неверно указан режим. Используйте ключевые слова compress/decompress");
                return false;
            }

            if (args[1] == args[2])
            {
                WriteError("Входной и выходной файлы не могут содержать одинаковые имена");
                return false;
            }

            if (!File.Exists(args[1]))
            {
                WriteError("Указанный исходный файл не существует");
                return false;
            }

            var fileName = Path.GetFileName(args[2]);
            string dir = args[2].Replace(fileName, string.Empty);
            if (!Directory.Exists(dir))
            {
                WriteError("Неверный путь для выходного файла");
                return false;
            }

            return true;
        }
        private static void WriteError(string message)
        {
            Console.WriteLine($"Error: {message}");
        }
    }
}
