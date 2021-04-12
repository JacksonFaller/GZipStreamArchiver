using System;

namespace GZipTest.Exceptions
{
    /// <summary>
    /// The exception that is thrown when mode parameter is invalid (not compress / decompress)
    /// </summary>
    [Serializable]
    class InvalidModeException : Exception
    {
        public new readonly string Message = "Mode parameter should be 'compress' or 'decompress'";
    }
}
