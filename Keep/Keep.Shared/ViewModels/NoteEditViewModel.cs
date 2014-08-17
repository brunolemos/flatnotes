using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.StartScreen;

using Keep.Commands;
using Keep.Models;
using Keep.Utils;

namespace Keep.ViewModels
{
    public class NoteEditViewModel
    {
        public bool Changed { get { return changed; } set { changed = value; } }
        private bool changed = false;

        public NoteToggleTypeCommand NoteToggleTypeCommand { get { return noteToggleTypeCommand; } }
        private NoteToggleTypeCommand noteToggleTypeCommand = new NoteToggleTypeCommand();

        public DeleteNoteCommand DeleteNoteCommand { get { return deleteNoteCommand; } }
        private DeleteNoteCommand deleteNoteCommand = new DeleteNoteCommand();

        public NoteTogglePinCommand NoteTogglePinCommand { get { return noteTogglePinCommand; } }
        private NoteTogglePinCommand noteTogglePinCommand = new NoteTogglePinCommand();

        public Note Note { get { return note; } set { note = value == null ? new Note() : value; } }
        private Note note = new Note();
    }
}
