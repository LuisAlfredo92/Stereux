namespace Connections.Models;

/// <summary>
/// Song class that saves all the necessary data to be shown or used
/// </summary>
public class Song : IEquatable<Song>, IComparable<Song>, IFormattable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Song"/> class.
    /// This constructor uses all the data given by text
    /// </summary>
    /// <param name="source">Source of the song, taken from <see cref="Sources"/></param>
    /// <param name="name">Name or title of the song</param>
    /// <param name="artists">Artist or artists involved into creation of the song (Usually avoid "feat" or "ft." artists)</param>
    /// <param name="albumCoverLink">URL to the album cover image on the Internet, it the song has no cover this param must be null and the program will use a generic cover</param>
    /// <param name="genre">(Optional) Genre or genres of the song</param>
    /// <param name="infoLink">URL that will lead to a web page where the viewers can find more information about the song, not the artists. This is used to create the QRCode</param>
    /// <param name="songLink">URL that leads to the song FILE on the Internet</param>
    /// <param name="albumCoverLocalPath">Once album cover has been downloaded, this will save the local path (on the user's computer) to the file of the downloaded image to be shown</param>
    /// <param name="songLocalPath">Once the song has been downloaded, this will save the local path to the file of the downloaded song</param>
    /// <param name="qRCodeLocalPath">Local path to the QRCode of the InfoLink</param>
    public Song(Sources? source, string name, string artists, string? albumCoverLink, string genre, string infoLink, string songLink, string? albumCoverLocalPath, string? songLocalPath, string? qRCodeLocalPath)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Artists = artists ?? throw new ArgumentNullException(nameof(artists));
        AlbumCoverLink = albumCoverLink ?? "No cover.png";
        Genre = genre;
        InfoLink = infoLink ?? throw new ArgumentNullException(nameof(infoLink));
        SongLink = songLink ?? throw new ArgumentNullException(nameof(songLink));
        AlbumCoverLocalPath = albumCoverLink != "No cover.png" ? albumCoverLocalPath : "No cover.png";
        SongLocalPath = songLocalPath;
        QrCodeLocalPath = qRCodeLocalPath ?? throw new ArgumentNullException(nameof(qRCodeLocalPath));
    }


    #region Properties
    /// <summary>
    /// Source of the song, taken from <see cref="Sources"/>
    /// </summary>
    public Sources Source { get; init; }
    /// <summary>
    /// Name or title of the song
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// Artist or artists involved into creation of the song (Usually avoid "feat" or "ft." artists)
    /// </summary>
    public string Artists { get; init; }
    /// <summary>
    /// URL to the album cover image on the Internet, it the song has no cover this param must be null and the program will use a generic cover
    /// </summary>
    public string AlbumCoverLink { get; init; }
    /// <summary>
    /// (Optional) Genre or genres of the song
    /// </summary>
    public string Genre { get; init; }
    /// <summary>
    /// URL that will lead to a web page where the viewers can find more information about the song, not the artists. This is used to create the QRCode
    /// </summary>
    public string InfoLink { get; init; }
    /// <summary>
    /// URL that leads to the song FILE on the Internet
    /// </summary>
    public string SongLink { get; init; }
    /// <summary>
    /// Once album cover has been downloaded, this will save the local path (on the user's computer) to the file of the downloaded image to be shown
    /// </summary>
    public string? AlbumCoverLocalPath { get; init; }
    /// <summary>
    /// Once the song has been downloaded, this will save the local path to the file of the downloaded song
    /// </summary>
    public string SongLocalPath { get; init; }
    /// <summary>
    /// Local path to the QRCode of the InfoLink
    /// </summary>
    public string QrCodeLocalPath { get; init; }
    #endregion


    #region 
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Song song && Equals(song);

    /// <inheritdoc />
    public bool Equals(Song? other) => Name == other?.Name && Artists == other.Artists;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Name);

    public static bool operator ==(Song left, Song right) => left.Equals(right);


    public static bool operator !=(Song left, Song right) => !(left == right);

    /// <inheritdoc />
    public int CompareTo(Song? other)
    {
        var result = string.Compare(Name, other?.Name, StringComparison.Ordinal);
        return result != 0 ? result : string.Compare(Artists, other?.Artists, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider) =>
        $"name: {Name}" +
        $"Artists: {Artists}" +
        $"AlbumCoverLink: {AlbumCoverLink}" +
        $"Genre: {Genre}" +
        $"InfoLink: {InfoLink}" +
        $"SongLink: {SongLink}";
    #endregion
}