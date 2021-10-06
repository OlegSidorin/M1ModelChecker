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

namespace M1ModelChecker
{
    public class DoTests_ExternalEventHandler : IExternalEventHandler
    {
        public ExternalCommandData CommandData;
        public MainWindow MainWindow;

        public void Execute(UIApplication uiapp)
        {
            //Document doc = CommandData.Application.ActiveUIDocument.Document;
            string pathToFile = MainWindow.textBlockFilePath.Text;
            Application app = CommandData.Application.Application;
            Document doc;
            try
            {
                doc = app.OpenDocumentFile(pathToFile);
                MessageBox.Show(doc.Title);
            }
            catch { };
            MessageBox.Show("Privet");
            return;
        }

        public string GetName()
        {
            return "External DoTests Event";
        }
    }
}