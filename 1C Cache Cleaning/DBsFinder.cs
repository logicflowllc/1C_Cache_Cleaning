using System;
using System.Collections.Generic;
using System.IO;

namespace _1C_Cache_Cleaning
{
    class DBsFinder
    {
        public SortedDictionary<string,string> getDBsList ()
        {
            // Read all lines from ibases.v8i file
            string[] Allv8iLines = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\1C\1CEStart\ibases.v8i");

            // Dictionary for DBs nescessary data 
            Dictionary<string, string> DBData = new Dictionary<string, string>();

            for (int index = 0; index < Allv8iLines.Length - 1; index++)
            {
                if (Allv8iLines[index].Contains("File"))
                {
                    // Get clean DB Name
                    string CurrentDBName = Allv8iLines[index - 1].Substring(1, Allv8iLines[index - 1].Length - 2);

                    // Get DB name in startup menu with replaced slashes
                    string CurrentFolderName = Allv8iLines[index + 3].Substring(8, Allv8iLines[index + 3].Length - 8).Replace("/",  " / ");
                    
                    // Add right name into the list (with subfolders or without)
                    if (CurrentFolderName != "")
                    {
                        CurrentDBName = CurrentFolderName + " / " + CurrentDBName;
                    }

                    // Get DB path
                    string CurrentDBPath = (Allv8iLines[index].Substring(14, Allv8iLines[index].Length - 16)).Replace(@"\\", @"\");

                    if (Char.IsLetter(CurrentDBPath, 0))
                    {
                        DBData.Add(CurrentDBName, CurrentDBPath);
                    }
                }
            }

            // Order list by letters
            SortedDictionary<string, string> FinalDBList = new SortedDictionary<string, string>(DBData);
            return FinalDBList;
        }
    }
}
