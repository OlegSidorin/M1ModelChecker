using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using System;
using Autodesk.Revit.DB.Events;
using System.Collections.ObjectModel;

namespace M1ModelChecker
{
    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    class Main : IExternalApplication
    {
        public static string DllLocation { get; set; }
        public static string DllFolderLocation { get; set; }
        public static string UserFolder { get; set; }
        public static string TabName { get; set; } = "Надстройки";
        public static string PanelName { get; set; } = "M1";

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = application.CreateRibbonPanel(PanelName);
            DllLocation = Assembly.GetExecutingAssembly().Location;
            DllFolderLocation = Path.GetDirectoryName(DllLocation);
            UserFolder = @"C:\Users\" + Environment.UserName;
            PushButtonData ButtonData = new PushButtonData("ButtonData", "Выполнить\nпроверки", DllLocation, "M1ModelChecker.MainCommand")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(DllLocation) + "\\res\\m1mc.png", UriKind.Absolute)),
                //ToolTipImage = PngImageSource("BatchAddingParameters.res.bap-icon.png"),
                ToolTip = "Позволяет передать значения параметров по экземпляру из связного докумета"
            };
            ButtonData.AvailabilityClassName = "M1ModelChecker.Availability";
            PushButton pushButton = panel.AddItem(ButtonData) as PushButton;
            pushButton.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(DllLocation) + "\\res\\m1mc.png", UriKind.Absolute));
            //8TechBtn.LargeImage = PngImageSource("BatchAddingParameters.res.bap-icon.png");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            return true;
        }
    }
}
