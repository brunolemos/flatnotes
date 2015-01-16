using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Keep.Common
{
    public class TrulyObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public event EventHandler CollectionItemChanged;

        public bool UpdateLayout { get { return updateLayout; } set { updateLayout = value; } }
        private bool updateLayout = true;

        public TrulyObservableCollection()
        {
            CollectionChanged += OnCollectionChanged;
        }

        public TrulyObservableCollection(bool updateLayout) : this()
        {
            UpdateLayout = updateLayout;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Object item in e.NewItems)
                    (item as INotifyPropertyChanged).PropertyChanged += (s, _e) => OnItemPropertyChanged(item, _e);

            if (e.OldItems != null)
                foreach (Object item in e.OldItems)
                    (item as INotifyPropertyChanged).PropertyChanged -= (s, _e) => OnItemPropertyChanged(item, _e);
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("OnItemPropertyChanged " + e.PropertyName);
            CollectionItemChanged(sender, e);

            if(UpdateLayout)
            {
                int index = IndexOf((T)sender);
                this[index] = (T)sender;

                //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

        }
    }
}