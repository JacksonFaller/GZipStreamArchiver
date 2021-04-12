using System;

namespace GZipTest.Exceptions
{
    /// <summary>
    /// The exception that is thrown when one or more parameter were not set
    /// </summary>
    [Serializable]
    class MissingParametersException : Exception
    {
        public new readonly string Message = "All parameters should be set";
        public readonly string Usage = "Usage: GZipTest.exe [mode] [input file name] [output file name]\nmode - compress / decompress";
    }
}
