﻿using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static StoryApi.ApiHandler.DataType.CommentData;

namespace KSP_WINUI2.Controls;

public sealed partial class ImageViewerControl : UserControl
{
    public ImageViewerControl(List<Medium> urlList, int index)
    {
        this.InitializeComponent();
        FvImages.ItemsSource = urlList;
        FvImages.SelectedIndex = index;
    }

    private void OnScrollViewerTapped(object sender, TappedRoutedEventArgs e) => ResetImageSize((sender as ScrollViewer).Content as Image, sender as ScrollViewer);

    private void ResetImageSize(Image image, ScrollViewer scrollViewer)
    {
        float heightFactor = (float)scrollViewer.ViewportHeight / (float)image.ActualHeight;
        float widthFactor = (float)scrollViewer.ViewportWidth / (float)image.ActualWidth;
        if(heightFactor < 1)
        {
            scrollViewer.ChangeView(scrollViewer.ScrollableWidth / 2, scrollViewer.ScrollableHeight / 2, heightFactor);
        }
        else if(widthFactor < 1)
        {
            scrollViewer.ChangeView(scrollViewer.ScrollableWidth / 2, scrollViewer.ScrollableHeight / 2, widthFactor);
        }
    }
    private void ImageLoaded(object sender, RoutedEventArgs e)
    {
        var image = sender as Image;
        var scrollViewer = image.Parent as ScrollViewer;
        ResetImageSize(image, scrollViewer);
    }

    private async void OnScrollViewerRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var index = FvImages.SelectedIndex;
        var list = FvImages.ItemsSource as List<Medium>;
        var medium = list[index];
        await Utils.SetImageClipboardFromUrl(medium.origin_url);
    }

    private void CloseButtonPointerEntered(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(true);

    private void CloseButtonPointerExited(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(false);

    private void CloseButtonTapped(object sender, TappedRoutedEventArgs e) => Pages.MainPage.HideOverlay();

    private void FvImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var flipView = (FlipView)sender;
        var item = flipView.ContainerFromItem(flipView.SelectedItem) as FlipViewItem;
        if (item != null)
        {
            var scrollViewer = item.ContentTemplateRoot as ScrollViewer;
            if(scrollViewer != null)
            {
                Console.WriteLine("SV");
                var image = scrollViewer.Content as Image;
                ResetImageSize(image, scrollViewer);
            }
        }
    }
}
