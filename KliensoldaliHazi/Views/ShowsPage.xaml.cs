using KliensoldaliHazi.Model.Show;
using KliensoldaliHazi.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KliensoldaliHazi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShowsPage : Page
    {
        public ShowsPage()
        {
            this.InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPageBind.getData(ShowPageBind.Searched, "searched");
        }

        public void ItemClick(object sender, ItemClickEventArgs e)
        {
            ShowPageBind.getData(((Show)e.ClickedItem).Title, "seasons");
        }

        public void SeasonItem_Click(object sender, ItemClickEventArgs e)
        {
            ShowPageBind.getEpisodes(((Season)e.ClickedItem).SeasonNumber, ((Season)e.ClickedItem).ShowTitle);
        }

        
    }
}
