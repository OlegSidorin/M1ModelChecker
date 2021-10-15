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
    public class FIXIT_ExternalEventHandler : IExternalEventHandler
    {
        public ExternalCommandData CommandData;
        public MainWindow MainWindow;
        public List<ParameterAndFamily> ParametersListToFIX;
        public void Execute(UIApplication uiapp)
        {
            string str = "";
            //foreach (var item in ParametersListToFIX)
            //{
            //    str += " ->" + item.ParameterName + " : " + item.Cause.ToFriendlyString() + "\n";
            //    foreach (var familyName in item.FamilyNames)
            //    {
            //        str += familyName + " ";
            //    }
            //    str += "\n";
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
            List<FamilySymbol> familySymbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();

            List<WrongParameter> parametersWitnWrongName = new List<WrongParameter>();
            foreach (ParameterAndFamily pf in ParametersListToFIX)
            {
                if (pf.Cause == Causes.GoodGuidWrongName)
                {
                    foreach (SharedParameterElement sp in sharedParameters)
                    {
                        if (sp.GuidValue.ToString() == pf.ParameterGuid)
                        {
                            WrongParameter wrongParameter = new WrongParameter()
                            {
                                WrongSharedParameterElement = sp,
                                Cause = Causes.GoodGuidWrongName,
                                ParameterAndFamily = pf,
                                Families = new List<Family>()
                            };
                            foreach (string fn in pf.FamilyNames)
                            {
                                foreach (FamilySymbol fs in familySymbols)
                                {
                                    if (fs.FamilyName == fn)
                                    {
                                        bool isFamilyInList = false;
                                        foreach (Family family in wrongParameter.Families)
                                        {
                                            if (family.Name == fs.FamilyName)
                                                isFamilyInList = true;
                                        }
                                        if (!isFamilyInList)
                                            wrongParameter.Families.Add(fs.Family);
                                    }
                                }
                            }
                            parametersWitnWrongName.Add(wrongParameter);
                        }
                    }
                }
            }


            //foreach (WrongParameter wp in parametersWitnWrongName)
            //{
            //    str += " ->" + wp.WrongSharedParameterElement.GetDefinition().Name + " : " + wp.Cause.ToFriendlyString() + "\n";
            //    foreach (Family f in wp.Families)
            //    {
            //        str += f.Name + " ";
            //    }
            //    str += "\n";
            //}
            //System.Windows.Forms.MessageBox.Show(str);

            foreach (WrongParameter wp in parametersWitnWrongName)
            {
                List<string> pathsToTempFamilies = new List<string>();
                foreach (Family family in wp.Families)
                {
                    Document familyDoc = doc.EditFamily(family);
                    if (null != familyDoc && familyDoc.IsFamilyDocument == true)
                    {
                        try
                        {
                            using (Transaction t1 = new Transaction(familyDoc, "Replace Family Parameters With New Names"))
                            {
                                t1.Start();
                                Definition d = wp.WrongSharedParameterElement.GetDefinition();
                                InternalDefinition id = d as InternalDefinition;
                                try
                                {
                                    FamilyParameter fp = familyDoc.FamilyManager.GetParameters().Where(x => x.Definition.Name == wp.WrongSharedParameterElement.GetDefinition().Name).FirstOrDefault();
                                    //FamilyParameter newfp = familyDoc.FamilyManager.AddParameter(fp.Definition.Name, fp.Definition.ParameterGroup, fp.Definition.ParameterType, fp.IsInstance);
                                    familyDoc.FamilyManager.ReplaceParameter(fp, fp.Definition.Name + " new", wp.WrongSharedParameterElement.GetDefinition().ParameterGroup, fp.IsInstance);
                                    System.Windows.Forms.MessageBox.Show("добавлен параметр " + fp.Definition.Name);
                                    //
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                                };
                                t1.Commit();
                            }
                            using (Transaction t2 = new Transaction(familyDoc, "Rename Parameters On Old Names"))
                            {
                                t2.Start();
                                List<FamilyParameter> fpset = familyDoc.FamilyManager.GetParameters().ToList();
                                //var fps = new FilteredElementCollector(familyDoc).OfClass(typeof(FamilyParameter)).Cast<FamilyParameter>().ToList();
                                //str += "\n" + fi.Name + " " + family.Name + "\n";
                                foreach (var fp in fpset)
                                {
                                    if (fp.Definition.Name.Contains(" new"))
                                    {
                                        string newname = fp.Definition.Name.Replace(" new", "");
                                        familyDoc.FamilyManager.RenameParameter(fp, newname);
                                    }

                                    //str += "    fp->>" + fp.Definition.Name + "\n";
                                }
                                t2.Commit();
                            }
                            using (Transaction t3 = new Transaction(familyDoc, "Delete Parameters On Old Names"))
                            {
                                t3.Start();
                                var sps = new FilteredElementCollector(familyDoc).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>().ToList();
                                foreach (SharedParameterElement sharedParameterElement in sps)
                                {
                                    if (sharedParameterElement.GetDefinition().Name == wp.ParameterAndFamily.FamilyName)
                                    {
                                        System.Windows.Forms.MessageBox.Show("Delete: \n" + DeleteSP(sharedParameterElement, familyDoc)); 
                                    }
                                }
                                t3.Commit();
                            }

                            using (Transaction t4 = new Transaction(familyDoc, "Replace Family Parameters With Shared Parameter"))
                            {
                                t4.Start();
                                
                                try
                                {
                                   
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                                };
                                t4.Commit();
                            }

                            string tmpFile = Path.Combine(@"C:\Users\o.sidorin\Downloads\Проекты ТЕСТ\Families\", family.Name + ".rfa"); //System.Reflection.Assembly.GetExecutingAssembly().Location

                            if (File.Exists(tmpFile))
                                File.Delete(tmpFile);
                            familyDoc.SaveAs(tmpFile);
                            pathsToTempFamilies.Add(tmpFile);
                            familyDoc.Close(false);

                            //using (Transaction trans = new Transaction(doc, "Load family."))
                            //{
                            //    trans.Start();
                            //    IFamilyLoadOptions famLoadOptions = new JtFamilyLoadOptions();
                            //    Family newFam = null;
                            //    doc.LoadFamily(tmpFile, new JtFamilyLoadOptions(), out newFam);
                            //    //TaskDialog.Show("Load Family", tmpFile.ToString());
                            //    trans.Commit();
                            //}

                            //File.Delete(tmpFile);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.ToString());
                        }
                    }
                }
            }
            /*
            
            var familyInstances1 = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(x => x.Symbol.FamilyName.Contains("M1_Балка_Швеллер_ПолкиПараллельные_ГОСТ8240-97")).ToList();

            foreach (var fi in familyInstances1)
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

            */
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

    public class WrongParameter
    {
        public SharedParameterElement WrongSharedParameterElement { get; set; }
        public Causes Cause { get; set; }
        public List<Family> Families { get; set; }
        public ParameterAndFamily ParameterAndFamily { get; set; }
    }
}