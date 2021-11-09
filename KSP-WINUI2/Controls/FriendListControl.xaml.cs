using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static KSP_WINUI2.ClassManager;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace KSP_WINUI2.Controls
{
    public sealed partial class FriendListControl : UserControl
    {
        public delegate void OnSelected(FriendProfile profile);
        public OnSelected Listener;
        public FriendListControl()
        {
            this.InitializeComponent();
        }

        public int Refresh(string query)
        {
            var listView = Content as ListView;
            if (string.IsNullOrEmpty(query))
                listView.Visibility = Visibility.Collapsed;
            else
            {
                var source = Pages.MainPage.Friends.profiles.Where(x => x.display_name.ToLower().Contains(query.ToLower())).Select(x => new FriendProfile { Id = x.id, Name = x.display_name, ProfileUrl = x.profile_video_url_square_small ?? x.profile_thumbnail_url }).ToList();
                var count = source.Count;
                if (source.Count == 0)
                    listView.Visibility = Visibility.Collapsed;
                else
                {
                    var max = Math.Min(source.Count, 10);
                    source = source.GetRange(0, max);
                    listView.Visibility = Visibility.Visible;
                    listView.ItemsSource = source;
                    return source.Count;
                }
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
}
