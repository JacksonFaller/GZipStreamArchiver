﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    interface ICompressor
    {
        void CompressBlock(object blockNumber);
        void DecompressBlock(object blockNumber);
    }
}
