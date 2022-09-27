using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Connections.Models;
using HtmlAgilityPack;

namespace Connections.Controllers
{
    /// <summary>
    /// Class that makes all the communication with the NCS web page
    /// </summary>
    public class Ncs : IGetSongs
    {
        private readonly HtmlWeb _htmlWeb;
        private HtmlNode _musicPage;
        private HtmlNode _songsDiv;
        private readonly int _maxPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ncs"/> class.
        /// This automatically takes all the necessary data from the Internet to get the songs info
        /// </summary>
        public Ncs()
        {
            _htmlWeb = new HtmlWeb();
            _musicPage = _htmlWeb.Load("https://ncs.io/music?page=1").DocumentNode;
            _songsDiv = _musicPage.SelectNodes("//div [contains(@class, 'row')]").First();
            var pages = _musicPage.SelectNodes("//li [contains(@class, 'page-item')]");
            _maxPage = Convert.ToInt32(pages.ElementAt(pages.Count - 2).InnerText);
        }

        /// <summary>
        /// Gets <b>all</b> the songs from the NCS web page and returns them
        /// </summary>
        /// <returns><![CDATA[Task<List<Song>?>]]></returns>
        public async Task<List<Song>?> GetSongs()
        {
            List<Song> songs = new();
            for (var i = 1; i <= _maxPage; i++)
            {
                _musicPage = _htmlWeb.Load($"https://ncs.io/music?page={i}").DocumentNode;
                _songsDiv = _musicPage.SelectNodes("//div [contains(@class, 'row')]").First();

                // Save every song into the list
                songs.AddRange(GetSongsDivs(_songsDiv).Select(GetSong));

                // This sleep is to avoid a block from the NCS web page
                /* Is this necessary? I have to check how many requests
                 I can do before getting blocked, they won't ever be more than 100
                 */
                Thread.Sleep(new Random().Next(1000, 2001) * 5);
            }
            return songs;
        }

        /// <summary>
        /// Gets the songs divs.
        /// </summary>
        /// <param name="songsDiv">The songs div.</param>
        /// <returns>A HtmlNodeCollection with the HTML divs with the songs.</returns>
        private static HtmlNodeCollection GetSongsDivs(HtmlNode songsDiv)
            => songsDiv.SelectNodes("//div [contains(@class, 'col-lg-2 item')]");

        /// <summary>
        /// Gets only <b>one</b> song /its info) from one div
        /// </summary>
        /// <param name="completeSongInfo">The div that contains the complete song info</param>
        /// <returns>A Song object</returns>
        private static Song GetSong(HtmlNode completeSongInfo)
        {
            HtmlNode songInfo = completeSongInfo.SelectSingleNode("a"),
                bottomSongInfo = songInfo.SelectNodes("div [contains(@class, 'bottom')]").First();

            string songLink = "https://ncs.io" + songInfo.Attributes["href"].Value,
                songImageStyle = songInfo.SelectNodes("div [contains(@class, 'inner')]/div [contains(@class, 'img')]")
                    .First()
                    .Attributes["Style"]
                    .Value,
                songImageLink = songImageStyle.Substring(
                    songImageStyle.IndexOf("'", StringComparison.Ordinal) + 1,
                    songImageStyle.LastIndexOf("'", StringComparison.Ordinal) - songImageStyle.IndexOf("'", StringComparison.Ordinal)
                ),
                songName = bottomSongInfo.SelectSingleNode("p/strong").InnerText.Replace("&#039;", "'"),
                songArtists = bottomSongInfo.SelectSingleNode("span").InnerText,
                songGenre = completeSongInfo
                    .SelectNodes(
                        "div [contains(@class, 'options')]/div [contains(@class, 'row align-items-center')]/div [contains(@class, 'col-6 col-lg-6')]/span/strong")
                    .First()
                    .InnerText,
                songDataLink = completeSongInfo
                    .SelectNodes(
                        "div [contains(@class, 'options')]/div [contains(@class, 'row align-items-center')]/div [contains(@class, 'col-6 col-lg-6')]")
                    .Last()
                    .SelectNodes("a")
                    .First()
                    .Attributes["data-url"]
                    .Value;
            return new Song(Sources.Ncs, songName, songArtists, songImageLink, songGenre, songLink, songDataLink, null, null, null);
        }
    }
}