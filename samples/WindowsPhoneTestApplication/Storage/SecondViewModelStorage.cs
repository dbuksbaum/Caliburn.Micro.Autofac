// Проект: WindowsPhoneTestApplication
// Имя файла: SecondViewModelStorage.cs
// GUID файла: 884E14D1-A606-4775-AD1F-A267CE9E8534
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 20.05.2012

using Caliburn.Micro;
using WindowsPhoneTestApplication.ViewModels;


namespace WindowsPhoneTestApplication.Storage
{
    /// <summary>
    /// 
    /// </summary>
    public class SecondViewModelStorage : StorageHandler<SecondViewModel>
    {
        /// <summary>
        /// Overrided by inheritors to configure the handler for use.
        /// </summary>
        public override void Configure()
        {
            Property(aModel => aModel.Comment).InAppSettings();
        }
    }
}