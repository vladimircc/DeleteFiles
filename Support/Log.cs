using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Configuration;
using Support;
using System.IO;

sealed public class Log : IDisposable
{
    #region Fields
    private StreamWriter currentFile = null;
    private DateTime currentDateTime;
    private DateTime fileDateTime;
    private string path;
    private string name;
    private static volatile Log instance;
    private static readonly object syncRoot = new object();
    private readonly Thread threadDequeue;
    private readonly Queue queueMessages = new Queue();
    private readonly EventWaitHandle waitHandle = new AutoResetEvent(false);

    /// <summary>
    /// Header to format exceptions
    /// </summary>
    private readonly string exceptionHeader =
        "################################################\n" +
        "#   BEGIN OF EXCEPTION                         #\n" +
        "################################################\n";

    /// <summary>
    /// Footer to format exceptions
    /// </summary>
    private readonly string exceptionFooter =
        "################################################\n" +
        "#   END OF EXCEPTION                           #\n" +
        "################################################\n";

    /// <summary>
    /// Header to format exception stack traces
    /// </summary>
    private readonly string stackTraceHeader =
        "################################################\n" +
        "#   BEGIN OF STACK TRACE                       #\n" +
        "################################################\n";

    /// <summary>
    /// Footer to format exception stack traces
    /// </summary>
    private readonly string stackTraceFooter =
        "################################################\n" +
        "#   END OF STACK TRACE                         #\n" +
        "################################################\n";

    /// <summary>
    /// Header to format inner exceptions
    /// </summary>
    private readonly string innerExceptionHeader =
        "################################################\n" +
        "#   BEGIN OF INNER EXCEPTION                   #\n" +
        "################################################\n";

    /// <summary>
    /// Footer to format inner exceptions
    /// </summary>
    private readonly string innerExceptionFooter =
        "################################################\n" +
        "#   END OF INNER EXCEPTION                     #\n" +
        "################################################\n";

    /// <summary>
    /// Header to format inner exception stack traces
    /// </summary>
    private readonly string innerStackTraceHeader =
        "################################################\n" +
        "#   BEGIN OF INNER STACK TRACE                 #\n" +
        "################################################\n";

    /// <summary>
    /// Footer to format inner exception stack traces
    /// </summary>
    private readonly string innerStackTraceFooter =
        "################################################\n" +
        "#   END OF INNER STACK TRACE                   #\n" +
        "################################################\n";
    #endregion

    #region Constructor
    private Log()
    {
        this.LoadConfiguration();
        this.threadDequeue = new Thread(this.ThreadWrite) { IsBackground = true };
        this.threadDequeue.Start();
    }
    #endregion

    #region Destructor
    /// <summary>
    /// Finalizes an instance of the <see cref="Log"/> class. 
    /// </summary>
    ~Log()
    {
        this.Dispose(false);
    }
    #endregion

    #region Properties
    public static Log Instance
    {
        get
        {
            if(instance == null)
            {
                lock(syncRoot)
                {
                    if(instance == null)
                    {
                        instance = new Log();
                    }
                }
            }

            return instance;
        }
    }

