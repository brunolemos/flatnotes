using Windows.UI.Xaml.Controls;
using Keep.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Keep.Controls
{
    public sealed partial class NotePreview : UserControl
    {
        public ChecklistMaxItemsConverter ChecklistMaxItemsConverter { get { return checklistMaxItemsConverter; } }
        private ChecklistMaxItemsConverter checklistMaxItemsConverter = new ChecklistMaxItemsConverter();

        private int maxItems = 6;

        public NotePreview()
        {
            this.InitializeComponent();

            ChangeNoteChecklisConverter();
        }

        private void ChangeNoteChecklisConverter()
        {
            NoteChecklistListView.SetBinding(ListView.ItemsSourceProperty, new Binding()
            {
                Path = new PropertyPath("Checklist"),
                Converter = ChecklistMaxItemsConverter,
                ConverterParameter = maxItems,
                Mode = BindingMode.OneWay
            });
        }
    }
}
