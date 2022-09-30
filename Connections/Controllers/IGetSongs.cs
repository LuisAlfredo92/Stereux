﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Connections.Models;

namespace Connections.Controllers;

public interface IGetSongs
{
    public List<Song>? GetSongs();
}