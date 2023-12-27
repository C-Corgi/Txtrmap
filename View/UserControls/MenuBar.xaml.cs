using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using txtr_converter;

namespace Txtrmap.View.UserControls
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {

        
        public MenuBar()
        {
            InitializeComponent();

        }



        public void btnOpen(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "SW Texture Files | *.txtr|PNG | *.png|All Image Files |*.txtr;*.png";
            fileDialog.FilterIndex = 3;

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;

                var window = Application.Current.MainWindow;
                (window as MainWindow).LoadFile(path);

            }

        }

        public void btnSave(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "SW Texture File | *.txtr|SW Compressed Texture file | *.txtr|PNG | *.png|All Files (*.*)|*.*";

            bool? success = saveDialog.ShowDialog();
            if (success == true)
            {
                bool flag = false;
                if (saveDialog.FilterIndex == 2)
                {
                    flag = true;
                }

                string path = saveDialog.FileName;

                var window = Application.Current.MainWindow;
                (window as MainWindow).SaveFile(path, flag);
            }
        }

        public void btnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
