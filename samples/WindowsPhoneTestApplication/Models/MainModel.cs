// Проект: WindowsPhoneTestApplication
// Имя файла: MainModel.cs
// GUID файла: 741CECA5-D15E-4942-B569-10B6ADE71AA0
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 21.05.2012

using System.Runtime.Serialization;


namespace WindowsPhoneTestApplication.Models
{
    /// <summary>
    /// Main ViewModel model object.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: Model types that you plan to store using EntireGraph<T>().InPhoneState() must be
    /// SERIALIZABLE!!!
    /// </remarks>
    [DataContract]
    public class MainModel
    {
        #region Public properties

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns a <c>String</c> that represents the current object.
        /// </summary>
        /// <returns>
        /// A <c>String</c> that represents the current object.
        /// </returns>
        public override string ToString()
        {
            string lResult = string.Format(
                "MainModel. Name: {0}, Description: {1}", Name, Description);
            return lResult;
        }

        #endregion
    }
}