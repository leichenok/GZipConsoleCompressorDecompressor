using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public abstract class GZipCDBase
    {
        protected int ThreadsNumber { get { return Environment.ProcessorCount; } }
        protected int BlockSize { get; set; }

        protected byte[][] InputArray { get; set; }
        protected byte[][] OutputArray { get; set; }


        public GZipCDBase()
        {
            InputArray = new byte[ThreadsNumber][];
            OutputArray = new byte[ThreadsNumber][];
        }


        public abstract int Compute(string inputFileName, string outputFileName);

        protected abstract void ProcessBlock(object index);
        protected abstract void ReadBlockFromInputFile(FileStream inputStream, int threadIndex);
        protected abstract void WriteBlockToOutputFile(FileStream inputStream, int threadIndex);
    }
}
