using FlatNotes.Common;
using System;

namespace FlatNotes.ViewModels
{
    public abstract class ModalViewModelBase : ViewModelBase
    {
        public event EventHandler CloseModal;
        public RelayCommand DispatchCloseModalEventCommand { get; private set; }

        public ModalViewModelBase()
        {
            DispatchCloseModalEventCommand = new RelayCommand(DispatchCloseModalEvent);
        }
        
        public void DispatchCloseModalEvent()
        {
            var handler = CloseModal;
            if (handler != null) handler(null, EventArgs.Empty);
        }
    }
}