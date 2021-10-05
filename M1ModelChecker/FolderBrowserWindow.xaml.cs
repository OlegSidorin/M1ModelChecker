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

namespace M1ModelChecker
{
    /// <summary>
    /// Логика взаимодействия для FolderBrowserWindow.xaml
    /// </summary>
    public partial class FolderBrowserWindow : Window
    {
        Collection<FilePathViewModel> CollectionOfParenFolders;
        public FolderBrowserWindow()
        {
            InitializeComponent();
            CollectionOfParenFolders = new Collection<FilePathViewModel>();
            comboBox.ItemsSource = GetFilesAndFoldersCollectionForComboBox(@"\\ukkalita.local\iptg\Строительно-девелоперский дивизион\М1 Проект\Проекты\10. Отдел информационного моделирования\01. REVIT\01. Библиотека семейств\#На рассмотрение\Тест"); // (@"C:\Users\" + Environment.UserName);
            //comboBox.SelectedIndex = 0;
        }

        private void buttonClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
        void AddParenFoldersToCollection(string inputPath)
        {
            var i = inputPath.Count(x => x == '\\') - 1;
            DirectoryInfo dirInfo = Directory.GetParent(inputPath);
            if (dirInfo != null)
            {
                FilePathViewModel filePathViewModel = new FilePathViewModel()
                {
                    Name = dirInfo.Name,
                    PathString = dirInfo.FullName,
                    ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\folder-icon.png",
                    LeftMargin = new Thickness(5 * i, 0, 0, 0)
                };
                if (filePathViewModel.Name.Contains(":"))
                    filePathViewModel.ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\harddisk.png";
                CollectionOfParenFolders.Insert(0, filePathViewModel);

                if (dirInfo.Parent != null)
                {
                    AddParenFoldersToCollection(dirInfo.FullName);
                };
            }
        }

        private Collection<FilePathViewModel> GetFilesAndFoldersCollectionForComboBox (string input)
        {
            Collection<FilePathViewModel> outputCollection = new Collection<FilePathViewModel>();

            AddParenFoldersToCollection(input);
            Collection<FilePathViewModel> parentFoldersCollection = CollectionOfParenFolders;
            foreach (var item in parentFoldersCollection)
            {
                outputCollection.Add(item);
            }
            var i = input.Count(x => x == '\\');
            try
            {
                if (Directory.Exists(input))
                {
                    string name = Path.GetFileName(input);
                    if (true)
                    {
                        var filePathViewModel = new FilePathViewModel()
                        {
                            Name = name,
                            PathString = input,
                            ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\folder-icon.png",
                            LeftMargin = new Thickness(5 * i, 0, 0, 0)
                        };
                        if (filePathViewModel.Name.Contains(":"))
                            filePathViewModel.ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\harddisk.png";
                        outputCollection.Add(filePathViewModel);
                    }
                }
                else { }
            }
            catch { };



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
        private Collection<FilePathViewModel> GetFilesAndFoldersCollectionForListView(string input)
        {
            Collection<FilePathViewModel> outputCollection = new Collection<FilePathViewModel>();

            try
            {
                string[] dirPaths = Directory.GetDirectories(input);
                foreach (var path in dirPaths)
                {
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            string name = Path.GetFileName(path);
                            if (!name.Contains("."))
                            {
                                var filePathViewModel = new FilePathViewModel()
                                {
                                    Name = name,
                                    PathString = path,
                                    ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\folder-icon.png"
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
                string[] filePaths = Directory.GetFiles(input);

                foreach (var path in filePaths)
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            string name = Path.GetFileName(path);
                            if (name.Contains(".rfa"))
                            {
                                var filePathViewModel = new FilePathViewModel()
                                {
                                    Name = name,
                                    PathString = path,
                                    ImgSource = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\revitfile-icon.png"
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
            string input = "";
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                FilePathViewModel filePathViewModel = (FilePathViewModel)comboBox.SelectedItem;
                input = filePathViewModel.PathString;
            }
            catch { };
            try
            {
                ListView listView = (ListView)sender;
                FilePathViewModel filePathViewModel = (FilePathViewModel)listView.SelectedItem;
                input = filePathViewModel.PathString;
            }
            catch { };
            // input - задали путь для текущей директории
            Collection<FilePathViewModel> paths = GetFilesAndFoldersCollectionForListView(input);
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
    }
    public class FilePathViewModel
    {
        public string PathString { get; set; }
        public string Name { get; set; }
        public string ImgSource { get; set; }
        public Thickness LeftMargin { get; set; }
    }
}
