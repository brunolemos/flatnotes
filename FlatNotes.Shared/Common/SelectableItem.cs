using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlatNotes.Common
{
    public class SelectableItem<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T Item { get { return _item; } set { _item = value; RaisePropertyChanged(); } }
        private T _item;

        public bool IsSelected { get { return _isSelected; } set { if (_isSelected == value) return; _isSelected = value; RaisePropertyChanged(); } }
        private bool _isSelected;

        public SelectableItem(T item)
        {
            this.Item = item;
        }

        protected void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
