using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ContactManager
{
    public class Logger
    {
        private static readonly ILog log4net = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // [DebugError methods:]
        // ['Debug.WriteLine' calls were replaced with 'Logger.Error' calls]

        public static void DebugError(string message)
        {
            log4net.Error(message);
            Debug.WriteLine(message);
        }

        public static void DebugError(string message, Exception ex)
        {
            log4net.Error(message, ex);
            string logText = string.Format("{0}: {1}\n {2}", message, ex.Message, ex.StackTrace);
            DebugError(logText);
        }

        public static void DebugError(Exception ex)
        {
            string logText = string.Format("{0}\n {1}", ex.Message, ex.StackTrace);
            DebugError(logText);
        }

        // [TraceError methods:]
        // ['Trace.WriteLine' calls were replaced with 'Logger.TraceError' calls]

        public static void TraceError(string message)
        {
            log4net.Error(message);
            Trace.WriteLine(message);
        }

        public static void TraceError(string message, Exception ex)
        {
            string logText = string.Format("{0}: {1}\n {2}", message, ex.Message, ex.StackTrace);
            TraceError(logText);
        }

        public static void TraceError(Exception ex)
        {
            string logText = string.Format("{0}\n {1}", ex.Message, ex.StackTrace);
            TraceError(logText);
        }

        // [Info methods:]
        // ['Debug.WriteLine' calls in non-error situations were replaced with 'Logger.Info' methods]

        public static void Info(string message)
        {
            log4net.Info(message);
            Info(false, message);
        }

        public static void Error(string message, Exception e = null)
        {
            if (e != null)
                log4net.Error(message, e);
            else
                log4net.Error(message);
        }

        public static void Info(bool forceLogToFile, string message)
        {
#if DEBUG
            log4net.Info(message);
#else
        if (forceLogToFile)
        {
            log4net.Info(message);
        }
#endif
            Debug.WriteLine(message);
        }
    }
}