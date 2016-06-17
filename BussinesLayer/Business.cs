using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Support;
using CommonLayer;

namespace BusinessLayer
{
    public class Business
    {
        ManagerXML xml;

        #region Constructor
        public Business()
        {
            xml = new ManagerXML();
        }
        #endregion

        public List<Directory> GetConfigDirectories(string pathXmlConfig)
        {
            List<Directory> lstDirs = new List<Directory>();

            try
            {
                lstDirs = xml.ReaderConfigDirs(pathXmlConfig);
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, "");
            }

            return lstDirs;
        }

        public void DeleteFiles(string pathXmlConfig)
        {
            List<Directory> lstDirs = new List<Directory>();

            try
            {
                lstDirs = GetConfigDirectories(pathXmlConfig);

                ManagerFiles.Instance.DeleteFiles(lstDirs);
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, String.Format(""));
            }
        }

        public void MoveFiles(string pathXmlConfig)
        {
            List<Directory> lstDirs = new List<Directory>();

            try
            {
                lstDirs = GetConfigDirectories(pathXmlConfig);

                ManagerFiles.Instance.MoveFiles(lstDirs);
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, String.Format(""));
            }
        }
    }
}
