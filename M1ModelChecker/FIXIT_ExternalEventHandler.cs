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
        public List<ParameterAndFamily> FamiliesListToFIX;
        public List<string> ParametersOnlyProjectToFIX;
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
            string docPath = filePathVM.PathString;
            Document doc = app.OpenDocumentFile(ModelPathUtils.ConvertUserVisiblePathToModelPath(docPath), openOptions);

            List<SharedParameterElement> sharedParameters = new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>().ToList();
            List<FamilySymbol> familySymbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();

            List<WrongParameter> wrongParametersList = new List<WrongParameter>();

            List<FamilyWithWrongParameters> familiesWithWrongParameters = new List<FamilyWithWrongParameters>();

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
                            wrongParametersList.Add(wrongParameter);
                        }
                    }
                }
                if (pf.Cause == Causes.WrongGuidAndName)
                {
                    foreach (SharedParameterElement sp in sharedParameters)
                    {
                        if (sp.GetDefinition().Name == pf.ParameterName)
                        {
                            WrongParameter wrongParameter = new WrongParameter()
                            {
                                WrongSharedParameterElement = sp,
                                Cause = Causes.WrongGuidAndName,
                                ParameterAndFamily = pf,
                                Families = new List<Family>()
                            };
                            wrongParametersList.Add(wrongParameter);
                        }
                    }
                }
            }

            foreach (ParameterAndFamily fl in FamiliesListToFIX)
            {
                foreach (FamilySymbol fs in familySymbols)
                {
                    if (fs.FamilyName == fl.FamilyName)
                    {
                        FamilyWithWrongParameters familyWithWrongParameters = new FamilyWithWrongParameters()
                        {
                            Family = fs.Family,
                            WrongParameters = new List<WrongParameter>()
                        };
                        bool familyIsIn = false;
                        foreach (var item in familiesWithWrongParameters)
                        {
                            if (item.Family.Name == fs.FamilyName)
                                familyIsIn = true;
                        }
                        if (!familyIsIn)
                        {
                            foreach (string parameterName in fl.ParameterNames)
                            {
                                foreach (WrongParameter wrongParameter in wrongParametersList)
                                {
                                    if (parameterName == wrongParameter.ParameterAndFamily.ParameterName)
                                    {
                                        familyWithWrongParameters.WrongParameters.Add(wrongParameter);
                                    }
                                }
                            }
                            familiesWithWrongParameters.Add(familyWithWrongParameters);
                        }
                    }

                }
            }

            //foreach (var fl in ParametersOnlyProjectToFIX)
            //{
            //    str += " ->" + fl + "\n";
            //}
            //System.Windows.Forms.MessageBox.Show(str);
            //return;
            //foreach (var fl in FamiliesListToFIX)
            //{
            //    str += " ->" + fl.FamilyName + "\n";
            //    foreach (var f in fl.ParameterNames)
            //    {
            //        str += "<" + f + ">";
            //    }
            //    str += "\n";
            //}
            //System.Windows.Forms.MessageBox.Show(str);
            //return;
            //foreach (var fwp in familiesWithWrongParameters)
            //{
            //    str += " ->" + fwp.Family.Name + "\n";
            //    foreach (var f in fwp.WrongParameters)
            //    {
            //        str += "<" + f.ParameterAndFamily.ParameterName + "> <" + f.ParameterAndFamily.Cause.ToFriendlyString() + "> \n(" + f.WrongSharedParameterElement.GetDefinition().Name + ")\n";
            //    }
            //    str += "\n";
            //}
            //System.Windows.Forms.MessageBox.Show(str);
            //return;
            //foreach (WrongParameter wp in wrongParametersList)
            //{
            //    str += " ->" + wp.WrongSharedParameterElement.GetDefinition().Name + " : " + wp.Cause.ToFriendlyString() + "\n";
            //    foreach (Family f in wp.Families)
            //    {
            //        str += "<" + f.Name + ">";
            //    }
            //    str += "\n";
            //}
            //System.Windows.Forms.MessageBox.Show(str);
            //return;

            List<string> pathsToTempFamilies = new List<string>();
            foreach (FamilyWithWrongParameters fwp in familiesWithWrongParameters)
            {
                try
                {
                    Document familyDoc = doc.EditFamily(fwp.Family);
                    if (null != familyDoc && familyDoc.IsFamilyDocument == true)
                    {
                        foreach (WrongParameter wp in fwp.WrongParameters)
                        {
                            switch (wp.Cause)
                            {
                                case Causes.WrongGuidAndName:
                                    using (Transaction t1 = new Transaction(familyDoc, "Convert Shared Parameter to Family Parameter With New Name"))
                                    {
                                        t1.Start();
                                        Definition d = wp.WrongSharedParameterElement.GetDefinition();
                                        InternalDefinition id = d as InternalDefinition;
                                        try
                                        {
                                            FamilyParameter fp = familyDoc.FamilyManager.GetParameters().Where(x => x.Definition.Name == wp.WrongSharedParameterElement.GetDefinition().Name).FirstOrDefault();
                                            //FamilyParameter newfp = familyDoc.FamilyManager.AddParameter(fp.Definition.Name, fp.Definition.ParameterGroup, fp.Definition.ParameterType, fp.IsInstance);
                                            familyDoc.FamilyManager.ReplaceParameter(fp, fp.Definition.Name + "_new", wp.WrongSharedParameterElement.GetDefinition().ParameterGroup, fp.IsInstance);
                                            //System.Windows.Forms.MessageBox.Show("1. замена общего параметра " + fp.Definition.Name + " на параметр семейства в семействе ( + new)" + fwp.Family.Name);
                                        }
                                        catch (Exception ex)
                                        {

                                        };
                                        t1.Commit();
                                    }
                                    using (Transaction t2 = new Transaction(familyDoc, "Rename Parameter On Old Names"))
                                    {
                                        t2.Start();
                                        List<FamilyParameter> fpset = familyDoc.FamilyManager.GetParameters().ToList();
                                        //var fps = new FilteredElementCollector(familyDoc).OfClass(typeof(FamilyParameter)).Cast<FamilyParameter>().ToList();
                                        //str += "\n" + fi.Name + " " + family.Name + "\n";
                                        foreach (var fp in fpset)
                                        {
                                            if (fp.Definition.Name.Contains("_new"))
                                            {
                                                string newname = fp.Definition.Name.Replace("_new", "");
                                                familyDoc.FamilyManager.RenameParameter(fp, newname);
                                                //System.Windows.Forms.MessageBox.Show("2. возврат к прежнему имени (- new) параметра" + fp.Definition.Name + " в семействе " + fwp.Family.Name);
                                            }

                                            //str += "    fp->>" + fp.Definition.Name + "\n";
                                        }
                                        t2.Commit();
                                    }
                                    using (Transaction t3 = new Transaction(familyDoc, "Delete Parameter"))
                                    {
                                        t3.Start();
                                        var sps = new FilteredElementCollector(familyDoc).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>().ToList();
                                        foreach (SharedParameterElement sharedParameterElement in sps)
                                        {
                                            if (sharedParameterElement.GetDefinition().Name == wp.ParameterAndFamily.ParameterName)
                                            {
                                                string result = DeleteSP(sharedParameterElement, familyDoc);
                                                //System.Windows.Forms.MessageBox.Show("3. удаление общего параметра " + result + " в семействе " + fwp.Family.Name);
                                            }
                                        }
                                        t3.Commit();
                                    }
                                    break;
                            }
                        }
                        foreach (WrongParameter wp in fwp.WrongParameters)
                        {
                            switch (wp.Cause)
                            {
                                case Causes.GoodGuidWrongName:
                                    using (Transaction t1 = new Transaction(familyDoc, "Convert Shared Parameter to Family Parameter With New Name"))
                                    {
                                        t1.Start();
                                        Definition d = wp.WrongSharedParameterElement.GetDefinition();
                                        InternalDefinition id = d as InternalDefinition;
                                        try
                                        {
                                            FamilyParameter fp = familyDoc.FamilyManager.GetParameters().Where(x => x.Definition.Name == wp.WrongSharedParameterElement.GetDefinition().Name).FirstOrDefault();
                                            //FamilyParameter newfp = familyDoc.FamilyManager.AddParameter(fp.Definition.Name, fp.Definition.ParameterGroup, fp.Definition.ParameterType, fp.IsInstance);
                                            familyDoc.FamilyManager.ReplaceParameter(fp, fp.Definition.Name + "_new", wp.WrongSharedParameterElement.GetDefinition().ParameterGroup, fp.IsInstance);
                                            //System.Windows.Forms.MessageBox.Show("1. замена общего параметра " + fp.Definition.Name + " на параметр семейства в семействе ( + new)" + fwp.Family.Name);
                                        }
                                        catch (Exception ex)
                                        {

                                        };
                                        t1.Commit();
                                    }
                                    using (Transaction t2 = new Transaction(familyDoc, "Rename Parameter On Old Names"))
                                    {
                                        t2.Start();
                                        List<FamilyParameter> fpset = familyDoc.FamilyManager.GetParameters().ToList();
                                        //var fps = new FilteredElementCollector(familyDoc).OfClass(typeof(FamilyParameter)).Cast<FamilyParameter>().ToList();
                                        //str += "\n" + fi.Name + " " + family.Name + "\n";
                                        foreach (var fp in fpset)
                                        {
                                            if (fp.Definition.Name.Contains("_new"))
                                            {
                                                string newname = fp.Definition.Name.Replace("_new", "");
                                                familyDoc.FamilyManager.RenameParameter(fp, newname);
                                                //System.Windows.Forms.MessageBox.Show("2. возврат к прежнему имени (- new) параметра" + fp.Definition.Name + " в семействе " + fwp.Family.Name);
                                            }

                                            //str += "    fp->>" + fp.Definition.Name + "\n";
                                        }
                                        t2.Commit();
                                    }
                                    using (Transaction t3 = new Transaction(familyDoc, "Delete Parameter"))
                                    {
                                        t3.Start();
                                        var sps = new FilteredElementCollector(familyDoc).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>().ToList();
                                        foreach (SharedParameterElement sharedParameterElement in sps)
                                        {
                                            if (sharedParameterElement.GetDefinition().Name == wp.ParameterAndFamily.ParameterName)
                                            {
                                                string result = DeleteSP(sharedParameterElement, familyDoc);
                                                //System.Windows.Forms.MessageBox.Show("3. удаление общего параметра " + result + " в семействе " + fwp.Family.Name);
                                            }
                                        }
                                        t3.Commit();
                                    }
                                    using (Transaction t4 = new Transaction(familyDoc, "Rename Parameters On New Name"))
                                    {
                                        t4.Start();
                                        List<FamilyParameter> fpset = familyDoc.FamilyManager.GetParameters().ToList();
                                        //var fps = new FilteredElementCollector(familyDoc).OfClass(typeof(FamilyParameter)).Cast<FamilyParameter>().ToList();
                                        //str += "\n" + fi.Name + " " + family.Name + "\n";
                                        foreach (var fp in fpset)
                                        {
                                            if (fp.Definition.Name == wp.ParameterAndFamily.ParameterName)
                                            {
                                                familyDoc.FamilyManager.RenameParameter(fp, fp.Definition.Name + "_new");
                                                //System.Windows.Forms.MessageBox.Show("4. переименование снова (+ new) параметра " + fp.Definition.Name + " в семействе " + fwp.Family.Name);
                                            }

                                            //str += "    fp->>" + fp.Definition.Name + "\n";
                                        }
                                        t4.Commit();
                                    }
                                    using (Transaction t5 = new Transaction(familyDoc, "Replace Family Parameters With Shared Parameter"))
                                    {
                                        t5.Start();
                                        DefinitionFile definitionFile = CommandData.Application.Application.OpenSharedParameterFile();

                                        try
                                        {
                                            SharedParameterFromFOP sharedParameterFromFOP = GetParameterFromFOPUsingGUID(wp.ParameterAndFamily.ParameterGuid);
                                            DefinitionGroup definitionGroup = definitionFile.Groups.get_Item(sharedParameterFromFOP.Group);
                                            Definition definition = definitionGroup.Definitions.get_Item(sharedParameterFromFOP.Name);
                                            ExternalDefinition externalDefinition = definition as ExternalDefinition;
                                            FamilyParameter fp = familyDoc.FamilyManager.GetParameters().Where(x => x.Definition.Name == wp.WrongSharedParameterElement.GetDefinition().Name + "_new").FirstOrDefault();
                                            familyDoc.FamilyManager.ReplaceParameter(fp, externalDefinition, fp.Definition.ParameterGroup, fp.IsInstance);
                                            //System.Windows.Forms.MessageBox.Show("5. Добавлен новый общий параметр с правильным именем " + definition.Name + " в семействе " + fwp.Family.Name);
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Windows.Forms.MessageBox.Show(wp.ParameterAndFamily.ParameterName + "\n" + ex.ToString());
                                        };
                                        t5.Commit();
                                    }
                                    break;
                            }
                        }
                        string tmpFile = Path.Combine(@"C:\Temp", fwp.Family.Name + ".rfa"); //System.Reflection.Assembly.GetExecutingAssembly().Location

                        if (File.Exists(tmpFile))
                            File.Delete(tmpFile);
                        familyDoc.SaveAs(tmpFile);
                        pathsToTempFamilies.Add(tmpFile);
                        //System.Windows.Forms.MessageBox.Show("Конец редактирования семейства: " + family.Name);
                        familyDoc.Close(false);
                    }
                } 
                catch (Exception ex)
                {

                }

                
            }

            foreach (WrongParameter wp in wrongParametersList)
            {
                using (Transaction t6 = new Transaction(doc, "Delete Wrong Shared Parameter From Doc"))
                {
                    t6.Start();
                    try
                    {
                        string result = DeleteSP(wp.WrongSharedParameterElement, doc);
                        //System.Windows.Forms.MessageBox.Show("6. Удаление неверного общего параметра из проекта\n" + wp.ParameterAndFamily.ParameterName + "\n" + result + "\n" + doc.Title);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                    };

                    t6.Commit();
                }
            }
            foreach (var wp in ParametersOnlyProjectToFIX)
            {
                using (Transaction t8 = new Transaction(doc, "Delete Wrong Shared Parameter From Doc"))
                {
                    t8.Start();

                        foreach (SharedParameterElement shp in sharedParameters)
                        {
                            try
                            {
                                if (shp.GetDefinition().Name == wp)
                                {
                                    string result = DeleteSP(shp, doc);
                                    //System.Windows.Forms.MessageBox.Show("6. Удаление неверного общего параметра из проекта\n" + wp.ParameterAndFamily.ParameterName + "\n" + result + "\n" + doc.Title);
                                }
                            }
                            catch
                            { }
                            
                        }
                        


                    t8.Commit();
                }
            }

            //List<string> reloadedParameters = new List<string>();

            //foreach (WrongParameter wp in wrongParametersList)
            //{
            //    using (Transaction t7 = new Transaction(doc, "Add Right parameters with guid of Wrong Shared Parameter From FOP"))
            //    {
            //        t7.Start();
            //        DefinitionFile definitionFile = CommandData.Application.Application.OpenSharedParameterFile();
            //        try
            //        {
            //            SharedParameterFromFOP sharedParameterFromFOP = GetParameterFromFOPUsingGUID(wp.ParameterAndFamily.ParameterGuid);
            //            DefinitionGroup definitionGroup = definitionFile.Groups.get_Item(sharedParameterFromFOP.Group);
            //            Definition definition = definitionGroup.Definitions.get_Item(sharedParameterFromFOP.Name);
            //            ExternalDefinition externalDefinition = definition as ExternalDefinition;
            //            Category cat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_GenericModel);
            //            CategorySet catSet = CommandData.Application.Application.Create.NewCategorySet();
            //            catSet.Insert(cat);
            //            InstanceBinding newIB = CommandData.Application.Application.Create.NewInstanceBinding(catSet);
            //            doc.ParameterBindings.Insert(externalDefinition, newIB, BuiltInParameterGroup.INVALID);
            //            reloadedParameters.Add(definition.Name);
            //        }
            //        catch (Exception ex)
            //        {
            //            System.Windows.Forms.MessageBox.Show(ex.ToString());
            //        }
            //        t7.Commit();
            //    }
            //}

            foreach (string path in pathsToTempFamilies)
            {

                using (Transaction trans = new Transaction(doc, "Load family."))
                {
                    trans.Start();
                    //System.Windows.Forms.MessageBox.Show("Начало загрузки семейства: " + Path.GetFileName(path) + " ");
                    try
                    {
                        IFamilyLoadOptions famLoadOptions = new JtFamilyLoadOptions();
                        Family newFam = null;
                        bool yes = doc.LoadFamily(path, new JtFamilyLoadOptions(), out newFam);
                        //if (yes)
                        //    File.Delete(path);
                        //System.Windows.Forms.MessageBox.Show("7. Загружено семейство " + yes.ToString() + " " + Path.GetFileName(path) + " в " + doc.Title + ".rvt\n");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                    }

                    trans.Commit();
                    //System.Windows.Forms.MessageBox.Show("Конец загрузки семейства: " + Path.GetFileName(path) + " ");
                }
            }

            //foreach (string rp in reloadedParameters)
            //{
            //    string parametername = rp;
            //    IEnumerable<ParameterElement> _params = new FilteredElementCollector(doc).WhereElementIsNotElementType()
            //            .OfClass(typeof(ParameterElement))
            //            .Cast<ParameterElement>();
            //    ParameterElement projectparameter = null;
            //    foreach (ParameterElement pElem in _params)
            //    {
            //        if (pElem.GetDefinition().Name.Equals(parametername))
            //        {
            //            projectparameter = pElem;
            //            break;
            //        }
            //    }
            //    if (projectparameter == null) return;
            //    using (Transaction t = new Transaction(doc, "remove projectparameter"))
            //    {
            //        t.Start();
            //        doc.Delete(projectparameter.Id);
            //        System.Windows.Forms.MessageBox.Show("удален параметр проекта " + rp);
            //        t.Commit();
            //    }
            //}

            try
            {
                //System.Windows.Forms.MessageBox.Show("Не совсем понятно я тут?");
                //doc.Save();
                var dt = DateTime.Now;
                var random = new Random();
                doc.SaveAs(docPath.Replace(".rvt", "") + "_"+ random.Next(1111, 9999) + ".rvt");
                doc.Close(false);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

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
    public class FamilyWithWrongParameters
    {
        public Family Family { get; set; }
        public List<WrongParameter> WrongParameters { get; set; }
    }
}