using Windows.UI.Input;
using System;
using FlatNotes.Utils;
using FlatNotes.Models;
using System.Diagnostics;
using Windows.UI.Xaml;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlatNotes.Views
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
                var exceptionProperties = new Dictionary<string, string>() { { "Details", "Failed to Enable Swipe Feature" } };
                var exceptionMetrics = new Dictionary<string, double>() { { "Input processors count", inputProcessors.Count } };
                App.TelemetryClient.TrackException(e, exceptionProperties, exceptionMetrics);
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
                App.TelemetryClient.TrackException(e, new Dictionary<string, string>() { { "Details", "Failed to Disable Swipe Feature" } });
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
                App.TelemetryClient.TrackEvent("ArchivedNoteSwiped");

                await AppData.RemoveArchivedNote(note);
            }
            else
            {
                Debug.WriteLine("Swiped note " + note.Title);
                App.TelemetryClient.TrackEvent("NoteSwiped");

                await AppData.ArchiveNote(note);
            }
        }
    }
}