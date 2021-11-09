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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace KSP_WINUI2.Controls
{
    public sealed partial class TimelineControl : UserControl
    {
        private PostData post;
        private readonly bool _isOverlay;
        public TimelineControl(PostData post, bool isShare = false, bool isOverlay = false)
        {
            this.InitializeComponent();
            this.post = post;
            _isOverlay = isOverlay;
            if (isOverlay)
                FaClose.Visibility = Visibility.Visible;
            if (isShare)
            {
                SpComments.Visibility = Visibility.Collapsed;
                SpEmotions.Visibility = Visibility.Collapsed;
                SpMenu.Visibility = Visibility.Collapsed;
                FrShareMargin.Visibility = Visibility.Visible;
                BdShare.Visibility = Visibility.Visible;
                SvContent.Padding = new Thickness(20, 0, 20, 20);
                GdMain.Margin = new Thickness(0);
            }
            if(post.@object != null)
                FrShare.Content = new TimelineControl(post.@object, true);
            if (post.scrap != null)
                FrLink.Content = new LinkControl(post.scrap);
            Initialize();
            Refresh(post);
        }

        private void RefreshBookmarkButton() => FaFavorite.Icon = post.bookmarked ? EFontAwesomeIcon.Solid_Star : EFontAwesomeIcon.Regular_Star;
        public void HideEmotionsButtonFlyout() => BtEmotions.Flyout.Hide();
        public void RefreshEmotionsButton()
        {
            if (post.liked)
            {
                var emotion = post.liked_emotion;
                if(emotion == "like")
                {
                    BtEmotions.Background = Utils.GetColorFromHexa("#FFE25434");
                    FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                    FaEmotions.Icon = EFontAwesomeIcon.Solid_Heart;
                }
                else if(emotion == "good")
                {
                    BtEmotions.Background = Utils.GetColorFromHexa("#FFBCCB3C");
                    FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                    FaEmotions.Icon = EFontAwesomeIcon.Solid_Star;
                }
                else if(emotion == "pleasure")
                {
                    BtEmotions.Background = Utils.GetColorFromHexa("#FFEFBD30");
                    FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                    FaEmotions.Icon = EFontAwesomeIcon.Solid_Smile;
                }
                else if(emotion == "sad")
                {
                    BtEmotions.Background = Utils.GetColorFromHexa("#FF359FB0");
                    FaEmotions.Foreground = Utils.GetColorFromHexa("#FFFFFFFF");
                    FaEmotions.Icon = EFontAwesomeIcon.Solid_Tint;
                }
                else if(emotion == "cheerup")
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
        public async Task RefreshPost() => post = await StoryApi.ApiHandler.GetPost(post.feed_id);
        private void RefreshUpButton()
        {
            if (post.sympathized)
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
            emotionsFlyout.Content = new EmotionsControl(post, this);
            BtEmotions.Flyout = emotionsFlyout;
            BtEmotions.Flyout.Opening += async (s, e) =>
            {
                if (post.liked)
                {
                    await StoryApi.ApiHandler.LikePost(post.id, null);
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

        private void Refresh(PostData post)
        {
            TbName.Text = post.actor.display_name;
            var timestampString = StoryApi.Utils.GetTimeString(post.created_at);
            TbTime.Text = timestampString;
            PpUser.ProfilePicture = Utils.GenerateImageUrlSource(post.actor.profile_video_url_square_small ?? post.actor.profile_thumbnail_url);

            if((post.media?.Count ?? 0) > 0)
                Utils.SetMediaContent(post.media, FvMedia);

            Utils.SetTextContent(post.content_decorators, TbContent);
        }

        private void OnDotMenuTapped(object sender, TappedRoutedEventArgs e)
        {
            var fontawesome = sender as FontAwesome;
            var flyout = new MenuFlyout();
            var menuAddFavorite = new MenuFlyoutItem() { Text = post.bookmarked ? "관심글 삭제하기" : "관심글로 저장하기" };
            menuAddFavorite.Click += (o,e2) =>
            {
                OnAddBookmarkTapped(null, null);
            };
            flyout.Items.Add(menuAddFavorite);
            flyout.Items.Add(new MenuFlyoutSeparator() );
            var menuHidePost = new MenuFlyoutItem() { Text = "이 글 숨기기" };
            menuHidePost.Click += async (o, e2) =>
            {
                await StoryApi.ApiHandler.HidePost(post.id);
                Pages.TimelinePage.HidePostFromTimeline(this);
                Pages.MainPage.HideOverlay();
            };
            flyout.Items.Add(menuHidePost);
            var menuBlockProfile = new MenuFlyoutItem() { Text = post.actor.is_feed_blocked ? $"'{post.actor.display_name}' 글 받기" : $"'{post.actor.display_name}' 글 안받기" };
            menuBlockProfile.Click += async (o, e2) =>
            {
                await StoryApi.ApiHandler.BlockProfile(post.actor.id, post.actor.is_feed_blocked);
                await RefreshPost();
            };
            flyout.Items.Add(menuBlockProfile);
            var menuMutePost = new MenuFlyoutItem() { Text = post.push_mute ? "이 글 알림 받기" : "이 글 알림 받지 않기" };
            menuMutePost.Click += async (o, e2) =>
            {
                await StoryApi.ApiHandler.MutePost(post.id, !post.push_mute);
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
                var isBookmarked = post.bookmarked;
                await StoryApi.ApiHandler.PinPost(post.id, isBookmarked);
                isBookmarking = false;
                await RefreshPost();
                RefreshBookmarkButton();
            }
        }

        private void OnCommentAttatchmentTapped(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void OnAttachPhotoPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var fontawesome = sender as FontAwesome;
            fontawesome.Foreground = Utils.GetColorFromHexa("#FFB6B6B6");
        }

        private void OnAttachPhotoPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var fontawesome = sender as FontAwesome;
            fontawesome.Foreground = Utils.GetColorFromHexa("#FFDDDDDD");
        }

        private async void OnUpButtonClicked(object sender, RoutedEventArgs e)
        {
            var isUp = post.sympathized;
            BtUp.IsEnabled = false;
            await StoryApi.ApiHandler.UpPost(post.id, isUp);
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
                var images = post.media.Where(x => x.content_type != "video/mp4").ToList();
                var control = new ImageViewerControl(images, index);
                Pages.MainPage.ShowOverlay(control, _isOverlay);
            }
        }

        private void TimeTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (!_isOverlay)
                Pages.MainPage.ShowOverlay(new TimelineControl(post, false, true));
        }

        private void PointerEnteredShowHand(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(true);

        private void PointerExitedShowHand(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(false);

        private void CloseButtonClicked(object sender, TappedRoutedEventArgs e) => Pages.MainPage.HideOverlay();

        private void OnUserProfilePictureTapped(object sender, TappedRoutedEventArgs e)
        {
            Pages.MainPage.HideOverlay();
            Pages.MainPage.ShowProfile(post.actor.id);
        }
    }
}