using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

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
                Console.WriteLine(ex.Message);
                Console.WriteLine(-1);
            }
        }

        private static bool IsArgumentsValid(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Error.WriteLine("Неверное количество аргументов1");
                return false;
            }

            string mode = args[0].ToLower();
            if (string.IsNullOrEmpty(mode) || (mode != "compress" && mode != "decompress"))
            {
                Console.Error.WriteLine("Неверно указан режим. Используйте ключевые слова compress/decompress");
                return false;
            }

            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Указанный входной файл не существует");
                return false;
            }

            var fileName = Path.GetFileName(args[2]);
            string dir = args[2].Replace(fileName, string.Empty);
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("Директория для выходного файла не существует");
                return false;
            }

            return true;
        }


    }
}
