// Проект: WindowsPhoneTestApplication
// Имя файла: MainViewModel.cs
// GUID файла: 3DB0DB4D-49F2-4235-8FEA-6867B11C7800
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 14.05.2012

using Caliburn.Micro;
using WindowsPhoneTestApplication.Models;


namespace WindowsPhoneTestApplication.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : Screen
    {
        #region Constructors

        public MainViewModel(MainModel aModel, INavigationService aNavigationService)
        {
            Model = aModel;
            NavigationService = aNavigationService;
        }

        #endregion

        #region Public properties

        public string Name
        {
            get { return Model.Name; }
            set
            {
                if (Model.Name == value)
                {
                    return;
                }

                Model.Name = value;

                NotifyOfPropertyChange(() => Name);
            }
        }

        public string Description
        {
            get { return Model.Description; }
            set
            {
                if (Model.Description == value)
                {
                    return;
                }

                Model.Description = value;

                NotifyOfPropertyChange(() => Description);
            }
        }

        #endregion

        #region Public methods

        public void GotoPageTwo()
        {
            NavigationService.UriFor<SecondViewModel>().Navigate();
        }


        #endregion

        #region Private properties

        private MainModel Model { get; set; }
        private INavigationService NavigationService { get; set; }

        #endregion
    }
}