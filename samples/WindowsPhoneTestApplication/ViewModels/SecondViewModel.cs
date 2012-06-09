// Проект: WindowsPhoneTestApplication
// Имя файла: SecondViewModel.cs
// GUID файла: 809ACC8B-60DC-4A20-8C25-95A01973B359
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 20.05.2012

using Caliburn.Micro;


namespace WindowsPhoneTestApplication.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class SecondViewModel : Screen
    {
        #region Public properties

        public string Comment
        {
            get { return mComment; }
            set
            {
                if (mComment == value)
                {
                    return;
                }

                mComment = value;

                NotifyOfPropertyChange(() => Comment);
            }
        }

        #endregion

        #region Private data

        private string mComment;

        #endregion
    }
}