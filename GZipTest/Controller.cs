using System;
using System.IO;
using System.Threading;

namespace GZipTest
{   
    /// <summary>
    /// Controls read, compress/decompress and write operations
    /// </summary>
    public class Controller
    {
        public static readonly int ThreadNumber = 10;
        public static readonly int BufferSize = 1024 * 1024;

        private byte[][] _inputBuffer;
        private byte[][] _outputBuffer;

        public static int ThreadCount
        { get; private set; }

        private EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private readonly Operation _targetOperation;

        private Compressor _compressor;

        private string _sourceFile, _targetFile;
        public enum Operation { Compress, Decompress }

        /// <param name="targetOperation">comress/decompress operation to execute</param>
        /// <param name="sourceFile">file name with source data (input)</param>
        /// <param name="targetFile">file name with target data (output)</param>
        public Controller(Operation targetOperation, string sourceFile, string targetFile)
        {
            _targetOperation = targetOperation;
            _sourceFile = sourceFile;
            if (targetOperation == Operation.Compress)
            {
                _targetFile = Path.ChangeExtension(targetFile, Path.GetExtension(sourceFile) + ".gz");
            }
            else
            {
                _targetFile = Path.GetFileNameWithoutExtension(targetFile) + 
                    Path.GetExtension(Path.GetFileNameWithoutExtension(sourceFile));
            }
            _inputBuffer = new byte[ThreadNumber][];
            _outputBuffer = new byte[ThreadNumber][];
            _compressor = new Compressor(_waitHandle, _inputBuffer, _outputBuffer);
            ThreadCount = ThreadNumber;
        }

        /// <summary>
        /// Execute compress / decompress operation in accordance with the startup parameters
        /// </summary>
        public void ExecuteOperation()
        {
            using (var inputStream = new FileStream(_sourceFile, FileMode.Open, FileAccess.Read))
            {
                using (var outputStream = new FileStream(_targetFile, FileMode.Create, FileAccess.Write))
                {
                    if (_targetOperation == Operation.Compress)
                    {
                        while (inputStream.Position < inputStream.Length)
                        {
                            _compressor.ResetSyncCounter();
                            _waitHandle.Reset();

                            ReadAndInvokeCompress(inputStream);
                            _waitHandle.WaitOne();
                            Write(outputStream);
                        }
                    }
                    else // target opeartion - decompress
                    {
                        _compressor.ResetSyncCounter();
                        ReadAndInvokeDecompress(inputStream, outputStream);
                    }
                }
            }
        }

        /// <summary>
        /// Read from the source file to the buffer and invoke CompressBlock method in a new thread
        /// </summary>
        void ReadAndInvokeCompress(FileStream inputStream)
        {
            int dataSize;

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
        }

        /// <summary>
        /// Read from the source file to the buffer and invoke DecompressBlock method in a new thread
        /// </summary>
        /// <param name="inputStream">source file stream</param>
        /// <param name="outputStream">target file stream</param>
        void ReadAndInvokeDecompress(FileStream inputStream, FileStream outputStream)
        {
            int searchPos, checkSum;
            int blockPosition = 0, blockCount = 0, offset = 3, additionalBytes = 375;
            byte[] dataBuffer = new byte[BufferSize + additionalBytes];

            int bytesReaded = inputStream.Read(dataBuffer, 0, dataBuffer.Length);

            // Check is this file a GZip archive
            if (dataBuffer[0] == 0x1f && dataBuffer[1] == 0x8b && dataBuffer[2] == 0x08)
            {
                for (;;)
                {
                    for (searchPos = offset; searchPos < bytesReaded - 2; searchPos++)
                    {
                        if (dataBuffer[searchPos] == 0x1f && dataBuffer[searchPos + 1] == 0x8b && dataBuffer[searchPos + 2] == 0x08)
                        {
                            checkSum = BitConverter.ToInt32(dataBuffer, searchPos - 4);
                            if (checkSum == BufferSize)
                            {
                                _inputBuffer[blockCount] = new byte[searchPos - blockPosition];
                                Buffer.BlockCopy(dataBuffer, blockPosition, _inputBuffer[blockCount], 0, searchPos - blockPosition);
                                blockPosition = searchPos;
                                _outputBuffer[blockCount] = new byte[BufferSize];
                                
                                new Thread(_compressor.DecompressBlock).Start(blockCount); // Invoke Decompress method in a new thread
                                blockCount++;

                                if (blockCount == ThreadNumber) // Check that we have maximum number of theads invoked
                                {
                                    _waitHandle.WaitOne();
                                    Write(outputStream);
                                    _compressor.ResetSyncCounter();
                                    blockCount = 0;
                                }
                            }
                        }
                    }
                    // All data was readed from stream, copy last block to the buffer, decompress it
                    // and write the rest decompressed blocks to the file
                    if (bytesReaded < dataBuffer.Length) 
                    {
                        _inputBuffer[blockCount] = new byte[bytesReaded - blockPosition];
                        Buffer.BlockCopy(dataBuffer, blockPosition, _inputBuffer[blockCount], 0, bytesReaded - blockPosition);

                        checkSum = BitConverter.ToInt32(dataBuffer, bytesReaded - 4);
                        _outputBuffer[blockCount] = new byte[checkSum];

                        ThreadCount = blockCount + 1;
                        new Thread(_compressor.DecompressBlock).Start(blockCount); // Invoke decompress method in new thread
                        _waitHandle.WaitOne(); // Waiting for all threads to complete
                        Write(outputStream);
                        break;
                    }
                    offset = searchPos - blockPosition + 1; 
                    inputStream.Position = inputStream.Position - (bytesReaded - blockPosition);
                    blockPosition = 0;
                    bytesReaded = inputStream.Read(dataBuffer, 0, dataBuffer.Length);
                }
            }
            else
            {
                throw new InvalidFormatException();
            }
        }

        /// <summary>
        /// Write compressed / decompressed data from the buffer to the target file
        /// </summary>
        /// <param name="outputStream">target file stream to write</param>
        public void Write(FileStream outputStream)
        {
            for (int i = 0; i < ThreadCount; i++)
            {
                outputStream.Write(_outputBuffer[i], 0, _outputBuffer[i].Length);
            }
        }
    }
}
