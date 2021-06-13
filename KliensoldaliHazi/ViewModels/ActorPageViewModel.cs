using KliensoldaliHazi.Model;
using KliensoldaliHazi.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace KliensoldaliHazi.ViewModels
{
    class ActorPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<String> ActorData { get; set; } = new ObservableCollection<String>();
        public ObservableCollection<String> ActorMovieCredits { get; set; } = new ObservableCollection<String>();

        private string _searched;
        public string Searched
        {
            get { return _searched; }
            set { Set(ref _searched, value); }
        }

        private string _biography;
        public string Biography
        {
            get { return _biography; }
            set
            {
                if (_biography == value) return;
                _biography = value;
                PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Biography)));
            }
        }

        public async void getData(String searched, String type)  //általános TRAKT függvény az API híváshoz
        {
            ActorData.Clear();
            ActorMovieCredits.Clear();
            Biography = "";

            var baseAddress = new Uri("https://api.trakt.tv/");

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-version", "2");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-key", "d289b4a1f31d7237ee3c2d4e3c0b6e5218a83dff07a127764f68849827b1af63"); // generált egyedi kulcs

                var query = (searched.ToLower()).Replace(" ", "-"); //A nagybetűket kicsire cserélem, a szóközöket kötőjelre cserélem, hogy jó legyen a formátum => bryan-cranston
                if (type == "actorData") // a lekérdezni kívánt adat függvényében más lesz a lekérdezés felépítése
                {
                    query = "people/" + query + "?extended=full"; //Összefűzöm a stringeket, hogy a megfelelő lekérdezést kapjam
                }
                else if(type == "actorMovieCredits")
                {
                    query = "people/" + query +"/movies";
                }

                using (var response = await httpClient.GetAsync(query)) // maga a lekérdezés hívása, ezt egy HttpResponseMessage-be mentem
                {
                    string responseData = await response.Content.ReadAsStringAsync(); // a HttpResponseMessage string-gé alakítása
                    if (type == "actorData")
                        getActorData(responseData);
                    else
                    if (type == "actorMovieCredits")
                        getActorMovieCredits(responseData);
                }
            }
        }

        public void getActorMovieCredits(string responseData) // feltölti a listát, hogy a színész mely filmekben játszott
        {
            ActorMovieCredits.Clear(); // kiürítem a listát
            JObject jobject; // létrehozok egy Json Objectet
            try //Exception kezelés, hogy pl. hibás adat megadása esetén üres Json üzenet érkezik
            {
                jobject = JObject.Parse(responseData); // a kapott JSon választ egy JSon Object-té alakítja
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return;
            }
            JArray cast = (JArray) jobject["cast"];
            foreach (JObject root in cast) // végigmegy a Json Array-en egy Json Object-tel
            {
                JToken character = root["characters"]; // az eredmény egy Json Token
                JToken title = root["movie"]["title"];
                string movieStr = title.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", ""); // a Token string-gé alakítása
                string characterStr = character.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "").Replace("[", "").Replace("]", "");
                ActorMovieCredits.Add("Movie: " + movieStr + "\nCharacter: " + characterStr + "\n"); // a listát egy ListView fogja olvasni, így a string-ek tárolása itt megfelelő
            }
        }

        public void getActorData(string responseData) // feltölti a színész/színésznő adatait tároló listát és property-t
        {
            Actor actor = new Actor();
            JObject root;
            try //Exception kezelés, hogy pl. hibás adat megadása esetén üres Json üzenet érkezik
            {
                root = JObject.Parse(responseData); // a kapott JSon választ egy JSon Object-té alakítja
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                ActorData.Add("No one found with this name.");
                return;
            }
            JToken nameActor = root["name"]; // az eredmény egy Json Token
            JToken birthday = root["birthday"];
            JToken birthplace = root["birthplace"];
            JToken biography = root["biography"];
            JToken instagram = root["social_ids"]["instagram"];

            if(birthday.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "") == "null") // ha üres a születésnap, az azt jelenti, hogy nem talált konkrét embert,
            {                                                                                  // így itt kicsit pontosabb nevet kell megadni
                ActorData.Add("Be more specific with the name.");
                return;
            }

            ActorData.Add("Name: " + (actor.Name = nameActor.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", ""))); // a listához adja, mint string
            ActorData.Add("Birthday: " + (actor.Birthday = birthday.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", ""))); // a visszakapott formátumban voltak idézőjelek, ezek itt nem kellenek
            ActorData.Add("Birthplace: " + (actor.Birthplace = birthplace.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "")));
            if(instagram.ToString(Newtonsoft.Json.Formatting.None) == "null")
            {
                ActorData.Add("Instagram: -"); // ha nincs Instagram
            }
            else
            {
                ActorData.Add("Instagram: @" + (actor.Instagram = instagram.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", "")));
            }
            Biography = ("Biography:\n\n" + (actor.Biography = biography.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", ""))).Replace("\\n\\n", "\n");
        }
    }
}
