using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;

using Keep.Models;
using Keep.Utils;

namespace Keep.Commands
{
    public class DeleteNoteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (parameter != null && parameter is Note);
        }

        public void Execute(object parameter)
        {
            Note note = parameter as Note;

            Notes notes = AppSettings.Instance.LoggedUser.Notes;
            note = notes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>();

            if (note != null) notes.Remove(note);
        }
    }
}
