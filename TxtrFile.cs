using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using BCnEncoder.Shared.ImageFiles;
using BCnEncoder.Shared;
using static System.Text.Encoding;

namespace txtr_converter
{
    public class TxtrFile
    {
        public TxtrHeader header;
        public List<TxtrMipmap> MipMaps { get; set; } = new List<TxtrMipmap>();
        public TxtrFile() { }
        public TxtrFile(TxtrHeader header)
        {
            this.header = header;
        }
        public void Write(Stream s)
        {
            if (header.mipMapCount < 1)
            {
                throw new InvalidOperationException("Txtr structure should have at least 1 mipmap level");
            }


            using (var bw = new BinaryWriter(s, UTF8, true))
            {
                //bw.WriteStruct(header);
                bw.Write(header.identifier);
                bw.Write(((uint)header.flag));
                bw.Write((uint)header.format);
                bw.Write(header.padding);
                bw.Write(header.height);
                bw.Write(header.width);
                bw.Write(header.mipMapCount);

                for (var mip = 0; mip < header.mipMapCount; mip++)
                {
                    var imageSize = MipMaps[mip].SizeInBytes;
                    bw.Write(imageSize);

                    bw.Write(MipMaps[mip].Faces[0].Data);

                    var mipPaddingBytes = 3 - (imageSize + 3) % 4;
                    bw.AddPadding(mipPaddingBytes);
                }

            }
        }

        public TxtrFile ConvertKtxToTxtrFile(KtxFile ktx, TxtrFormat format)
        {
            TxtrFile txtr = new();
            txtr.header = TxtrHeader.Initialize(format, (int)ktx.header.PixelHeight, (int)ktx.header.PixelWidth, ktx.MipMaps.Count);
            var mipmaps = ktx.MipMaps;
            foreach (var mips in mipmaps)
            {
                TxtrMipmap mipmap = new TxtrMipmap(mips.SizeInBytes, mips.Width, mips.Height);
                List< TxtrMipFace > faces= new List< TxtrMipFace >();
                foreach (var face in mips.Faces)
                {
                    faces.Add( new TxtrMipFace(face.Data,face.Width,face.Height));
                }
                mipmap.Faces = faces.ToArray();
                txtr.MipMaps.Add(mipmap);
            }
            return txtr;
        }
    }

    public class TxtrMipFace
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint SizeInBytes { get; }
        public byte[] Data { get; set; }

        public TxtrMipFace(byte[] data, uint width, uint height)
        {
            Width = width;
            Height = height;
            SizeInBytes = (uint)data.Length;
            Data = data;
        }
    }

    public class TxtrMipmap
    {
        public uint SizeInBytes { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public TxtrMipFace[] Faces { get; set; }
        public TxtrMipmap(uint sizeInBytes, uint width, uint height)
        {
            SizeInBytes = sizeInBytes;
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Faces = new TxtrMipFace[1];
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct TxtrHeader
    {
        public uint identifier; // txtr
        public TxtrFlag flag; // SW uses value 3 and CC2 value 2
        public TxtrFormat format; // value 2 uncompressed, value 3 compressed bc3/dxt5
        public Byte padding; // padding? 
        public ushort height;
        public ushort width;
        public ushort mipMapCount;

        public static TxtrHeader Initialize(TxtrFormat format, int height, int width, int mipMapCount = 1)
        {
            TxtrHeader header = new();
            header.identifier = 1381259348;
            header.flag = TxtrFlag.SW;
            header.format = format;
            header.padding = 0;
            header.height = (ushort)height;
            header.width = (ushort)width;
            header.mipMapCount = (ushort)mipMapCount;
            return header;
        }
    }



    public enum TxtrFlag : uint
    {
        SW = 3,
        CC2 = 2
    }
    
    public enum TxtrFormat : uint
    {
        Raw_Single_Mip = 2,
        Compressed_MipMaped_Dxt5 = 3
    }


}
