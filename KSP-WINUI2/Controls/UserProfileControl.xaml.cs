using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KSP_WINUI2.Controls;

public sealed partial class UserProfileControl : UserControl
{
    private readonly string _id;

    public UserProfileControl(string id)
    {
        this.InitializeComponent();
        _id = id;
        _ = Refresh();
    }

    public void IndicateFavorite(bool isFavorite)
    {
        if(!isFavorite)
        {
            RtFavorite.Fill = Utils.GetColorFromHexa("#FF808080");
            FaFavorite.Foreground = Utils.GetColorFromHexa("#FFD3D3D3");
        }
        else
        {
            RtFavorite.Fill = Utils.GetColorFromHexa("#FFD15F4E");
            FaFavorite.Foreground = Utils.GetColorFromHexa("#FFD7D7D7");
        }
    }

    public async Task SetFavorite()
    {
        var relationship = await StoryApi.ApiHandler.GetProfileRelationship(_id);
        await StoryApi.ApiHandler.RequestFavorite(_id, relationship.is_favorite);
        relationship = await StoryApi.ApiHandler.GetProfileRelationship(_id);
        IndicateFavorite(relationship.is_favorite);
    }
    public async Task Refresh()
    {
        var user = await StoryApi.ApiHandler.GetProfileFeed(_id, null, true);
        var profile = user.profile;
        PpProfilePicture.ProfilePicture = Utils.GenerateImageUrlSource(user.profile.profile_video_url_square_small ?? user.profile.profile_thumbnail_url);
        ImgProfileBackground.Source = Utils.GenerateImageUrlSource(profile.bg_image_url);
        TbName.Text = profile.display_name;

        if (profile.status_objects?.Count > 0)
            TbDescription.Text = profile.status_objects?[0]?.message ?? "";
        else
            TbDescription.Text = "";

        if (string.IsNullOrEmpty(TbDescription.Text))
            TbDescription.Visibility = Visibility.Collapsed;

        if (user.profile.relationship != "F")
            GdFavorite.Visibility = Visibility.Collapsed;
        else
            IndicateFavorite(profile.is_favorite);
    }

    private async void OnFavoriteTapped(object sender, TappedRoutedEventArgs e) => await SetFavorite();

    private void FavoritePointerEntered(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(true);

    private void FavoritePointerExited(object sender, PointerRoutedEventArgs e) => Utils.ChangeCursor(false);
}
