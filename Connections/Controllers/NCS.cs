using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Connections.Models;
using HtmlAgilityPack;
using Ookii.Dialogs.Wpf;

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
        private List<Song>? _songs;

        private ProgressDialog _progressDialog = new()
        {
            WindowTitle = "Getting songs from NCS",
            Text = "Getting songs from No Copyright Sounds",
            Description = "Processing...",
            ShowTimeRemaining = true
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Ncs"/> class.
        /// This automatically takes all the necessary data from the Internet to get the songs info
        /// </summary>
        public Ncs()
        {
            _htmlWeb = new HtmlWeb();
            try
            {
                _musicPage = _htmlWeb.Load("https://ncs.io/music?page=1").DocumentNode;
            }
            catch (System.Net.WebException)
            {
                bool error;
                HtmlDocument webPage = new();
                do
                {
                    var result =
                        MessageBox.Show(
                            $"There was an error during connection. You may not be connected to Internet{Environment.NewLine}Try again?",
                            "Error during connection", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result != MessageBoxResult.Yes)
                    {
                        _progressDialog.ReportProgress(100, "Cancelled", "");
                        _progressDialog.Dispose();
                        throw;
                    }

                    try
                    {
                        webPage = _htmlWeb.Load($"https://ncs.io/music?page=1");
                        error = false;
                    }
                    catch
                    {
                        error = true;
                    }
                } while (error);
                _musicPage = webPage.DocumentNode;
            }
            _songsDiv = _musicPage.SelectNodes("//div [contains(@class, 'row')]").First();
            var pages = _musicPage.SelectNodes("//li [contains(@class, 'page-item')]");
            _maxPage = Convert.ToInt32(pages.ElementAt(pages.Count - 2).InnerText);
        }

        /// <summary>
        /// Gets <b>all</b> the songs from the NCS web page and returns them
        /// </summary>
        /// <returns><![CDATA[Task<List<Song>?>]]></returns>
        public List<Song>? GetSongs()
        {
            if (_progressDialog.IsBusy)
            {
                MessageBox.Show("Songs are already being obtained", "Work in progress");
                return null;
            }

            _songs = new List<Song>();
            _progressDialog.DoWork += GetList;
            _progressDialog.Show();
            do
            {
                Thread.Sleep(3000);
            } while (_progressDialog.IsBusy);
            return _songs;
        }

        private void GetList(object? sender, DoWorkEventArgs doWorkEventArgs)
        {
            byte progress = 0;
            for (var i = 1; i <= _maxPage; i++)
            {
                // Report progress to progress dialog
                _progressDialog.ReportProgress(progress,
                    "Getting songs from No Copyright Sounds",
                    $"Progress: {i} of {_maxPage} ({progress}%)");

                try
                {
                    _musicPage = _htmlWeb.Load($"https://ncs.io/music?page={i}").DocumentNode;
                }
                catch (System.Net.WebException)
                {
                    HtmlDocument webPage = new();
                    bool error;
                    do
                    {
                        var result =
                            MessageBox.Show(
                                $"There was an error during connection. You may not be connected to Internet{Environment.NewLine}Try again?",
                                "Error during connection", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        if (result != MessageBoxResult.Yes)
                        {
                            _progressDialog.ReportProgress(100, "Cancelled", "");
                            _progressDialog.Dispose();
                            _songs = null;
                            return;
                        }

                        try
                        {
                            webPage = _htmlWeb.Load($"https://ncs.io/music?page={i}");
                            error = false;
                        }
                        catch
                        {
                            error = true;
                        }
                    } while (error);
                    _musicPage = webPage.DocumentNode;
                }
                _songsDiv = _musicPage.SelectNodes("//div [contains(@class, 'row')]").First();

                // Save every song into the list
                _songs!.AddRange(GetSongsDivs(_songsDiv).Select(GetSong));

                if (_progressDialog.CancellationPending)
                    return;

                // This sleep is to avoid a block from the NCS web page
                /* Is this necessary? I have to check how many requests
                 I can do before getting blocked, they won't ever be more than 100
                 */
                Thread.Sleep(new Random().Next(1000, 2001) * 5);
                progress += (byte)(_maxPage / 100);
            }
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
                    songImageStyle.LastIndexOf("'", StringComparison.Ordinal) - songImageStyle.IndexOf("'", StringComparison.Ordinal) - 1
                ).Trim(),
                songName = bottomSongInfo.SelectSingleNode("p/strong")
                    .InnerText
                    .Replace("&#039;", "'")
                    .Replace("&amp;", "&")
                    .Trim(),
                songArtists = bottomSongInfo.SelectSingleNode("span").InnerText.Trim(),
                songGenre = completeSongInfo
                    .SelectNodes(
                        "div [contains(@class, 'options')]/div [contains(@class, 'row align-items-center')]/div [contains(@class, 'col-6 col-lg-6')]/span/strong")
                    .First()
                    .InnerText
                    .Trim(),
                songDataLink = completeSongInfo
                    .SelectNodes(
                        "div [contains(@class, 'options')]/div [contains(@class, 'row align-items-center')]/div [contains(@class, 'col-6 col-lg-6')]")
                    .Last()
                    .SelectNodes("a")
                    .First()
                    .Attributes["data-url"]
                    .Value
                    .Trim();
            songImageLink = Uri.IsWellFormedUriString(songImageLink, UriKind.Relative)
                ? "https://ncs.io" + songImageLink
                : songImageLink;
            return new Song(Sources.Ncs, songName, songArtists, songImageLink, songGenre, songLink, songDataLink, null, null);
        }
    }
}