using FontAwesome5;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static StoryApi.ApiHandler.DataType.CommentData;

namespace KSP_WINUI2.Controls;

public sealed partial class TimelineControl : UserControl
{
    private PostData _post;
    private readonly bool _isOverlay;
    private readonly bool _isShare;

    public TimelineControl(PostData post, bool isShare = false, bool isOverlay = false)
    {
        this.InitializeComponent();
        this._post = post;
        _isOverlay = isOverlay;
        _isShare = isShare;
        if (isOverlay)
            FaClose.Visibility = Visibility.Visible;
        if (isShare)
        {
            SpComment.Visibility = Visibility.Collapsed;
            SpEmotions.Visibility = Visibility.Collapsed;
            SpMenu.Visibility = Visibility.Collapsed;
            FrShareMargin.Visibility = Visibility.Visible;
            BdShare.Visibility = Visibility.Visible;
            SvContent.Padding = new Thickness(20, 0, 20, 20);
            GdMain.Margin = new Thickness(0);
        }
        if (post.@object != null)
            FrShare.Content = new TimelineControl(post.@object, true);
        if (post.scrap != null)
            FrLink.Content = new LinkControl(post.scrap);
        Initialize();
        FrComment.Content = new InputControl("댓글을 입력하세요.");
        Refresh(post);
    }

    private void RefreshBookmarkButton() => FaFavorite.Icon = _post.bookmarked ? EFontAwesomeIcon.Solid_Star : EFontAwesomeIcon.Regular_Star;
    public void HideEmotionsButtonFlyout() => BtEmotions.Flyout.Hide();
    public void RefreshEmotionsButton()
    {
        if (_post.liked)
        {
            var emotion = _post.liked_emotion;
            if (emotion == "like")
            {
                BtEmotions.Background = Utils.GetColorFromHexa("#FFE25434");
                FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                FaEmotions.Icon = EFontAwesomeIcon.Solid_Heart;
            }
            else if (emotion == "good")
            {
                BtEmotions.Background = Utils.GetColorFromHexa("#FFBCCB3C");
                FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                FaEmotions.Icon = EFontAwesomeIcon.Solid_Star;
            }
            else if (emotion == "pleasure")
            {
                BtEmotions.Background = Utils.GetColorFromHexa("#FFEFBD30");
                FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                FaEmotions.Icon = EFontAwesomeIcon.Solid_Smile;
            }
            else if (emotion == "sad")
            {
                BtEmotions.Background = Utils.GetColorFromHexa("#FF359FB0");
                FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                FaEmotions.Icon = EFontAwesomeIcon.Solid_Tint;
            }
            else if (emotion == "cheerup")
            {
                BtEmotions.Background = Utils.GetColorFromHexa("#FF9C62AE");
                FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                FaEmotions.Icon = EFontAwesomeIcon.Solid_Bolt;
            }
        }
        else
        {
            BtEmotions.Background = Utils.GetColorFromHexa("#00000000");
            FaEmotions.Foreground = Utils.GetColorFromHexa("#FF888D94");
            FaEmotions.Icon = EFontAwesomeIcon.Solid_Heart;
        }
    }
    public async Task RefreshPost() => _post = await StoryApi.ApiHandler.GetPost(_post.id);
    private void RefreshUpButton()
    {
        if (_post.sympathized)
        {
            BtUp.Background = Utils.GetColorFromHexa("#FF838383");
            FaUp.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
        }
        else
        {
            BtUp.Background = Utils.GetColorFromHexa("#00000000");
            FaUp.Foreground = Utils.GetColorFromHexa("#FF888D94");
        }
    }
    private void Initialize()
    {
        var emotionsFlyout = new Flyout();
        emotionsFlyout.Content = new EmotionsControl(_post, this);
        BtEmotions.Flyout = emotionsFlyout;
        BtEmotions.Flyout.Opening += async (s, e) =>
        {
            if (_post.liked)
            {
                await StoryApi.ApiHandler.LikePost(_post.id, null);
                await RefreshPost();
                HideEmotionsButtonFlyout();
                RefreshEmotionsButton();
            }
        };
        BtEmotions.Flyout.Placement = FlyoutPlacementMode.TopEdgeAlignedLeft;

        var shareFlyout = new MenuFlyout();
        shareFlyout.Items.Add(new MenuFlyoutItem() { Text = "스토리로 공유" });
        shareFlyout.Items.Add(new MenuFlyoutItem() { Text = $"URL 복사하기" });
        BtShare.Flyout = shareFlyout;

        RefreshUpButton();
        RefreshBookmarkButton();
        RefreshEmotionsButton();
    }

