using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BossDirections;

public static class OfferManager
{
    // Call once in Awake() to load and cache your YAML
    public static List<Offering> Offerings { get; private set; }

    public static void Initialize(OfferingsYaml? yaml)
    {
        Offerings = yaml?.offerings ?? new List<Offering>();
        // --- 2) Normalize each Offering for safety ---
        foreach (Offering? off in Offerings)
        {
            // ensure non-null
            off.items = off.items ?? new Dictionary<string, int>();
            off.quotes = off.quotes ?? new List<string>();

            // rebuild items dict with normalized keys
            Dictionary<string, int> normDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, int> kv in off.items)
            {
                string key = Normalize(kv.Key);
                normDict[key] = kv.Value;
            }

            off.items = normDict;
        }
    }

    // helper: strip out everything but letters+digits, lowercase
    static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        char[] arr = s.Where(char.IsLetterOrDigit).ToArray();
        return new string(arr).ToLowerInvariant();
    }

    /// <summary>
    /// Try to handle a “burn offering” on this fireplace.
    /// Returns true if we consumed it / showed a message, false to let vanilla run.
    /// </summary>
    public static bool TryOffer(ItemDrop.ItemData item, Vector3 firePos)
    {
        // --- guard against NREs ---
        Player? player = Player.m_localPlayer;
        if (item == null || player == null || Offerings.Count == 0)
            return false;

        // --- build all lookup keys from this item ---
        HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1) prefab name
        if (item.m_dropPrefab?.name is string pf && pf.Length > 0)
            keys.Add(Normalize(pf));

        // 2) shared-name token (strip leading $)
        if (!string.IsNullOrEmpty(item.m_shared?.m_name))
        {
            string rawShared = item.m_shared?.m_name.TrimStart('$');
            keys.Add(Normalize(rawShared));

            // 3) localized display name
            string display = Localization.instance?.Localize(item.m_shared.m_name) ?? rawShared;
            keys.Add(Normalize(display));
        }

        // no valid keys → bail out
        if (keys.Count == 0)
            return false;

        // --- search each offering ---
        foreach (Offering? off in Offerings)
        {
            // try each variant
            foreach (string? key in keys)
            {
                if (!off.items.TryGetValue(key, out int needed))
                    continue;

                // we found a match!
                if (item.m_stack < needed)
                {
                    // show localized shared name or prefab as fallback
                    string? nameDisplay = Localization.instance?.Localize(item.m_shared.m_name) ?? item.m_dropPrefab.name;
                    player.Message(MessageHud.MessageType.Center, $"Not enough {nameDisplay}! ({item.m_stack}/{needed})");
                    return true;
                }

                // consume exactly this instance
                player.GetInventory().RemoveItem(item, needed);

                // pick a random quote (if any)
                string talk = "";
                if (off.quotes.Count > 0)
                {
                    talk = off.quotes[UnityEngine.Random.Range(0, off.quotes.Count)];
                    if (off.addname)
                        talk = $"[{off.name}] {talk}";
                }

                player.Message(MessageHud.MessageType.Center, talk);

                ZoneSystem.instance.FindClosestLocation(off.location, firePos, out ZoneSystem.LocationInstance loc);
                player.SetLookDir(loc.m_position - player.transform.position, 3.5f);

                return true;
            }
        }

        // not one of ours → let vanilla UseItem run
        return false;
    }
}