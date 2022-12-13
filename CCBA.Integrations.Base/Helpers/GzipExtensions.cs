using System;
using System.IO;
using System.IO.Compression;

namespace CCBA.Integrations.Base.Helpers
{
    public static class GzipExtensions
    {
        public static BinaryData GZipCompress(this BinaryData source)
        {
            using var to = new MemoryStream();
            using var gZipStream = new GZipStream(to, CompressionMode.Compress);
            source.ToStream().CopyTo(gZipStream);
            gZipStream.Flush();
            return BinaryData.FromBytes(to.ToArray());
        }

        public static BinaryData GZipDecompress(this BinaryData source)
        {
            using var from = new MemoryStream(source.ToArray());
            using var to = new MemoryStream();
            using var gZipStream = new GZipStream(from, CompressionMode.Decompress);
            gZipStream.CopyToAsync(to);
            return BinaryData.FromBytes(to.ToArray());
        }
    }
}