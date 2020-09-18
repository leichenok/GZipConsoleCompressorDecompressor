using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public abstract class GZipCDBase
    {
        protected int ThreadsNumber { get { return Environment.ProcessorCount; } }
        protected byte[][] InputArray { get; set; }
        protected byte[][] OutputArray { get; set; }

        protected virtual int BlockSize { get; set; } = 1048576;

        public abstract int Compute(string inputFileName, string outputFileName);
        protected abstract void ProcessBlock(object index);
    }
}
