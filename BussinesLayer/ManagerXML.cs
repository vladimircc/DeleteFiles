using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using Support;
using CommonLayer;

namespace BusinessLayer
{
    public class ManagerXML
    {
        private readonly string pathConfigDirs;

        public ManagerXML()
        {
        }

        public List<Directory> ReaderConfigDirs(string pathFile)
        {
            
            List<Directory> lstDirectories = new List<Directory>();
            
            using(XmlTextReader reader = new XmlTextReader(@pathFile))
            {
                try
                {
                    Directory dir = null;
                    while(reader.Read())
                    {
                        if(reader.NodeType == XmlNodeType.Element && reader.Name.Equals("Directory"))
                        {
                            dir = new Directory();
                            dir.Patterns = new List<string>();
                        }
                        else
                        {
                            switch(reader.Name)
                            {
                                case "Path":
                                    Log.Instance.Message("Reading Configuration....");
                                    dir.Path = reader.ReadString();
                                    Log.Instance.Message("Path: " + dir.Path);
                                    break;
                                case "MoveTo":
                                    dir.MoveTo = reader.ReadString();
                                    Log.Instance.Message("MoveTo: " + dir.MoveTo);
                                    break;
                                case "ForceDelete":
                                    dir.ForceDelete = Convert.ToBoolean(reader.ReadString());
                                    Log.Instance.Message("ForceDelete: " + dir.ForceDelete);
                                    break;
                                case "Pattern":
                                    dir.Patterns.Add(reader.ReadString());
                                    Log.Instance.Message("Pattern: " + reader.ReadString());
                                    break;
                                case "KeepDaysOfLog":
                                    dir.KeepDaysOfLog = Convert.ToInt32(reader.ReadString());
                                    Log.Instance.Message("KeepDaysOfLog: " + dir.KeepDaysOfLog);
                                    
                                    //Agregamos a la lista y ponemos en null después porque es el ultimo elemento de
                                    //Directory, si agregaramos otro debemos recorrer estas dos lineas al siguiente "case"
                                    lstDirectories.Add(dir);
                                    dir = null;
                                    Log.Instance.Message("CONFIG READER");
                                    break;
                            }
                        }
                    }
                    Log.Instance.MessageColor("Lista de Directorios: " + lstDirectories.Count);
                }
                catch(Exception ex)
                {
                    Log.Instance.Exception(ex, "");
                }
            }

            return lstDirectories;
        }
    }
}
