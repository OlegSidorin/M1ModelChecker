using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Collections.ObjectModel;
using static M1ModelChecker.CM;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Controls;

namespace M1ModelChecker
{
    public class DoTests_ExternalEventHandler : IExternalEventHandler
    {
        public ExternalCommandData CommandData;
        public MainWindow MainWindow;
        public FlowDocument FlowDocument { get; set; }

        public void Execute(UIApplication uiapp)
        {
            //Document doc = CommandData.Application.ActiveUIDocument.Document;
            Application app = CommandData.Application.Application;
            FilePathViewModel filePathVM = new FilePathViewModel();
            try
            {
                filePathVM = (FilePathViewModel)MainWindow.textBlockFilePath.Tag;
                Document doc = app.OpenDocumentFile(filePathVM.PathString);

                FlowDocument = new FlowDocument();
                Paragraph pHead = new Paragraph();
                pHead.Margin = new Thickness(0, 5, 0, 5);
                Run run = new Run();
                run.Text = "Параметры не из ФОП, не рекомендуется использовать:";
                run.FontWeight = FontWeights.Bold;
                run.FontSize = 14;
                run.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                pHead.Inlines.Add(run);

                FlowDocument.Blocks.Add(pHead);
                string str = "Параметры не из ФОП, не рекомендуется использовать:";
                var sharedParameters = new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement)).ToElements();
                
                foreach (SharedParameterElement parameter in sharedParameters)
                {
                    string report = "";
                    string guid = parameter.GuidValue.ToString();
                    string name = parameter.Name;
                    if (IsSomethingWrongWithParameter(guid, name, out report))
                    {
                        Paragraph pBody = new Paragraph();
                        pBody.Margin = new Thickness(7, 0, 0, 0);
                        Run runName = new Run();
                        runName.Text = $"{name}";
                        runName.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                        runName.FontWeight = FontWeights.Bold;
                        runName.FontSize = 12;
                        pBody.Inlines.Add(runName);
                        Run runReport = new Run();
                        runReport.Text = $" - {report}";
                        runReport.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                        runReport.FontWeight = FontWeights.Normal;
                        runReport.FontSize = 12;
                        pBody.Inlines.Add(runReport);
                        FlowDocument.Blocks.Add(pBody);

                        str += $"\n{name} - {report}";
                    }
                    
                }

                bool docClosed = doc.Close(false);
                MainWindow.FlowDocument = FlowDocument;
                MainWindow.Report = str;
                MainWindow.buttonShowReport.Visibility = System.Windows.Visibility.Visible;
                //MessageBox.Show($"\n{str}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Модель не выбрана");
            };
            
            //MessageBox.Show("Privet");
            return;
        }

        public string GetName()
        {
            return "External DoTests Event";
        }
    }
}