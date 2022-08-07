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
using static StoryApi.ApiHandler.DataType.CommentData;

namespace KSP_WINUI2.Controls;

public sealed partial class CommentControl : UserControl
{
    public delegate void ReplyClick(Comment comment);
    public ReplyClick OnReplyClick;
    private Comment _comment;
    private readonly string _postId;
    private readonly bool _isOverlay;

    public CommentControl(Comment comment, string postId, bool isOverlay)
    {
        this.InitializeComponent();
        _comment = comment;
        _postId = postId;
        _isOverlay = isOverlay;
        Refresh(comment);
    }

    private void Refresh(Comment comment)
    {
        RtbContent.Blocks.Clear();
        TbName.Text = comment.writer.display_name;
        TbTime.Text = StoryApi.Utils.GetTimeString(comment.created_at) + (comment.updated_at.Year > 1 ? " (수정됨)" : "");
        PpUser.ProfilePicture = Utils.GenerateImageUrlSource(comment.writer.profile_video_url_square_small ?? comment.writer.profile_thumbnail_url);
        if (comment.like_count == 0)
        {
            FaHeart.Visibility = Visibility.Collapsed;
            SpLike.Visibility = Visibility.Collapsed;
        }
        else
        {
            FaHeart.Visibility = Visibility.Visible;
            SpLike.Visibility = Visibility.Visible;
            TbLike.Text = comment.like_count.ToString();
        }
        Utils.SetTextContent(comment.decorators, RtbContent);
        var commentMedia = comment.decorators.FirstOrDefault(x => x.media?.origin_url != null);
        if (!string.IsNullOrEmpty(commentMedia?.media?.origin_url))
        {
            ImgMain.Visibility = Visibility.Visible;
            ImgMain.Source = Utils.GenerateImageUrlSource(commentMedia.media.origin_url);
            ImgMain.Tapped += (s, e) =>
            {
                var medium = new Medium
                {
                    origin_url = commentMedia?.media?.origin_url
                };
                var control = new ImageViewerControl(new List<Medium> { medium }, 0);
                Pages.MainPage.ShowOverlay(control, _isOverlay);
            };
        }
        else
            ImgMain.Visibility = Visibility.Collapsed;
    }

    private async void OnLikeButtonClick(object sender, RoutedEventArgs e)
    {
        var isSuccess = await StoryApi.ApiHandler.LikeComment(_postId, _comment.id, _comment.liked);
        if (isSuccess)
        {
            _comment = (await StoryApi.ApiHandler.GetPost(_postId)).comments.FirstOrDefault(x => x.id == _comment.id);
            Refresh(_comment);
        }
    }

    private void OnReplyTapped(object sender, TappedRoutedEventArgs e) => OnReplyClick.Invoke(_comment);
    private void OnPointerEntered(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(true);
    private void OnPointerExited(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(false);
}
