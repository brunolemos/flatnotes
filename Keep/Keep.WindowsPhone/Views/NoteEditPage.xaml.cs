using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Keep.Views
{
    public sealed partial class NoteEditPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public NoteEditViewModel viewModel { get { return (NoteEditViewModel)DataContext; } }
        private Brush previousBackground;

        private bool checklistChanged = false;

        public NoteEditPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //Color Picker
            ColorPickerAppBarToggleButton.Checked += (s, _e) => NoteColorPicker.Open();
            ColorPickerAppBarToggleButton.Unchecked += (s, _e) => NoteColorPicker.Close();
            NoteColorPicker.Opened += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = true; };
            NoteColorPicker.Closed += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = false; };
            NoteColorPicker.SelectionChanged += (s, _e) => { viewModel.Note.Color = _e.AddedItems[0] as NoteColor; };
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.ChangeStatusBarColor(Colors.Black);

            if(e.NavigationParameter is Note)
                viewModel.Note = e.NavigationParameter as Note;

            viewModel.Note.Changed = false;
            viewModel.Note.Checklist.CollectionChanged += Checklist_CollectionChanged;
            viewModel.Note.Checklist.CollectionItemChanged += Checklist_CollectionItemChanged;

            previousBackground = App.RootFrame.Background;
            App.RootFrame.Background = Background;
        }

        private async void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            App.RootFrame.Background = previousBackground;

            //deleted
            if (viewModel.Note == null) return;

            //remove change binding
            viewModel.Note.Checklist.CollectionChanged -= Checklist_CollectionChanged;
            viewModel.Note.Checklist.CollectionItemChanged -= Checklist_CollectionItemChanged;

            //trim
            viewModel.Note.Trim();

            //save
            if (viewModel.Note.Changed)
            await AppData.CreateOrUpdateNote(viewModel.Note);

            //checklist changed (fix cache problem with converter)
            if (checklistChanged) viewModel.Note.NotifyChanges();
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Checklist_CollectionItemChanged(object sender, EventArgs e)
        {
            checklistChanged = true;
            viewModel.Note.Touch();
        }

        private void Checklist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            checklistChanged = true;
            viewModel.Note.Touch();
        }

        private T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0) return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child != null && child is T)
                    return (T)child;

                else
                {
                    var result = FindFirstElementInVisualTree<T>(child);
                    if (result != null) return result;
                }
            }

            return null;
        }

        private void NoteChecklistListView_Holding(object sender, HoldingRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            (sender as ListView).ReorderMode = ListViewReorderMode.Enabled;
#endif
        }

        private void NoteTitleTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!viewModel.Note.IsChecklist)
                    NoteTextTextBox.Focus(FocusState.Programmatic);
                else
                {
                    int count = NoteChecklistListView.Items.Count;

                    if (count <= 0)
                        NewChecklistItemTextBox.Focus(FocusState.Programmatic);
                    else
                    {
                        FrameworkElement listViewItem = NoteChecklistListView.ContainerFromIndex(0) as FrameworkElement;
                        TextBox textBox = FindFirstElementInVisualTree<TextBox>(listViewItem);
                        if (textBox != null)
                        {
                            textBox.Select(textBox.Text.Length, 0);
                            textBox.Focus(FocusState.Programmatic);
                        }
                    }
                }
            }
        }

        private void NoteChecklistItemTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            ChecklistItem item = (sender as TextBox).DataContext as ChecklistItem;
            int position = (NoteChecklistListView.ItemsSource as Checklist).IndexOf(item);
            int count = NoteChecklistListView.Items.Count;

            if (String.IsNullOrEmpty((sender as TextBox).Text))
            {
                if (e.Key == Windows.System.VirtualKey.Back)
                {
                    if (count > 1)
                    {
                        int new_position = position > 0 ? position - 1 : position + 1;
                        FrameworkElement listViewItem = NoteChecklistListView.ContainerFromIndex(new_position) as FrameworkElement;
                        TextBox textBox = FindFirstElementInVisualTree<TextBox>(listViewItem);
                        if (textBox != null)
                        {
                            textBox.Select(textBox.Text.Length, 0);
                            textBox.Focus(FocusState.Programmatic);
                        }

                        (NoteChecklistListView.ItemsSource as Checklist).Remove(item);
                    }
                }
                //else if ( e.Key == System.Windows.Input.Key.Enter && position == count - 1 )
                //{
                //    ( NoteChecklistListView.ItemsSource as Checklist ).Remove( item );
                //    NoteNewItemText.Focus();
                //}
            }
            else
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    (NoteChecklistListView.ItemsSource as Checklist).Insert(position + 1, new ChecklistItem());
                    NoteChecklistListView.UpdateLayout();

                    FrameworkElement listViewItem = NoteChecklistListView.ContainerFromIndex(position) as FrameworkElement;
                    TextBox textBox = FindFirstElementInVisualTree<TextBox>(listViewItem);
                    if (textBox != null)
                    {
                        e.Handled = true;

                        string text1 = textBox.Text.Substring(0, textBox.SelectionStart);
                        string text2 = textBox.Text.Substring(textBox.SelectionStart, textBox.Text.Length - textBox.SelectionStart);

                        FrameworkElement listViewItem2 = NoteChecklistListView.ContainerFromIndex(position + 1) as FrameworkElement;
                        TextBox textBox2 = FindFirstElementInVisualTree<TextBox>(listViewItem2);
                        if (textBox2 != null)
                        {
                            textBox.Text = text1;
                            textBox2.Text = text2;

                            textBox2.Focus(FocusState.Programmatic);
                        }
                    }
                }
                else if (e.Key == Windows.System.VirtualKey.Back)
                {
                    if (position <= 0) return;

                    FrameworkElement listViewItem = NoteChecklistListView.ContainerFromIndex(position) as FrameworkElement;
                    TextBox textBox = FindFirstElementInVisualTree<TextBox>(listViewItem);
                    if (textBox != null)
                    {
                        if (textBox.SelectionStart > 0) return;

                        FrameworkElement listViewItem2 = NoteChecklistListView.ContainerFromIndex(position - 1) as FrameworkElement;
                        TextBox textBox2 = FindFirstElementInVisualTree<TextBox>(listViewItem2);
                        if (textBox2 != null)
                        {
                            int pos = textBox2.Text.Length;

                            textBox2.Text += textBox.Text;
                            textBox2.Select(pos, 0);
                            textBox2.Focus(FocusState.Programmatic);

                            (NoteChecklistListView.ItemsSource as Checklist).Remove(item);
                        }
                    }
                }
            }
        }

        private void NewChecklistItemTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            string text = NewChecklistItemTextBox.Text;

            if (e.Key == Windows.System.VirtualKey.Enter && !String.IsNullOrEmpty(text))
            {
                viewModel.Note.Checklist.Add(new ChecklistItem(text));

                NewChecklistItemTextBox.Text = String.Empty;
            }
        }
    }
}
