using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Markup;
using BCnEncoder.Encoder;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using BCnEncoder.Shared.ImageFiles;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using Microsoft.Win32;
using Txtrmap.View.UserControls;
using System.Diagnostics;

namespace Txtrmap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MemoryStream imageStream;
        public Image<Rgba32> image32;
        public bool isCompressedImage = false;

        public MainWindow()
        {
            InitializeComponent();
            SetDropDownMenuToBeRightAligned();

            string[] e = Environment.GetCommandLineArgs();

            if (e != null && e.Length > 1)
            {
                string filePath = e[1];
                LoadFile(filePath);
                
            }

            
        }


        public void LoadFile(string path)
        {
            isCompressedImage = false;
            FileName.Content = Path.GetFileName(path);
            using FileStream fs = File.OpenRead(path);
            string extension = Path.GetExtension(path);
            switch (extension)
            {
                case ".txtr":
                    {
                        try
                        {
                            Program.BinaryReaderTxtr(in fs, out KtxFile ktx, out bool isCompressed);
                            isCompressedImage = isCompressed;
                            BcDecoder decoder = new BcDecoder();

                            using Image<Rgba32> image = decoder.DecodeToImageRgba32(ktx);
                            image.Mutate(x => x.Flip(FlipMode.Vertical));
                            image32 = image.Clone();
                            Dimensions.Content = image.Size;

                            MemoryStream outFs = new();
                            image.SaveAsPng(outFs);
                            imageStream = outFs;

                            ViewFile();
                        }
                        catch (Exception)
                        {
                            throw new Exception("Failed to load .txtr");
                        }
                    
                    }
                    break;
                case ".png":
                    {
                        try
                        {
                            BcDecoder decoder = new BcDecoder();

                            using Image<Rgba32> image = Image.Load<Rgba32>(path);
                            image32 = image.Clone();
                            Dimensions.Content = image.Size;

                            MemoryStream outFs = new();
                            image.SaveAsPng(outFs);
                            imageStream = outFs;

                            ViewFile();
                        }
                        catch (Exception)
                        {

                            throw new Exception("Failed to load .png"); ;
                        }
 
                    }
                    break;
            }

            string text = "Is Txtr Compressed= ";
            if (isCompressedImage == true)
            {
                IsTXTRCmpressed.Content = text + "True";
            }
            else
            {
                IsTXTRCmpressed.Content = text + "False";

            }


        }

        public void SaveFile(string path, bool flag)
        {
            string extension = Path.GetExtension(path);
            switch (extension)
            {
                case ".txtr":
                    {
                        try
                        {
                            using FileStream fileStream = File.Create(path);
                            {
                                Image<Rgba32> imageR = image32.Clone();
                                imageR.Mutate(x => x.Flip(FlipMode.Vertical));
                                if (flag)
                                {
                                    Program.BinaryWriterTxtrtCompressed(imageR, fileStream);
                                }
                                else
                                {
                                    Program.BinaryWriterTxtrtRaw(imageR, fileStream);
                                }
                                imageR.Dispose();
                            }
                        }
                        catch (Exception)
                        {

                            throw new Exception("Failed to Save .txtr");
                        }
                        
                    }
                    break;
                case ".png":
                    {
                        try
                        {
                            using FileStream fileStream = File.Create(path);
                            {
                                fileStream.Write(imageStream.ToArray());
                            }
                        }
                        catch (Exception)
                        {

                            throw new Exception("Failed to Save .png");
                        }
                      
                    }
                    break;
            }

            
        }

        public void ViewFile()
        {
            using (var stream = imageStream)
            {
                var bi = BitmapFrame.Create(stream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                texture.Source = bi;
            }
        }
        private static void SetDropDownMenuToBeRightAligned()
        {
            var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            Action setAlignmentValue = () =>
            {
                if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null) menuDropAlignmentField.SetValue(null, false);
            };

            setAlignmentValue();

            SystemParameters.StaticPropertyChanged += (sender, e) =>
            {
                setAlignmentValue();
            };
        }
    }
}
