using GZipTest.Interafaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    /// <summary>
    /// Contains compress / decompress file methods
    /// </summary>
    public class Compressor : ICompressor
    {
        private readonly byte[][] _inputBuffer;
        private readonly byte[][] _outputBuffer;

        private readonly EventWaitHandle _waitHandle;    // synchronization event
        private int _syncCounter;           // synchronization counter
        private int _target;
        private readonly object _locker = new object();

        public List<Exception> Errors { get; private set; } = new List<Exception>();

        /// <param name="waitHandle">reference to synchronization event</param>
        /// <param name="inputBuffer">buffer for input data</param>
        /// <param name="outputBuffer">buffer for output data</param>
        /// <param name="controller">countroller instance for subscribing to SyncCounterResetEvent</param>
        public Compressor(EventWaitHandle waitHandle, byte[][] inputBuffer, byte[][] outputBuffer)
        {
            _waitHandle = waitHandle;
            _inputBuffer = inputBuffer ?? throw new ArgumentNullException(nameof(inputBuffer));
            _outputBuffer = outputBuffer ?? throw new ArgumentNullException(nameof(outputBuffer));
        }

        public void ResetCounter() => _syncCounter = 0;

        public int Target
        {
            get { lock (_locker) return _target; }
            set { lock (_locker) _target = value; }
        }


        /// <summary>
        /// Compress block of data from the input buffer and write result to the output buffer (called in a new thread)
        /// </summary>
        /// <param name="blockNumber">Number of block to compress</param>
        public void CompressBlock(int blockNumber)
        {
            try
            {
                using (MemoryStream output = new MemoryStream(_inputBuffer[blockNumber].Length))
                {
                    using (GZipStream gZipStream = new GZipStream(output, CompressionMode.Compress))
                    {
                        gZipStream.Write(_inputBuffer[blockNumber], 0, _inputBuffer[blockNumber].Length);
                    }
                    _outputBuffer[blockNumber] = output.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Errors.Add(ex);
            }
            finally
            {
                IncrementCounter();
            }
        }

        /// <summary>
        /// Decompress block of data from the input buffer and write result to the output buffer (called in a new thread)
        /// </summary>
        /// <param name="blockNumber">Number of block to decompress</param>
        public void DecompressBlock(int blockNumber)
        {
            try
            {
                using (MemoryStream input = new MemoryStream(_inputBuffer[blockNumber]))
                {
                    using (var gzipStream = new GZipStream(input, CompressionMode.Decompress))
                    {
                        gzipStream.Read(_outputBuffer[blockNumber], 0, _outputBuffer[blockNumber].Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Errors.Add(ex);

            }
            finally
            {
                IncrementCounter();
            }
        }

        private void IncrementCounter()
        {
            int counter = _syncCounter;
            while (Interlocked.CompareExchange(ref _syncCounter, counter + 1, counter) != counter)
            {
                counter = _syncCounter;
                Thread.SpinWait(1);
            }

            if (counter == Target - 1) _waitHandle.Set();
        }
    }
}
