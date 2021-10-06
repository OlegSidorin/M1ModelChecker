using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using Path = System.IO.Path;
using Autodesk.Revit.UI;
using ComboBox = System.Windows.Controls.ComboBox;

namespace M1ModelChecker
{
    /// <summary>
    /// Логика взаимодействия для FolderBrowserWindow.xaml
    /// </summary>
    public partial class FolderBrowserWindow : Window
    {
        Collection<FilePathViewModel> CollectionOfParentFolders;
        public MainWindow MainWindow;
        public ExternalCommandData CommandData;
        public FolderBrowserWindow()
        {
            InitializeComponent();
            CollectionOfParentFolders = new Collection<FilePathViewModel>();
            comboBox.ItemsSource = GetFilesAndFoldersCollectionForComboBox(@"C:\Users\" + Environment.UserName); //  (@"\\ukkalita.local\iptg\Строительно-девелоперский дивизион\М1 Проект\Проекты\10. Отдел информационного моделирования\01. REVIT\01. Библиотека семейств\#На рассмотрение\Тест");
            //comboBox.SelectedIndex = 0;
        }

        private void buttonClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void buttonOpen(object sender, RoutedEventArgs e)
        {
            if (MainWindow != null)
            {
                MainWindow.textBlockFilePath.Text = textBlockSelectFileOrFolder.Text;
                //MainWindow.textBlockFilePath.Tag = textBlockSelectFileOrFolder.Tag;
                Close();
            }
            else
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.textBlockFilePath.Text = textBlockSelectFileOrFolder.Text;
                //mainWindow.textBlockFilePath.Tag = textBlockSelectFileOrFolder.Tag;
                mainWindow.Show();
                Close();
            }
            
        }
        void AddParentFoldersToCollection(string inputPath)
        {
            var i = inputPath.Count(x => x == '\\');

            DirectoryInfo dirInfo = Directory.GetParent(inputPath);
            if (dirInfo != null)
            {
                FilePathViewModel filePathViewModel = new FilePathViewModel()
                {
                    Name = dirInfo.Name + " " + (5*i).ToString(),
                    PathString = dirInfo.FullName,
                    ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-folder.png",
                    LeftMargin = new Thickness(5 * i, 0, 0, 0)
                };
                if (filePathViewModel.Name.Contains(":"))
                    filePathViewModel.ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-ssd.png";
                if (filePathViewModel.Name.Contains("C:"))
                    filePathViewModel.ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-c-drive.png";
                if (filePathViewModel.Name.Contains("ukkalita.local"))
                    filePathViewModel.ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-network.png";
                CollectionOfParentFolders.Insert(0, filePathViewModel);

                if (dirInfo.Parent != null)
                {
                    AddParentFoldersToCollection(dirInfo.FullName);
                };
            }
        }
        void AddFoldersToCollection(string inputPath)
        {
            var i = inputPath.Count(x => x == '\\') + 1;
            try
            {
                if (Directory.Exists(inputPath))
                {
                    string name = Path.GetFileName(inputPath);
                    if (true)
                    {
                        var filePathViewModel = new FilePathViewModel()
                        {
                            Name = name + " " + (5 * i).ToString(),
                            PathString = inputPath,
                            ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-folder.png",
                            LeftMargin = new Thickness(5 * i, 0, 0, 0)
                        };
                        if (filePathViewModel.Name.Contains(":"))
                            filePathViewModel.ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-ssd.png";
                        if (filePathViewModel.Name.Contains("C:"))
                            filePathViewModel.ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-c-drive.png";
                        if (filePathViewModel.Name.Contains("ukkalita.local"))
                            filePathViewModel.ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-network.png";
                        CollectionOfParentFolders.Add(filePathViewModel);
                    }
                }
                else { }
            }
            catch { };
            AddParentFoldersToCollection(inputPath);
        }
        private Collection<FilePathViewModel> GetFilesAndFoldersCollectionForComboBox (string inputPath)
        {
            Collection<FilePathViewModel> outputCollection = new Collection<FilePathViewModel>();

            AddFoldersToCollection(inputPath);

            Collection<FilePathViewModel> foldersCollection = CollectionOfParentFolders;
            foreach (var item in foldersCollection)
            {
                outputCollection.Add(item);
            }

            var filePathViewModel = new FilePathViewModel()
            {
                Name = @"Проекты",
                PathString = @"\\ukkalita.local\iptg\Строительно-девелоперский дивизион\М1 Проект\Проекты",
                ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-network.png",
                LeftMargin = new Thickness(5,0,0,0)
            };
            outputCollection.Add(filePathViewModel);

            //var filePathViewModel = new FilePathViewModel()
            //{
            //    Name = Environment.UserName,
            //    PathString = @"C:\Users\" + Environment.UserName,
            //    ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\folder-icon.png"
            //};
            //outputCollection.Add(filePathViewModel);

            //string[] drivePaths = Directory.GetLogicalDrives();
            //foreach (var drivePath in drivePaths)
            //{
            //    var fpmv = new FilePathViewModel()
            //    {
            //        Name = drivePath,
            //        PathString = drivePath,
            //        ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\harddisk.png"
            //    };
            //    outputCollection.Add(fpmv);
            //}
            return outputCollection;
        }
        private Collection<FilePathViewModel> GetFilesAndFoldersCollectionForListView(string inputPath)
        {
            Collection<FilePathViewModel> outputCollection = new Collection<FilePathViewModel>();

            try
            {
                string[] dirPaths = Directory.GetDirectories(inputPath);
                foreach (var path in dirPaths)
                {
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            string name = Path.GetFileName(path);
                            if (true)
                            {
                                var filePathViewModel = new FilePathViewModel()
                                {
                                    Name = name,
                                    PathString = path,
                                    ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\icons8-folder.png"
                                };
                                outputCollection.Add(filePathViewModel);
                            }
                        }
                        else { }
                    }
                    catch { };
                }
            }
            catch { }

