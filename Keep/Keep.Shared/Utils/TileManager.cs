using Keep.Models;
using NotificationsExtensions.TileContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace Keep.Utils
{
    //NotificationsExtensions: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn642158.aspx
    //Tile Template Catalog: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh761491.aspx
    public static class TileManager
    {
        public static void UpdateDefaultTile(bool transparentTile = true)
        {
            var tileSubFolder = transparentTile ? "Transparent" :  "Solid";
            var tileSquare71Content = TileContentFactory.CreateTileSquare71x71Image();
            ITileNotificationContent biggerTile;

#if WINDOWS_PHONE_APP
            tileSquare71Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square71x71Logo.png", tileSubFolder);
#else
            tileSquare71Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square70x70Logo.png", tileSubFolder);
#endif

            var tileSquare150Content = TileContentFactory.CreateTileSquare150x150Image();
            tileSquare150Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Logo.png", tileSubFolder);
            tileSquare150Content.Square71x71Content = tileSquare71Content;

            var tileWideContent = TileContentFactory.CreateTileWide310x150Image();
            tileWideContent.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/WideLogo.png", tileSubFolder);
            tileWideContent.Square150x150Content = tileSquare150Content;

#if WINDOWS_PHONE_APP
            biggerTile = tileWideContent;
#else
            var tileSquare310Content = TileContentFactory.CreateTileSquare310x310Image();
            tileSquare310Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square310x310Logo.png", tileSubFolder);
            tileSquare310Content.Wide310x150Content = tileWideContent;

            biggerTile = tileSquare310Content;
#endif

            TileUpdateManager.CreateTileUpdaterForApplication().Update(biggerTile.CreateNotification());
        }

        public static async Task<bool> CreateOrUpdateNoteTile(Note note)
        {
            if (note == null || note.IsEmpty()) return false;

            //update content
            if (SecondaryTile.Exists(note.ID)) return UpdateNoteTileIfExists(note);


#if WINDOWS_PHONE_APP
            //create (and suspend)
            return await CreateNoteTile(note);
#else
            //create and update
            var success = await CreateNoteTile(note);
            UpdateNoteTileIfExists(note);

            return success;
#endif
        }

        private static async Task<bool> CreateNoteTile(Note note)
        {
            var tile = new SecondaryTile(note.ID, "Flat Notes", GenerateNavigationArgumentFromNote(note), 
                new Uri("ms-appx:///Assets/Tiles/Transparent/Logo.png"), TileSize.Wide310x150);

            tile.VisualElements.ForegroundText = ForegroundText.Light;
            tile.VisualElements.BackgroundColor = new Color().FromHex(note.Color.DarkColor1);

            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ShowNameOnWide310x150Logo = true;
            tile.VisualElements.ShowNameOnSquare310x310Logo = true;

            tile.RoamingEnabled = true;

#if WINDOWS_PHONE_APP
            tile.VisualElements.Square30x30Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square71x71Logo.png");
            tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square71x71Logo.png");
#else
            tile.VisualElements.Square30x30Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square30x30Logo.png");
            tile.VisualElements.Square70x70Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square70x70Logo.png");
#endif
            tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Logo.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/WideLogo.png");
            tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square310x310Logo.png");
            
            return await tile.RequestCreateAsync();
        }

        public static bool UpdateNoteTileIfExists(Note note)
        {
            //must exists
            if (!SecondaryTile.Exists(note.ID)) return false;

            //update background
            TryUpdateNoteTileBackgroundColor(note);

            var tileSquare71Content = TileContentFactory.CreateTileSquare71x71Image();

#if WINDOWS_PHONE_APP
            tileSquare71Content.Image.Src = "ms-appx:///Assets/Tiles/Transparent/Square71x71Logo.png";
#else
            tileSquare71Content.Image.Src = "ms-appx:///Assets/Tiles/Transparent/Square70x70Logo.png";
#endif

            ISquare150x150TileNotificationContent tileSquare150Content;

            if (note.Images.Count > 0)
            {
                tileSquare150Content = TileContentFactory.CreateTileSquare150x150PeekImageAndText02();

                var _tileSquare150Content = tileSquare150Content as ITileSquare150x150PeekImageAndText02;
                _tileSquare150Content.TextHeading.Text = note.Title;
                _tileSquare150Content.TextBodyWrap.Text = note.GetContent(true);
                _tileSquare150Content.Image.Src = note.Images[0].URL;
                _tileSquare150Content.Square71x71Content = tileSquare71Content;
            }
            else
            {
                tileSquare150Content = TileContentFactory.CreateTileSquare150x150Text02();

                var _tileSquare150Content = tileSquare150Content as ITileSquare150x150Text02;
                _tileSquare150Content.TextHeading.Text = note.Title;
                _tileSquare150Content.TextBodyWrap.Text = note.GetContent(true);
                _tileSquare150Content.Square71x71Content = tileSquare71Content;
            }


            IWide310x150TileNotificationContent tileWideContent;
            if (note.Images.Count > 0)
            {
                tileWideContent = TileContentFactory.CreateTileWide310x150PeekImage05();

                var _tileWideContent = tileWideContent as ITileWide310x150PeekImage05;
                _tileWideContent.TextHeading.Text = note.Title;
                _tileWideContent.TextBodyWrap.Text = note.GetContent(true);
                _tileWideContent.ImageMain.Src = note.Images[0].URL;
                _tileWideContent.ImageSecondary.Src = note.Images[0].URL;
                _tileWideContent.Square150x150Content = tileSquare150Content;
            }
            else
            {
                tileWideContent = TileContentFactory.CreateTileWide310x150Text09();

                var _tileWideContent = tileWideContent as ITileWide310x150Text09;
                _tileWideContent.TextHeading.Text = note.Title;
                _tileWideContent.TextBodyWrap.Text = note.GetContent(true);
                _tileWideContent.Square150x150Content = tileSquare150Content;
            }

            var tileSquare310Content = TileContentFactory.CreateTileSquare310x310ImageAndTextOverlay02();
            tileSquare310Content.TextHeadingWrap.Text = note.Title;
            tileSquare310Content.TextBodyWrap.Text = note.GetContent(true);
            tileSquare310Content.Image.Src = note.Images.Count > 0 ? note.Images[0].URL : null;
            tileSquare310Content.Wide310x150Content = tileWideContent;
            
            //create notification
            var notification = tileSquare310Content.CreateNotification();
            notification.Tag = note.ID.Substring(0, 16);

            //update
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(note.ID);
            tileUpdater.Clear();
            tileUpdater.EnableNotificationQueue(false);
            tileUpdater.Update(notification);

            return true;
        }

        private static async void TryUpdateNoteTileBackgroundColor(Note note)
        {
            try
            {
                var tile = new SecondaryTile(note.ID);
                tile.VisualElements.BackgroundColor = new Color().FromHex(note.Color.DarkColor1);
                await tile.UpdateAsync();
            }
            catch (Exception) { }
        }

        public static async void RemoveTileIfExists(Note note)
        {
            //must exists
            if (!SecondaryTile.Exists(note.ID)) return;

            var tile = new SecondaryTile(note.ID);
            await tile.RequestDeleteAsync();
        }

        public static Note TryToGetNoteFromNavigationArgument(string navigationParameter)
        {
            if (String.IsNullOrEmpty(navigationParameter)) return null;

            string noteId;

            try
            {
                WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(navigationParameter);
                Dictionary<string, string> queryParams = decoder.ToDictionary(x => x.Name, x => x.Value);

                if (!queryParams.ContainsKey("noteId")) return null;
                noteId = queryParams["noteId"];
            }
            catch (Exception)
            {
                return null;
            }

            return AppData.Notes.FirstOrDefault(n => n.ID == noteId);
        }

        private static string GenerateNavigationArgumentFromNote(Note note)
        {
            return String.Format("?noteId={0}", note.ID);
        }
    }
}
