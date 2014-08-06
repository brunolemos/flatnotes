using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using Keep.Utils;

namespace Keep.Models
{
    public class Notes : TrulyObservableCollection<Note> { }
}
