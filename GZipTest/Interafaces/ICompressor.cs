namespace GZipTest.Interafaces
{
    public interface ICompressor
    {
        void CompressBlock(int blockNumber);
        void DecompressBlock(int blockNumber);
        void ResetCounter();
        void SetTarget(int target);
    }
}
