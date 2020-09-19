using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    public class Compressor : GZipCDBase
    {
        public Compressor()
        {
            BlockSize = 1048576; //1MB
        }


        public override int Compute(string inputFileName, string outputFileName)
        {
            try
            {
                using (var inputStream = new FileStream(inputFileName, FileMode.Open))
                {
                    using (var outputStream = new FileStream(outputFileName, FileMode.Create))
                    {
                        Thread[] threads;

                        while (inputStream.Position < inputStream.Length)
                        {
                            threads = new Thread[ThreadsNumber];

                            int threadIndex;
                            for (threadIndex = 0; (threadIndex < ThreadsNumber) && (inputStream.Position < inputStream.Length); threadIndex++)
                            {
                                ReadBlockFromInputFile(inputStream, threadIndex);

                                threads[threadIndex] = new Thread(ProcessBlock);
                                threads[threadIndex].Start(threadIndex);
                            }


                            for (int i = 0; i < threadIndex; i++)
                            {
                                threads[i].Join();

                                WriteBlockToOutputFile(outputStream, i);
                            }

                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return 1;
            }
        }

        protected override void WriteBlockToOutputFile(FileStream outputStream, int threadIndex)
        {
            //пишем размер блока сразу после GZip Magic numbers и Compression method
            var blockSizeInBytes = BitConverter.GetBytes(OutputArray[threadIndex].Length + 1);
            blockSizeInBytes.CopyTo(OutputArray[threadIndex], 4);

            outputStream.Write(OutputArray[threadIndex], 0, OutputArray[threadIndex].Length);
        }
        protected override void ReadBlockFromInputFile(FileStream inputStream, int threadIndex)
        {
            if (inputStream.Length - inputStream.Position <= BlockSize)
                BlockSize = (int)(inputStream.Length - inputStream.Position);

            InputArray[threadIndex] = new byte[BlockSize];
            inputStream.Read(InputArray[threadIndex], 0, BlockSize);
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
