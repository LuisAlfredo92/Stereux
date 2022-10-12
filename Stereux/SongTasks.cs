using System.Net.Http;

namespace Stereux;

public class SongTasks
{
    public SongTasks(Task<HttpResponseMessage> downloadSongTask, Task<HttpResponseMessage> downloadCoverTask, Task copySongTask, Task copyCoverTask)
    {
        DownloadSongTask = downloadSongTask;
        DownloadCoverTask = downloadCoverTask;
        CopySongTask = copySongTask;
        CopyCoverTask = copyCoverTask;
    }

    public Task<HttpResponseMessage> DownloadSongTask { get; }
    public Task<HttpResponseMessage> DownloadCoverTask { get; }
    public Task CopySongTask { get; }
    public Task CopyCoverTask { get; }
}