    public LevelMessage Level
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }
    #endregion

    #region Enums
    /// <summary>
    /// Menssage type
    /// </summary>
    public enum LevelMessage
    {
        /// <summary>
        /// Generic message
        /// </summary>
        Message = 1,

        /// <summary>
        /// Debug logging
        /// </summary>
        Debug = 2,

        /// <summary>
        /// Debug 2 logging
        /// </summary>
        Debug2 = 3
    }

    /// <summary>
    /// Message type
    /// </summary>
    private enum Type
    {
        /// <summary>
        /// Generic message
        /// </summary>
        Msg,

        /// <summary>
        /// Message Color
        /// </summary>
        MsgColor,
        
        /// <summary>
        /// Warning for not normal that ocurred
        /// </summary>
        War,

        /// <summary>
        /// Error that could stop the process
        /// </summary>
        Err,

        /// <summary>
        /// Exception occurred
        /// </summary>
        Exc,

        /// <summary>
        /// Debug logging
        /// </summary>
        Dbg,

        /// <summary>
        /// Debug 2 logging
        /// </summary>
        Dbg2
    }
    #endregion

    #region Methods
    #region Public

    /// <summary>
    /// Log a standard message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void Message(string message, params object[] args)
    {
        try
        {
            string formattedMessage = string.Format(message, args);
            this.Write(formattedMessage, Type.Msg, null);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Exception writing message to Log: {0}", ex.Message);
            Console.WriteLine("Stack trace writing message to Log: {0}", ex.StackTrace);
        }
    }

    /// <summary>
    /// Log a Color message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void MessageColor(string message, params object[] args)
    {
        try
        {
            string formattedMessage = string.Format(message, args);
            this.Write(formattedMessage, Type.MsgColor, null);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Exception writing message to Log: {0}", ex.Message);
            Console.WriteLine("Stack trace writing message to Log: {0}", ex.StackTrace);
        }
    }

    /// <summary>
    /// Log an error message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void Error(string message, params object[] args)
    {
        try
        {
            string formattedMessage = string.Format(message, args);
            this.Write(formattedMessage, Type.Err, null);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Exception writing error to Log: {0}", ex.Message);
            Console.WriteLine("Stack trace writing message to Log: {0}", ex.StackTrace);
        }
    }

    /// <summary>
    /// Log an exception message
    /// </summary>
    /// <param name="ex">
    /// The exception to be logged.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void Exception(Exception ex, string message, params object[] args)
    {
        try
        {
            string formattedMessage = string.Format(message, args);
            this.Write(formattedMessage, Type.Exc, ex);
        }
        catch(Exception exc)
        {
            Console.WriteLine("Exception writing exception to Log: {0}", exc.Message);
            Console.WriteLine("Stack trace writing message to Log: {0}", ex.StackTrace);
        }
    }

    /// <summary>
    /// Log an exception message
    /// </summary>
    /// <param name="ex">
    /// The exception to be logged.
    /// </param>
    public void Exception(Exception ex)
    {
        try
        {
            this.Write(string.Empty, Type.Exc, ex);
        }
        catch(Exception exc)
        {
            Console.WriteLine("Exception writing exception to Log: {0}", exc.Message);
            Console.WriteLine("Stack trace writing message to Log: {0}", ex.StackTrace);
        }
    }

    /// <summary>
    /// Log a warning message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void Warning(string message, params object[] args)
    {
        try
        {
            string formattedMessage = string.Format(message, args);
            this.Write(formattedMessage, Type.War, null);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Exception writing warning to Log: {0}", ex.Message);
            Console.WriteLine("Stack trace writing message to Log: {0}", ex.StackTrace);
        }
    }

    /// <summary>
    /// Log a message for debugging
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void Debug(string message, params object[] args)
    {
        try
        {
            string formattedMessage = string.Format(message, args);
            this.Write(formattedMessage, Type.Dbg, null);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Exception writing debug with message : " + message);
            Console.WriteLine("Exception writing debug to Log: {0}", ex.Message);
            Console.WriteLine("Stack trace writing message to Log: {0}", ex.StackTrace);
        }
    }

    /// <summary>
    /// Log a message for debugging
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void Debug2(string message, params object[] args)
    {
        try
        {
            string formattedMessage = string.Format(message, args);
            this.Write(formattedMessage, Type.Dbg2, null);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Exception writing debug2 to Log: {0}", ex.Message);
            Console.WriteLine("Stack trace writing message to Log: {0}", ex.StackTrace);
        }
    }

    /// <summary>
    /// Dispose Method
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.Dispose(true);
    }

    #endregion

    #region Protected
    /// <summary>
    /// Dispose Method
    /// </summary>
    /// <param name="disposing">true to perform the disposal</param>
    protected void Dispose(bool disposing)
    {
        if(!disposing)
        {
            return;
        }
    }
    #endregion

    #region Private
    /// <summary>
    /// Format exception to be logged
    /// </summary>
    /// <param name="exception">
    /// The exception.
    /// </param>
    /// <returns>
    /// Formatted exception
    /// </returns>
    private string FormatException(Exception exception)
    {
        if(exception != null)
        {
            StringBuilder sb = new StringBuilder();

            string message = string.IsNullOrEmpty(exception.Message) ? string.Empty : exception.Message;
            sb.Append(this.exceptionHeader);
            sb.Append(message + "\n");
            sb.Append(this.exceptionFooter);

            string stackTrace = string.IsNullOrEmpty(exception.StackTrace) ? string.Empty : exception.StackTrace;
            sb.Append(this.stackTraceHeader);
            sb.Append(stackTrace + "\n");
            sb.Append(this.stackTraceFooter);

            Exception innerException = exception.InnerException;
            if(innerException != null)
            {
                string innerMessage = string.IsNullOrEmpty(innerException.Message) ? string.Empty : innerException.Message;
                sb.Append(this.innerExceptionHeader);
                sb.Append(innerMessage + "\n");
                sb.Append(this.innerExceptionFooter);

                string innerStackTrace = string.IsNullOrEmpty(innerException.StackTrace) ? string.Empty : innerException.StackTrace;
                sb.Append(this.innerStackTraceHeader);
                sb.Append(innerStackTrace + "\n");
                sb.Append(this.innerStackTraceFooter);
            }

            return sb.ToString();
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Enqueues a line to be logged
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="messageType">Message type</param>
    /// <param name="exception">exception argument</param>
    private void Write(string message, Type messageType, Exception exception)
    {
        lock(syncRoot)
        {
            this.queueMessages.Enqueue(new object[] { message, messageType, exception, Thread.CurrentThread.ManagedThreadId });
            this.waitHandle.Set();
        }
    }

    /// <summary>
    /// Verify whether the message is to be logged according to configured log level
    /// </summary>
    /// <param name="messageType">
    /// The message type.
    /// </param>
    /// <returns>
    /// Evaluation for logging
    /// </returns>
    private bool MessageCanBeLogged(Log.Type messageType)
    {
        bool result = false;

        switch(this.Level)
        {
            case LevelMessage.Message:
                if(messageType == Type.Err || messageType == Type.Exc || messageType == Type.War || messageType == Type.Msg || messageType == Type.MsgColor)
                {
                    result = true;
                }

                break;
            case LevelMessage.Debug:
                if(messageType == Type.Err || messageType == Type.Exc || messageType == Type.War || messageType == Type.Msg || messageType == Type.Dbg)
                {
                    result = true;
                }

                break;
            case LevelMessage.Debug2:
                if(messageType == Type.Err || messageType == Type.Exc || messageType == Type.War || messageType == Type.Msg || messageType == Type.Dbg || messageType == Type.Dbg2 || messageType == Type.MsgColor)
                {
                    result = true;
                }

                break;
        }

        return result;
    }

    private string GetCurrentTime()
    {
        return string.Format("{0}:{1}:{2}.{3}", this.currentDateTime.Hour.ToString("00"), currentDateTime.Minute.ToString("00"), currentDateTime.Second.ToString("00"), currentDateTime.Millisecond.ToString("000"));
    }

    /// <summary>
    /// Log through trace object
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="messageType">
    /// The message type.
    /// </param>
    /// <param name="exception">
    /// The exception.
    /// </param>
    private void LogToTrace(string message, Log.Type messageType, Exception exception, int threadId)
    {
        this.currentDateTime = DateTime.Now;

        var string2print = string.Format("{0} {1} - {2} |- {3}"
            , DateTime.Now.ToString("dd/MM/yyyy"), this.GetCurrentTime(), threadId.ToString("00000"), message);

        this.CheckDate();

        switch(messageType)
        {
            case Type.Msg:
            case Type.Dbg:
            case Type.Dbg2:
                this.currentFile.WriteLine(string2print);
                Console.WriteLine(string2print);
                break;

            case Type.War:
                Console.Out.Flush();
                Console.ForegroundColor = ConsoleColor.Yellow;
                this.currentFile.WriteLine(string2print);
                Console.WriteLine(string2print);
                Console.ResetColor();
                break;

            case Type.Exc:
            case Type.Err:
                Console.Out.Flush();
                Console.ForegroundColor = ConsoleColor.Red;
                string2print += '\n' + FormatException(exception);
                this.currentFile.WriteLine(string2print);
                Console.WriteLine(string2print);
                Console.ResetColor();
                break;

            case Type.MsgColor:
                Console.Out.Flush();
                Console.ForegroundColor = ConsoleColor.Cyan;
                this.currentFile.WriteLine(string2print);
                Console.WriteLine(string2print);
                Console.ResetColor();
                break;
        }
    }

    /// <summary>
    /// Actions for worker thread that persists logs asynchronously
    /// </summary>
    private void ThreadWrite()
    {
        while(true)
        {
            try
            {
                object data = null;

                lock(syncRoot)
                {
                    if(this.queueMessages.Count > 0)
                    {
                        data = this.queueMessages.Dequeue();
                    }
                }

                // If no data was found (i.e. queue is empty), wait
                if(data == null)
                {
                    this.waitHandle.WaitOne();

                    // Once signaled, there is already data enqueued, continue loop
                    continue;
                }

                object[] parameters = (object[])data;
                var message = (string)parameters[0];
                var messageType = (Log.Type)parameters[1];
                var exception = (Exception)parameters[2];
                int threadId = (int)parameters[3];

                if(this.MessageCanBeLogged(messageType))
                {
                    // Log to console and general file
                    this.LogToTrace(message, messageType, exception, threadId);
                }
            }
            catch(Exception ex)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    string message = string.Format("Exception logging: {0} \n {1}", ex.Message, ex.StackTrace);
                    Console.ResetColor();
                }
                catch
                {
                }
            }
        }
    }

    private void ChangeFileName()
    {
        var path = this.currentDateTime.Year.ToString() + Path.DirectorySeparatorChar + this.currentDateTime.Month.ToString() + Path.DirectorySeparatorChar + this.currentDateTime.Day.ToString() + Path.DirectorySeparatorChar;
        path = Path.Combine(this.path, path);

        if(!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        if(this.currentFile != null)
            this.currentFile.Close();

        this.currentFile = new System.IO.StreamWriter(path + this.name + ".txt", true);
        this.currentFile.AutoFlush = true;
        this.fileDateTime = DateTime.Now;
    }

    private void LoadConfiguration()
    {
        try
        {
            this.Level = Utils.Instance.ConvertStringToEnum<Log.LevelMessage>(ConfigurationManager.AppSettings.Get("LogLevel"));
        }
        catch
        {
            this.Level = Log.LevelMessage.Message;
        }

        try
        {
            this.path = ConfigurationManager.AppSettings.Get("LogPath");
        }
        catch
        {
            this.path = Utils.Instance.CurrentPath;
        }

        try
        {
            this.name = ConfigurationManager.AppSettings.Get("LogName");
        }
        catch
        {
            this.name = Utils.Instance.AppName;
        }
    }

    private void CheckDate()
    {
        if(DateTime.Now.ToShortDateString() != this.fileDateTime.ToShortDateString())
        {
            this.ChangeFileName();
        }
    }
    #endregion
    #endregion
}