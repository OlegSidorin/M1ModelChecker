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
using Autodesk.Revit.DB.Events;

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
                app.FailuresProcessing += FaliureProcessor;

                Document doc = app.OpenDocumentFile(ModelPathUtils.ConvertUserVisiblePathToModelPath(filePathVM.PathString), openOptions);

                string fop = "";
                var ps = GetSharedParametersFromFOP();
                var er = GetGroups();
                foreach (var rr in er)
                {
                    fop += rr.Number + ":" + rr.Name;
                }
                //foreach (var sp in ps)
                //{
                //    fop += sp.Name + " : " + sp.Group + "\n";
                //}
                System.Windows.MessageBox.Show(fop);

                #region FlowDocument start
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
                #endregion 

                string str = "Параметры не из ФОП, не рекомендуется использовать:";
                List<SharedParameterElement> sharedParameters = new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>().ToList();

                List<ParameterAndFamily> listOfParametersAndComments = new List<ParameterAndFamily>();
                foreach (SharedParameterElement parameter in sharedParameters)
                {
                    Report report;
                    string guid = parameter.GuidValue.ToString();
                    string name = parameter.Name;
                    if (IsSomethingWrongWithParameter(guid, name, out report))
                    {
                        var parameterAndFamily = new ParameterAndFamily()
                        {
                            ParameterName = name,
                            ParameterGuid = guid,
                            Cause = report.Cause,
                            Comment = report.Comment //+ " : " + report.Cause.ToFriendlyString() 
                        };
                        listOfParametersAndComments.Add(parameterAndFamily);
                        str += $"\n{name} - {report}";
                    }
                }

                #region FlowDocument Block 02
                foreach (ParameterAndFamily pf in listOfParametersAndComments)
                {
                    Paragraph pBody = new Paragraph();
                    pBody.Margin = new Thickness(7, 0, 0, 0);
                    Run runName = new Run();
                    runName.Text = $"{pf.ParameterName}";
                    runName.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                    runName.FontWeight = FontWeights.Bold;
                    runName.FontSize = 12;
                    pBody.Inlines.Add(runName);
                    Run runReport = new Run();
                    runReport.Text = $" - {pf.Comment}";
                    runReport.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                    runReport.FontWeight = FontWeights.Normal;
                    runReport.FontSize = 12;
                    pBody.Inlines.Add(runReport);
                    FlowDocument.Blocks.Add(pBody);
                }
                #endregion
                
                var familySymbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).ToElements();

                var listOfParametersAndFamilies = new List<ParameterAndFamily>();
                foreach (FamilySymbol fs in familySymbols)
                {
                    foreach(var shPar in listOfParametersAndComments)
                    {
                        Parameter p = fs.LookupParameter(shPar.ParameterName);
                        if (p != null)
                        {
                            try
                            {
                                if (p.GUID != null)
                                {
                                    listOfParametersAndFamilies.Add(new ParameterAndFamily()
                                    {
                                        ParameterName = shPar.ParameterName,
                                        ParameterGuid = shPar.ParameterGuid,
                                        Cause = shPar.Cause,
                                        FamilyName = fs.FamilyName
                                    });
                                }
                            }
                            catch { };
                        }
                    }
                }

                ParameterAndFamily PF = new ParameterAndFamily();
                var listOfParametersAndFamiliesDistinct = PF.GetDistinct(listOfParametersAndFamilies);

                #region FlowDocument Block 03
                //foreach (ParameterAndFamily pf in listOfParametersAndFamiliesDistinct)
                //{
                //    Paragraph pBody = new Paragraph();
                //    pBody.Margin = new Thickness(7, 0, 0, 0);
                //    Run run01 = new Run();
                //    run01.Text = $"Параметр {pf.ParameterName} {pf.ParameterGuid} содержится в семействе {pf.FamilyName}";
                //    run01.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                //    run01.FontWeight = FontWeights.Light;
                //    run01.FontSize = 12;
                //    pBody.Inlines.Add(run01);
                //    FlowDocument.Blocks.Add(pBody);
                //}

                Paragraph pBody101 = new Paragraph();
                pBody101.Margin = new Thickness(0, 5, 0, 5);
                Run run101 = new Run
                {
                    Text = "Параметры со списком семейств, в которых он был обнаружен:",
                    FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                    FontWeight = FontWeights.Bold,
                    FontSize = 14
                };
                pBody101.Inlines.Add(run101);
                FlowDocument.Blocks.Add(pBody101);
                #endregion
                
                var listOfParameterAndFamilies = PF.GetParametersWithListOfFamilies(listOfParametersAndFamiliesDistinct);
                MainWindow.ParametersListToFIX = listOfParameterAndFamilies;

                #region FlowDocument Block 04
                foreach (var item in listOfParameterAndFamilies)
                {
                    Paragraph pBody01 = new Paragraph();
                    pBody01.Margin = new Thickness(7, 0, 0, 0);
                    Run run011 = new Run
                    {
                        Text = $"Параметр ",
                        FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                        FontWeight = FontWeights.Normal,
                        FontSize = 12
                    };
                    pBody01.Inlines.Add(run011);
                    Run run012 = new Run()
                    {
                        Text = $"{item.ParameterName} ({item.Cause.ToFriendlyString()})",
                        FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                        FontWeight = FontWeights.Bold,
                        FontSize = 12
                    };
                    pBody01.Inlines.Add(run012);
                    Run run013 = new Run
                    {
                        Text = $" содержится в семействах:",
                        FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                        FontWeight = FontWeights.Normal,
                        FontSize = 12
                    };
                    pBody01.Inlines.Add(run013);
                    FlowDocument.Blocks.Add(pBody01);
                    Paragraph pBody02 = new Paragraph();
                    pBody02.Margin = new Thickness(25, 0, 0, 0);
                    Run run02 = new Run();
                    run02.Text = $"{item.GetFamiliesInOneSting()}";
                    run02.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                    run02.FontWeight = FontWeights.Normal;
                    run02.FontStyle = FontStyles.Italic;
                    run02.FontSize = 12;
                    pBody02.Inlines.Add(run02);
                    FlowDocument.Blocks.Add(pBody02);
                }

                Paragraph pBody202 = new Paragraph()
                {
                    Margin = new Thickness(0, 5, 0, 5),
                };
                Run run202 = new Run
                {
                    Text = "Семейства со списком нежелательных параметров:",
                    FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                    FontWeight = FontWeights.Bold,
                    FontSize = 14
                };
                pBody202.Inlines.Add(run202);
                FlowDocument.Blocks.Add(pBody202);
                #endregion

                var listOfFamilyAndParameters = PF.GetFamilyWithListOfParameters(listOfParametersAndFamiliesDistinct);

                #region FlowDocument Block 05 
                foreach (var item in listOfFamilyAndParameters)
                {
                    Paragraph pBody01 = new Paragraph();
                    pBody01.Margin = new Thickness(7, 0, 0, 0);
                    Run run011 = new Run
                    {
                        Text = $"В семействе ",
                        FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                        FontWeight = FontWeights.Normal,
                        FontSize = 12
                    };
                    pBody01.Inlines.Add(run011);
                    Run run012 = new Run()
                    {
                        Text = $"{item.FamilyName}",
                        FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                        FontWeight = FontWeights.Bold,
                        FontSize = 12
                    };
                    pBody01.Inlines.Add(run012);
                    Run run013 = new Run
                    {
                        Text = $" содержатся нежелательные параметры:",
                        FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                        FontWeight = FontWeights.Normal,
                        FontSize = 12
                    };
                    pBody01.Inlines.Add(run013);
                    FlowDocument.Blocks.Add(pBody01);
                    Paragraph pBody02 = new Paragraph();
                    pBody02.Margin = new Thickness(25, 0, 0, 0);
                    Run run02 = new Run();
                    run02.Text = $"{item.GetParametersInOneSting()}";
                    run02.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                    run02.FontWeight = FontWeights.Normal;
                    run02.FontStyle = FontStyles.Italic;
                    run02.FontSize = 12;
                    pBody02.Inlines.Add(run02);
                    FlowDocument.Blocks.Add(pBody02);
                }

                /*
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
                */

                #endregion

                bool docClosed = doc.Close(false);
                FlowDocument.TextAlignment = TextAlignment.Left;
                MainWindow.FlowDocument = FlowDocument;
                MainWindow.Report = str;
                MainWindow.buttonShowReport.Visibility = System.Windows.Visibility.Visible;

                //MessageBox.Show($"\n{str}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Возникли проблемы с открытием. И вот какие: \n" + ex.ToString());
            };
            
            //MessageBox.Show("Privet");
            return;
        }

        public string GetName()
        {
            return "External DoTests Event";
        }

        private void FaliureProcessor(object sender, FailuresProcessingEventArgs e)
        {
            bool hasFailure = false;
            FailuresAccessor fas = e.GetFailuresAccessor();
            List<FailureMessageAccessor> fma = fas.GetFailureMessages().ToList();
            List<ElementId> ElemntsToDelete = new List<ElementId>();
            foreach (FailureMessageAccessor fa in fma)
            {
                try
                {

                    //use the following lines to delete the warning elements
                    List<ElementId> FailingElementIds = fa.GetFailingElementIds().ToList();
                    ElementId FailingElementId = FailingElementIds[0];
                    if (!ElemntsToDelete.Contains(FailingElementId))
                    {
                        ElemntsToDelete.Add(FailingElementId);
                    }

                    hasFailure = true;
                    fas.DeleteWarning(fa);

                }
                catch (Exception ex)
                {
                }
            }
            if (ElemntsToDelete.Count > 0)
            {
                fas.DeleteElements(ElemntsToDelete);
            }
            //use the following line to disable the message supressor after the external command ends
            //CachedUiApp.Application.FailuresProcessing -= FaliureProcessor;
            if (hasFailure)
            {
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
            }
            e.SetProcessingResult(FailureProcessingResult.Continue);
        }
    }
}