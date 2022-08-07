using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static KSP_WINUI2.ClassManager;

namespace KSP_WINUI2.Controls;

public sealed partial class FriendListControl : UserControl
{
    public delegate void OnSelected(FriendProfile profile);
    public OnSelected Listener;
    public FriendListControl()
    {
        this.InitializeComponent();
    }

    public void SetSource(List<FriendProfile> source)
    {
        var listView = Content as ListView;
        if (source.Count == 0)
            listView.Visibility = Visibility.Collapsed;
        else
        {
            var max = Math.Min(source.Count, 10);
            source = source.GetRange(0, max);
            listView.Visibility = Visibility.Visible;
            listView.ItemsSource = source;
        }
    }
    public void ShowIds(List<FriendProfile> ids)
    {
        SetSource(ids);
    }
    public int SearchFriendList(string nameQuery)
    {
        var listView = Content as ListView;
        if (string.IsNullOrEmpty(nameQuery))
            listView.Visibility = Visibility.Collapsed;
        else
        {
            var source = Pages.MainPage.Friends.profiles.Where(x => x.display_name.ToLower().Contains(nameQuery.ToLower())).Select(x => new FriendProfile { Id = x.id, Name = x.display_name, ProfileUrl = x.profile_video_url_square_small ?? x.profile_thumbnail_url }).ToList();
            SetSource(source);
            return source.Count;
        }
        return 0;
    }

    private void SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = Content as ListView;
        Listener.Invoke(listView.SelectedItem as FriendProfile);
        listView.SelectedItem = null;
    }
}
