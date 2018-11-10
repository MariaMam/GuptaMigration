using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;


namespace GuptaMigration.Services
{
    public class FileReader
    {
        static string FolderPath = "D:\\MA\\Nov2018\\subversion\\sanalogic\\trunk";

        public static List<Tuple<string,string>> ReadGuptaFiles()
        {
            List < Tuple< string,string>> files = new List<Tuple<string, string>>();
            try
            {   // Open the text file using a stream reader.

                foreach (string file in Directory.EnumerateFiles(FolderPath, "*.ap*", SearchOption.AllDirectories))
                {
                    string contents = File.ReadAllText(file);
                    string name = Path.GetFileNameWithoutExtension(file);
                    Tuple<string, string> tuple = new Tuple<string, string>(name, contents);
                    files.Add(tuple);
                }                

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return files;
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

        public void WriteToExcel()
        {
            //Create the data set and table
            DataSet ds = new DataSet("New_DataSet");
            DataTable dt = new DataTable("New_DataTable");
            /*
            //Set the locale for each
            ds.Locale = System.Threading.Thread.CurrentThread.CurrentCulture;
            dt.Locale = System.Threading.Thread.CurrentThread.CurrentCulture;

            //Open a DB connection (in this example with OleDB)
            OleDbConnection con = new OleDbConnection(dbConnectionString);
            con.Open();

            //Create a query and fill the data table with the data from the DB
            string sql = "SELECT Whatever FROM MyDBTable;";
            OleDbCommand cmd = new OleDbCommand(sql, con);
            OleDbDataAdapter adptr = new OleDbDataAdapter();

            adptr.SelectCommand = cmd;
            adptr.Fill(dt);
            con.Close();

            //Add the table to the data set
            ds.Tables.Add(dt);

            //Here's the easy part. Create the Excel worksheet from the data set
            ExcelLibrary.DataSetHelper.CreateWorkbook("MyExcelFile.xls", ds);
            */

        }
    }
}
