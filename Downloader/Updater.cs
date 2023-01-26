using System;
using System.Threading.Tasks;
using Octokit;

namespace Downloader;

/// <summary>
/// Class to update Stereux taking info from GitHub
/// </summary>
public class Updater
{
    // Thanks to https://stackoverflow.com/a/65029587/11756870, I just removed some comments and adapt it to
    // this program
    /// <summary>
    /// Checks updates from GitHub
    /// </summary>
    /// <param name="currentVersion">The current version.</param>
    /// <returns><![CDATA[Task<bool>]]> that indicates if there's a new version</returns>
    public static bool CheckUpdates(string currentVersion)
    {
        var client = new GitHubClient(new ProductHeaderValue("Stereux"));
        var release = client.Repository.Release.GetAll("LuisAlfredo92", "Stereux").Result[0];

        Version latestGitHubVersion = new(release.TagName), localVersion = new(currentVersion);

        return localVersion.CompareTo(latestGitHubVersion) < 0;
        // TODO: I could download the .exe file itself with an HttpClient like songs
    }
}