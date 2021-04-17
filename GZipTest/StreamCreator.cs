using GZipTest.Interafaces;
using System.IO;

namespace GZipTest
{
    class StreamCreator : IStreamCreator
    {
        private readonly string _sourceFile;
        private readonly string _targetFile;

        private Stream _source;
        private Stream _target;

        public StreamCreator(string sourceFile, string targetFile)
        {
            _sourceFile = sourceFile;
            _targetFile = targetFile;
        }

        public Stream Source
        {
            get
            {
                if (_source == null)
                    _source = new FileStream(_sourceFile, FileMode.Open, FileAccess.Read);

                return _source;
            }
        }

        public Stream Target
        {
            get
            {
                if (_target == null)
                    _target = new FileStream(_targetFile, FileMode.Create, FileAccess.Write);

                return _target;
            }
        }

        public void Dispose()
        {
            _target.Dispose();
            _source.Dispose();
        }
    }
}
