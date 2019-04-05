using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Analyser.Models.AnalyserBaseModels
{
    public class TrainModel
    {
        public string CompanyName { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Provience { get; set; }
    }
}