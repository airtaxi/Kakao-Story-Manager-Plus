using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KSP_WINUI2.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private static bool _isFirstLogin = true;
        public LoginPage()
        {
            this.InitializeComponent();
            var values = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
            if (values.ContainsKey("Email") && values.ContainsKey("Password"))
            {
                TbxLogin.Text = (string)values["Email"];
                PbxLogin.Password = (string)values["Password"];
                CbxAutoLogin.IsChecked = true;
                if(_isFirstLogin)
                    OnLoginButtonClick(null, null);
            }
            WvMain.NavigationCompleted += OnNavigationCompleted;
        }

        private void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            _isFirstLogin = false;
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (CbxAutoLogin.IsChecked == true)
            {
                if (settings.Values.ContainsKey("Email"))
                    settings.Values["Email"] = TbxLogin.Text;
                else
                    settings.Values.Add("Email", TbxLogin.Text);

                if (settings.Values.ContainsKey("Password"))
                    settings.Values["Password"] = PbxLogin.Password;
                else
                    settings.Values.Add("Password", PbxLogin.Password);
            }
            else
            {
                if (settings.Values.ContainsKey("Email"))
                    settings.Values.Remove("Email");
                if (settings.Values.ContainsKey("Password"))
                    settings.Values.Remove("Password");
            }
            BtLogin.IsEnabled = false;
            PbLogin.Visibility = Visibility.Visible;
            WvMain.Source = new Uri("https://story.kakao.com/s/logout");
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += async (s2, e2) =>
            {
                timer.Stop();
                var cookies = GetBrowserCookie(new Uri("https://story.kakao.com/"));
                var isLoggedIn = cookies.Any(x => x.Name == "_karmt");
                if (BtLogin.IsEnabled == false && !isLoggedIn)
                {
                    await new MessageDialog("로그인 실패", "오류").ShowAsync();
                    WvMain.Visibility = Visibility.Visible;
                }
            };
            timer.Start();
        }

        private bool isFirst = true;
        private async void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                bool wasFirst = isFirst;
                if (isFirst)
                {
                    isFirst = false;
                    await WvMain.InvokeScriptAsync("eval", new string[] { $"document.getElementById(\"id_email_2\").value = \"{TbxLogin.Text}\";" });
                    await WvMain.InvokeScriptAsync("eval", new string[] { $"document.getElementById(\"id_password_3\").value = \"{PbxLogin.Password}\";" });
                    await WvMain.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName(\"btn_g btn_confirm submit\")[0].click();" });
                }
                var cookies = GetBrowserCookie(new Uri("https://story.kakao.com/"));
                var isLoggedIn = cookies.Any(x => x.Name == "_karmt");
                if (isLoggedIn)
                {
                    var cookieContainer = new CookieContainer();
                    foreach (var cookie in cookies)
                    {
                        cookieContainer.Add(new Cookie() { Name = cookie.Name, Value = cookie.Value, Domain = cookie.Domain });
                    }
                    StoryApi.ApiHandler.Init(cookieContainer);
                    Frame.Navigate(typeof(MainPage));
                }
                else if(!wasFirst)
                {
                    await new MessageDialog("로그인에 실패하였습니다.", "오류").ShowAsync();
                    PbLogin.Visibility = Visibility.Collapsed;
                    BtLogin.IsEnabled = true;
                }
            });
        }

        private HttpCookieCollection GetBrowserCookie(Uri targetUri)
        {
            var httpBaseProtocolFilter = new HttpBaseProtocolFilter();
            var cookieManager = httpBaseProtocolFilter.CookieManager;
            return cookieManager.GetCookies(targetUri);
        }
    }
}
