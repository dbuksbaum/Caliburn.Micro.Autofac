// Проект: WindowsPhoneTestApplication
// Имя файла: MainModelStorage.cs
// GUID файла: 989F1D31-46ED-4E08-854E-2BD04A1A4E34
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 20.05.2012

using Caliburn.Micro;
using WindowsPhoneTestApplication.Models;


namespace WindowsPhoneTestApplication.Storage
{
    /// <summary>
    /// 
    /// </summary>
    public class MainModelStorage : StorageHandler<MainModel>
    {
        #region Public methods

        /// <summary>
        /// Overrided by inheritors to configure the handler for use.
        /// </summary>
        public override void Configure()
        {
            EntireGraph<MainModel>().InPhoneState();
        }

        #endregion
    }
}