            try
            {
                string[] filePaths = Directory.GetFiles(inputPath);

                foreach (var path in filePaths)
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            string name = Path.GetFileName(path);
                            if (name.Contains(".rvt"))
                            {
                                var filePathViewModel = new FilePathViewModel()
                                {
                                    Name = name,
                                    PathString = path,
                                    ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\revitfile.png"
                                };
                                outputCollection.Add(filePathViewModel);
                            }
                        }
                        else { }
                    }
                    catch { };
                }
            }
            catch { }

            return outputCollection;
        }
        private void changeNewContentInListViewAndComboBox(object sender, EventArgs e)
        {
            string inputPath = "";
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                FilePathViewModel filePathViewModel = (FilePathViewModel)comboBox.SelectedItem;
                inputPath = filePathViewModel.PathString;
            }
            catch { };
            try
            {
                ListView listView = (ListView)sender;
                FilePathViewModel filePathViewModel = (FilePathViewModel)listView.SelectedItem;
                inputPath = filePathViewModel.PathString;
            }
            catch { };
            // input - задали путь для текущей директории
            Collection<FilePathViewModel> paths = GetFilesAndFoldersCollectionForListView(inputPath);
            if (paths.Count != 0)
            {
                listViewFiles.ItemsSource = paths;
            }
            else
            {
                paths.Clear();
                listViewFiles.ItemsSource = paths;
            }
        }
        private void gotoParentDirectoryInListview(object sender, RoutedEventArgs e)
        {
            //FilePathViewModel filePathViewModel = (FilePathViewModel)listViewFiles.SelectedItem;
            //string path = filePathViewModel.PathString;
            //string parentPath = Directory.GetParent(path);
        }
        private void listViewFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilePathViewModel filePathViewModel = new FilePathViewModel()
            {
                PathString = "",
                Name = "Ничего не выбрано"
            };
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                filePathViewModel = (FilePathViewModel)comboBox.SelectedItem;
            }
            catch { };
            try
            {
                ListView listView = (ListView)sender;
                filePathViewModel = (FilePathViewModel)listView.SelectedItem;
            }
            catch { };

            try
            {
                textBlockSelectFileOrFolder.Text = filePathViewModel.Name;
                textBlockSelectFileOrFolder.Tag = filePathViewModel;
            }
            catch { }
            

        }

    }
    public class FilePathViewModel
    {
        public string PathString { get; set; }
        public string Name { get; set; }
        public string ImgSource { get; set; }
        public Thickness LeftMargin { get; set; }
        public bool IsFile
        {
            get
            {
                if (Name.Contains(".rvt"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
