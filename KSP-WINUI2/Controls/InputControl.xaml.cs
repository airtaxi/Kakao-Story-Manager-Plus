using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static KSP_WINUI2.ClassManager;
using static StoryApi.ApiHandler.DataType;

namespace KSP_WINUI2.Controls;

public sealed partial class InputControl : UserControl
{
    private string _realString = "";
    private class Quote
    {
        public string type = "text";
        public string id;
        public string text;
    }

    public InputControl(string placeholder = null)
    {
        this.InitializeComponent();
        var control = new FriendListControl();
        PuDropdown.Child = control;
        control.Listener += OnSelected;
        TbxMain.PlaceholderText = placeholder ?? "";
    }

    private void OnSelected(FriendProfile profile)
    {
        if(profile != null)
        {
            PuDropdown.IsOpen = false;
            var text = TbxMain.Text;
            var before = text.Substring(0, text.IndexOf("@"));
            TbxMain.Text = before + "{!{{" + "\"id\":\"" + profile.Id + "\", \"type\":\"profile\", \"text\":\"" + profile.Name + "\"}}!} ";
            TbxMain.Focus(FocusState.Keyboard);
            TbxMain.SelectionStart = TbxMain.Text.Length;
            TbxMain.SelectionLength = 0;
        }
    }

    public TextBox GetTextBox() => TbxMain;
    public void SetWidth(double width) => TbxMain.Width = width;
    public void SetMinHeight(double minHeight) => TbxMain.MinHeight = minHeight;
    public void SetMaxHeight(double maxHeight) => TbxMain.MaxHeight = maxHeight;
    public void AcceptReturn(bool willAllow) => TbxMain.AcceptsReturn = willAllow;
    public void WrapText(bool willWrap) => TbxMain.TextWrapping = willWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;

    public List<QuoteData> GetQuoteDatas() => StoryApi.Utils.GetQuoteDataFromString(TbxMain.Text);
    private void ShowNameSuggestion(string name)
    {
        var friendListControl = PuDropdown.Child as FriendListControl;
        var count = friendListControl.SearchFriendList(name);
        if (PuDropdown.IsOpen && (count == 0 || string.IsNullOrEmpty(name)))
            PuDropdown.IsOpen = false;
        else if (!PuDropdown.IsOpen)
            PuDropdown.IsOpen = true;
    }

    private void TbxMain_TextChanged(object sender, RoutedEventArgs e)
    {
        var text = TbxMain.Text;
        if (text.Contains("@"))
        {
            var name = text.Substring(text.IndexOf("@")+1);
            ShowNameSuggestion(name);
        }
    }

    internal void AppendText(string append)
    {
        TbxMain.Text += append;
    }
}
