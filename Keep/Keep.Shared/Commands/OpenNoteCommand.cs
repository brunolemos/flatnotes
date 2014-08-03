using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Popups;

using Keep.Models;

namespace Keep.Commands
{
    public class OpenNoteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (parameter is Note);
        }

        public async void Execute(object parameter)
        {
            Note note = parameter as Note;

            var dialog = new MessageDialog(note.Title);
            await dialog.ShowAsync();
        }
    }
}
