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
        protected override int BlockSize { get; set; }

        public Decompressor()
        {
            InputArray = new byte[ThreadsNumber][];
            OutputArray = new byte[ThreadsNumber][];
        }


        public override int Compute(string inputFileName, string outputFileName)
        {
            try
            {
                int _dataPortionSize;
                //int compressedBlockLength;
                Thread[] threads;
                Console.Write("Decompressing...");
                byte[] buffer = new byte[8];

                var timer = new Stopwatch();
                timer.Start();

                using (var inputStream = new FileStream(inputFileName, FileMode.Open))
                {
                    using (var outputStream = new FileStream(outputFileName, FileMode.Append))
                    {
                        while (inputStream.Position < inputStream.Length)
                        {
                            Console.Write(".");
                            threads = new Thread[ThreadsNumber];
                            for (int portionCount = 0;
                                 (portionCount < ThreadsNumber) && (inputStream.Position < inputStream.Length);
                                 portionCount++)
                            {
                                inputStream.Read(buffer, 0, 8);
                                BlockSize = BitConverter.ToInt32(buffer, 4) - 1;
                                OutputArray[portionCount] = new byte[BlockSize + 1];
                                buffer.CopyTo(OutputArray[portionCount], 0);

                                inputStream.Read(OutputArray[portionCount], 8, BlockSize - 8);
                                _dataPortionSize = BitConverter.ToInt32(OutputArray[portionCount], BlockSize - 4);
                                InputArray[portionCount] = new byte[_dataPortionSize];

                                threads[portionCount] = new Thread(ProcessBlock);
                                threads[portionCount].Start(portionCount);
                            }

                            for (int portionCount = 0; (portionCount < ThreadsNumber) && (threads[portionCount] != null);)
                            {
                                if (threads[portionCount].ThreadState == System.Threading.ThreadState.Stopped)
                                {
                                    outputStream.Write(InputArray[portionCount], 0, InputArray[portionCount].Length);
                                    portionCount++;
                                }
                            }
                        }
                    }
                }
                

                timer.Stop();
                Console.WriteLine("Elapsed time: {0} ms", timer.ElapsedMilliseconds);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message);
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
