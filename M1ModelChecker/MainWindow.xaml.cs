using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace M1ModelChecker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ExternalCommandData CommandData;
        public DoTests_ExternalEventHandler DoTests_ExternalEventHandler;
        public ExternalEvent DoTests_ExternalEvent;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                DoTests_ExternalEventHandler = new DoTests_ExternalEventHandler();
                DoTests_ExternalEvent = ExternalEvent.Create(DoTests_ExternalEventHandler);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FindFileOrFolderForModelChecker(object sender, RoutedEventArgs e)
        {
            //var fileContent = string.Empty;
            //var filePath = string.Empty;

            //using (OpenFileDialog openFileDialog = new OpenFileDialog())
            //{
            //    openFileDialog.InitialDirectory = @"C:\Users\" + Environment.UserName; ;
            //    openFileDialog.Filter = "rfa files (*.rfa)|*.rfa|All files (*.*)|*.*";
            //    openFileDialog.FilterIndex = 1;
            //    openFileDialog.RestoreDirectory = true;

            //    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        filePath = openFileDialog.FileName;
            //    }
            //}
            //textBlockFilePath.Text = filePath;
            //MessageBox.Show(fileContent, "File Content at path: " + filePath, MessageBoxButtons.OK);
            FolderBrowserWindow folderBrowserWindow = new FolderBrowserWindow();
            folderBrowserWindow.MainWindow = this;
            folderBrowserWindow.CommandData = CommandData;
            folderBrowserWindow.Show();
        }

        private void DoTests(object sender, RoutedEventArgs e)
        {
            DoTests_ExternalEventHandler.MainWindow = this;
            DoTests_ExternalEventHandler.CommandData = CommandData;
            
            DoTests_ExternalEvent.Raise();

        }
    }
}
