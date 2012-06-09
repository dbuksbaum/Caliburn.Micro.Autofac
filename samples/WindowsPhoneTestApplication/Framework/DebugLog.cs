// Проект: WindowsPhoneTestApplication
// Имя файла: DebugLog.cs
// GUID файла: F9295F6E-BCDF-4987-A74A-50F5B258E069
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 20.05.2012

using System;
using System.Diagnostics;
using Caliburn.Micro;


namespace WindowsPhoneTestApplication.Framework
{
    /// <summary>
    /// 
    /// </summary>
    public class DebugLog : ILog
    {
        /// <summary>
        /// Logs the message as info.
        /// </summary>
        /// <param name="format">A formatted message.</param><param name="args">Parameters to be injected into the formatted message.</param>
        public void Info(string format, params object[] args)
        {
            Debug.WriteLine("INFO:" + string.Format(format, args));
        }

        /// <summary>
        /// Logs the message as a warning.
        /// </summary>
        /// <param name="format">A formatted message.</param><param name="args">Parameters to be injected into the formatted message.</param>
        public void Warn(string format, params object[] args)
        {
            Debug.WriteLine("WARN: " + string.Format(format, args));
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Error(Exception exception)
        {
            Debug.WriteLine("ERROR: " + exception.Message);
        }
    }
}