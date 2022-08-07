﻿using System;
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

namespace KSP_WINUI2.Controls;

public sealed partial class WritePostControl : UserControl
{
    private readonly InputControl _inputControl;
    private readonly Button _button;

    public WritePostControl(Button button)
    {
        this.InitializeComponent();
        _inputControl = new InputControl();
        _button = button;
        FrInputControl.Content = _inputControl;
        _inputControl.SetMinHeight(200);
        _inputControl.SetWidth(this.Width);
        _inputControl.SetMaxHeight(300);
        _inputControl.AcceptReturn(true);
        _inputControl.WrapText(true);
    }

    private List<string> _permissons = new List<string>
    {
        "A",
        "F",
        "P",
        "M"
    };

    private async void OnWriteButtonClicked(object sender, RoutedEventArgs e)
    {
        var quoteDatas = StoryApi.Utils.GetQuoteDataFromString(_inputControl.GetTextBox().Text);
        PbMain.Visibility = Visibility.Visible;
        await StoryApi.ApiHandler.WritePost(quoteDatas, null, _permissons[CbxPermission.SelectedIndex], true, true, null, null);
        PbMain.Visibility = Visibility.Collapsed;
        _button.Flyout.Hide();
    }
}
