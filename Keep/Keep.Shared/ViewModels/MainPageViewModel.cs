using System;
using System.Collections.Generic;
using System.Text;

using Keep.Models;

namespace Keep.ViewModels
{
    public class MainPageViewModel
    {
        public Notes Notes = new Notes() {
            new Note() { Color = NoteColor.BLUE, Title = "Title", Text = "Hi" },
            new Note() { Color = NoteColor.GREEN, Title = "Title", Text = "Olar" }
        };
    }
}
