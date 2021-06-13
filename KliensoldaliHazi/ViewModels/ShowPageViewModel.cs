using KliensoldaliHazi.Model.Show;
using KliensoldaliHazi.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace KliensoldaliHazi.ViewModels
{
    public class ShowPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ObservableCollection<Show> PopularShows { get; set; } =
            new ObservableCollection<Show>(); // népszerű sorozatok listája, jobb oldalra fog kerülni az oldal betöltésekor

        public static ObservableCollection<Show> SearchedShows { get; set; } =
            new ObservableCollection<Show>(); // azon sorozatok, amiket a keresett kulcsszó alapján találtunk

        public static ObservableCollection<Season> Seasons { get; set; } =
            new ObservableCollection<Season>(); // egy adott sorozat évadjait tárolja
        public static ObservableCollection<Episode> EpisodeList { get; set; } =
            new ObservableCollection<Episode>(); // egy adott sorozat adott évadának epizódjait tárolja

        public static ObservableCollection<String> EpisodesData { get; set; } =
            new ObservableCollection<String>(); // a megjelenítéshez szükséges, az epizódok adatait tárolja szöveges formátumban

        private string _searched; // a kulcsszó, amire rákerestünk
        public string Searched // a kulcsszó property-je
        {
            get { return _searched; }
            set { _searched = value; }
        }

        public ShowPageViewModel()
        {
            getData("shows/trending", "popular"); // az oldal megnyitásakor betöltődnek a népszerű filmek a jobb oldali listába
        }

        public async void getData(String query, String type) //általános TRAKT függvény az API híváshoz
        {
            var baseAddress = new Uri("https://api.trakt.tv/");
            var forSeasons = query;

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-version", "2");

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-key", "d289b4a1f31d7237ee3c2d4e3c0b6e5218a83dff07a127764f68849827b1af63"); // az API használathoz generált kulcs

                if (type == "searched") // a lekérdezés típusától függően máshogy épül fel a lekérdezés
                {
                    //A nagybetűket kicsire cserélem, a szóközöket kötőjelre cserélem, hogy jó legyen a formátum => mortal-kombat
                    //Összefűzöm a stringeket, hogy a megfelelő lekérdezést kapjam
                    query = "/search/show?query=" + (query.ToLower().Replace(" ", "-")); 
                }
                if (type == "seasons")
                {
                    query = "/shows/" + (query.ToLower().Replace(" ", "-")) + "/seasons?extended=full";
                }

                using (var response = await httpClient.GetAsync(query))
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    if(type == "popular")
                    {
                        getShows(query, responseData, PopularShows);
                    }
                    else if(type == "searched")
                    {
                        getShows(query, responseData, SearchedShows);
                    }
                    else if(type == "seasons")
                    {
                        getSeasons(forSeasons, responseData);
                    } 
                }
            }
        }

        public void getShows(String query, string responseData, ObservableCollection<Show> List)
        {
            List.Clear();
            JArray jsonArray;
            try
            {
                jsonArray = JArray.Parse(responseData);
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return;
            }
            foreach (JObject root in jsonArray)
            {
                Show show = new Show();
                JToken title = root["show"]["title"];
                var year = root["show"]["year"];
                show.Title = title.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "");
                List.Add(show);
            }
            if (List.Count == 0)
            {
                Show show = new Show();
                show.Title = "No movie found with this title.";
                List.Add(show);
            }
        }

        public void getSeasons(String query, string responseData) //shows/{id}/seasons/{season}{?translations}
        {
            Seasons.Clear();
            JArray jsonArray;
            try
            {
                jsonArray = JArray.Parse(responseData);
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return;
            }
            foreach (JObject root in jsonArray)
            {
                Season season = new Season();
                JToken number = root["number"];
                JToken rating = root["rating"];
                JToken title = root["title"];
                JToken numberOfEp = root["episode_count"];

                season.ShowTitle = query.ToLower().Replace(" ", "-");
                season.EpisodeCount = numberOfEp.ToString(Newtonsoft.Json.Formatting.None).ToInteger();
                season.Title = title.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "");
                season.SeasonNumber = number.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "").ToInteger() + 1;
                season.Rating = rating.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "");
                Seasons.Add(season);
            }
        }

        public async void getEpisodes(int seasonNumber, string ShowTitle) //shows/game-of-thrones/seasons/1
        {
            EpisodesData.Clear();
            var baseAddress = new Uri("https://api.trakt.tv/");

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-version", "2");

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-key", "d289b4a1f31d7237ee3c2d4e3c0b6e5218a83dff07a127764f68849827b1af63");

                using (var response = await httpClient.GetAsync("shows/" + ShowTitle + "/seasons/" + seasonNumber.ToString() + "?translations=es"))
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    JArray jsonArray;
                    try
                    {
                        jsonArray = JArray.Parse(responseData);
                    }
                    catch (Newtonsoft.Json.JsonReaderException)
                    {
                        return;
                    }
                    foreach (JObject jobject in jsonArray)
                    {
                        Episode ep = new Episode();
                        JToken number = jobject["number"];
                        JToken titleEp = jobject["title"];

                        ep.Title = titleEp.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "");
                        ep.EpisodeNumber = number.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "").ToInteger();
                        EpisodesData.Add(ep.Title);
                    }
                }
            }
        }

        public void NavigateToShows()
        {
            NavigationService.Navigate(typeof(ShowsPage));
        }
    }
}
