using Keep.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Keep.Controls
{
    public sealed partial class NotePreview : UserControl
    {
        public static readonly DependencyProperty HideImageProperty = DependencyProperty.Register("HideImage", typeof(bool), typeof(NotePreview), new PropertyMetadata(false));
        public bool HideImage { get { return (bool)GetValue(HideImageProperty); } set { SetValue(HideImageProperty, value); } }

        public static readonly DependencyProperty MaxChecklistItemsProperty = DependencyProperty.Register( "MaxChecklistItems", typeof(int), typeof(NotePreview), new PropertyMetadata(5) );
        public int MaxChecklistItems { get { return (int)GetValue(MaxChecklistItemsProperty); } set { SetValue(MaxChecklistItemsProperty, value); } }

        public NotePreview()
        {
            this.InitializeComponent();
        }
    }
}
