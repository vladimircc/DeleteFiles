using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Configuration;

namespace Support
{
    public class Utils
    {
        #region Fields
        private static volatile Utils instance;
        private static readonly object syncRoot = new object();
        #endregion

        #region Constructor
        private Utils()
        {
            this.CurrentPath = @System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + System.IO.Path.DirectorySeparatorChar;
            this.AppName = AppDomain.CurrentDomain.FriendlyName;
        }
        #endregion

        #region Properties
        public static Utils Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(syncRoot)
                    {
                        if(instance == null)
                        {
                            instance = new Utils();
                        }
                    }
                }

                return instance;
            }
        }

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
        #endregion

        #region Methods
        public int GetIntegerFromXAttribute(XElement xml, string key)
        {
            var returnValue = 0;
            if(xml.Attribute(key) != null)
            {
                try
                {
                    returnValue = (int)xml.Attribute(key);
                }
                catch
                {
                    returnValue = 0;
                }
            }
            else
            {
                returnValue = 0;
            }

            return returnValue;
        }

        public bool GetBooleanFromXAttribute(XElement xml, string key)
        {
            var returnValue = false;
            if(xml.Attribute(key) != null)
            {
                try
                {
                    returnValue = (bool)xml.Attribute(key);
                }
                catch
                {
                    returnValue = false;
                }
            }
            else
            {
                returnValue = false;
            }

            return returnValue;
        }

        public string GetStringFromXAttribute(XElement xml, string key)
        {
            var returnValue = string.Empty;
            if(xml.Attribute(key) != null)
            {
                try
                {
                    returnValue = (string)xml.Attribute(key);
                }
                catch
                {
                    returnValue = string.Empty;
                }
            }
            else
            {
                returnValue = string.Empty;
            }

            return returnValue;
        }

        public int GetIntegerFromXElement(XElement xml, string key)
        {
            var returnValue = 0;
            if(xml.Element(key) != null)
            {
                try
                {
                    returnValue = (int)xml.Element(key);
                }
                catch
                {
                    returnValue = 0;
                }
            }
            else
            {
                returnValue = 0;
            }

            return returnValue;
        }

        public bool GetBooleanFromXElement(XElement xml, string key)
        {
            var returnValue = false;
            if(xml.Element(key) != null)
            {
                try
                {
                    returnValue = (bool)xml.Element(key);
                }
                catch
                {
                    returnValue = false;
                }
            }
            else
            {
                returnValue = false;
            }

            return returnValue;
        }

        public string GetStringFromXElement(XElement xml, string key)
        {
            var returnValue = string.Empty;
            if(xml.Element(key) != null)
            {
                try
                {
                    returnValue = (string)xml.Element(key);
                }
                catch
                {
                    returnValue = string.Empty;
                }
            }
            else
            {
                returnValue = string.Empty;
            }

            return returnValue;
        }

        public T ConvertStringToEnum<T>(string enumString)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), enumString, true);
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, "Exception in Utils.ConverStringToEnum -- {0}", ex.Message);
                T temp = default(T);
                string s = String.Format("'{0}' is not a valid enumeration of '{1}'", enumString, temp.GetType().Name);
                throw new Exception(s, ex);
            }
        }

        public string GetAppKey(string key)
        {
            string value = "";

            try
            {
                value = ConfigurationManager.AppSettings.Get(key);
            }
            catch(Exception ex)
            {
                Log.Instance.Exception(ex, String.Format("EXCEPTION IN GetAppKey(string key ({0}))", key));
            }

            return value;
        }
        #endregion
    }
}

