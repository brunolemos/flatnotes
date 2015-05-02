using FlatNotes.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace FlatNotes.Views
{
    public sealed partial class NoteEditPage : Page
    {
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
                    CheckBox checkBox = FindFirstElementInVisualTree<CheckBox>(listViewItem);
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

                        FrameworkElement checkBoxItem2 = NoteChecklistListView.ContainerFromIndex(position + 1) as FrameworkElement;
                        CheckBox checkBox2 = FindFirstElementInVisualTree<CheckBox>(listViewItem2);
                        if(checkBox2 != null && String.IsNullOrEmpty(text1))
                        {
                            checkBox2.IsChecked = checkBox.IsChecked;
                            checkBox.IsChecked = false;
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
                        System.Diagnostics.Debug.WriteLine("textBox.SelectionStart " + textBox.SelectionStart);
                        if (textBox.SelectionStart > 0) return;

                        FrameworkElement listViewItem2 = NoteChecklistListView.ContainerFromIndex(position - 1) as FrameworkElement;
                        TextBox textBox2 = FindFirstElementInVisualTree<TextBox>(listViewItem2);
                        if (textBox2 != null)
                        {
                            if(String.IsNullOrEmpty(textBox2.Text))
                            {
                                CheckBox checkBox2 = FindFirstElementInVisualTree<CheckBox>(listViewItem2);
                                checkBox2.IsChecked = item.IsChecked;
                            }

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
