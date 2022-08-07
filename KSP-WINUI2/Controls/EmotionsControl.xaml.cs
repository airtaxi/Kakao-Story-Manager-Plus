using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static StoryApi.ApiHandler.DataType.CommentData;

namespace KSP_WINUI2.Controls;

public sealed partial class EmotionsControl : UserControl
{
    private readonly PostData _post;
    private readonly TimelineControl _timeline;
    public EmotionsControl(PostData post, TimelineControl timeline)
    {
        this.InitializeComponent();
        this._post = post;
        this._timeline = timeline;
    }

    private async void OnEmotionButtonClick(object sender, RoutedEventArgs e)
    {
        var emotion = (sender as Button).Tag as string;
        await StoryApi.ApiHandler.LikePost(_post.id, emotion);
        await _timeline.RefreshPost();
        _timeline.RefreshEmotionsButton();
        _timeline.HideEmotionsButtonFlyout();
    }
}
