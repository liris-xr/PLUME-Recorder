using System;
using UnityEngine;

namespace PLUME.Core.Recorder
{
    public class Logger
    {
        private static readonly Logger Instance = new();
        private readonly UnityEngine.Logger _logger;

        private Logger()
        {
            _logger = new UnityEngine.Logger(new LogHandler());
        }

        public static void Log(LogType logType, object message)
        {
            Instance._logger.Log(logType, message);
        }

        public static void Log(LogType logType, object message, UnityEngine.Object context)
        {
            Instance._logger.Log(logType, message, context);
        }

        public static void Log(LogType logType, string tag, object message)
        {
            Instance._logger.Log(logType, tag, message);
        }

        public static void Log(LogType logType, string tag, object message, UnityEngine.Object context)
        {
            Instance._logger.Log(logType, tag, message, context);
        }

        public static void Log(object message)
        {
            Instance._logger.Log(message);
        }

        public static void Log(string tag, object message)
        {
            Instance._logger.Log(tag, message);
        }

        public static void Log(string tag, object message, UnityEngine.Object context)
        {
            Instance._logger.Log(tag, message, context);
        }
        
        public static void LogWarning(object message)
        {
            Instance._logger.LogWarning("", message);
        }
        
        public static void LogWarning(string tag, object message)
        {
            Instance._logger.LogWarning(tag, message);
        }

        public static void LogWarning(string tag, object message, UnityEngine.Object context)
        {
            Instance._logger.LogWarning(tag, message, context);
        }

        public static void LogError(string tag, object message)
        {
            Instance._logger.LogError(tag, message);
        }
        
        public static void LogError(string tag, object message, UnityEngine.Object context)
        {
            Instance._logger.LogError(tag, message, context);
        }

        public static void LogFormat(LogType logType, string format, params object[] args)
        {
            Instance._logger.LogFormat(logType, format, args);
        }

        public static void LogException(Exception exception)
        {
            Instance._logger.LogException(exception);
        }

        private class LogHandler : ILogHandler
        {
            private const string Prefix = "<color=#3498db>[PLUME]</color> ";
            
            public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                Debug.unityLogger.logHandler.LogFormat(logType, context, Prefix + format, args);
            }

            public void LogException(Exception exception, UnityEngine.Object context)
            {
                Debug.unityLogger.logHandler.LogException(exception, context);
            }
        }
    }
}