using System;

namespace GZipTest
{
    /// <summary>
    /// The exception that is thrown when mode parameter is invalid (not compress / decompress)
    /// </summary>
    class InvalidModeException : Exception
    {
        public new readonly string Message = "Mode parameter should be 'compress' or 'decompress'";
    }

    /// <summary>
    /// The exception that is thrown when one or more parameter were not set
    /// </summary>
    class MissingParametersException : Exception
    {
        public new readonly string Message = "All parameters should be set";
        public readonly string Usage = "Usage: GZipTest.exe [mode] [input file name] [output file name]\nmode - compress / decompress";
    }

    /// <summary>
    /// The exception that is thrown when input archive has invalid format (not GZip)
    /// </summary>
    class InvalidFormatException : Exception
    {
        public new readonly string Message = "Input archive file should be GZip format";
    }
}