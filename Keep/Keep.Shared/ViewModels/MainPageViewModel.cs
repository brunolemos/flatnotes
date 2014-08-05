using System;
using System.Collections.Generic;
using System.Text;

using Keep.Commands;
using Keep.Models;

namespace Keep.ViewModels
{
    public class MainPageViewModel
    {
        public SendFeedbackCommand SendFeedbackCommand { get { return sendFeedbackCommand; } }
        private SendFeedbackCommand sendFeedbackCommand = new SendFeedbackCommand();

        public Double CellWidth { get; set; }

        public Notes Notes { get { return notes; } }
        private Notes notes = new Notes() {
            new Note() { Color = NoteColor.BLUE, Title = "Big text", Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." },
            new Note() { Color = NoteColor.ORANGE, Title = "Title", Text = "Text" },
            new Note() { Color = NoteColor.GREEN, Title = "Title", Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." },
            new Note() { Color = NoteColor.TEAL, Title = "Title", Text = "Text" },
            new Note() { Color = NoteColor.GRAY, Title = "Title", Text = "Text" },
        };
    }
}
