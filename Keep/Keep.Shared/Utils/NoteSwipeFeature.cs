using Windows.UI.Input;
using System;
using Keep.Utils;
using Keep.Models;
using System.Diagnostics;
using Windows.UI.Xaml;
using System.Collections.Generic;
using System.Linq;

namespace Keep.Views
{
    public class NoteSwipeFeature
    {
        public Dictionary<FrameworkElement, ManipulationInputProcessor> inputProcessors = new Dictionary<FrameworkElement, ManipulationInputProcessor>();
        public Dictionary<FrameworkElement, EventHandler> enableSwipeEventHandlers = new Dictionary<FrameworkElement, EventHandler>();
        public Dictionary<FrameworkElement, EventHandler> disableSwipeEventHandlers = new Dictionary<FrameworkElement, EventHandler>();

        public void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame)
        {
            try
            {
                //already enabled
                if (inputProcessors.ContainsKey(element)) return;

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

        public void DisableSwipeFeature(FrameworkElement element)
        {
            if (element == null || !inputProcessors.ContainsKey(element)) return;
            //Debug.WriteLine("DisableSwipeFeature");

            try
            {
                //handlers
                inputProcessors[element].ItemSwiped -= OnItemSwiped;

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

            bool isArchived = AppData.ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;

            if (isArchived)
            {
                Debug.WriteLine("Swiped archived note " + note.Title);
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "swipe", "archived_note", 0);

                await AppData.RemoveArchivedNote(note);
            }
            else
            {
                Debug.WriteLine("Swiped note " + note.Title);
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "swipe", "note", 0);

                await AppData.ArchiveNote(note);
            }
        }
    }
}