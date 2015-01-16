using Windows.UI.Xaml.Controls;
using Windows.UI.Input;
using System;
using Keep.Utils;
using Keep.Models;
using System.Diagnostics;
using Windows.UI.Xaml;
using System.Collections.Generic;

namespace Keep.Views
{
    public sealed partial class MainPage : Page
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
            Debug.WriteLine("DisableSwipeFeature");

            //handlers
            inputProcessors[element].ItemSwiped -= OnItemSwiped;

            //disable
            inputProcessors[element].Disable();
            inputProcessors.Remove(element);
        }

        private async void OnItemSwiped(object sender, EventArgs e)
        {
            Note note = (sender as FrameworkElement).DataContext as Note;
            if (note == null) return;

            Debug.WriteLine("Swiped note " + note.Title);
            await AppData.ArchiveNote(note);
        }
    }
}