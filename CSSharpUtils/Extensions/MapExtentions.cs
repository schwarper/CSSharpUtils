using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using System.Net.Http;

namespace CSSharpUtils.Extensions;

public static class MapExtentions
{
    private static readonly string MapsPath =
        Path.Combine(Server.GameDirectory, "csgo", "maps");

    private static readonly List<string> ValidMaps = [];

    public static bool IsMapValid(string mapname) => ValidMaps.Contains(mapname);

    public static void LoadValidMaps()
    {
        ValidMaps.Clear();

        var files = Directory.GetFiles(MapsPath, "*.vpk").ToList();

        foreach (var file in files)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            ValidMaps.Add(fileNameWithoutExtension);
        }
    }
}