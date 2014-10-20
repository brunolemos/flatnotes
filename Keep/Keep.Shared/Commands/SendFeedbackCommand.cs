using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Keep.Models;
using Keep.Utils;
using System.Diagnostics;
using Windows.ApplicationModel;

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
            Debug.WriteLine(parameter.ToString());
            Windows.ApplicationModel.Email.EmailMessage mail = new Windows.ApplicationModel.Email.EmailMessage();

            //if (parameter is FrameworkElement)
            //{
            //    RenderTargetBitmap renderTargetBitmap = await (parameter as FrameworkElement).ToBitmap();
            //    IBuffer pixels = await renderTargetBitmap.GetPixelsAsync();

            //    InMemoryRandomAccessStream msras = new InMemoryRandomAccessStream();
            //    await msras.WriteAsync(pixels);

            //    mail.Attachments.Add(new Windows.ApplicationModel.Email.EmailAttachment("print.jpg", RandomAccessStreamReference.CreateFromStream(msras)));
            //    Debug.WriteLine("Added attached");
            //}

            mail.To.Add(new Windows.ApplicationModel.Email.EmailRecipient("keep@brunolemos.org"));
            mail.Subject = String.Format("Feedback - Keep v{0}.{1}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor);
            mail.Body = "";
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(mail);
#endif
        }
    }
}
