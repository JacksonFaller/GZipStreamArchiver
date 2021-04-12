using System;

namespace GZipTest.Exceptions
{
    /// <summary>
    /// The exception that is thrown when input archive has invalid format (not GZip)
    /// </summary>
    [Serializable]
    class InvalidFormatException : Exception
    {
        public new readonly string Message = "Input archive file should be GZip format";
    }
}