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
                Thread[] threads;

                Console.Write("Compressing...");

                var timer = new Stopwatch();
                timer.Start();

                using (var inFile = new FileStream(inputFileName, FileMode.Open))
                {
                    using (var outFile = new FileStream(outputFileName, FileMode.Append))
                    {
                        while (inFile.Position < inFile.Length)
                        {
                            Console.Write("."); //indication

                            threads = new Thread[ThreadsNumber];

                            for (int i = 0; (i < ThreadsNumber) && (inFile.Position < inFile.Length); i++)
                            {
                                if (inFile.Length - inFile.Position <= BlockSize)
                                    BlockSize = (int)(inFile.Length - inFile.Position);

                                InputArray[i] = new byte[BlockSize];
                                inFile.Read(InputArray[i], 0, BlockSize);

                                threads[i] = new Thread(ProcessBlock);
                                threads[i].Start(i);
                            }

                            //пишем OutputArray в архивный файл
                            int portionCount = 0;

                            while ((portionCount < ThreadsNumber) && (threads[portionCount] != null))
                            {
                                if (threads[portionCount].ThreadState == System.Threading.ThreadState.Stopped)
                                {
                                    var bytes = BitConverter.GetBytes(OutputArray[portionCount].Length + 1);
                                    bytes.CopyTo(OutputArray[portionCount], 4);
                                    outFile.Write(OutputArray[portionCount], 0, OutputArray[portionCount].Length);

                                    portionCount++;
                                }
                            }
                        }
                    }
                }

                timer.Stop();
                Console.WriteLine("Elapsed time = {0} ms", timer.ElapsedMilliseconds);

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
