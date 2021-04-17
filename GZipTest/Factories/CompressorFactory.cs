using GZipTest.Interafaces;
using System.Threading;

namespace GZipTest.Factories
{
    class CompressorFactory : ICompressorFactory
    {
        public ICompressor Make(EventWaitHandle waitHandle, byte[][] inputBuffer, byte[][] outputBuffer)
        {
            return new Compressor(waitHandle, inputBuffer, outputBuffer);
        }
    }
}
