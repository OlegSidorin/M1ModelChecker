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
using static M1ModelChecker.CM;

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

        public DeleteSP_ExternalEventHandler DeleteSP_ExternalEventHandler;
        public ExternalEvent DeleteSP_ExternalEvent;

        public string Report { get; set; }
        public static FlowDocument FlowDocument { get; set; }
        public static string MainFileName { get; set; }
        public static List<ParameterAndFamily> ParametersList {get; set; }
        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                DoTests_ExternalEventHandler = new DoTests_ExternalEventHandler();
                DoTests_ExternalEvent = ExternalEvent.Create(DoTests_ExternalEventHandler);

                DeleteSP_ExternalEventHandler = new DeleteSP_ExternalEventHandler();
                DeleteSP_ExternalEvent = ExternalEvent.Create(DeleteSP_ExternalEventHandler);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FindFileOrFolderForModelChecker(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            buttonShowReport.Visibility = Visibility.Hidden;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\Users\" + Environment.UserName; ;
                openFileDialog.Filter = "rvt files (*.rvt)|*.rvt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    MainFileName = System.IO.Path.GetFileName(filePath);
                }
            }
            FilePathViewModel filePathViewModel = new FilePathViewModel()
            {
                Name = System.IO.Path.GetFileName(filePath),
                PathString = filePath
            };
            textBlockFilePath.Text = filePathViewModel.Name;
            textBlockFilePath.Tag = filePathViewModel;

            //MessageBox.Show(fileContent, "File Content at path: " + filePath, MessageBoxButtons.OK);

            //FolderBrowserWindow folderBrowserWindow = new FolderBrowserWindow();
            //folderBrowserWindow.MainWindow = this;
            //folderBrowserWindow.CommandData = CommandData;
            //folderBrowserWindow.Show();
        }

        private void DoTests(object sender, RoutedEventArgs e)
        {
            bool ifTest01Rise = chBox_Test01.IsChecked.GetValueOrDefault();
            if (ifTest01Rise)
            {
                buttonShowReport.Visibility = Visibility.Hidden;
                DoTests_ExternalEventHandler.MainWindow = this;
                DoTests_ExternalEventHandler.CommandData = CommandData;
                DoTests_ExternalEvent.Raise();
            }
            if (!ifTest01Rise)
                MessageBox.Show("Не выбран Тест №1");
        }

        private void ShowReport(object sender, RoutedEventArgs e)
        {
            ReportWindow reportWindow = new ReportWindow();

            FlowDocument flowDocument = MainWindow.FlowDocument;
            
            try
            {
                reportWindow.flowDocScrollViewer.Document = flowDocument;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            reportWindow.Show();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
            
        }

        private void DeleteSharedParameters(object sender, RoutedEventArgs e)
        {
            DeleteSP_ExternalEventHandler.MainWindow = this;
            DeleteSP_ExternalEventHandler.CommandData = CommandData;
            DeleteSP_ExternalEventHandler.ParametersList = ParametersList;
            DeleteSP_ExternalEvent.Raise();
        }
    }
}
