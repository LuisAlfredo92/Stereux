using System.Collections.Generic;
using Connections.Models;

namespace Connections.Controllers;

public interface IGetSongs
{
    public List<Song>? GetSongs();
}