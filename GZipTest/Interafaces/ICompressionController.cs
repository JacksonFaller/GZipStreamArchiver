using System;
using System.IO;

namespace GZipTest.Interafaces
{
    public interface ICompressionController : IDisposable
    {
        void ReadAndInvokeCompress(Stream inputStream, Stream outputStream);
        void ReadAndInvokeDecompress(Stream inputStream, Stream outputStream);
        void ValidateArchive(Stream inputStream);
    }
}