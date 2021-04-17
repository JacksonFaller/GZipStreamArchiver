using System.Threading;

namespace GZipTest.Interafaces
{
    public interface ICompressorFactory
    {
        ICompressor Make(EventWaitHandle waitHandle, byte[][] inputBuffer, byte[][] outputBuffer);
    }
}
