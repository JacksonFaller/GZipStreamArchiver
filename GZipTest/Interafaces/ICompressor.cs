using System;
using System.Collections.Generic;

namespace GZipTest.Interafaces
{
    public interface ICompressor
    {
        void CompressBlock(int blockNumber);
        void DecompressBlock(int blockNumber);
        void ResetCounter();
        int Target { get; set; }
        List<Exception> Errors { get; }
    }
}
