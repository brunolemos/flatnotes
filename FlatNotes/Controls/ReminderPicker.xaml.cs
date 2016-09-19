using FlatNotes.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Controls
{
    public sealed partial class ReminderPicker : UserControl
    {
        public static readonly DependencyProperty ReminderProperty = DependencyProperty.Register("Reminder", typeof(Reminder), typeof(ReminderPicker), new PropertyMetadata(new Reminder(), OnReminderPropertyChanged));

        public Reminder Reminder { get { return (Reminder)GetValue(ReminderProperty); } set { SetValue(ReminderProperty, value != null ? value : new Reminder()); } }

        public Reminder TemporaryReminder { get; set; }

        public event EventHandler Saved;
        public event EventHandler Canceled;

        public ReminderPicker()
        {
            this.InitializeComponent();
            Loaded += (s, e) => updateValues();
            Unloaded += (s, e) => { Reminder = new Reminder(); TemporaryReminder = null; };
        }

        private static void OnReminderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reminderPicker = d as ReminderPicker;
            if (reminderPicker == null) return;

            reminderPicker.updateValues();
        }

        private void updateValues()
        {
            //if (Reminder == null)  Reminder = new Reminder();

            if (Reminder.Date.HasValue && Reminder.Date.Value != null)
                TemporaryReminder = new Reminder(Reminder.Date.Value);
            else
                TemporaryReminder = new Reminder(DateTimeOffset.Now.AddMinutes(1));

            calendarDatePicker.MinDate = DateTimeOffset.Now;

            calendarDatePicker.Date = TemporaryReminder.Date.Value;
            //datePicker.Date = TemporaryReminder.Date.Value;
            timePicker.Time = TemporaryReminder.Date.Value.TimeOfDay;
        }

        private bool updateCanSave()
        {
            AcceptButton.IsEnabled = TemporaryReminder.Date.HasValue && TemporaryReminder.Date.Value > DateTimeOffset.Now;
            return AcceptButton.IsEnabled;
        }

        private void calendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            TemporaryReminder.Date = args.NewDate?.Date + timePicker.Time;
            updateCanSave();
        }

        //private void datePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        //{
        //    TemporaryReminder.Date = e.NewDate.Date + timePicker.Time;
        //    updateCanSave();
        //}

        private void timePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            TemporaryReminder.Date = calendarDatePicker.Date?.Date + e.NewTime;
            updateCanSave();
        }

        //private void TomorrowButton_Click(object sender, RoutedEventArgs e)
        //{
        //    calendarDatePicker.Date = DateTimeOffset.Now.AddDays(1).Date;
        //    timePicker.Time = new TimeSpan(8, 0, 0);
        //}

        //private void NextWeekButton_Click(object sender, RoutedEventArgs e)
        //{
        //    calendarDatePicker.Date = DateTimeOffset.Now.AddDays(7).Date;
        //    timePicker.Time = new TimeSpan(8, 0, 0);
        //}

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!updateCanSave()) return;

            Reminder.Date = TemporaryReminder.Date;
            Saved?.Invoke(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }
    }
}
