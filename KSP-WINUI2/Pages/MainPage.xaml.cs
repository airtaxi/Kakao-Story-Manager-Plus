using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static KSP_WINUI2.ClassManager;
using static StoryApi.ApiHandler.DataType.FriendData;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KSP_WINUI2.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static MainPage _instance;
        public static StoryApi.ApiHandler.DataType.UserProfile.ProfileData Me;
        public static Friends Friends = null;

        public MainPage()
        {
            this.InitializeComponent();
            _ = Refresh();
            _instance = this;
            Window.Current.CoreWindow.KeyDown += OnKeyDown;
            var writeFlyout = new Flyout();
            var inputControl = new Controls.InputControl();
            writeFlyout.Content = inputControl;
            inputControl.SetWidth(300);
            inputControl.SetMaxHeight(200);
            inputControl.AcceptReturn(true);
            inputControl.WrapText(true);
            BtWrite.Flyout = writeFlyout;
        }

        private async Task RefreshFriends()
        {
            Friends = await StoryApi.ApiHandler.GetFriends();
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if(args.VirtualKey == Windows.System.VirtualKey.Escape)
                HideOverlay();
        }

        public static void ShowOverlay(UIElement element, bool isSecond = false)
        {
            var overlay = isSecond ? _instance.GdOverlay2 : _instance.GdOverlay;
            var frame = isSecond ? _instance.FrOverlay2 : _instance.FrOverlay;
            overlay.Visibility = Visibility.Visible;
            frame.Content = element;
        }
        public static void HideOverlay(bool willDispose = true)
        {
            var isSecond = _instance.GdOverlay2.Visibility == Visibility.Visible;
            var overlay = isSecond ? _instance.GdOverlay2 : _instance.GdOverlay;
            var frame = isSecond ? _instance.FrOverlay2 : _instance.FrOverlay;
            overlay.Visibility = Visibility.Collapsed;
            if(willDispose)
                frame.Content = null;
        }

        private async Task Refresh()
        {
            await RefreshFriends();
            var friends = await StoryApi.ApiHandler.GetFriends();
            TbFriendCount.Text = $"내 친구 {friends.profiles.Count}";
            var friendProfiles = new List<FriendProfile>();
            foreach(var profile in friends.profiles)
            {
                var friendProfile = new FriendProfile
                {
                    ProfileUrl = profile.profile_video_url_square_small ?? profile.profile_thumbnail_url,
                    Name = profile.display_name,
                    Id = profile.id
                };
                friendProfiles.Add(friendProfile);
            }
            LvFriends.ItemsSource = friendProfiles;
            Me = await StoryApi.ApiHandler.GetProfileData();
            TbName.Text = Me.display_name;
            var profileUrl = Me.profile_video_url_square_small ?? Me.profile_thumbnail_url;
            if (!string.IsNullOrEmpty(profileUrl))
                PpMyProfile.ProfilePicture = Utils.GenerateImageUrlSource(profileUrl);
            FrContent.Navigate(typeof(TimelinePage), null);
        }

        private void OnFriendListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            var data = listView.SelectedItem as FriendProfile;
            FrContent.Navigate(typeof(TimelinePage), data.Id);
        }

        private void ProfilePointerEntered(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(true);

        private void ProfilePointerExited(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(false);

        private void OnLogoutButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        public static void ShowProfile(string id)
        {
            HideOverlay();
            _instance.FrContent.Navigate(typeof(TimelinePage), id);
        }

        private void FriendPointerEntered(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(true);

        private void FriendPointerExited(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(false);

        private void ProfileTapped(object sender, TappedRoutedEventArgs e) => FrContent.Navigate(typeof(TimelinePage), Me.id);

        private void TitleTapped(object sender, TappedRoutedEventArgs e) => FrContent.Navigate(typeof(TimelinePage));

        private void OnNotificationsButtonClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var flyout = new Flyout();
            flyout.Content = new Controls.NotificationControl();
            flyout.Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft;
            flyout.ShowAt(button);
        }

        private void SearchFriend(AutoSuggestBox sender)
        {
            var text = sender.Text.ToLower();
            if (!string.IsNullOrEmpty(text))
            {
                var newFriends = Friends.profiles.Where(x => x.display_name.ToLower().Contains(text)).Select(x => new FriendProfile { Name = x.display_name, ProfileUrl = x.profile_video_url_square_small ?? x.profile_thumbnail_url, Id = x.id }).ToList();
                sender.ItemsSource = newFriends;
            }
            else
                sender.ItemsSource = null;
        }
        private void SearchFriendTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => SearchFriend(sender);

        private void SearchFriendSelected(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var friend = args.SelectedItem as FriendProfile;
            var id = friend.Id;
            FrContent.Navigate(typeof(TimelinePage), id);
            sender.Text = "";
            sender.ItemsSource = null;
        }

        private void SearchFriendQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) => SearchFriend(sender);
    }
}
