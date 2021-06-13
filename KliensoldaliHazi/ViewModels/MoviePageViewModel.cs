using KliensoldaliHazi.Model;
using KliensoldaliHazi.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using TraktNet;
using TraktNet.Objects.Get.Shows;
using TraktNet.Requests.Parameters;
using TraktNet.Responses;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace KliensoldaliHazi.ViewModels
{
    public class MoviePageViewModel : ViewModelBase, INotifyPropertyChanged
    {

        public ObservableCollection<Movie> PopularMovies { get; set; } = new ObservableCollection<Movie>(); // a jobb oldalt megjelenő lista a népszerű filmekkel
        public static ObservableCollection<Movie> SearchedMovies { get; set; } = new ObservableCollection<Movie>(); // a bal oldalt kereséskor megjelenő lista
        public static ObservableCollection<String> MovieDetails { get; set; } = new ObservableCollection<String>(); // a film részletei, string formátumban

        public event PropertyChangedEventHandler PropertyChanged;

        private string _searched; // a kulcsszó, amire rákerestünk
        public string Searched
        {
            get { return _searched; }
            set { Set(ref _searched, value); }
        }

        private string _overview; // a film leírása, ebből egy paragraph lesz
        public string Overview
        {
            get { return _overview; }
            set
            {
                if (_overview == value) return;
                _overview = value;
                PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Overview)));
            }
        }

        public MoviePageViewModel()
        {
            getData("movies/trending", "popular"); // a lap megnyitásakor megjelennek rögtön a népszerű filmek a jobb oldali listában
        }

        public async void getData(String query, String type) // általános TRAKT függvény az API híváshoz, ebben hívom meg a speciálisabb lekérdező függvényeket
        {
            var baseAddress = new Uri("https://api.trakt.tv/");
            
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-version", "2");

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-key", "d289b4a1f31d7237ee3c2d4e3c0b6e5218a83dff07a127764f68849827b1af63"); // az API használathoz generált kulcs

                if(type == "searched" ) // a lekérdezni kívánt adat típusától függően más a lekérdezés felépítése
                {
                    if(query != null) // előfordulhat, hogy üres a search bar, mikor keresünk
                    {
                        var tempQuery = query;
                        tempQuery = (tempQuery.ToLower()).Replace(" ", "-"); //A nagybetűket kicsire cserélem, a szóközöket kötőjelre cserélem, hogy jó legyen a formátum => mortal-kombat
                        query = "/search/movie?query=" + tempQuery; //Összefűzöm a stringeket, hogy a megfelelő lekérdezést kapjam
                    }
                    else
                    {
                        return;
                    }
                }
                else if(type == "detail")
                {
                    var tempQuery = query;
                    tempQuery = (tempQuery.ToLower()).Replace(" ", "-"); //A nagybetűket kicsire cserélem, a szóközöket kötőjelre cserélem, hogy jó legyen a formátum => mortal-kombat
                    query = "/movies/" + tempQuery + "?extended=full"; //Összefűzöm a stringeket, hogy a megfelelő lekérdezést kapjam
                }

                using (var response = await httpClient.GetAsync(query))
                {
                    

                    string responseData = await response.Content.ReadAsStringAsync(); // maga a lekérdezés hívása, ezt egy HttpResponseMessage - be mentem
                    if (type == "popular")
                    {
                        getPopularMovies(responseData);
                    }
                    else if(type == "searched")
                    {
                        getSearchedMovies(responseData);
                    }
                    else if(type == "detail")
                    {
                        getMovieDetail(responseData);
                    }
                    
                }
            }
        }

        public void getMovieDetail(string responseData) // film részleteit listába menti
        {
            MovieDetails.Clear(); // az előző lekérdezés eredményét törli
            Movie movie = new Movie();
            JObject root;
            try // ha esetleg üres lenne a válasz
            {
                root = JObject.Parse(responseData); // a kapott JSon választ egy JSon Object-té alakítja
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                MovieDetails.Add("No movie found with this title.");
                return;
            }

            JToken title = root["title"]; // a JSon üzenet részei JSon Token-ekbe mentve
            JToken year = root["year"];
            JToken released = root["released"];
            JToken country = root["country"];
            JToken rating = root["rating"];
            JToken overview = root["overview"];
            JToken genres = root["genres"];

            // A kapott adatokat string-ként tárolja egy lista, az idézőjelek törölve vannak belőle
            MovieDetails.Add("Title: " + (movie.Title = title.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "")));
            MovieDetails.Add("Production year: " + (movie.Year = year.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "")));
            MovieDetails.Add("Release date: " + (movie.Released = released.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "")));
            MovieDetails.Add("Country: " + (movie.Country = country.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "")));
            MovieDetails.Add("Rating: " + (movie.Rating = rating.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "").Substring(0, 3)));
            MovieDetails.Add("Genres: " + (movie.Genre = genres.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "").Replace("[", "").Replace("]", "").Replace("-", " ").Replace(",", ", ")));
            Overview = "Overview:\n\n" + overview.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", ""); // ebből egy paragraph lesz, string property-ként van tárolva
        }

        public void getSearchedMovies(string responseData) // a keresett kulcsszó alapján hoz létre listát a filmekből
        {
            SearchedMovies.Clear(); // az előző lekérdezés eredményét törli
            JArray jsonArray;
            try // ha esetleg üres lenne a válasz
            {
                jsonArray = JArray.Parse(responseData); // a kapott JSon választ egy JSon Object-té alakítja
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return;
            }
            foreach (JObject root in jsonArray) // végigmegy a JSon Array tömbön
            {
                Movie movie = new Movie(); // minden alkalommal létrejön egy film objektum, amit a listához fűz
                JToken title = root["movie"]["title"]; // JToken-ekbe menti a kapott választ
                JToken ID = root["movie"]["ids"]["slug"];
                movie.Title = title.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", ""); // a film címe
                movie.SlugID = ID.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", ""); // a film azonosítója (több fajta van, ez a "slug")
                SearchedMovies.Add(movie);
            }
            if(SearchedMovies.Count == 0) // ha nincs találat
            {
                Movie movie = new Movie
                {
                    Title = "No movie found with this title."
                };
                SearchedMovies.Add(movie);
            }
        }

        public void getPopularMovies(string responseData) // a népszerű filmek listáját tölti fel
        {
            PopularMovies.Clear(); // az előző lekérdezés eredményét törli
            JArray jsonArray;
            try // ha esetleg üres lenne a válasz
            {
                jsonArray = JArray.Parse(responseData); // a kapott JSon választ egy JSon Object-té alakítja
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return;
            }
            foreach (JObject root in jsonArray) // végigmegy a JSon Array tömbön
            {
                Movie movie = new Movie(); // minden alkalommal létrejön egy film objektum, amit a listához fűz
                JToken title = root["movie"]["title"]; // JToken-ekbe menti a kapott választ
                movie.Title = title.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", ""); // a film címe, string-gé alakítva
                PopularMovies.Add(movie);
            }
        }
    }
}
