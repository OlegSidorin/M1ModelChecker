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
    public class DeleteSP_ExternalEventHandler : IExternalEventHandler
    {
        public ExternalCommandData CommandData;
        public MainWindow MainWindow;
        public List<ParameterAndFamily> ParametersList;
        public void Execute(UIApplication uiapp)
        {
            string str = "";
            //foreach (var item in ParametersList)
            //{
            //    str += " ->" + item.ParameterName + "\n";
            //}
            //System.Windows.Forms.MessageBox.Show(str);
            str = "";
            Application app = CommandData.Application.Application;
            List<string> output = new List<string>();
            var filePathVM = (FilePathViewModel)MainWindow.textBlockFilePath.Tag;
            OpenOptions openOptions = new OpenOptions()
            {

                AllowOpeningLocalByWrongUser = true,
                Audit = false,
                DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets
            };

            Document doc = app.OpenDocumentFile(ModelPathUtils.ConvertUserVisiblePathToModelPath(filePathVM.PathString), openOptions);
            List<SharedParameterElement> sharedParameters = new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>().ToList();

           

            var familyInstances = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(x => x.Symbol.FamilyName.Contains("M1_Балка_Швеллер_ПолкиПараллельные_ГОСТ8240-97")).ToList();

            foreach (var fi in familyInstances)
            {
                Family family = fi.Symbol.Family;
                Document familyDoc = doc.EditFamily(family);
                if (null != familyDoc && familyDoc.IsFamilyDocument == true)
                {
                    try
                    {

                        using (Transaction tr = new Transaction(familyDoc, "Replace Family Parameters"))
                        {
                            tr.Start();

                            str += "You add new parameters in " + familyDoc.Title;

                            //familyDoc.FamilyManager.SortParameters(ParametersOrder.Ascending);

                            var sps = new FilteredElementCollector(familyDoc).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>().ToList();
                            str += "\n" + fi.Name + " " + family.Name + "\n";
                            foreach (var sp in sps)
                            {
                                Definition d = sp.GetDefinition();
                                InternalDefinition id = d as InternalDefinition;
                                try
                                {
                                    FamilyParameter fp = familyDoc.FamilyManager.GetParameters().Where(x => x.Definition.Name == sp.GetDefinition().Name).FirstOrDefault();
                                    //FamilyParameter newfp = familyDoc.FamilyManager.AddParameter(fp.Definition.Name, fp.Definition.ParameterGroup, fp.Definition.ParameterType, fp.IsInstance);
                                    familyDoc.FamilyManager.ReplaceParameter(fp, fp.Definition.Name + " new", sp.GetDefinition().ParameterGroup, fp.IsInstance);
                                    System.Windows.Forms.MessageBox.Show("добавлен параметр " + fp.Definition.Name);
                                    //
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                                };

                                //str += "    sp->" + sp.Name + "\n";
                            }
                            tr.Commit();
                        }

                        using (Transaction t2 = new Transaction(familyDoc, "Rename Parameters"))
                        {
                            t2.Start();
                            List<FamilyParameter> fpset = familyDoc.FamilyManager.GetParameters().ToList();
                            //var fps = new FilteredElementCollector(familyDoc).OfClass(typeof(FamilyParameter)).Cast<FamilyParameter>().ToList();
                            str += "\n" + fi.Name + " " + family.Name + "\n";
                            foreach (var fp in fpset)
                            {
                                if (fp.Definition.Name.Contains(" new"))
                                {
                                    string newname = fp.Definition.Name.Replace(" new", "");
                                    familyDoc.FamilyManager.RenameParameter(fp, newname);
                                }

                                str += "    fp->>" + fp.Definition.Name + "\n";
                            }
                            t2.Commit();
                        }
                        

                        string tmpFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), family.Name + ".rfa");

                        if (File.Exists(tmpFile))
                            File.Delete(tmpFile);
                        familyDoc.SaveAs(tmpFile);
                        
                        familyDoc.Close(false);
                        
                        using (Transaction trans = new Transaction(doc, "Load family."))
                        {
                            trans.Start();
                            IFamilyLoadOptions famLoadOptions =  new JtFamilyLoadOptions();
                            Family newFam = null;
                            doc.LoadFamily(tmpFile, new JtFamilyLoadOptions(), out newFam);
                            //TaskDialog.Show("Load Family", tmpFile.ToString());
                            trans.Commit();
                        }

                        File.Delete(tmpFile);
                        
                    }
                    catch (Exception ex) 
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                    }
                }
            }
            //using (Transaction tr = new Transaction(doc, "Q"))
            //{
            //    tr.Start();

            //        foreach(SharedParameterElement sp in sharedParameters)
            //        {

            //                try
            //                {
            //                    output.Add(DeleteSP(sp, doc));
            //                }
            //                catch (Exception ex)
            //                {
            //                    System.Windows.Forms.MessageBox.Show(ex.ToString());
            //                };

            //        }


            //    tr.Commit();
            //}


            
            //foreach (var item in output)
            //{
            //    str += item + "\n";
            //}
            TaskDialog.Show("1", str);
            doc.Close(true);
            return;
           
        }

        public string GetName()
        {
            return "External Delete Event";
        }

        public string DeleteSP(SharedParameterElement sp, Document doc)
        {
            var outputid = doc.Delete(sp.GetDefinition().Id);
            string t = "";
            foreach(var item in outputid)
            {
                t += item.IntegerValue;
            }
            return t;
        }
        private void DisplayParametersInAscendingOrder(Document familyDoc)
        {
            FamilyManager familyManager = familyDoc.FamilyManager;
            familyManager.SortParameters(ParametersOrder.Ascending);
        }
        

    }

    class JtFamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(
          bool familyInUse,
          out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(
          Family sharedFamily,
          bool familyInUse,
          out FamilySource source,
          out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }
}