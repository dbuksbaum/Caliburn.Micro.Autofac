// Проект: WindowsPhoneTestApplication
// Имя файла: App.xaml.cs
// GUID файла: A257A557-D066-4B6F-8336-A2DF7272E408
// Автор: Mike Eshva
// Дата создания: 14.05.2012

using System.Diagnostics;
using System.Windows;


namespace WindowsPhoneTestApplication
{
    public partial class App
    {
        #region Constructors

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += OnUnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();
        }

        #endregion

        // Code to execute on Unhandled Exceptions

        #region Private methods

        private void OnUnhandledException
            (object aSender, ApplicationUnhandledExceptionEventArgs aEventArgs)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #endregion
    }
}