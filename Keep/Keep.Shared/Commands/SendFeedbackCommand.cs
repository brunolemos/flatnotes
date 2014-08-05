using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

using Keep.Models;

namespace Keep.Commands
{
    public class SendFeedbackCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
#if WINDOWS_PHONE_APP
            return true;
#else
            return false;
#endif
        }

        public async void Execute(object parameter)
        {
#if WINDOWS_PHONE_APP
            Windows.ApplicationModel.Email.EmailMessage mail = new Windows.ApplicationModel.Email.EmailMessage();

            //mail.Attachments.Add(new Windows.ApplicationModel.Email.EmailAttachment());
            mail.To.Add(new Windows.ApplicationModel.Email.EmailRecipient("keep@brunolemos.org"));
            mail.Subject = "Feedback - #Keep Beta";
            mail.Body = "";
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(mail);
#endif
        }
    }
}
