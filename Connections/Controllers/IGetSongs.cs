using System.Collections.Generic;
using Connections.Models;

namespace Connections.Controllers;

/// <summary>
/// Interface used to call the same methods from different sources. It might be removed
/// </summary>
public interface IGetSongs
{
    /// <summary>
    /// Gets the songs.
    /// </summary>
    /// <returns><![CDATA[A List<Song>? .]]></returns>
    public List<Song>? GetSongs();
}