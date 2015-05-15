using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Linq;

namespace FlatNotes.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel viewModel { get { return _viewModel; } }
        private static MainViewModel _viewModel = new MainViewModel();

        public ObservableCollection<NavLink> NavLinks { get; } = new ObservableCollection<NavLink>()
        {
            new NavLink() { Label = "Notes", Symbol = Symbol.Emoji  },
            new NavLink() { Label = "Reminders", Symbol = Symbol.Clock },
            new NavLink() { Label = "Archive", Symbol = Symbol.Emoji },
            new NavLink() { Label = "Trash", Symbol = Symbol.Delete },
        };

        public ObservableCollection<NavLink> NavFooterLinks { get; } = new ObservableCollection<NavLink>()
        {
            new NavLink() { Label = "Accounts", Symbol = Symbol.Contact },
            new NavLink() { Label = "Feedback", Symbol = Symbol.Favorite },
            new NavLink() { Label = "Settings", Symbol = Symbol.Setting },
        };

        public MainPage()
        {
            this.InitializeComponent();

            var checklistMyStrengths = new Checklist();
            checklistMyStrengths.Add(new ChecklistItem("ASBD AHJSBdjk nsadnk", false));
            checklistMyStrengths.Add(new ChecklistItem(" asdkls ndKLSANDK n", false));
            checklistMyStrengths.Add(new ChecklistItem("KJASNDKjasn dklasn kda", false));
            checklistMyStrengths.Add(new ChecklistItem("ALJDnsaldn salkndkajnd kA", false));
            checklistMyStrengths.Add(new ChecklistItem("Tempo", false));


            var checklistMyWeakness = new Checklist();
            checklistMyWeakness.Add(new ChecklistItem("a DKJotagem", false));
            checklistMyWeakness.Add(new ChecklistItem("Falta de ASD LAKSMLD Msl", false));
            checklistMyWeakness.Add(new ChecklistItem("AK DNSALKmd  / Pouco AKM DLKSdk ", false));
            checklistMyWeakness.Add(new ChecklistItem("ASDN KSANDKAN jk a", false));
            checklistMyWeakness.Add(new ChecklistItem("Pouco KASDL ASdklm d (mais AKLSMDLKAS MDL)", false));
            checklistMyWeakness.Add(new ChecklistItem("KLASDMSADlk asm de A", false));
            checklistMyWeakness.Add(new ChecklistItem("Falta de KASD KASMDLK m / ALK DMSLD", false));
            checklistMyWeakness.Add(new ChecklistItem("Geralmente KALSMD LKMDLK am ", false));

            var checklistMyOpportunities = new Checklist();

            var checklistMyThreats = new Checklist();
            checklistMyThreats.Add(new ChecklistItem("Pouco AKSDM ASLMDLK AMS", false));
            checklistMyThreats.Add(new ChecklistItem("Ambiente ASKDM KSAMDK saapsodk asafiador", false));
            checklistMyThreats.Add(new ChecklistItem("Pessoas ASKDM SAMDjk / Más AOD kmsaas", false));
            checklistMyThreats.Add(new ChecklistItem("Falta de AKJ SDKASJ nd / Grandes AKJ DNksa", false));

            var checklistBooks = new Checklist();
            checklistBooks.Add(new ChecklistItem("A Arte do Começo", true));
            checklistBooks.Add(new ChecklistItem("O Poder do Hábito", true));
            checklistBooks.Add(new ChecklistItem("Trabalhe 4 horas por semana", true));
            checklistBooks.Add(new ChecklistItem("The Four Steps To The Epiphany - Steve Blanks", false));


            viewModel.Notes.Add(new Note("IMEI Moto G 2014", "353334062411489", NoteColor.BLUE));
            viewModel.Notes.Add(new Note("", "\"Enquanto você pensar em centavos, você irá ganhar em centavos\"", NoteColor.GRAY));
            viewModel.Notes.Add(new Note("My Strengths", checklistMyStrengths, NoteColor.YELLOW));
            viewModel.Notes.Add(new Note("My Weaknesses", checklistMyWeakness, NoteColor.RED));
            viewModel.Notes.Add(new Note("My Opportunities", checklistMyOpportunities, NoteColor.GREEN));
            viewModel.Notes.Add(new Note("My Threats", checklistMyThreats, NoteColor.ORANGE));
            viewModel.Notes.Add(new Note("Ler livros", checklistBooks, NoteColor.DEFAULT));
        }

        private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void MenuToggleButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        //private void NotesListView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    var listControl = sender as ItemsControl;
        //    if (listControl.ItemsPanelRoot?.Children == null) return;

        //    UpdateListAppearence(listControl.ItemsPanelRoot?.Children);
        //}

        //private void UpdateListAppearence(UIElementCollection elements)
        //{
        //    if (elements == null || elements.Count <= 0) return;

        //    int columns = 4, nextColumn = 0;
        //    double[] lastYInColumn = new double[columns];
        //    UIElement[] lastElementInColumn = new UIElement[columns];

        //    for (int i = 0; i < elements.Count; i++)
        //    {
        //        var item = elements[i];

        //        if (i < columns)
        //        {
        //            lastElementInColumn[i] = item;
        //            lastYInColumn[i] = item.DesiredSize.Height;

        //            if (i > 0) RelativePanel.SetRightOf(item, elements[i - 1]);

        //            continue;
        //        }

        //        for (int j = columns - 1; j >= 0; j--)
        //            if (lastYInColumn[j] <= lastYInColumn[nextColumn])
        //                nextColumn = j;

        //        RelativePanel.SetBelow(item, lastElementInColumn[nextColumn]);
        //        RelativePanel.SetAlignHorizontalCenterWith(item, lastElementInColumn[nextColumn]);

        //        lastElementInColumn[nextColumn] = item;
        //        lastYInColumn[nextColumn] += item.DesiredSize.Height;
        //    }
        //}
    }
}
