using System;
using System.Threading.Tasks;
using Octokit;

namespace Downloader
{
    public class Updater
    {
        // Thanks to https://stackoverflow.com/a/65029587/11756870, I just removed some comments and adapt it to
        // this program
        public static async Task<bool> CheckUpdates(string currentVersion)
        {
            var client = new GitHubClient(new ProductHeaderValue("Stereux"));
            var release = await client.Repository.Release.GetLatest("LuisAlfredo92", "Stereux");

            Version latestGitHubVersion = new(release.TagName),
                localVersion = new(currentVersion);

            return localVersion.CompareTo(latestGitHubVersion) < 0;
        }
    }
}