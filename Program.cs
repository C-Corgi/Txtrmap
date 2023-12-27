using System;
using System.IO;
using BCnEncoder.Encoder;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using BCnEncoder.Shared.ImageFiles;
using System.Text;
using SixLabors.ImageSharp.Formats.Bmp;
using Microsoft.Toolkit.HighPerformance;
using txtr_converter;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using System.Windows.Media.Imaging;
//using txtr_converter.Extensions;
//using ColorRgba32 = txtr_converter.Extensions.ColorRgba32;


public class Program
{
    /*
    public static void EncodeImageToTxtr()
    {
        using Image<Rgba32> image = Image.Load<Rgba32>("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\custom_image.png");


        using FileStream fs = File.OpenWrite("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\logo_d.txtr");
        BinaryWriterTxtrtRaw(image,fs);
    }

    public static void DecodeTxtrToImage()
    {
        using FileStream fs = File.OpenRead("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\atlas_gameicons.txtr");
        BinaryReaderTxtr(in fs, out KtxFile ktx);

        BcDecoder decoder = new BcDecoder();
        using Image<Rgba32> image = decoder.DecodeToImageRgba32(ktx);
        

        using FileStream outFs = File.OpenWrite("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\custom_image.png");
        image.SaveAsPng(outFs);
    }

    public static void ConvertTxtrToKtx()
    {
        using FileStream fs = File.OpenRead("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\logo_d.txtr");
        BinaryReaderTxtr(in fs, out KtxFile ktx);

        using FileStream fs2 = File.OpenWrite("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\atlas_gameicons_custom.ktx");
        ktx.Write(fs2);
    }


    static void decode()
    {
        using FileStream fs = File.OpenRead("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\atlas_gameicons.dds");

        BcDecoder decoder = new BcDecoder();

        using Image<Rgba32> image = decoder.DecodeToImageRgba32(fs);


        using FileStream outFs = File.OpenWrite("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\custom.png");
        image.SaveAsPng(outFs);
    }

    static void encode()
    {
        using Image<Rgba32> image = Image.Load<Rgba32>("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\custom_image.png");

        BcEncoder encoder = new BcEncoder();

        encoder.OutputOptions.GenerateMipMaps = false;
        encoder.OutputOptions.Quality = CompressionQuality.Balanced;
        encoder.OutputOptions.Format = CompressionFormat.Rgba;
        encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

        using FileStream fs = File.OpenWrite("C:\\Users\\X605\\source\\repos\\txtrConverter\\txtr converter\\bin\\Debug\\net6.0\\logoSingle_raw.dds");
        encoder.EncodeToStream(image, fs);
    }

    */
    public static void BinaryWriterTxtrtRaw(in Image<Rgba32> image, FileStream fs)
    {
        var input = BCnEncoderExtensions.ImageToMemory2D(image);
        BcEncoder encoder = new BcEncoder();
        
        
        encoder.OutputOptions.GenerateMipMaps = false;
        encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
        encoder.OutputOptions.Format = CompressionFormat.Rgba;
        encoder.OutputOptions.FileFormat = OutputFileFormat.Ktx;

        var ktxFile = encoder.EncodeToKtx(input);
        TxtrFile txtrFile = new();
        txtrFile = txtrFile.ConvertKtxToTxtrFile(ktxFile, TxtrFormat.Raw_Single_Mip);
        txtrFile.Write(fs);
    }

    public static void BinaryWriterTxtrtCompressed(in Image<Rgba32> image, FileStream fs)
    {
        var input = BCnEncoderExtensions.ImageToMemory2D(image);
        BcEncoder encoder = new BcEncoder();


        encoder.OutputOptions.GenerateMipMaps = true;
        encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
        encoder.OutputOptions.Format = CompressionFormat.Bc3;
        encoder.OutputOptions.FileFormat = OutputFileFormat.Ktx;

        var ktxFile = encoder.EncodeToKtx(input);
        TxtrFile txtrFile = new();
        txtrFile = txtrFile.ConvertKtxToTxtrFile(ktxFile, TxtrFormat.Compressed_MipMaped_Dxt5);
        txtrFile.Write(fs);
    }

