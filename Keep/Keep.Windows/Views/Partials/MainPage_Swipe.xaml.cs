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
        static Dictionary<FrameworkElement, ManipulationInputProcessor> inputProcessors = new Dictionary<FrameworkElement, ManipulationInputProcessor>();
        static Dictionary<FrameworkElement, EventHandler> enableSwipeEventHandlers = new Dictionary<FrameworkElement, EventHandler>();
        static Dictionary<FrameworkElement, EventHandler> disableSwipeEventHandlers = new Dictionary<FrameworkElement, EventHandler>();

        partial void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame)
        {
            try
            {
                GestureRecognizer gestureRecognizer = new GestureRecognizer();
                ManipulationInputProcessor elementInputProcessor = new ManipulationInputProcessor(gestureRecognizer, element, referenceFrame);

                //handlers
                elementInputProcessor.ItemSwiped += OnItemSwiped;

                //save
                inputProcessors[element] = elementInputProcessor;
            }
            catch (Exception e)
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendException(String.Format("Failed to Enable Swipe Feature: {0} (Stack trace: {1})", e.Message, e.StackTrace), false);
            }
        }

        partial void DisableSwipeFeature(FrameworkElement element)
        {
            if (element == null || !inputProcessors.ContainsKey(element)) return;
            //Debug.WriteLine("DisableSwipeFeature");

            try
            {
                //handlers
                inputProcessors[element].ItemSwiped -= OnItemSwiped;
                enableSwipeEventHandlers.Remove(element);
                disableSwipeEventHandlers.Remove(element);

                //disable
                inputProcessors[element].Disable();
                inputProcessors.Remove(element);
            }
            catch (Exception e) {
                GoogleAnalytics.EasyTracker.GetTracker().SendException(String.Format("Failed to Disable Swipe Feature: {0} (Stack trace: {1})", e.Message, e.StackTrace), false);
            }
        }

        private async void OnItemSwiped(object sender, EventArgs e)
        {
            Note note = (sender as FrameworkElement).DataContext as Note;
            if (note == null) return;

            Debug.WriteLine("Swiped note " + note.Title);
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "swipe", "note", 0);

            await AppData.ArchiveNote(note);
        }
    }
}