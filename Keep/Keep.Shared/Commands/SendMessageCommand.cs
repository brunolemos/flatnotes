using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Popups;

namespace Keep.Commands
{
    public class SendMessageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return !( parameter == null || String.IsNullOrEmpty( parameter.ToString() ) );
        }

        public async void Execute(object parameter)
        {
            var dialog = new MessageDialog(parameter.ToString());
            await dialog.ShowAsync();
        }
    }
}
