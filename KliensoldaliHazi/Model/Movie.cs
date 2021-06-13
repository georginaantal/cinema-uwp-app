using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraktNet.Objects.Get.Movies;

namespace KliensoldaliHazi.Model
{
    public class Movie
    {
        public String SlugID { get; set; }
        public String Title { get; set; }
        public String Year { get; set; }
        public String Rating { get; set; }
        public String Released { get; set; }
        public String Country { get; set; }
        public String Overview { get; set; }
        public String Genre { get; set; }

    }
}
