using System;
using System.IO;

namespace GZipTest
{
    interface IStreamCreator : IDisposable
    {
        Stream Source { get; }
        Stream Target { get; }
    }
}