using KliensoldaliHazi.Model;
using KliensoldaliHazi.ViewModels;
using KliensoldaliHazi.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Services.NavigationService;
using TraktNet;
using TraktNet.Objects.Get.Shows;
using TraktNet.Requests.Parameters;
using TraktNet.Responses;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KliensoldaliHazi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
      
        public MainPage()
        {
            InitializeComponent();
            frame.Navigate(typeof(MoviesPage));
        }

        private void MoviesButton_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(MoviesPage));
        }

        private void ShowsButton_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(ShowsPage));
        }

        private void ActorsButton_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(ActorsPage));
        }
    }
}
