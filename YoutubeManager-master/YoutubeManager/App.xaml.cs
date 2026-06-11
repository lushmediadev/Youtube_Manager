using System;
using System.Windows;
#if Key
using System.IO;
using TqkLibrary.WinApi;
#endif

namespace YoutubeManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if Key
        private const string guid = "{ECFEDBF3-B503-49C1-9D67-D0AF8566FA63}";
#endif
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            if (ex is AggregateException ae) ex = ae.InnerException!;
            MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", ex.GetType().FullName);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if Key
            string data = HardWareId.CalcHashVolumeSerialNumber(guid);
            bool flag = false;
            if (File.Exists(Singleton.Key))
            {
                using (StreamReader sr = new StreamReader(Singleton.Key))
                {
                    string key = sr.ReadToEnd();
                    if (data.Equals(key)) flag = true;
                }
            }
            if (!flag)
            {
                MessageBox.Show("Sai máy sử dụng");
                throw new Exception();
            }
#endif
        }
    }
}
