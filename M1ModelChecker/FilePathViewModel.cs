using System.Windows;

namespace M1ModelChecker
{
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
