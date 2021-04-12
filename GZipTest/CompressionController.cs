using GZipTest.Exceptions;
using System;
using System.IO;
using System.Threading;

namespace GZipTest
{
    /// <summary>
    /// Controls read, compress/decompress and write operations
    /// </summary>
    public partial class CompressionController : ICompressionController
    {
        public static int ThreadCount { get; private set; }

        public readonly int ThreadNumber = 10;
        public readonly int BufferSize = 1024 * 1024;

        private readonly byte[][] _inputBuffer;
        private readonly byte[][] _outputBuffer;

        private readonly ICompressor _compressor;

        private readonly EventWaitHandle _waitHandle;

        public delegate void SyncEventHandler();
        public event SyncEventHandler SyncCounterResetEvent;
        private void OnSyncCounterReset()
        {
            SyncCounterResetEvent?.Invoke();
        }

        /// <param name="targetOperation">comress/decompress operation to execute</param>
        /// <param name="sourceFile">file name with source data (input)</param>
        /// <param name="targetFile">file name with target data (output)</param>
        public CompressionController(int threadNumber, byte[][] inputBuffer, byte[][] outputBuffer,
            ICompressor compressor, EventWaitHandle waitHandle)
        {
            ThreadNumber = threadNumber;
            _waitHandle = waitHandle;
            _inputBuffer = inputBuffer;
            _outputBuffer = outputBuffer;
            _compressor = compressor;
            ThreadCount = ThreadNumber;
        }

        /// <summary>
        /// Read from the source file to the buffer and invoke CompressBlock method in a new thread
        /// </summary>
        public void ReadAndInvokeCompress(Stream inputStream, Stream outputStream)
        {
            int dataSize;

            while (inputStream.Position < inputStream.Length)
            {
                OnSyncCounterReset(); // Invoke SyncCounterResetEvent

                for (int blockCounter = 0;
                    (blockCounter < ThreadNumber) && (inputStream.Position < inputStream.Length);
                    blockCounter++)
                {
                    if (inputStream.Length - inputStream.Position <= BufferSize)
                    {
                        dataSize = (int)(inputStream.Length - inputStream.Position);
                        ThreadCount = blockCounter + 1;
                    }
                    else
                    {
                        dataSize = BufferSize;
                    }
                    _inputBuffer[blockCounter] = new byte[dataSize];
                    inputStream.Read(_inputBuffer[blockCounter], 0, dataSize);
                    new Thread(_compressor.CompressBlock).Start(blockCounter);
                }
                _waitHandle.WaitOne();
                Write(outputStream);
            }
        }

        /// <summary>
        /// Read from the source file to the buffer and invoke DecompressBlock method in a new thread
        /// </summary>
        /// <param name="inputStream">source file stream</param>
        /// <param name="outputStream">target file stream</param>
        public void ReadAndInvokeDecompress(Stream inputStream, Stream outputStream)
        {
            int searchPos, checkSum, bytesReaded, blockPosition;
            int blockCounter = 0, offset = 3, additionalBytes = 375;
            byte[] dataBuffer = new byte[BufferSize + additionalBytes];

            for (; ; )
            {
                bytesReaded = inputStream.Read(dataBuffer, 0, dataBuffer.Length);
                blockPosition = 0;

                for (searchPos = offset; searchPos < bytesReaded - 2; searchPos++)
                {
                    // 0x1f and  0x8b is "Magic numbers" that describe / identify GZIP compression
                    // 0x08 - compression method (Deflate)
                    if (dataBuffer[searchPos] == 0x1f && dataBuffer[searchPos + 1] == 0x8b && dataBuffer[searchPos + 2] == 0x08)
                    {
                        // input size of uncompressed data (4 bytes)
                        checkSum = BitConverter.ToInt32(dataBuffer, searchPos - 4);
                        if (checkSum == BufferSize)
                        {
                            _inputBuffer[blockCounter] = new byte[searchPos - blockPosition];
                            Buffer.BlockCopy(dataBuffer, blockPosition, _inputBuffer[blockCounter], 0, searchPos - blockPosition);
                            blockPosition = searchPos;
                            _outputBuffer[blockCounter] = new byte[BufferSize];

                            new Thread(_compressor.DecompressBlock).Start(blockCounter); // Invoke Decompress method in a new thread
                            blockCounter++;

                            if (blockCounter == ThreadNumber) // Check that we have maximum number of theads invoked
                            {
                                _waitHandle.WaitOne();
                                Write(outputStream);
                                OnSyncCounterReset(); // Invoke SyncCounterResetEvent
                                blockCounter = 0;
                            }
                        }
                    }
                }
                // All data was readed from stream, copy last block to the buffer, decompress it
                // and write the rest decompressed blocks to the file
                if (bytesReaded < dataBuffer.Length)
                {
                    _inputBuffer[blockCounter] = new byte[bytesReaded - blockPosition];
                    Buffer.BlockCopy(dataBuffer, blockPosition, _inputBuffer[blockCounter], 0, bytesReaded - blockPosition);

                    checkSum = BitConverter.ToInt32(dataBuffer, bytesReaded - 4);
                    _outputBuffer[blockCounter] = new byte[checkSum];

                    ThreadCount = blockCounter + 1;
                    new Thread(_compressor.DecompressBlock).Start(blockCounter); // Invoke decompress method in new thread
                    _waitHandle.WaitOne(); // Waiting for all threads to complete
                    Write(outputStream);
                    break;
                }
                offset = searchPos - blockPosition + 1;
                inputStream.Position -= (bytesReaded - blockPosition);
            }
        }

        /// <summary>
        /// Check is this file a GZip archive
        /// </summary>
        /// <param name="inputStream">source file stream</param>
        public void ValidateArchive(Stream inputStream)
        {
            byte[] dataBuffer = new byte[3];
            inputStream.Read(dataBuffer, 0, dataBuffer.Length);

            // 0x1f and  0x8b is "Magic numbers" that describe / identify GZIP compression
            // 0x08 - compression method (Deflate)
            if (dataBuffer[0] != 0x1f || dataBuffer[1] != 0x8b || dataBuffer[2] != 0x08)
            {
                throw new InvalidFormatException();
            }
            inputStream.Position = 0; // Reset the stream position
        }

        /// <summary>
        /// Write compressed / decompressed data from the buffer to the target file
        /// </summary>
        /// <param name="outputStream">target file stream to write</param>
        private void Write(Stream outputStream)
        {
            for (int i = 0; i < ThreadCount; i++)
            {
                outputStream.Write(_outputBuffer[i], 0, _outputBuffer[i].Length);
            }
        }
    }
}
