using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AngleSharp.Dom;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using WebApplication8.Models;


namespace WebApplication8.Controllers
{
    public class homeController : Controller
    {
        ModelResults results = new ModelResults();
        bool k;
        // GET: home
        public ActionResult Index()
        {
            string[] values = new string[] { @"Calculate number of links in the text",
                "Calculate number of occurrences on the page of each word",
            "Calculate number of occurrences on the page of each word listed in meta tags",
                "Filter out stop-words (e.g. ‘or’, ‘and’, ‘a’, ‘the’ etc) from results" };
            return View(values);
        }

        //Post: home
        [HttpPost]
        public ActionResult Textsearch(string text, string[] checkedValues)
        {
            if (checkedValues == null)
                checkedValues = new string[] {""};
            foreach (string tr in checkedValues)
            {
                if (tr == "Calculate number of links in the text")
                    Counthref(text);
                if (tr == "Calculate number of occurrences on the page of each word")
                    FindcountTKinText(text);
                if (tr == "Calculate number of occurrences on the page of each word listed in meta tags")
                    FindcountkeyWinText(text);
                if (tr == "Filter out stop-words (e.g. ‘or’, ‘and’, ‘a’, ‘the’ etc) from results")
                    k = true;
                else k = false;
            }
            if (ViewBag.Error=="404")
            {
                ViewBag.Error = "Wrong address, please re-enter";
                return PartialView("Partial", results);
            }

            return PartialView("Partial",results);
        }

        string Stopwords(string page)
        {
            string keywords = "";
            string[] textstop = new string[] {"a","the", "or", "and"};
            List<string> textarr = keywords.Split(new char[] {' '}).ToList();
            foreach (string t in textarr.ToArray())
            {
                foreach (string m in textstop)
                {
                    if (t == m)
                        textarr.Remove(m);
                }
            }
            keywords = "";
            foreach (string w in textarr)
                keywords = keywords + w +" ";

            return keywords;
        }//This method delete stop-words in keywords

        string Gethtmlcode(string address)
        {
            HttpWebRequest req;
            HttpWebResponse resp;
            StreamReader sr;
            string content = "";

            if (!address.Contains("http://"))
                String.Concat("http://", address);
            try
            {
                req = (HttpWebRequest)WebRequest.Create(address);

                resp = (HttpWebResponse)req.GetResponse();

                sr = new StreamReader(resp.GetResponseStream(), Encoding.Default);
                content = sr.ReadToEnd();
                sr.Close();

                IHtmlDocument angle = new HtmlParser().Parse(content);

                foreach (IElement element in angle.QuerySelectorAll("meta"))
                {
                    try
                    {
                        if (element.GetAttribute("content").Contains("charset="))
                            content = element.GetAttribute("content");
                    }
                    catch
                    {
                        content = element.GetAttribute("charset");
                    }
                }

                var re = new Regex(".*?charset=");
                content = re.Replace(content, "");

                req = (HttpWebRequest)WebRequest.Create(address);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(content));
                content = "";
                content = sr.ReadToEnd();
                sr.Close();

                return content;
            }
            catch
            {
                ViewBag.Error = "404";
                return ViewBag.Error;
            }

        }// This method get html code from page

        void Counthref(string address)
        {

            string content = Gethtmlcode(address);

            List<string> hrefTags = new List<string>();
            IHtmlDocument angle = new HtmlParser().Parse(content);
            foreach (IElement element in angle.QuerySelectorAll("a"))
            {
               hrefTags.Add(element.GetAttribute("href"));
            }
             results.Hrefcount = hrefTags.Count;
        }//This method calculate links on page

        string Keywords(string page)
        {
            string keywords = "";
            string content = Gethtmlcode(page);
            List<string> metaTags = new List<string>();
            IHtmlDocument angle = new HtmlParser().Parse(content);
            foreach (IElement element in angle.QuerySelectorAll("meta"))
            {
                if ((element.GetAttribute("name") == "Keywords") || (element.GetAttribute("name") == "keywords"))
                {
                    metaTags.Add(element.GetAttribute("content"));
                }
            }
            foreach (string key in metaTags)
            {
                keywords = keywords + key;
            }
            string[] array = keywords.Split(new char[] { ',', '.',';',' ' });
            keywords = "";
            foreach (string a in array)
                keywords = keywords + a + " ";
            if(k)
              keywords = Stopwords(keywords);
            return keywords;
        }// This method find keywords on page

        string GettextfromHtml(string page)
        {
            string content = Gethtmlcode(page);
            List<string> metaTags = new List<string>();
            IHtmlDocument angle = new HtmlParser().Parse(content);
            foreach (IElement element in angle.QuerySelectorAll("p"))
            {              
                    metaTags.Add(element.TextContent);
            }
            content = "";
            foreach (string a in metaTags)
            {
                content = content + a;             
            }

            var re = new Regex("<.*?>");
            content = re.Replace(content, " ");
            return content;
        }// This method parse html and get text
        void FindcountkeyWinText(string page)
        {
            List<string> textarr = GettextfromHtml(page).Split(new char[] { ' ' }).ToList();
            List<string> keyarr = Keywords(page).Split(new char[] { ' ' }).ToList();
            KeyIndex _keyindex;
            foreach (string k in textarr.ToArray())
                if (k == "")
                    textarr.Remove(k);

            foreach (string k in keyarr.ToArray())
                if (k == "")
                    keyarr.Remove(k);

            int index = 0;
            foreach (string k in keyarr.ToArray())
            {
                _keyindex = new KeyIndex();
                foreach (string t in textarr)
                {
                    if (t == k)
                        index++;
                                       
                }
                _keyindex.Count = index;
                _keyindex.Word = k;
                results.FindKeywordsConsid.Add(_keyindex);
                index = 0;
                keyarr.Remove(k);
            }
        }// This method find compare between keywords and text on page
        void FindcountTKinText(string page)
        {
            List<string> textarr = GettextfromHtml(page).Split(new char[] { ' ' }).ToList();
            string keywords = "";
            KeyIndex _keyindex;
            foreach (string key in textarr)
            {
                keywords = keywords + key+" ";
            }
            List<string> array = keywords.Split(new char[] { ',', '.', ';',' ','!','?' }).ToList();
            foreach (string k in textarr.ToArray())
                if (k == "")
                    textarr.Remove(k);

            foreach (string k in array.ToArray())
                if (k == "")
                    array.Remove(k);

            int index = 0;
            foreach (string k in array.ToArray())
            {
                _keyindex = new KeyIndex();
                foreach (string t in textarr)
                {
                    if (t == k)
                        index++;
                }
                _keyindex.Count = index;
                _keyindex.Word = k;
                results.FindTextconsid.Add(_keyindex);
                array.Remove(k);
                index = 0;
            }

        }// This method find how many times replays words in text
    }
}