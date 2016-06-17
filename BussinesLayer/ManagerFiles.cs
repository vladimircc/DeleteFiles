using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BusinessLayer
{
    public class ManagerFiles
    {
        #region Fields
        private static volatile ManagerFiles instance;
        private static readonly object syncRoot = new object();
        #endregion

        #region Constructor
        private ManagerFiles()
        {
            this.CurrentPath = @System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + System.IO.Path.DirectorySeparatorChar;
            this.AppName = AppDomain.CurrentDomain.FriendlyName;
        }
        #endregion

        #region Properties
        public string CurrentPath
        {
            get;
            private set;
        }

        public string AppName
        {
            get;
            private set;
        }

        public static ManagerFiles Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(syncRoot)
                    {
                        if(instance == null)
                        {
                            instance = new ManagerFiles();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        #region Methods
        public void DeleteFiles(List<CommonLayer.Directory> directories)
        {
            try
            {
                Log.Instance.Message("ManagerFiles - IN DeleteFiles(List<CommonLayer.Directory> directories)");

                foreach(CommonLayer.Directory dir in directories)
                {
                    if(Directory.Exists(dir.Path))
                    {
                        Log.Instance.Message(String.Format("ManagerFiles - Directory found {0}", dir.Path));
                        
                        DirectoryInfo dirInfo = new DirectoryInfo(dir.Path);
                        List<FileInfo> lstFilesFiltereds = new List<FileInfo>();

                        foreach(string pattern in dir.Patterns)
                        {
                            var files = SearchFilesByPattern(dirInfo, pattern, dir.KeepDaysOfLog);

                            if(files.Count() > 0)
                                lstFilesFiltereds.AddRange(files.ToList());
                        }

                        var counter = lstFilesFiltereds.Count();

                        if(counter > 0)
                        {
                            Log.Instance.MessageColor(String.Format("ManagerFiles - {0} Files Founded", counter));

                            foreach(var file in lstFilesFiltereds)
                            {
                                Log.Instance.Message(String.Format("ManagerFiles - Last Write Time: {0} {1}", file.LastWriteTime, file.FullName));
                                DeleteFile(file.FullName);
                                DeleteDirectory(file.DirectoryName, false);
                            }

                            DeleteDirectorys(dir.Path, true);
                        }
                        else
                            Log.Instance.Message(String.Format("ManagerFiles - There are not files in the directory {0}", dir));
                    }
                    else
                        Log.Instance.Warning(String.Format("ManagerFiles - DIRECTORY NOT EXIST {0}", dir));
                }

                Log.Instance.Message("ManagerFiles - OUT DeleteFiles(List<CommonLayer.Directory> directories)");
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, "");
            }
        }

        public List<FileInfo> SearchFilesByPattern(DirectoryInfo pDirInfo, string pPattern, int pDaysToKeep)
        {
            List<FileInfo> lstFileInfo = new List<FileInfo>();

            try
            {
                var files = pDirInfo.GetFiles(pPattern, SearchOption.AllDirectories)
                            .Where(a => a.LastWriteTime < DateTime.Now.AddDays(-pDaysToKeep));

                lstFileInfo.AddRange(files.ToList());
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, "");
            }

            return lstFileInfo;
        }

        public void DeleteFile(string pFullPath)
        {
            try
            {
                if(File.Exists(pFullPath))
                {
                    File.Delete(pFullPath);
                    Log.Instance.MessageColor(String.Format("ManagerFiles - FILE DELETED: {0} ", pFullPath));
                }
                else
                    Log.Instance.MessageColor(String.Format("ManagerFiles - FILE NOT EXIST {0}", pFullPath));
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, "");
            }
        }

        public void DeleteDirectorys(string pFullPath, bool pForceDeleteWithFiles)
        {
            try
            {
                Log.Instance.MessageColor("ManagerFiles - Deleting directories...");

                if(Directory.Exists(pFullPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(pFullPath);

                    var counter = dirInfo.GetDirectories().Count();

                    if(counter > 0)
                    {
                        Log.Instance.Message(String.Format("ManagerFiles - {0} Directories founded", counter));

                        var subDirs = dirInfo.GetDirectories();

                        foreach(var dir in subDirs)
                        {
                            DeleteDirectory(dir.FullName, pForceDeleteWithFiles);
                        }
                    }
                    else
                    {
                        Log.Instance.Warning(string.Format("ManagerFiles - There are not Directories in {0}", pForceDeleteWithFiles));
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, "");
            }
        }

        public void DeleteDirectory(string pFullPath, bool pForceDeleteWithFiles)
        {
            try
            {
                if(Directory.Exists(pFullPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(pFullPath);

                    var countFile = dir.GetFiles("*.*",SearchOption.AllDirectories).Count();
                    var countDirs = dir.GetDirectories().Count();

                    if(countFile == 0 && countDirs == 0)
                    {
                        Directory.Delete(pFullPath);
                        Log.Instance.MessageColor(String.Format("ManagerFiles - DIRECTORY DELETED: {0}", pFullPath));
                    }
                    else
                    {
                        if(pForceDeleteWithFiles)
                        {
                            Directory.Delete(pFullPath);
                            Log.Instance.MessageColor(String.Format("ManagerFiles - DIRECTORY DELETED (Force Delete {0}): {1}", pForceDeleteWithFiles, pFullPath));
                        }
                        else
                            Log.Instance.Warning(String.Format("ManagerFiles - Can't be delete the directory {0} already contains {1} Directories | {2} Files (Force Delete: {3})",
                                pFullPath, countDirs, countFile, pForceDeleteWithFiles));
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, String.Format("EXCEPTION IN DeleteDirectory(string pFullPath ({0}), bool pForceDeleteWithFiles)", pFullPath));
            }
        }
        #endregion

        public void MoveFiles(List<CommonLayer.Directory> directories)
        {
            try
            {

            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, String.Format(""));    
            }
        }
    }
}
