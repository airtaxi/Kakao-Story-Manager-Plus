using StoryApi;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KSP_WINUI2.Pages;

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
        Border border = VisualTreeHelper.GetChild(LvContent, 0) as Border;
        ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
        scrollViewer.ViewChanged += OnScrollViewerViewChanged;
    }


    public static void HidePostFromTimeline(Controls.TimelineControl control) => _instance.LvContent.Items.Remove(control);

    string lastFeed = null;
    private async Task Refresh(string from = null)
    {
        PrLoading.Visibility = Visibility.Visible;
        if (from == null)
            LvContent.Items.Clear();
        if (_id == null)
        {
            var data = await StoryApi.ApiHandler.GetFeed(lastFeed);
            foreach (var feed in data.feeds)
            {
                if (IsValidFeed(feed))
                {
                    var control = new Controls.TimelineControl(feed);
                    LvContent.Items.Add(control);
                }
                lastFeed = feed.id;
            }
        }
        else
        {
            var profileFrame = new Frame();
            profileFrame.Content = new Controls.UserProfileControl(_id);
            profileFrame.Visibility = Visibility.Visible;
            LvContent.Items.Add(profileFrame);

            var data = await StoryApi.ApiHandler.GetProfileFeed(_id, lastFeed);
            foreach (var feed in data.activities)
            {
                if (IsValidFeed(feed))
                {
                    var control = new Controls.TimelineControl(feed);
                    LvContent.Items.Add(control);
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
