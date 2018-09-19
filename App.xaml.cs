using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IndiaHacksGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                string message = "An Application error has occured.";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                message = DateTime.Now.ToString() + message + Environment.NewLine + " Error: " + e.Exception.Message + Environment.NewLine + "Stack trace: " + e.Exception.StackTrace + Environment.NewLine;
                System.IO.File.AppendAllText("error.log", message);
                e.Handled = true;
            }
            catch
            {

            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                string message = "An Application error has occured.";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                message = DateTime.Now.ToString() + message + Environment.NewLine + " Error: " + e.ToString() + Environment.NewLine + "Stack trace: " + e.ToString() + Environment.NewLine;
                System.IO.File.AppendAllText("error.log", message);
            }
            catch
            {

            }
        }
    }
}
