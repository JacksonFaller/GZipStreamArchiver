using System;
using System.IO;

namespace GZipTest.Interafaces
{
    interface IStreamCreator : IDisposable
    {
        Stream Source { get; }
        Stream Target { get; }
    }
}