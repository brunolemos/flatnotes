using System;
using System.Collections.Generic;
using System.Text;

using Keep.Commands;
using Keep.Models;

namespace Keep.ViewModels
{
    public class MainPageViewModel
    {

        private OpenNoteCommand openNoteCommand = new OpenNoteCommand();
        public OpenNoteCommand OpenNoteCommand { get { return openNoteCommand; } }

        public Notes Notes { get { return notes; } }
        private Notes notes = new Notes() {
            new Note() { Color = NoteColor.BLUE, Title = "Title1", Text = "Hi" },
            new Note() { Color = NoteColor.GREEN, Title = "Title2", Text = "Olar" }
        };
    }
}