    public static void BinaryReaderTxtr(in FileStream fs, out KtxFile ktx, out bool isCompressed)
    {
        isCompressed = false;
        ktx = new KtxFile();
        using (var br = new BinaryReader(fs, Encoding.UTF8, true))
        {
            TxtrHeader txtrHeader = new();
            txtrHeader.identifier = br.ReadUInt32(); // txtr
            if (txtrHeader.identifier != 1381259348)
            {
                throw new FormatException("File doesn't contain txtr.");
            }

            txtrHeader.flag = (TxtrFlag)br.ReadUInt32(); // SW uses value 3 and CC2 value 2
            if (txtrHeader.flag != TxtrFlag.SW)
            {
                throw new FormatException("Uknown flag. File is not from SW.");
            }

            txtrHeader.format = (TxtrFormat)br.ReadUInt32(); // value 2 uncompressed, value 3 compressed bc3/dxt5
            if (txtrHeader.format != TxtrFormat.Raw_Single_Mip && txtrHeader.format != TxtrFormat.Compressed_MipMaped_Dxt5)
            {
                throw new FormatException("Uknown type.");

            }

            txtrHeader.padding = br.ReadByte(); // padding? 
            txtrHeader.height = br.ReadUInt16();
            txtrHeader.width = br.ReadUInt16();
            txtrHeader.mipMapCount = br.ReadUInt16();

            Console.WriteLine("identifier {0}", txtrHeader.identifier);
            Console.WriteLine("flag {0}", txtrHeader.flag);
            Console.WriteLine("type {0}", txtrHeader.format);
            Console.WriteLine("padding {0}", txtrHeader.padding);
            Console.WriteLine("height {0}", txtrHeader.height);
            Console.WriteLine("width {0}", txtrHeader.width);
            Console.WriteLine("mipMapCount {0}", txtrHeader.mipMapCount);

            switch (txtrHeader.format)
            {
                case TxtrFormat.Raw_Single_Mip:
                    var rawHeader = KtxHeader.InitializeCompressed(txtrHeader.width, txtrHeader.height, GlInternalFormat.GlRgba8, GlFormat.GlRgba);
                    ktx.header = rawHeader;

                    for (int mip = 0; mip < 1; mip++)
                    {
                        uint dataSize = br.ReadUInt32();
                        byte[] data = br.ReadBytes((int)dataSize);
                        Console.WriteLine("dataSize {0}", dataSize);

                        ktx.MipMaps.Add(new KtxMipmap((uint)data.Length, (uint)txtrHeader.width, (uint)txtrHeader.height, 1));
                        ktx.MipMaps[mip].Faces[0] = new KtxMipFace(data, (uint)txtrHeader.width, (uint)txtrHeader.height);
                    }
                    ktx.header.NumberOfFaces = 1;
                    ktx.header.NumberOfMipmapLevels = txtrHeader.mipMapCount;
                    break;
                case TxtrFormat.Compressed_MipMaped_Dxt5:
                    var compHeader = KtxHeader.InitializeCompressed(txtrHeader.width, txtrHeader.height, GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext, GlFormat.GlRgba);
                    ktx.header = compHeader;
                    isCompressed = true;

                    //var mipChain = GenerateMipChain(ref width, ref height, ref mipMapCount);
                    for (int mipmaps = 0; mipmaps < txtrHeader.mipMapCount; mipmaps++)
                    {
                        uint dataSize = br.ReadUInt32();
                        byte[] data = br.ReadBytes((int)dataSize);
                        Console.WriteLine("dataSize {0}", dataSize);
                        /*
                        ktx.MipMaps.Add(new KtxMipmap((uint)data.Length, (uint)mipChain[mipmaps].Width, (uint)mipChain[mipmaps].Height, 1));
                        ktx.MipMaps[mipmaps].Faces[0] = new KtxMipFace(data, (uint)mipChain[mipmaps].Width,   (uint)mipChain[mipmaps].Height);
                        */
                        ktx.MipMaps.Add(new KtxMipmap((uint)data.Length, (uint)txtrHeader.width, (uint)txtrHeader.height, 1));
                        ktx.MipMaps[mipmaps].Faces[0] = new KtxMipFace(data, (uint)txtrHeader.width, (uint)txtrHeader.height);

                    }
                    ktx.header.NumberOfFaces = 1;
                    ktx.header.NumberOfMipmapLevels = txtrHeader.mipMapCount;
                    break;
            }
            
        }
    }
    

}