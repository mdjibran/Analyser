using Analyser.Models.AnalyserBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;

namespace Analyser.Controllers
{
    public partial class AnalyserController 
    {
        private void removeFromTemp(string word)
        {
            List<string> currentTemp = new List<string>();

            using (StreamReader temp = new StreamReader(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\temp.csv"))
            {
                currentTemp = temp.ReadToEnd().Split(',').ToList();                
                currentTemp.Remove(word);                
            }

            using (StreamWriter sw = new StreamWriter(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\temp.csv"))
            {
                foreach (var item in currentTemp)
                {
                    if(item!="" && item!=null)
                        sw.Write(item + ",");
                }
            }
        }

        public ActionResult AddTechnology(string word)
        {
            using (StreamWriter sw = new StreamWriter(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\Technical.csv", true))
            {
                sw.Write(word + ",");
            }
            removeFromTemp(word);
            return RedirectToAction("saveNewWords");
        }

        public ActionResult AddLanguage(string word)
        {
            using (StreamWriter sw = new StreamWriter(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\Language.csv", true))
            {
                sw.Write(word + ",");
            }
            removeFromTemp(word);
            return RedirectToAction("saveNewWords");
        }

        public ActionResult AddOthers(string word)
        {
            using (StreamWriter sw = new StreamWriter(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\Others.csv", true))
            {
                sw.Write(word + ",");
            }
            removeFromTemp(word);
            return RedirectToAction("saveNewWords");
        }

        public ActionResult AddScrap(string word)
        {
            using (StreamWriter sw = new StreamWriter(@"D:\Files\Works\C#\Analyser\Analyser\Analyser\Files\Scrap.csv", true))
            {
                sw.Write(word + ",");
            }
            removeFromTemp(word);
            return RedirectToAction("saveNewWords");
        }
    }
}