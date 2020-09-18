using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest
{
    public class Decompressor : GZipCDBase
    {
        protected override int DefaultBlockSize { get; set; }


        public Decompressor()
        {
            InputArray = new byte[ThreadsNumber][];
            OutputArray = new byte[ThreadsNumber][];
        }


        public override int Compute(string inputFileName, string outputFileName)
        {
            try
            {     
                Console.Write("Please wait...");

                using (var inputStream = new FileStream(inputFileName, FileMode.Open))
                {
                    using (var outputStream = new FileStream(outputFileName, FileMode.Append))
                    {
                        Thread[] threads;

                        while (inputStream.Position < inputStream.Length)
                        {
                            threads = new Thread[ThreadsNumber];

                            int threadIndex;
                            for (threadIndex = 0; (threadIndex < ThreadsNumber) && (inputStream.Position < inputStream.Length); threadIndex++)
                            {
                                //читаем первые 8 байт блока 
                                byte[] blockHeader = new byte[8];
                                inputStream.Read(blockHeader, 0, 8);

                                //берем из последние 4 и получаем из них размер блока
                                DefaultBlockSize = BitConverter.ToInt32(blockHeader, 4) - 1;
                                OutputArray[threadIndex] = new byte[DefaultBlockSize + 1];
                                blockHeader.CopyTo(OutputArray[threadIndex], 0);

                                inputStream.Read(OutputArray[threadIndex], 8, DefaultBlockSize - 8);
                                int decompressedBlockSize = BitConverter.ToInt32(OutputArray[threadIndex], DefaultBlockSize - 4);
                                InputArray[threadIndex] = new byte[decompressedBlockSize];

                                threads[threadIndex] = new Thread(ProcessBlock);
                                threads[threadIndex].Start(threadIndex);
                            }

                            for (int i = 0; i < threadIndex; i++)
                            {
                                threads[i].Join();
                                outputStream.Write(InputArray[i], 0, InputArray[i].Length);
                            }
                        }
                    }
                }
                
                Console.WriteLine("Done!");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return 1;
            }
        }

        protected override void ProcessBlock(object index)
        {
            using (MemoryStream input = new MemoryStream(OutputArray[(int)index]))
            {
                using (GZipStream ds = new GZipStream(input, CompressionMode.Decompress))
                {
                    ds.Read(InputArray[(int)index], 0, InputArray[(int)index].Length);
                }
            }
        }
    }
}
