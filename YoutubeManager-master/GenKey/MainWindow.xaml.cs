using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Windows;
using TqkLibrary.WinApi;

namespace GenKey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BT_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BT_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            saveFileDialog.Filter = "json file (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.FileName = "key.txt";
            saveFileDialog.ValidateNames = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName)) sw.Write(Key.Text);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Key.Text = HardWareId.CalcHashVolumeSerialNumber("{ECFEDBF3-B503-49C1-9D67-D0AF8566FA63}");
        }
    }
}