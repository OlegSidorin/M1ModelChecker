using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M1ModelChecker
{
    class CM
    {
        public static string PathToFOP { get; } = @"C:\Users\o.sidorin\source\repos\M1ModelChecker\M1ModelChecker\res\ФОП2019.txt";
        public static List<SharedParameterFromFOP> SharedParametersFromFOP { get; } = GetSharedParametersFromFOP();
        public static List<SharedParameterFromFOP> GetSharedParametersFromFOP()
        {
            List<SharedParameterFromFOP> outputList = new List<SharedParameterFromFOP>();
            string line;
            string guid, name, type;
            
            int startGuid, endGuid, startName, endName, startType, endType;

            // Read the file line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader(PathToFOP);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("PARAM"))
                {
                    startGuid = line.IndexOf('\t', 0) + 1;
                    endGuid = line.IndexOf('\t', startGuid + 1);
                    guid = line.Substring(startGuid, endGuid - startGuid);
                    startName = endGuid + 1;
                    endName = line.IndexOf('\t', startName + 1);
                    name = line.Substring(startName, endName - startName);
                    startType = endName + 1;
                    endType = line.IndexOf('\t', startType + 1);
                    type = line.Substring(startType, endType - startType);
                    SharedParameterFromFOP sharedParameterFromFOP = new SharedParameterFromFOP()
                    {
                        Guid = guid,
                        Name = name,
                        Type = type
                    };
                    outputList.Add(sharedParameterFromFOP);
                }
            }

            file.Close();
            return outputList;
        }
        public static bool IsSomethingWrongWithParameter (string guid, string name, out string report)
        {
            report = "";
            bool output = true;
            foreach (var par in SharedParametersFromFOP)
            {
                if (guid == par.Guid && name == par.Name)
                {
                    report = "Параметр из ФОП";
                    return false;
                }
                if (guid != par.Guid && name == par.Name)
                {
                    report = "Параметр имеет неверный guid";
                    return true;
                }
                if (guid == par.Guid && name != par.Name)
                {
                    report = "Параметр имеет неверное имя";
                    return true;
                }
                if (guid != par.Guid && name != par.Name)
                {
                    report = "Параметр не из ФОП";
                }
            }

            return output;
        }
        public static string MakeHTML(string str)
        {
            string begin = $@"<!DOCTYPE html><html lang=""ru"" xmlns=""http://www.w3.org/1999/xhtml""><head><meta charset = ""utf-8"" /><title> Отчет </title></head ><body style=""font-family: verdana; "">";
            string end = $@"</body></html>";
            return begin + str + end;
        }
    }
    class SharedParameterFromFOP
    {
        public string Name { get; set; }
        public string Guid { get; set; }
        public string Type { get; set; }
    }
    class WrongParameter
    {
        public bool NotInFOP { get; set; }
        public bool WrongGuid { get; set; }
        public bool WrongName { get; set; }
    }
}
