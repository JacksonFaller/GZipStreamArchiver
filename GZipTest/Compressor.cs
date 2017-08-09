using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    /// <summary>
    /// Contains compress / decompress file methods
    /// </summary>
    public class Compressor
    {
        private byte[][] _inputBuffer;
        private byte[][] _outputBuffer;

        private EventWaitHandle _waitHandle;    // synchronization event
        private int _syncCounter = 0;           // synchronization counter
        public void ResetSyncCounter()          // Resets _syncCounter;
        {
            _syncCounter = 0;
        }

        /// <param name="waitHandle">reference to synchronization event</param>
        /// <param name="inputBuffer">buffer for input data</param>
        /// <param name="outputBuffer">buffer for output data</param>
        public Compressor(EventWaitHandle waitHandle, byte[][] inputBuffer, byte[][] outputBuffer)
        {
            _waitHandle = waitHandle;
            _inputBuffer = inputBuffer;
            _outputBuffer = outputBuffer;
        }

        /// <summary>
        /// Compress block of data from the input buffer and write result to the output buffer (called in a new thread)
        /// </summary>
        /// <param name="blockNumber">Number of block to compress</param>
        public void CompressBlock(object blockNumber)
        {
            try
            {
                using (MemoryStream output = new MemoryStream(_inputBuffer[(int)blockNumber].Length))
                {
                    using (GZipStream gZipStream = new GZipStream(output, CompressionMode.Compress))
                    {
                        gZipStream.Write(_inputBuffer[(int)blockNumber], 0, _inputBuffer[(int)blockNumber].Length);
                    }
                    _outputBuffer[(int)blockNumber] = output.ToArray();
                }
                lock (_waitHandle)
                {
                    _syncCounter++;
                    if (_syncCounter == Controller.ThreadCount) _waitHandle.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Decompress block of data from the input buffer and write result to the output buffer (called in a new thread)
        /// </summary>
        /// <param name="blockNumber">Number of block to decompress</param>
        public void DecompressBlock(object blockNumber)
        {
            try
            {
                using (MemoryStream input = new MemoryStream(_inputBuffer[(int)blockNumber]))
                {
                    using (var gzipStream = new GZipStream(input, CompressionMode.Decompress))
                    {
                        gzipStream.Read(_outputBuffer[(int)blockNumber], 0, _outputBuffer[(int)blockNumber].Length);
                    }
                }
                lock (_waitHandle)
                {
                    _syncCounter++;
                    if (_syncCounter == Controller.ThreadCount) _waitHandle.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
