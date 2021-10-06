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

namespace M1ModelChecker
{
    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    class MainCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.CommandData = commandData;
            mainWindow.Show();
            int mysumm(int x, int y) => x + y;
            int r = mysumm(2, 3);
            (int, string) xy = (25, "sd");
            string e = xy.Item1 + xy.Item2;
            //FolderBrowserWindow folderBrowserWindow = new FolderBrowserWindow();
            //folderBrowserWindow.CommandData = commandData;
            //folderBrowserWindow.Show();
            //FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //folderBrowserDialog.ShowDialog();
            return Result.Succeeded;
        }
    }
}
