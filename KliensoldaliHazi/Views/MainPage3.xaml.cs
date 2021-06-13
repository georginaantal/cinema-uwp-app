using KliensoldaliHazi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        public Movie Movie { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            var client = new TraktClient("d289b4a1f31d7237ee3c2d4e3c0b6e5218a83dff07a127764f68849827b1af63");
            getShow(client);

          
            //DataContext = Movie;
        }

        public async void getShow(TraktClient client)
        {
            TraktResponse<ITraktShow> response = await client.Shows.GetShowAsync("game-of-thrones", new TraktExtendedInfo().SetFull());

            if (response)
            {
                ITraktShow show = response.Value;
                Console.WriteLine($"Title: {show.Title} / Year: {show.Year}");
                Console.WriteLine(show.Overview);
            }

            DataContext = response.Value.Title;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
