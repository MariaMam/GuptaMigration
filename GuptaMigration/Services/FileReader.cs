using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace GuptaMigration.Services
{
    public class FileReader
    {
        string FolderPath = "D:\\MA\\Nov2018\\subversion\\sanalogic\\trunk";

        public void ReadGuptaFiles()
        {
            try
            {   // Open the text file using a stream reader.

                foreach (string file in Directory.EnumerateFiles(FolderPath, "*.apl"))
                {
                    string contents = File.ReadAllText(file);

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        public List<string> ReadTableNames()
        {
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            List<string> TableNames = new List<string>();
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(dir+"\\TableNames.txt"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        TableNames.Add(line);
                    }
                }                

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return TableNames;
        }
    }
}
