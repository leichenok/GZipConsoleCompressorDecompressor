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
                bool lifeTime = true;
                while (lifeTime)
                {
                    Console.Write("Enter mode: ");
                    var mode = Console.ReadLine().ToLower();

                    Console.Write("Enter input file: ");
                    var inputFileName = Console.ReadLine();

                    Console.Write("Enter output file: ");
                    var outputFileName = Console.ReadLine();

                    if (!File.Exists(inputFileName))
                        throw new Exception("Не найден входной файл");

                    if (mode == "c" && File.Exists(outputFileName))
                        File.Delete(outputFileName);

                    int result;
                    switch (mode)
                    {
                        case "c":
                            var compressor = new Compressor();
                            result = compressor.Compute(inputFileName, outputFileName);
                            break;

                        case "d":
                            var decompressor = new Decompressor();
                            result = decompressor.Compute(inputFileName, outputFileName);
                            break;

                        default:
                            throw new Exception("Нераспознан режим работы. Выберете с - compress или d - decompress");
                    }

                    Console.WriteLine("result = {0}", result);

                    if (Console.ReadLine() == "exit")
                        lifeTime = false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            

            Console.ReadKey();
        }

        
    }
}
