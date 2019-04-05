using Analyser.Models.AnalyserBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using Analyser.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Analyser.Controllers
{
    public partial class AnalyserController : Controller
    {
        string AllText = "";
        List<string> currentLanguage = new List<string>();
        List<string> currentTechnical = new List<string>();
        List<string> currentOthers = new List<string>();
        List<string> currentScraps = new List<string>();
        List<string> currentTemps = new List<string>();
        List<TrainModel> postings = new List<TrainModel>();
        string city = "Toronto";
        string provience = "ON";
        string Jobtitle = "Big Data Developer";

        public ActionResult TrainAnalyser(TrainModel train)
        {
            string title = Uri.EscapeDataString(Jobtitle);
        
            string location = Uri.EscapeDataString(city +","+ provience);

            string query = title + location;
            string site = "https://ca.indeed.com/";
            string searchTerm = "jobs?q="+title+"&l="+location+"&limit=50&ts=1504132125324&rq=1&fromage=last";
            string url = site + searchTerm;

            var webGet = new HtmlWeb();
            webGet.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36";


            var document = webGet.Load(url);
            int count = 1;
            WorkOnCurrentPage(document, site);
            var pagination = document.DocumentNode.SelectNodes("//div[@class='pagination']/a[@href]").ToArray();
            string curPage = "";

            foreach (var page in pagination)
            {
                count++;
                curPage = page.GetAttributeValue("href", string.Empty);
                document = webGet.Load(site + curPage);
                WorkOnCurrentPage(document, site);
            }

            return View(postings);
        }

        private void WorkOnCurrentPage(HtmlDocument document, string site)
        {
            string link = "";

            // Select URLs
            var urls = document.DocumentNode.SelectNodes("//h2/a[@href]").ToArray();
            var titles = document.DocumentNode.SelectNodes("//h2/a").ToArray();
            var compayNames = document.DocumentNode.SelectNodes("//span[@class='company']/span").ToArray();
            int i = 0;

            foreach (var item in urls)
            {
                link = item.GetAttributeValue("href", string.Empty);
                if ((!link.StartsWith(site)) && (!link.Contains("www.")))
                {
                    string fullLink = site + link;
                    string desc = getDescription(fullLink);


                    postings.Add(new TrainModel { Url = GetDestinationURL(fullLink), Title = titles[i].InnerText, CompanyName = compayNames[i].InnerText, Description = desc });
                    AllText += desc;
                }
                else
                {
                    string desc = getDescription(link);
                    postings.Add(new TrainModel { Url = GetDestinationURL(link), Title = titles[i].InnerText, CompanyName = compayNames[i].InnerText, Description = desc });
                    AllText += desc;
                }
                i++;
            }

            // Get Sponsor links
            //var urls = document.DocumentNode.SelectNodes("//div[contains(@class, 'row') and contains(@class, 'result')]/a[@href]").ToArray();
            //var titles = document.DocumentNode.SelectNodes("//div[contains(@class, 'row') and contains(@class, 'result')]/a").ToArray();
            var ScompayNames = document.DocumentNode.SelectNodes("//div[contains(@class, 'row') and contains(@class, 'result')]/div[contains(@class, 'sjcl')]/span[@class='company']").ToArray();

            for (int j = 1; j <= ScompayNames.Length; j++)
            {
                string id = "sja" + j;
                var sUrl = document.GetElementbyId(id).GetAttributeValue("href", "");
                var sTitle = document.GetElementbyId(id).InnerText;

                if ((!sUrl.StartsWith(site)) && (!sUrl.Contains("www.")))
                {
                    string fullLink = site + sUrl;
                    string desc = getDescription(fullLink);
                    postings.Add(new TrainModel { Url = GetDestinationURL(fullLink), Title = sTitle, CompanyName = ScompayNames[j - 1].InnerText, Description = desc });
                    AllText += desc;
                }
                else
                {
                    string desc = getDescription(sUrl);
                    postings.Add(new TrainModel { Url = GetDestinationURL(sUrl), Title = sTitle, CompanyName = ScompayNames[j - 1].InnerText, Description = desc });
                    AllText += desc;
                }
            }

            SavePostings(postings);
            TeachAnalyser();
        }

        private string GetDestinationURL(string fullLink)
        {
            string result = "";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(fullLink);
                HttpWebResponse myResp = (HttpWebResponse)req.GetResponse();
                result = myResp.ResponseUri.ToString();
            }
            catch(Exception e)
            {
                result = fullLink;
                string msg = e.Message;
            }
            return result;
        }

        private void SavePostings(List<TrainModel> postings)
        {
            DataContext db = new DataContext();
            Description desc;
            foreach (var item in postings)
            {

                int count = 0;
                count = db.Descriptions.Where(x => x.URL == item.Url).Count();
                if (count == 0 && item.Description != "" && item.Description != null)
                {
                    desc = new Description();
                    desc.Id = desc.Create();
                    desc.Text = item.Description;
                    desc.Source = "Indeed.com";
                    desc.URL = item.Url;
                    desc.Company = item.CompanyName;
                    desc.City = city;
                    desc.Provience = provience;
                    desc.Title = item.Title;
                    try
                    {
                        db.Descriptions.Add(desc);
                        db.SaveChanges();
                    }
                    catch(Exception e)
                    {
                        string msg = e.Message;
                    }
                }

                else
                {
                    FailedToGetText failed = new FailedToGetText();
                    failed.URL = item.Url;
                    failed.Company = item.CompanyName;
                    failed.City = city;
                    failed.Provience = provience;
                    failed.Title = item.Title;
                    failed.Id = failed.Create();
                    try
                    {
                        db.FailedToGetTexts.Add(failed);
                        db.SaveChanges();
                    }
                    catch(Exception e)
                    {
                        string msg = e.Message;
                    }
                }
            }
        }


        private string getDescription(string url)
        {
            string text = "";
            try
            {
                var webGet = new HtmlWeb();
                webGet.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36";
                var singlePosting = webGet.Load(url);
                singlePosting.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style")
                .ToList()
                .ForEach(n => n.Remove());
                var uri = webGet.ResponseUri;
                text = SourceSpecificScrapping(singlePosting, uri.ToString());
            }
            catch { return text; }

            return text;
        }

        private string SourceSpecificScrapping(HtmlDocument singlePosting, string url)
        {
            string text = "";
            if(url.Contains("indeed.com"))
            {
                var desc = singlePosting.DocumentNode.SelectNodes("//p").ToArray();
                foreach (var p in desc)
                {
                    text += HttpUtility.HtmlDecode(p.InnerText);
                }
            }
            //if(url.Contains("rbc.com"))
            else
            {
                //var desc = singlePosting.DocumentNode.SelectNodes("//p/span").ToArray();
                //foreach (var p in desc)
                //{
                //    text += HttpUtility.HtmlDecode(p.InnerText);
                //}

                foreach (HtmlNode node in singlePosting.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']"))
                {
                    text += node.InnerText.Trim();
                }

                //text  = Regex.Replace(text, "<.*?>", String.Empty);
            }
            return text;
        }

        public ActionResult TeachAnalyser()
        {
            getCountsOfFileContents();
            List<string> allKeywords = new List<string>();
            allKeywords = currentLanguage.Concat(currentTechnical).Concat(currentOthers).ToList();

                AllText = Regex.Replace(AllText, @"[^0-9a-zA-Z]+", " ");
            List<string> newPostingText = new List<string>();
            newPostingText = AllText.Split(' ').ToList();

            List<string> newWords = new List<string>();
            foreach (string item in newPostingText)
            {
                if (!allKeywords.Contains(item) && (!newWords.Contains(item)))
                    newWords.Add(item);
            }

            using (StreamWriter sw = new StreamWriter(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\temp.csv"))
            {
                foreach (var item in newWords)
                {
                    sw.Write(item.ToLower() + ",");
                }
            }
            ViewBag.NewWordCount = newPostingText.Count();
            ViewBag.UnknownWordCount = newWords.Count();
            return View();
        }

        public ActionResult saveNewWords(TeachAnalyser teach)
        {
            List<string> currentTemp = new List<string>();

            using (StreamReader temp = new StreamReader(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\temp.csv"))
            {
                currentTemp = temp.ReadToEnd().Split(',').ToList();
            }
            ViewBag.Word = currentTemp.FirstOrDefault();
            ViewBag.NextWords = currentTemp.Skip(1).Take(50);

            getCountsOfFileContents();
            return View();
        }

        private void getCountsOfFileContents()
        {
            using (StreamReader sr = new StreamReader(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\Language.csv"))
            {
                currentLanguage = sr.ReadToEnd().Split(',').ToList();
                ViewBag.LanguageCount = currentLanguage.Count();
            }


            using (StreamReader sr = new StreamReader(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\Technical.csv"))
            {
                currentTechnical = sr.ReadToEnd().Split(',').ToList();
                ViewBag.TechnicalCount = currentTechnical.Count();
            }

            using (StreamReader sr = new StreamReader(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\Others.csv"))
            {
                currentOthers = sr.ReadToEnd().Split(',').ToList();
                ViewBag.OtherCount = currentOthers.Count();
            }

            using (StreamReader sr = new StreamReader(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\Scrap.csv"))
            {
                currentScraps = sr.ReadToEnd().Split(',').ToList();
                ViewBag.ScrapCount = currentScraps.Count();
            }


            using (StreamReader sr = new StreamReader(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\temp.csv"))
            {
                currentTemps = sr.ReadToEnd().Split(',').ToList();
                ViewBag.UnknownCount = currentTemps.Count();
            }
        }
    }
}