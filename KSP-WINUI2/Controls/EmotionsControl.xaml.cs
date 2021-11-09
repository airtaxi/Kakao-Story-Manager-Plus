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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace KSP_WINUI2.Controls
{
    public sealed partial class EmotionsControl : UserControl
    {
        private readonly PostData post;
        private readonly TimelineControl timeline;
        public EmotionsControl(PostData post, TimelineControl timeline)
        {
            this.InitializeComponent();
            this.post = post;
            this.timeline = timeline;
        }

        private async void OnEmotionButtonClick(object sender, RoutedEventArgs e)
        {
            var emotion = (sender as Button).Tag as string;
            await StoryApi.ApiHandler.LikePost(post.id, emotion);
            await timeline.RefreshPost();
            timeline.RefreshEmotionsButton();
            timeline.HideEmotionsButtonFlyout();
        }
    }
}
