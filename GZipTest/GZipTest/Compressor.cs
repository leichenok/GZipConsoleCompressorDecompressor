using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    public class Compressor : GZipCDBase
    {
        public Compressor()
        {
            InputArray = new byte[ThreadsNumber][];
            OutputArray = new byte[ThreadsNumber][];
        }


        public override int Compute(string inputFileName, string outputFileName)
        {
            try
            {
                Console.WriteLine("Wait please...");

                using (var inFile = new FileStream(inputFileName, FileMode.Open))
                {
                    using (var outFile = new FileStream(outputFileName + ".gz", FileMode.Append))
                    {
                        Thread[] threads;

                        while (inFile.Position < inFile.Length)
                        {
                            threads = new Thread[ThreadsNumber];

                            int threadIndex;
                            for (threadIndex = 0; (threadIndex < ThreadsNumber) && (inFile.Position < inFile.Length); threadIndex++)
                            {
                                //если в конце остался блок != BlockSize
                                if (inFile.Length - inFile.Position <= DefaultBlockSize)
                                    DefaultBlockSize = (int)(inFile.Length - inFile.Position);

                                //читаем блок из файла в InputArray
                                InputArray[threadIndex] = new byte[DefaultBlockSize];
                                inFile.Read(InputArray[threadIndex], 0, DefaultBlockSize);

                                //стартуем процесс для сжатия блока
                                threads[threadIndex] = new Thread(ProcessBlock);
                                threads[threadIndex].Start(threadIndex);
                            }


                            for (int i = 0; i < threadIndex; i++)
                            {
                                threads[i].Join();
                            
                                //потоки пишем по очереди 1, 2, 3 и 4. Ждем пока нужный поток не закончит.
                                //если поток threads[index] завершился, то пишем в файл
                                //получаем размер блока в байтах и пишем его сразу после GZip Magic numbers и Compression method в свободную область со сдвигом в 4 
                                var bytes = BitConverter.GetBytes(OutputArray[i].Length + 1);
                                bytes.CopyTo(OutputArray[i], 4);

                                //записываем массив в файл в порядке чтения
                                outFile.Write(OutputArray[i], 0, OutputArray[i].Length);
                            }

                        }
                    }
                }

                Console.WriteLine("Done!");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return 1;
            }
        }

        protected override void ProcessBlock(object index)
        {
            using (MemoryStream output = new MemoryStream(InputArray[(int)index].Length))
            {
                using (GZipStream cs = new GZipStream(output, CompressionMode.Compress))
                {
                    cs.Write(InputArray[(int)index], 0, InputArray[(int)index].Length);
                }

                OutputArray[(int)index] = output.ToArray();
            }
        }
    }
}
