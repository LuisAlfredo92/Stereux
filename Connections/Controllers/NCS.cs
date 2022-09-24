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
    // TODO: Make this class with its variables static?
    internal class NCS
    {
        private readonly HtmlWeb _htmlWeb;
        private HtmlNode _musicPage;
        private HtmlNode _songsDiv;
        private readonly int _maxPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="NCS"/> class.
        /// This automatically takes all the necessary data from the Internet to get the songs info
        /// </summary>
        public NCS()
        {
            _htmlWeb = new HtmlWeb();
            _musicPage = _htmlWeb.Load("https://ncs.io/music?page=1").DocumentNode;
            _songsDiv = _musicPage.SelectNodes("//div [contains(@class, 'row')]").First();
            HtmlNodeCollection pages = _musicPage.SelectNodes("//li [contains(@class, 'page-item')]");
            _maxPage = Convert.ToInt32(pages.ElementAt(pages.Count - 2).InnerText);
        }

        //TODO: recibir la información asíncronamente
        /// <summary>
        /// Gets <b>all</b> the songs from the NCS web page and saves them into the database
        /// </summary>
        /// <returns><![CDATA[Task<bool>]]></returns>
        public async Task<bool> GetAllSongsAndSave()
        {
            //SongsTableAdapter songTableAdapter = new();
            for (int i = 1; i <= _maxPage; i++)
            {
                _musicPage = _htmlWeb.Load($"https://ncs.io/music?page={i}").DocumentNode;
                _songsDiv = _musicPage.SelectNodes("//div [contains(@class, 'row')]").First();

                // Save every song into the database
                /*foreach (var completeSongInfo in GetSongsDivs(_songsDiv))
                    songTableAdapter.InsertSong(GetSong(completeSongInfo));*/

                // This sleep is to avoid a block from the NCS web page
                Thread.Sleep(new Random().Next(10, 41) * 1000);
            }
            return true;
        }

        /// <summary>
        /// Gets the songs divs.
        /// </summary>
        /// <param name="songsDiv">The songs div.</param>
        /// <returns>A HtmlNodeCollection with the HTML divs with the songs.</returns>
        public static HtmlNodeCollection GetSongsDivs(HtmlNode songsDiv)
        {
            return songsDiv.SelectNodes("//div [contains(@class, 'col-lg-2 item')]");
        }

        /// <summary>
        /// Gets the songs from only one page in the NCS web page
        /// </summary>
        /// <returns><![CDATA[List<Song>]]></returns>
        public List<Song> GetSongs()
        {
            List<Song> songs = new();

            foreach (var completeSongInfo in GetSongsDivs(_songsDiv))
                songs.Add(GetSong(completeSongInfo));

            return songs;
        }

        /// <summary>
        /// Gets only <b>one</b> song /its info) from one div
        /// </summary>
        /// <param name="completeSongInfo">The div that contains the complete song info</param>
        /// <returns>A Song object</returns>
        public Song GetSong(HtmlNode completeSongInfo)
        {
            HtmlNode songInfo = completeSongInfo.SelectSingleNode("a"),
                bottomSongInfo = songInfo.SelectNodes("//div [contains(@class, 'bottom')]").First();

            string songLink = songInfo.Attributes["href"].Value,
                songImageStyle = songInfo.SelectSingleNode("//div [contains(@class, 'img')]").Attributes["Style"].Value,
                songImageLink = songImageStyle.Substring(
                    songImageStyle.IndexOf("'", StringComparison.Ordinal) + 1,
                    songImageStyle.LastIndexOf("'", StringComparison.Ordinal) - songImageStyle.IndexOf("'", StringComparison.Ordinal)
                ),
                songName = bottomSongInfo.SelectSingleNode("p/strong").InnerText,
                songArtists = bottomSongInfo.SelectSingleNode("span").InnerText,
                songGenre = completeSongInfo.SelectSingleNode("//div [contains(@class, 'col-6 col-lg-6')]/span/strong").InnerText,
                songDataLink = completeSongInfo.SelectSingleNode("//a [contains(@class, 'btn black player-play')]").Attributes["data-url"].Value;
            return new Song(0, songName, songArtists, songImageLink, songGenre, songLink, songDataLink, null, null, null);
        }
    }
}