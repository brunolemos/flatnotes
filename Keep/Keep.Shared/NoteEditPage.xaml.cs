using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Animation;

namespace Keep
{
    public sealed partial class NoteEditPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        NoteEditViewModel viewModel;

        public SolidColorBrush AnimatedColor { get { return (SolidColorBrush)GetValue(AnimatedColorProperty); } protected set { SetValue(AnimatedColorProperty, value); } }
        public readonly DependencyProperty AnimatedColorProperty = DependencyProperty.Register("AnimatedColor", typeof(SolidColorBrush), typeof(Page), null);

        public NoteEditPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Colors.Black;
                statusBar.BackgroundOpacity = 0.20;
                statusBar.ForegroundColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFE);
            }
#endif

            viewModel = new NoteEditViewModel();
            Note note = null;
            bool isAlreadyAdded = false;

            if (e.NavigationParameter != null && e.NavigationParameter is Note)
            {
                Notes notes = AppSettings.Instance.LoggedUser.Notes;
                note = notes.Where<Note>(x => x.ID == ((Note)e.NavigationParameter).ID).FirstOrDefault<Note>();
                isAlreadyAdded = (note != null);
                if(note == null) note = e.NavigationParameter as Note;
            }
            else if (e.NavigationParameter != null && e.NavigationParameter is string)
            {
                Notes notes = AppSettings.Instance.LoggedUser.Notes;
                note = notes.Where<Note>(x => x.ID == e.NavigationParameter.ToString()).FirstOrDefault<Note>();
                isAlreadyAdded = (note != null);
            }

            if (note == null)
                note = new Note();

            //if (!isAlreadyAdded)
            //    AppSettings.Instance.LoggedUser.Notes.Insert(0, note);

            viewModel.Note = note;
            this.DataContext = viewModel;

            //AnimatedColor = new SolidColorBrush(new Color().FromHex(viewModel.Note.Color.Color));
            //this.SetBinding(BackgroundProperty, new Binding() { Path = new PropertyPath("AnimatedColor"), Source = this });
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 0;
                statusBar.ForegroundColor = null;
            }
#endif

            if (viewModel.Note == null) return;
            viewModel.Note.Trim();
            
            //update binding (fix problem when creating a note and press back button while textbox is focused)
            ForceTextBoxBindingUpdate();

            Notes notes = AppSettings.Instance.LoggedUser.Notes;
            Note note = notes.Where<Note>(x => x.ID == viewModel.Note.ID).FirstOrDefault<Note>();

            if (note == null)
            {
                if (!(viewModel.Note == null || viewModel.Note.IsEmpty()))
                    AppSettings.Instance.LoggedUser.Notes.Insert(0, viewModel.Note);
            }
            else
            {
                if ((viewModel.Note == null || viewModel.Note.IsEmpty()))
                    if (viewModel.DeleteNoteCommand.CanExecute(viewModel.Note))
                        viewModel.DeleteNoteCommand.Execute(viewModel.Note);
            }
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



        public void ForceTextBoxBindingUpdate()
        {
            //force binding update (fix problem when editing textbox before click appbar)
            NoteTitleTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            NoteTextTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        public void AnimateColorTo(Color color)
        {
            //NoteColorAnimation.To = color;
            //NoteColorAnimationStoryboard.Begin();
        }
        
        private void DeleteNoteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.DeleteNoteCommand.CanExecute(viewModel.Note))
            {
                viewModel.DeleteNoteCommand.Execute(viewModel.Note);
                viewModel.Note = null;

                navigationHelper.GoBack();
            }
        }

        private void ColorPickerAppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(LayoutRoot);
        }

        private void ColorPickerAppBarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            FlyoutBase.GetAttachedFlyout(LayoutRoot).Hide();
        }

        private void ColorPickerFlyout_Opened(object sender, object e)
        {
            ColorPickerAppBarToggleButton.IsChecked = true;
        }

        private void ColorPickerFlyout_Closed(object sender, object e)
        {
            ColorPickerAppBarToggleButton.IsChecked = false;
        }

        private void NoteColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 0)
                return;
            
            NoteColor newColor = e.AddedItems[0] as NoteColor;
            viewModel.Note.Color = newColor;

            AnimateColorTo(new Color().FromHex(newColor.Color));
        }

        //private void NoteChecklistListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        //{
        //    Debug.WriteLine("NoteChecklistListView_DataContextChanged");
        //    if (!(args.NewValue is Checklist))
        //        return;

        //    Checklist checklist = args.NewValue as Checklist;
        //    if(checklist.Count <= 0 ||  !String.IsNullOrEmpty(checklist[checklist.Count-1].Text))
        //        checklist.Add(new ChecklistItem());
        //}

        private void NoteChecklistItemTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            ChecklistItem item = (sender as TextBox).DataContext as ChecklistItem;
            int position = (NoteChecklistListView.ItemsSource as Checklist).IndexOf(item);
            int count = NoteChecklistListView.Items.Count;

            if ((sender as TextBox).Text == "")
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
                            textBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
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

                            textBox2.Focus(Windows.UI.Xaml.FocusState.Programmatic);
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
                            textBox2.Focus(Windows.UI.Xaml.FocusState.Programmatic);

                            (NoteChecklistListView.ItemsSource as Checklist).Remove(item);
                        }
                    }
                }
            }
        }
        private T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var result = FindFirstElementInVisualTree<T>(child);
                    if (result != null)
                        return result;

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

        private void NewChecklistItemTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            bool isChecked = NewChecklistItemCheckbox.IsChecked == true;
            string text = NewChecklistItemTextBox.Text;


            if (e.Key == Windows.System.VirtualKey.Enter && !String.IsNullOrEmpty(text))
            {
                viewModel.Note.Checklist.Add(new ChecklistItem(text, isChecked));

                NewChecklistItemCheckbox.IsChecked = false;
                NewChecklistItemTextBox.Text = String.Empty;
            }
        }
    }
}