    private async void Refresh(PostData _post)
    {
        if(_isOverlay)
            _post = await StoryApi.ApiHandler.GetPost(_post.id);

        TbName.Text = _post.actor.display_name;
        var timestampString = StoryApi.Utils.GetTimeString(_post.created_at);
        TbTime.Text = timestampString;
        PpUser.ProfilePicture = Utils.GenerateImageUrlSource(_post.actor.profile_video_url_square_small ?? _post.actor.profile_thumbnail_url);
        SpComments.Children.Clear();
        if ((_post.comments?.Count ?? 0) > 0)
        {
            var comments = await StoryApi.ApiHandler.GetComments(_post.id, null);
            SvComments.Visibility = Visibility.Visible;
            BdComments.Visibility = Visibility.Visible;
            foreach (var comment in comments)
            {
                var commentControl = new CommentControl(comment, _post.id, _isOverlay);
                commentControl.OnReplyClick += (Comment sender) =>
                {
                    var profile = sender.writer;
                    var inputContol = FrComment.Content as InputControl;
                    inputContol.AppendText("{!{{" + "\"id\":\"" + profile.id + "\", \"type\":\"profile\", \"text\":\"" + profile.display_name + "\"}}!} ");
                };
                SpComments.Children.Add(commentControl);
            }
        }
        else
        {
            SvComments.Visibility = Visibility.Collapsed;
            BdComments.Visibility = Visibility.Collapsed;
        }

        if ((_post.media?.Count ?? 0) > 0)
            Utils.SetMediaContent(_post.media, FvMedia);

        Utils.SetTextContent(_post.content_decorators, RTbContent);
    }

    private void OnDotMenuTapped(object sender, TappedRoutedEventArgs e)
    {
        var fontawesome = sender as FontAwesome;
        var flyout = new MenuFlyout();
        var menuAddFavorite = new MenuFlyoutItem() { Text = _post.bookmarked ? "관심글 삭제하기" : "관심글로 저장하기" };
        menuAddFavorite.Click += (o, e2) =>
        {
            OnAddBookmarkTapped(null, null);
        };
        flyout.Items.Add(menuAddFavorite);
        flyout.Items.Add(new MenuFlyoutSeparator());
        var menuHidePost = new MenuFlyoutItem() { Text = "이 글 숨기기" };
        menuHidePost.Click += async (o, e2) =>
        {
            await StoryApi.ApiHandler.HidePost(_post.id);
            //Pages.TimelinePage.HidePostFromTimeline(this);
            Pages.MainPage.HideOverlay();
        };
        flyout.Items.Add(menuHidePost);
        var menuBlockProfile = new MenuFlyoutItem() { Text = _post.actor.is_feed_blocked ? $"'{_post.actor.display_name}' 글 받기" : $"'{_post.actor.display_name}' 글 안받기" };
        menuBlockProfile.Click += async (o, e2) =>
        {
            await StoryApi.ApiHandler.BlockProfile(_post.actor.id, _post.actor.is_feed_blocked);
            await RefreshPost();
        };
        flyout.Items.Add(menuBlockProfile);
        var menuMutePost = new MenuFlyoutItem() { Text = _post.push_mute ? "이 글 알림 받기" : "이 글 알림 받지 않기" };
        menuMutePost.Click += async (o, e2) =>
        {
            await StoryApi.ApiHandler.MutePost(_post.id, !_post.push_mute);
            await RefreshPost();
        };
        flyout.Items.Add(menuMutePost);
        flyout.ShowAt(fontawesome);
    }
    private bool isBookmarking = false;
    private async void OnAddBookmarkTapped(object sender, TappedRoutedEventArgs e)
    {
        if (!isBookmarking)
        {
            isBookmarking = true;
            var isBookmarked = _post.bookmarked;
            await StoryApi.ApiHandler.PinPost(_post.id, isBookmarked);
            isBookmarking = false;
            await RefreshPost();
            RefreshBookmarkButton();
        }
    }

    private async void OnUpButtonClicked(object sender, RoutedEventArgs e)
    {
        var isUp = _post.sympathized;
        BtUp.IsEnabled = false;
        await StoryApi.ApiHandler.UpPost(_post.id, isUp);
        BtUp.IsEnabled = true;
        await RefreshPost();
        RefreshUpButton();
    }

    private void OnMediaTapped(object sender, TappedRoutedEventArgs e)
    {
        var index = FvMedia.SelectedIndex;
        UIElement item = (FvMedia.ItemsSource as List<UIElement>)[index];
        if (item is Image)
        {
            var images = _post.media.Where(x => x.content_type != "video/mp4").ToList();
            var control = new ImageViewerControl(images, index);
            Pages.MainPage.ShowOverlay(control, _isOverlay);
        }
    }

    private void TimeTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        if (!_isOverlay)
            Pages.MainPage.ShowOverlay(new TimelineControl(_post, false, true));
    }

    private void PointerEnteredShowHand(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(true);

    private void PointerExitedShowHand(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(false);

    private void CloseButtonClicked(object sender, TappedRoutedEventArgs e) => Pages.MainPage.HideOverlay();

    private void OnUserProfilePictureTapped(object sender, TappedRoutedEventArgs e)
    {
        Pages.MainPage.HideOverlay();
        Pages.MainPage.ShowProfile(_post.actor.id);
    }
}