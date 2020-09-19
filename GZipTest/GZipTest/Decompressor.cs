using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    public class Decompressor : GZipCDBase
    {
        public Decompressor()
        {
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
            outputStream.Write(InputArray[threadIndex], 0, InputArray[threadIndex].Length);
        }
        protected override void ReadBlockFromInputFile(FileStream inputStream, int threadIndex)
        {
            //читаем первые 8 байт блока 
            byte[] blockHeader = new byte[8];
            inputStream.Read(blockHeader, 0, 8);

            //берем из последние 4 и получаем из них размер блока
            BlockSize = BitConverter.ToInt32(blockHeader, 4) - 1;
            OutputArray[threadIndex] = new byte[BlockSize + 1];
            blockHeader.CopyTo(OutputArray[threadIndex], 0);

            inputStream.Read(OutputArray[threadIndex], 8, BlockSize - 8);
            int decompressedBlockSize = BitConverter.ToInt32(OutputArray[threadIndex], BlockSize - 4);
            InputArray[threadIndex] = new byte[decompressedBlockSize];
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
