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
using System.Linq;

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
                OpenOptions openOptions = new OpenOptions()
                {
                    AllowOpeningLocalByWrongUser = true,
                    Audit = false,
                    DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets
                };

                Document doc = app.OpenDocumentFile(ModelPathUtils.ConvertUserVisiblePathToModelPath(filePathVM.PathString), openOptions);

                FlowDocument = new FlowDocument();

                Paragraph pTitle = new Paragraph();
                pTitle.Margin = new Thickness(0, 0, 0, 0);
                Run runTitle01 = new Run();
                runTitle01.Text = $"Документ: ";
                runTitle01.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                runTitle01.FontWeight = FontWeights.Bold;
                runTitle01.FontSize = 12;
                pTitle.Inlines.Add(runTitle01);
                Run runTitle02 = new Run();
                runTitle02.Text = $"{doc.Title}.rvt";
                runTitle02.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                runTitle02.FontWeight = FontWeights.Normal;
                runTitle02.FontSize = 12;
                pTitle.Inlines.Add(runTitle02);
                FlowDocument.Blocks.Add(pTitle);

                Paragraph pDate = new Paragraph();
                pDate.Margin = new Thickness(0, 0, 0, 0);
                Run runDate01 = new Run();
                runDate01.Text = $"Дата проверки: ";
                runDate01.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                runDate01.FontWeight = FontWeights.Bold;
                runDate01.FontSize = 12;
                pDate.Inlines.Add(runDate01);
                Run runDate02 = new Run();
                runDate02.Text = $"{DateTime.Now}";
                runDate02.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                runDate02.FontWeight = FontWeights.Normal;
                runDate02.FontSize = 12;
                pDate.Inlines.Add(runDate02);
                FlowDocument.Blocks.Add(pDate);

                Paragraph pHead = new Paragraph();
                pHead.Margin = new Thickness(0, 5, 0, 5);
                Run run = new Run();
                run.Text = "Тест №1. Параметры не из ФОП, не рекомендуется использовать:";
                run.FontWeight = FontWeights.Bold;
                run.FontSize = 14;
                run.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                pHead.Inlines.Add(run);

                FlowDocument.Blocks.Add(pHead);
                string str = "Параметры не из ФОП, не рекомендуется использовать:";
                var sharedParameters = new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement)).ToElements();
                var badSharedParameters = new List<SharedParameterViewModel>();

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

                        badSharedParameters.Add(new SharedParameterViewModel()
                        {
                            Name = name,
                            Guid = guid
                        });

                        str += $"\n{name} - {report}";
                    }
                    
                }
                var familySymbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).ToElements();
                var listOfParametersAndFamilies = new List<ParameterAndFamily>();
                foreach (FamilySymbol fs in familySymbols)
                {
                    foreach(var shPar in badSharedParameters)
                    {
                        Parameter p = fs.LookupParameter(shPar.Name);
                        if (p != null)
                        {
                            listOfParametersAndFamilies.Add(new ParameterAndFamily()
                            {
                                ParameterName = shPar.Name,
                                FamilyName = fs.FamilyName
                            });
                            
                        }
                    }
                    
                }
                var listOfParametersAndFamiliesDistinct = new List<ParameterAndFamily>();
                bool isInList (ParameterAndFamily pf, List<ParameterAndFamily> listOfPF)
                {
                    foreach (var item in listOfPF)
                    {
                        if (item.ParameterName == pf.ParameterName)
                        {
                            if (item.FamilyName == pf.FamilyName)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
               
                foreach (ParameterAndFamily pf in listOfParametersAndFamilies)
                {
                    if(!isInList(pf, listOfParametersAndFamiliesDistinct))
                    {
                        listOfParametersAndFamiliesDistinct.Add(pf);
                    }
                }
                var listOfParametersAndListOfFamilies = new List<ParameterAndFamily>();
                bool nameIsAlreadyInList(ParameterAndFamily pf, List<ParameterAndFamily> listOfPF)
                {
                    foreach(var item in listOfParametersAndFamilies)
                    {
                        if (pf.ParameterName == item.ParameterName)
                            return true;
                    }
                    return false;
                }
                foreach (ParameterAndFamily pf in listOfParametersAndFamiliesDistinct)
                {
                    if (!nameIsAlreadyInList(pf, listOfParametersAndListOfFamilies))
                    {
                        pf.FamilyNames.Add(pf.FamilyName);
                    }
                }
                foreach (ParameterAndFamily pf in listOfParametersAndFamiliesDistinct)
                {
                    Paragraph pBody = new Paragraph();
                    pBody.Margin = new Thickness(7, 0, 0, 0);
                    Run run01 = new Run();
                    run01.Text = $"Параметр {pf.ParameterName} содержится в семействе {pf.FamilyName}";
                    run01.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                    run01.FontWeight = FontWeights.Light;
                    run01.FontSize = 12;
                    pBody.Inlines.Add(run01);
                    FlowDocument.Blocks.Add(pBody);
                }
                bool docClosed = doc.Close(false);
                MainWindow.FlowDocument = FlowDocument;
                MainWindow.Report = str;
                MainWindow.buttonShowReport.Visibility = System.Windows.Visibility.Visible;
                //MessageBox.Show($"\n{str}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Возникли проблемы с открытием. \n" + ex.ToString());
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