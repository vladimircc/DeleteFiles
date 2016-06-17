using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Support;
using System.Threading;
using BusinessLayer;

namespace DeleteFiles
{
    class Program
    {
        private Thread thrDelete;
        private bool isRunning, moveFiles, deleteFiles;
        private int segToCheck;
        private string pathXmlConfigDirs, timeExecDelFiles;
        Business bl;

        public Program()
        {
            bl = new Business();

            LoadConfiguration();
            Start();

            Log.Instance.MessageColor("***** Delete Files Started *****");
        }

        private void LoadConfiguration()
        {
            try
            {
                segToCheck = Convert.ToInt32(Utils.Instance.GetAppKey("segToCheck")) * 1000;
                pathXmlConfigDirs = Utils.Instance.GetAppKey("PathFileConfigDirs");
                timeExecDelFiles = Utils.Instance.GetAppKey("TimeExecDelFiles");
                moveFiles = Convert.ToBoolean(Utils.Instance.GetAppKey("MoveFiles"));
                deleteFiles = Convert.ToBoolean(Utils.Instance.GetAppKey("DeleteFiles"));

                Log.Instance.Message(String.Format("Time Interval Check (mili-seconds): {0}", segToCheck));
                Log.Instance.Message(String.Format("Path to config Dirs: {0}", pathXmlConfigDirs));
                Log.Instance.Message(String.Format("Time to Delete Files: {0}", timeExecDelFiles));

                //Read XML
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, "");
            }
        }

        private void Start()
        {
            try
            {
                thrDelete = new Thread(InitProcess);
                thrDelete.Start();

                bl.DeleteFiles(pathXmlConfigDirs);
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, "");
            }
        }

        private void InitProcess()
        {
            isRunning = true;

            while(isRunning)
            {
                try
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");

                    if(time.Equals(timeExecDelFiles))
                    {
                        /*if(moveFiles)
                        {
                            Log.Instance.Message("DeleteFiles - Moving files...");
                            bl.MoveFiles(pathXmlConfigDirs);
                            Log.Instance.Message("FILES MOVED");
                        }
                        else if(deleteFiles)
                        {
                            Log.Instance.Message("DeleteFiles - Deleting files...");
                            bl.DeleteFiles(pathXmlConfigDirs);
                            Log.Instance.Message("FILES DELETED");
                        }*/

                        Log.Instance.Message("DeleteFiles - Deleting files...");
                        bl.DeleteFiles(pathXmlConfigDirs);
                        Log.Instance.Message("FILES DELETED");
                    }
                    
                    Thread.Sleep(segToCheck);
                }
                catch(Exception ex)
                {
                    Log.Instance.Exception(ex, "");
                }
            }
        }

        static void Main(string[] args)
        {
            Program p = new Program();
        }
    }
}
