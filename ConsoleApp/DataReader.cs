namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport)
        {
            ImportedObjects = new List<ImportedObject>() { new ImportedObject() };

            try
            {
                var streamReader = new StreamReader(fileToImport);

                var importedLines = new List<string>();
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    importedLines.Add(line);
                }

                for (int i = 0; i < importedLines.Count; i++)
                {
                    var importedLine = importedLines[i];
                    var values = importedLine.Split(';');
                    var importedObject = new ImportedObject();
                    if (values.Length == 7)
                    {
                        importedObject.Type = values[0];
                        importedObject.Name = values[1];
                        importedObject.Schema = values[2];
                        importedObject.ParentName = values[3];
                        importedObject.ParentType = values[4];
                        importedObject.DataType = values[5];
                        importedObject.IsNullable = values[6];
                        ((List<ImportedObject>)ImportedObjects).Add(importedObject);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // clear and correct imported data
            foreach (var importedObject in ImportedObjects)
            {
                if (importedObject.Type != null)
                    importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                else
                    importedObject.Type = "";
                if (importedObject.Name != null)
                    importedObject.Name = importedObject.Name.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                else
                    importedObject.Name = "";
                if (importedObject.Schema != null)
                    importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                else
                    importedObject.Schema = "";
                if (importedObject.ParentName != null)
                    importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                else
                    importedObject.ParentName = "";
                if (importedObject.ParentType != null)
                    importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                else
                    importedObject.ParentType = "";
            }

            // assign number of children
            for (int i = 0; i < ImportedObjects.Count(); i++)
            {
                var importedObject = ImportedObjects.ToArray()[i];
                foreach (var impObj in ImportedObjects)
                {
                    if (impObj.ParentType == importedObject.Type && impObj.ParentName == importedObject.Name)
                    {
                        importedObject.NumberOfChildren = 1 + importedObject.NumberOfChildren;
                    }
                }
            }

            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    // print all database's tables
                    foreach (var table in ImportedObjects)
                    {
                        if (table.ParentType.ToUpper() == database.Type && table.ParentName == database.Name)
                        {
                            Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                            // print all table's columns
                            foreach (var column in ImportedObjects)
                            {
                                if (column.ParentType.ToUpper() == table.Type && column.ParentName == table.Name)
                                {
                                    Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                }
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
