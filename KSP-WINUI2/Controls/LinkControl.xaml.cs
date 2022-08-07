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
using static StoryApi.ApiHandler.DataType;

namespace KSP_WINUI2.Controls;

public sealed partial class LinkControl : UserControl
{
    private readonly string _url;
    public LinkControl(TimeLineData.Scrap data)
    {
        this.InitializeComponent();
        if ((data.image?.Count ?? 0) > 0)
            ImgLink.Source = Utils.GenerateImageUrlSource(data.image[0]);
        TbLinkTitle.Text = data.title ?? "";
        TbLinkDesc.Text = data.description ?? "";
        TbLinkUrl.Text = data.host ?? "";

        if (string.IsNullOrEmpty(TbLinkTitle.Text))
            TbLinkTitle.Visibility = Visibility.Collapsed;
        if (string.IsNullOrEmpty(TbLinkDesc.Text))
            TbLinkDesc.Visibility = Visibility.Collapsed;
        if (string.IsNullOrEmpty(TbLinkUrl.Text))
            TbLinkUrl.Visibility = Visibility.Collapsed;

        _url = data.url;
        ToolTipService.SetToolTip(TbLinkTitle, data.title);
        ToolTipService.SetToolTip(TbLinkDesc, data.description);
        ToolTipService.SetToolTip(TbLinkUrl, data.url);
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(true);

    private void OnPointerExited(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(false);

    private async void OnTapped(object sender, TappedRoutedEventArgs e) => await Windows.System.Launcher.LaunchUriAsync(new Uri(_url, UriKind.Absolute));
}
