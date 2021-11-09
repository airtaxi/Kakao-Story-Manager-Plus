using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static StoryApi.ApiHandler.DataType;
using static StoryApi.ApiHandler.DataType.CommentData;

namespace KSP_WINUI2
{
    public static class Utils
    {
        public static BitmapImage GenerateImageUrlSource(string url)
        {
            var imageUrl = new Uri(url);
            var bitmap = new BitmapImage();
            bitmap.UriSource = imageUrl;
            return bitmap;
        }
        public static void SetMediaContent(List<Medium> mediums, FlipView flipView)
        {
            var medias = new List<UIElement>();
            foreach (var media in mediums)
            {
                var isVideo = media.content_type == "video/mp4";
                if (isVideo)
                {
                    var videoMedia = new MediaElement();
                    videoMedia.Source = new Uri(media.url_hq);
                    videoMedia.AreTransportControlsEnabled = true;
                    videoMedia.IsMuted = true;
                    videoMedia.AutoPlay = false;
                    videoMedia.RightTapped += async (s, e) =>
                    {
                        await Utils.SetTextClipboard(media.url_hq, "링크가 복사되었습니다.");
                    };
                    medias.Add(videoMedia);
                }
                else
                {
                    var imageMedia = new Image();
                    imageMedia.Stretch = Stretch.UniformToFill;
                    imageMedia.HorizontalAlignment = HorizontalAlignment.Center;
                    imageMedia.VerticalAlignment = VerticalAlignment.Center;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.UriSource = new Uri(media.origin_url);
                    imageMedia.Source = bitmapImage;
                    imageMedia.RightTapped += async (s, e) =>
                    {
                        await Utils.SetImageClipboardFromUrl(media.origin_url);
                    };
                    medias.Add(imageMedia);
                }
            }
            flipView.Visibility = Visibility.Visible;
            flipView.ItemsSource = medias;
        }
        public static void SetTextContent(List<QuoteData> contentDecorators, RichTextBlock richTextBlock)
        {
            var wordCount = 0;
            Paragraph paragraph = new Paragraph();
            foreach (var decorator in contentDecorators)
            {
                if (decorator.type.Equals("profile"))
                {
                    var hyperlink = new Hyperlink();
                    hyperlink.FontWeight = FontWeights.Bold;
                    hyperlink.UnderlineStyle = UnderlineStyle.None;
                    hyperlink.Inlines.Add(new Run { Text = decorator.text });
                    hyperlink.Click += (s, e) =>
                    {
                        Pages.MainPage.HideOverlay();
                        Pages.MainPage.ShowProfile(decorator.id);
                    };
                    paragraph.Inlines.Add(hyperlink);
                }
                else
                {
                    var run = new Run();
                    var text = decorator.text;
                    run.Text = text;
                    if (decorator.type.Equals("hashtag"))
                        run.FontWeight = FontWeights.Bold;
                    paragraph.Inlines.Add(run);
                    wordCount += text.Length;
                }
            }
            richTextBlock.Blocks.Add(paragraph);
            if (wordCount == 0)
                richTextBlock.Visibility = Visibility.Collapsed;
        }
        public static async Task SetTextClipboard(string text, string message = "복사되었습니다.")
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
            if (!string.IsNullOrEmpty(message))
                await new MessageDialog(message, "안내").ShowAsync();
        }
        public static async Task SetImageClipboardFromUrl(string url, string message = "이미지가 클립보드에 저장되었습니다.")
        {
            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(url)));
            Clipboard.SetContent(dataPackage);
            if (!string.IsNullOrEmpty(message))
                await new MessageDialog(message, "안내").ShowAsync();
        }
        public static void ChangeCursor(bool isHand) => Window.Current.CoreWindow.PointerCursor = isHand
                ? new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1)
                : new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        public static SolidColorBrush GetColorFromHexa(string hexaColor)
        {
            return new SolidColorBrush(
                Color.FromArgb(
                    Convert.ToByte(hexaColor.Substring(1, 2), 16),
                    Convert.ToByte(hexaColor.Substring(3, 2), 16),
                    Convert.ToByte(hexaColor.Substring(5, 2), 16),
                    Convert.ToByte(hexaColor.Substring(7, 2), 16)
                )
            );
        }
    }
}
