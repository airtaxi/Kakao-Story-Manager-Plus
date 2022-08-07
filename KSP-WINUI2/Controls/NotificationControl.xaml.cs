using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static StoryApi.ApiHandler.DataType;

namespace KSP_WINUI2.Controls;

public sealed partial class NotificationControl : UserControl
{
    private class NotificationData
    {
        public string ProfilePictureUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Time { get; set; }
        public string Scheme { get; set; }
        public string ActorId { get; set; }
        public Visibility UnreadBarVisiblity { get; set; }
    }
    public NotificationControl()
    {
        this.InitializeComponent();
        _ = Refresh();
    }

    private async Task Refresh()
    {
        var notificationDatas = new List<NotificationData>();
        var notifications = await StoryApi.ApiHandler.GetNotifications();
        foreach(var notification in notifications)
        {
            string contentMessage = notification.content ?? "내용 없음";
            if (contentMessage.Contains("\n"))
                contentMessage = contentMessage.Split(new string[] { "\n" }, StringSplitOptions.None)[0];
            var notificationData = new NotificationData
            {
                Title = notification.message,
                Description = contentMessage,
                ProfilePictureUrl = notification.actor?.profile_video_url_square_small ?? notification.actor?.profile_thumbnail_url,
                Time = StoryApi.Utils.GetTimeString(notification.created_at),
                UnreadBarVisiblity = notification.is_new ? Visibility.Visible : Visibility.Collapsed,
                Scheme = notification.scheme,
                ActorId = notification.actor.id
            };
            notificationDatas.Add(notificationData);
        }
        (Content as ListView).ItemsSource = notificationDatas;
    }

    private void PpProfileImage_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var id = (sender as PersonPicture).Tag as string;
        Pages.MainPage.ShowProfile(id);
        e.Handled = true;
    }

    private async void NotificationSelected(object sender, SelectionChangedEventArgs e)
    {
        var listView = sender as ListView;
        var notificationData = listView.SelectedItem as NotificationData;
        var scheme = notificationData.Scheme;
        if (scheme.Contains("?profile_id="))
        {
            var objectStringStr = scheme.Split(new string[] { "?profile_id=" }, StringSplitOptions.None);
            var id = objectStringStr[0].Split(new string[] { "activities/" }, StringSplitOptions.None)[1];
            var post = await StoryApi.ApiHandler.GetPost(id);
            Pages.MainPage.HideOverlay();
            Pages.MainPage.ShowOverlay(new TimelineControl(post, false, true));
        }
        else if (scheme.Contains("kakaostory://profiles/"))
        {
            string id = scheme.Replace("kakaostory://profiles/", "");
            Pages.MainPage.HideOverlay();
            Pages.MainPage.ShowProfile(id);
        }
    }
}
