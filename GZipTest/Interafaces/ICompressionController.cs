using System.IO;

namespace GZipTest
{
    public interface ICompressionController
    {
        event CompressionController.SyncEventHandler SyncCounterResetEvent;

        void ReadAndInvokeCompress(Stream inputStream, Stream outputStream);
        void ReadAndInvokeDecompress(Stream inputStream, Stream outputStream);
        void ValidateArchive(Stream inputStream);
    }
}