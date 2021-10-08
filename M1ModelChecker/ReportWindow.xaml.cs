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

namespace M1ModelChecker
{
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public string Report { get; set; }
        public ReportWindow()
        {
            InitializeComponent();
            
        }

        private void ClickOK(object sender, RoutedEventArgs e)
        {
            
            //TextRange t = new TextRange(flowDocScrollViewer.Document.ContentStart, flowDocScrollViewer.Document.ContentEnd);
            //using (FileStream file = new FileStream(@"C:\Users\o.sidorin\Downloads\test.rtf", FileMode.Create))
            //{
            //    t.Save(file, System.Windows.DataFormats.Rtf);
            //}

            Close();
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            using (SaveFileDialog dlgSave = new SaveFileDialog())
                try
                {
                    // Available file extensions
                    dlgSave.Filter = "RTF Files (*.rtf)|*.rtf";
                    // SaveFileDialog title
                    dlgSave.Title = "Сохранить отчет в RTF формате";
                    // Show SaveFileDialog
                    if (dlgSave.ShowDialog() == System.Windows.Forms.DialogResult.OK && dlgSave.FileName.Length > 0)
                    {
                        TextRange t = new TextRange(flowDocScrollViewer.Document.ContentStart, flowDocScrollViewer.Document.ContentEnd);
                        using (FileStream file = new FileStream(dlgSave.FileName, FileMode.Create))
                        {
                            t.Save(file, System.Windows.DataFormats.Rtf);
                        }
                    }
                }
                catch (Exception errorMsg)
                {
                    System.Windows.MessageBox.Show(errorMsg.Message);
                }

        }
    }
}
