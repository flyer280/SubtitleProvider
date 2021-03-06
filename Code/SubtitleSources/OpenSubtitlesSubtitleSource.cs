using System.Collections;
using System.Collections.Generic;
using CookComputing.XmlRpc;
using MediaBrowser.Library.Entities;
using SubtitleProvider.ExtensionMethods;

namespace SubtitleProvider
{
    public class OpenSubtitlesSubtitleSource : SubtitleSourceBase
    {
        #region Private Members

        private IOpenSubtitles proxy;
        
        #endregion

        #region Implementations

        protected override IEnumerable<Subtitle> Find(Video video, List<string> languages)
        {
            var videoHashString = video.GetVideoHashString();
            var videoSize = video.GetVideoSize();

            var subtitleCollection = this.FindSubtitlesByHash(video, videoHashString, videoSize, languages);

            return subtitleCollection;
        }

        #endregion

        #region Private Methods

        private IEnumerable<Subtitle> FindSubtitlesByHash(Video video, string videoHash, long videoSize, List<string> languages)
        {

            var token = LoginToOpenSubtitles();

            var searchArray = CreateSearchArray(languages, videoHash, videoSize);

            var subtitles = GetSubtitles(token, searchArray);

            var subtitleCollection = CreateSubtitlesFromReturnData(languages, subtitles, video);

            LogOutFromOpenSubtitles(token);

            return subtitleCollection;
        }

        private void LogOutFromOpenSubtitles(string token)
        {
            proxy.Logout(token);
        }

        private IEnumerable<Subtitle> CreateSubtitlesFromReturnData(IEnumerable<string> languages, XmlRpcStruct subtitles, Video video)
        {

            if (subtitles == null || subtitles.Count < 1)
                return null;

            var data = subtitles["data"] as object[];

            if (data == null)
                return null;

            var subtitleCollection = new List<Subtitle>();
            foreach (Hashtable subtitleStructure in data)
            {
                var subtitleLanguage = (string)subtitleStructure["LanguageName"];

                var subtitle = new Subtitle(video.Name, subtitleLanguage, (string)subtitleStructure["ZipDownloadLink"]);
                subtitleCollection.Add(subtitle);

            }

            return subtitleCollection;
        }

        private XmlRpcStruct GetSubtitles(string token, List<XmlRpcStruct> searchArray)
        {
            return proxy.SearchSubtitles(token, searchArray.ToArray());
        }

        private string LoginToOpenSubtitles()
        {
            if (proxy == null)
                proxy = XmlRpcProxyGen.Create<IOpenSubtitles>();

            var loginInfo = proxy.Login("", "", "en", "SubtitleProvider for Media Browser v0.2");

            return (string)loginInfo["token"];
        }

        private List<XmlRpcStruct> CreateSearchArray(List<string> languages, string hashString, long videoSize)
        {
            var searchArray = new List<XmlRpcStruct>();

            var searchStructure = CreateSearchStructure(languages, hashString, videoSize);

            searchArray.Add(searchStructure);
            return searchArray;
        }

        private XmlRpcStruct CreateSearchStructure(List<string> languages, string hashString, long videoSize)
        {
            var searchStructure = new XmlRpcStruct();
            var preferredLanguages = this.GetSubtitleString(languages);
            searchStructure.Add("sublanguageid", preferredLanguages);
            searchStructure.Add("moviehash", hashString);
            searchStructure.Add("moviebytesize", videoSize.ToString());
            searchStructure.Add("imdbid", "");
            return searchStructure;
        }

        private string GetSubtitleString(IEnumerable<string> languages)
        {

            var languageCodes = new List<string>();

            foreach (var language in languages)
            {
                if (language.ToLower() == "english")
                    languageCodes.Add("eng");

                if (language.ToLower() == "swedish")
                    languageCodes.Add("swe");

                if (language.ToLower() == "finnish")
                    languageCodes.Add("fin");

                if (language.ToLower() == "spanish")
                    languageCodes.Add("spa");

                if (language.ToLower() == "icelandic")
                    languageCodes.Add("ice");

                if (language.ToLower() == "danish")
                    languageCodes.Add("dan");

                if (language.ToLower() == "french")
                    languageCodes.Add("fre");

                if (language.ToLower() == "norwegian")
                    languageCodes.Add("nor");

                if (language.ToLower() == "dutch")
                    languageCodes.Add("dut");
            }

            var languageString = languageCodes.BuildString(",");

            return languageString;
        }

        #endregion
    }

    [XmlRpcUrl("http://api.opensubtitles.org/xml-rpc")]
    public interface IOpenSubtitles
    {

        [XmlRpcMethod("ServerInfo")]
        XmlRpcStruct ServerInfo();

        [XmlRpcMethod("LogIn")]
        XmlRpcStruct Login(string username, string password, string language, string useragent);

        [XmlRpcMethod("LogOut")]
        XmlRpcStruct Logout(string token);

        [XmlRpcMethod("SearchSubtitles")]
        XmlRpcStruct SearchSubtitles(string token, XmlRpcStruct[] searchStructure);

    }
}