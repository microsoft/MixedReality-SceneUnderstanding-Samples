// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Helper methods to log messages to the Unity log. Prefixes the messages with date and time and also allows toggling of logging at the global level.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Writes messages to Unity logs, if set to true.
        /// </summary>    
        public static bool WriteLogs = true;

        private enum LogType
        {
            Info,
            Warning,
            Error
        }
        
        /// <summary>
        /// Writes a message to the Unity log.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Log(string message)
        {
            LogInternal(message, LogType.Info);
        }

        /// <summary>
        /// Writes a warning message to the Unity log.
        /// </summary>
        /// <param name="message">The warning message to write.</param>
        public static void LogWarning(string message)
        {
            LogInternal(message, LogType.Warning);
        }
        
        /// <summary>
        /// Writes an error message to the Unity log.
        /// </summary>
        /// <param name="message">The error message to write.</param>
        public static void LogError(string message)
        {
            LogInternal(message, LogType.Error);
        }

        /// <summary>
        /// Writes all exception related information to the Unity log.
        /// </summary>
        /// <param name="e">Exception to log.</param>
        public static void LogException(Exception e)
        {
            Logger.LogWarning("Exception encountered.");
            Logger.LogError(e.ToString());
            Logger.LogWarning("HResult: " + e.HResult.ToString());
            Logger.LogWarning("Message: " + e.Message);
            Logger.LogWarning("Stack: " + e.StackTrace);
            Logger.LogWarning("InnerException: " + (e.InnerException == null ? "null" : e.InnerException.ToString()));
        }

        /// <summary>
        /// Internal method used for logging messages to the Unity log.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="logType">Type of message, i.e. Info, Warning, etc.</param>
        private static void LogInternal(string message, LogType logType)
        {
            if (Logger.WriteLogs)
            {
                string toWrite = string.Format("{0} - {1}", DateTime.Now.ToString(), message);

                switch (logType)
                {
                    case LogType.Error:
                        Debug.LogError(toWrite);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(toWrite);
                        break;
                    case LogType.Info:
                    default:
                        Debug.Log(toWrite);
                        break;
                }
            }
        }
    }
}