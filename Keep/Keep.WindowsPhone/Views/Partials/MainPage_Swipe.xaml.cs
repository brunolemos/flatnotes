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
        Dictionary<FrameworkElement, ManipulationInputProcessor> elementInputProcessors = new Dictionary<FrameworkElement, ManipulationInputProcessor>();

        partial void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame)
        {
            GestureRecognizer gestureRecognizer = new GestureRecognizer();
            ManipulationInputProcessor elementInputProcessor = new ManipulationInputProcessor(gestureRecognizer, element, referenceFrame);

            elementInputProcessor.ItemSwiped += OnItemSwiped;

            elementInputProcessors[element] = elementInputProcessor;
        }

        partial void DisableSwipeFeature(FrameworkElement element)
        {
            if (!elementInputProcessors.ContainsKey(element)) return;

            elementInputProcessors[element].ItemSwiped -= OnItemSwiped;

            elementInputProcessors[element].Disable();
            elementInputProcessors.Remove(element);
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