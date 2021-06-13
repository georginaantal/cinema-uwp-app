using KliensoldaliHazi.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace KliensoldaliHazi.ViewModels
{
    class MainPageViewModel : ViewModelBase
    {
        public void NavigateToMain()
        {
            NavigationService.Navigate(typeof(MainPage));
        }
    }
}
