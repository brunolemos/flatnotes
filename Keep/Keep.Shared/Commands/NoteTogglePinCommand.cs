using System;
using System.Collections.Generic;
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
    public class NoteTogglePinCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (parameter != null && parameter is Note);
        }

        public async void Execute(object parameter)
        {
            Note note = parameter as Note;
            note.IsPinned = SecondaryTile.Exists(note.ID);

            if (note.IsPinned)
            {
                SecondaryTile tile = new SecondaryTile(note.ID);
                await tile.RequestDeleteAsync();
                note.IsPinned = false;
            }
            else
            {
                string title = (!String.IsNullOrEmpty(note.Title) ? note.Title : "#Keep");

                SecondaryTile tile = new SecondaryTile(note.ID, title, note.ID, new Uri("ms-appx:///Assets/Logo.png"), TileSize.Square150x150);
                tile.VisualElements.BackgroundColor = Colors.Transparent; //new Color().FromHex(note.Color.Color);
                tile.VisualElements.ForegroundText = ForegroundText.Dark;

                await tile.RequestCreateAsync();
                note.IsPinned = true;
            }
        }
    }
}
