using GZipTest.Exceptions;
using GZipTest.Interafaces;
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
        private readonly int _threadNumber;
        private readonly IThreadStorage<int> _storage;
        public readonly int BufferSize = 1024 * 1024;
        public readonly int ExtraBytes = 375;
        public readonly int BlockSizeHeaderLength = 4;

        private readonly byte[][] _inputBuffer;
        private readonly byte[][] _outputBuffer;

        private readonly ICompressor _compressor;
        private readonly EventWaitHandle _waitHandle;

        /// <param name="targetOperation">comress/decompress operation to execute</param>
        /// <param name="sourceFile">file name with source data (input)</param>
        /// <param name="targetFile">file name with target data (output)</param>
        public CompressionController(int threadNumber, ICompressorFactory compressorFactory, IThreadStorage<int> storage)
        {
            _threadNumber = threadNumber;
            _storage = storage;
            _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _inputBuffer = new byte[threadNumber][];
            _outputBuffer = new byte[threadNumber][];
            _compressor = compressorFactory.Make(_waitHandle, _inputBuffer, _outputBuffer);
        }

        /// <summary>
        /// Read from the source file to the buffer and invoke CompressBlock method in a new thread
        /// </summary>
        public void ReadAndInvokeCompress(Stream inputStream, Stream outputStream)
        {
            int dataSize;
            while (inputStream.Position < inputStream.Length)
            {
                ResetCounterAndTarget();
                int threadCount = _threadNumber;
                for (int blockCounter = 0;
                    (blockCounter < _threadNumber) && (inputStream.Position < inputStream.Length);
                    blockCounter++)
                {
                    dataSize = (int)(inputStream.Length - inputStream.Position);
                    if (dataSize <= BufferSize)
                    {
                        threadCount = blockCounter + 1;
                        _compressor.SetTarget(threadCount);
                    }
                    else
                    {
                        dataSize = BufferSize;
                    }

                    _inputBuffer[blockCounter] = new byte[dataSize];
                    inputStream.Read(_inputBuffer[blockCounter], 0, dataSize);
                    _storage.EnqueueTask(new ThreadTask<int>(_compressor.CompressBlock, blockCounter));
                }
                _waitHandle.WaitOne();
                Write(outputStream, threadCount);
            }
        }

        /// <summary>
        /// Read from the source file to the buffer and invoke DecompressBlock method in a new thread
        /// </summary>
        /// <param name="inputStream">source file stream</param>
        /// <param name="outputStream">target file stream</param>
        public void ReadAndInvokeDecompress(Stream inputStream, Stream outputStream)
        {
            int searchPos, checkSum, bytesRead, blockPosition;
            int blockCounter = 0, offset = 3;
            byte[] dataBuffer = new byte[BufferSize + ExtraBytes];

            ResetCounterAndTarget();
            for (; ; )
            {
                bytesRead = inputStream.Read(dataBuffer, 0, dataBuffer.Length);
                blockPosition = 0;

                for (searchPos = offset; searchPos < bytesRead - 2; searchPos++)
                {
                    if (!IsGzipArchive(dataBuffer, searchPos))
                        continue;

                    checkSum = GetCheckSum(dataBuffer, searchPos);
                    if (checkSum != BufferSize)
                        continue;

                    DecompressBlock(dataBuffer, ref blockCounter, ref blockPosition, searchPos);
                    if (blockCounter == _threadNumber)
                    {
                        WaitAndWrite(outputStream, blockCounter);
                        blockCounter = 0;
                        ResetCounterAndTarget();
                    }
                }
                // All data was read from stream
                if (bytesRead < dataBuffer.Length)
                {
                    DecompressRest(dataBuffer, blockCounter, blockPosition, bytesRead);
                    WaitAndWrite(outputStream, blockCounter + 1);
                    break;
                }
                offset = searchPos - blockPosition + 1;
                inputStream.Position -= (bytesRead - blockPosition);
            }
        }

        /// <summary>
        /// Check if the file is a GZip archive
        /// </summary>
        /// <param name="inputStream">source file stream</param>
        public void ValidateArchive(Stream inputStream)
        {
            byte[] dataBuffer = new byte[3];
            inputStream.Read(dataBuffer, 0, dataBuffer.Length);

            if (!IsGzipArchive(dataBuffer, 0)) throw new InvalidFormatException();
            inputStream.Position = 0; // Reset the stream position
        }

        public void Dispose()
        {
            _storage.Dispose();
        }

        private void DecompressBlock(byte[] dataBuffer, ref int blockCounter, ref int blockPos, int searchPos)
        {
            FillInputBuffer(dataBuffer, blockCounter, blockPos, searchPos - blockPos);
            blockPos = searchPos;
            _outputBuffer[blockCounter] = new byte[BufferSize];

            _storage.EnqueueTask(new ThreadTask<int>(_compressor.DecompressBlock, blockCounter));
            blockCounter++;
        }

        private void DecompressRest(byte[] dataBuffer, int blockCounter, int blockPos, int bytesRead)
        {
            FillInputBuffer(dataBuffer, blockCounter, blockPos, bytesRead - blockPos);
            int checkSum = GetCheckSum(dataBuffer, bytesRead);
            _outputBuffer[blockCounter] = new byte[checkSum];

            _compressor.SetTarget(blockCounter + 1);
            _storage.EnqueueTask(new ThreadTask<int>(_compressor.DecompressBlock, blockCounter));
        }

        private int GetCheckSum(byte[] dataBuffer, int bytesRead)
        {
            return BitConverter.ToInt32(dataBuffer, bytesRead - BlockSizeHeaderLength);
        }

        private void ResetCounterAndTarget()
        {
            _compressor.ResetCounter();
            _compressor.SetTarget(_threadNumber);
        }

        private void WaitAndWrite(Stream stream, int blockCount)
        {
            _waitHandle.WaitOne();
            Write(stream, blockCount);
        }

        private void FillInputBuffer(byte[] dataBuffer, int blockCounter, int blockPosition, int length)
        {
            _inputBuffer[blockCounter] = new byte[length];
            Buffer.BlockCopy(dataBuffer, blockPosition, _inputBuffer[blockCounter], 0, length);
        }

        private bool IsGzipArchive(byte[] header, int index)
        {
            // 0x1f and  0x8b is "Magic numbers" that describe / identify GZIP compression
            // 0x08 - compression method (Deflate)
            return header[index] == 0x1f && header[index + 1] == 0x8b && header[index + 2] == 0x08;
        }

        /// <summary>
        /// Write compressed / decompressed data from the buffer to the target file
        /// </summary>
        /// <param name="outputStream">target file stream to write</param>
        private void Write(Stream outputStream, int blockCount)
        {
            for (int i = 0; i < blockCount; i++)
            {
                outputStream.Write(_outputBuffer[i], 0, _outputBuffer[i].Length);
            }
        }
    }
}
