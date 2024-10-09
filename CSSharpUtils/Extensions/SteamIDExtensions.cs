using CounterStrikeSharp.API.Modules.Entities;
using System.Net.Http;

namespace CSSharpUtils.Extensions;

public static class SteamIDExtensions
{
    private static readonly HttpClient _httpClient = new();
    private const ulong minSteamID = 76561197960265728;

    public static bool IsSteamID(string steamid)
    {
        if (steamid.Length != 17)
        {
            return false;
        }

        if (!ulong.TryParse(steamid, out ulong steamID))
        {
            return false;
        }

        return steamID >= minSteamID;
    }

    public static bool IsSteamID(ulong steamid)
    {
        if (steamid.ToString().Length != 17)
        {
            return false;
        }

        return steamid >= minSteamID;
    }

    public static async Task<string> GetPlayerNameFromSteamID(ulong steamID)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync($"https://steamcommunity.com/profiles/{steamID}/?xml=1");
            response.EnsureSuccessStatusCode();

            string xmlContent = await response.Content.ReadAsStringAsync();

            System.Xml.XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlContent);

            System.Xml.XmlNode? nameNode = xmlDoc.SelectSingleNode("//steamID");

            string? name = nameNode?.InnerText.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return steamID.ToString();
            }

            return name;
        }
        catch (Exception)
        {
            return steamID.ToString();
        }
    }
}