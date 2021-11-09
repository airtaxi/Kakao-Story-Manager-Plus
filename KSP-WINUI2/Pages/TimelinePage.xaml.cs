using StoryApi;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KSP_WINUI2.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TimelinePage : Page
    {
        private string _id = null;
        private static TimelinePage _instance;
        public TimelinePage()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string id = e.Parameter as string;
            _id = id;
            _instance = this;
            await Refresh();
        }


        public static void HidePostFromTimeline(Controls.TimelineControl control) => _instance.SpContent.Children.Remove(control);

        string lastFeed = null;
        private async Task Refresh(string from = null)
        {
            PrLoading.Visibility = Visibility.Visible;
            if (from == null)
                SpContent.Children.Clear();
            if(_id == null)
            {
                var data = await StoryApi.ApiHandler.GetFeed(lastFeed);
                foreach(var feed in data.feeds)
                {
                    if (IsValidFeed(feed))
                    {
                        var control = new Controls.TimelineControl(feed);
                        SpContent.Children.Add(control);
                    }
                    lastFeed = feed.id;
                }
            }
            else
            {
                FrProfile.Content = new Controls.UserProfileControl(_id);
                FrProfile.Visibility = Visibility.Visible;
                var data = await StoryApi.ApiHandler.GetProfileFeed(_id, lastFeed);
                foreach (var feed in data.activities)
                {
                    if (IsValidFeed(feed))
                    {
                        var control = new Controls.TimelineControl(feed);
                        SpContent.Children.Add(control);
                    }
                    lastFeed = feed.id;
                }
            }
            PrLoading.Visibility = Visibility.Collapsed;
        }

        private bool IsValidFeed(ApiHandler.DataType.CommentData.PostData feed)
        {
            return feed.deleted != true && (feed.@object?.deleted ?? false) != true && feed.blinded != true && (feed.@object?.blinded ?? false) != true && (feed.verb == "post" || feed.verb == "share");
        }

        private async void OnPullToRefresh(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            await Refresh();
            args.GetDeferral().Complete();
            args.GetDeferral().Dispose();
        }

        private bool isRefreshing = false;
        private async void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!isRefreshing)
            {
                var scrollViewer = sender as ScrollViewer;
                var verticalOffset = scrollViewer.VerticalOffset;
                var maxVerticalOffset = scrollViewer.ScrollableHeight;

                if (maxVerticalOffset < 0 || verticalOffset == maxVerticalOffset)
                {
                    isRefreshing = true;
                    await Refresh(lastFeed);
                    isRefreshing = false;
                }
            }
        }
    }
}
