using Windows.UI.Xaml.Controls;
using Windows.UI.Input;
using System;
using FlatNotes.Utils;
using FlatNotes.Models;
using System.Diagnostics;
using Windows.UI.Xaml;
using System.Collections.Generic;

namespace FlatNotes.Views
{
    public sealed partial class NoteEditPage : Page
    {
        Dictionary<FrameworkElement, ManipulationInputProcessor> inputProcessors = new Dictionary<FrameworkElement, ManipulationInputProcessor>();
        Dictionary<FrameworkElement, EventHandler> enableSwipeEventHandlers = new Dictionary<FrameworkElement, EventHandler>();
        Dictionary<FrameworkElement, EventHandler> disableSwipeEventHandlers = new Dictionary<FrameworkElement, EventHandler>();

        partial void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame)
        {
            GestureRecognizer gestureRecognizer = new GestureRecognizer();
            ManipulationInputProcessor elementInputProcessor = new ManipulationInputProcessor(gestureRecognizer, element, referenceFrame);

            //handlers
            elementInputProcessor.ItemSwiped += OnItemSwiped;

            //save
            inputProcessors[element] = elementInputProcessor;
        }

        partial void DisableSwipeFeature(FrameworkElement element)
        {
            if (!inputProcessors.ContainsKey(element)) return;

            //handlers
            inputProcessors[element].ItemSwiped -= OnItemSwiped;

            //disable
            inputProcessors[element].Disable();
            inputProcessors.Remove(element);
        }

        private void OnItemSwiped(object sender, EventArgs e)
        {
            ChecklistItem item = (sender as FrameworkElement).DataContext as ChecklistItem;
            if (item == null) return;

            Debug.WriteLine("Swiped checklist item ");
            viewModel.Note.Checklist.Remove(item);
        }
    }
}