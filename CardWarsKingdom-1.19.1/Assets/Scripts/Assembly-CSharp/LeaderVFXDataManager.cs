using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LeaderVFXDataManager : DataManager<LeaderVFXData>
{
    private static LeaderVFXDataManager _instance;

    public static LeaderVFXDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                string path = Path.Combine(SQSettings.CDN_URL, "Blueprints", "db_LeaderVFX.json");
                _instance = new LeaderVFXDataManager(path);
            }
            return _instance;
        }
    }

    public LeaderVFXDataManager(string path)
    {
        base.FilePath = path;
    }

    protected override void ParseRows(List<object> jlist)
    {
        if (jlist == null || jlist.Count == 0)
        {
            Debug.LogWarning("LeaderVFXDataManager: Empty or null jlist received.");
            return;
        }

        LeaderVFXData leaderVFXData = null;
        foreach (object item in jlist)
        {
            try
            {
                if (!(item is Dictionary<string, object> dictionary))
                {
                    Debug.LogWarning($"LeaderVFXDataManager: Skipping item: not a dictionary. Item type: {item?.GetType().Name ?? "null"}");
                    continue;
                }

                string text = TFUtils.LoadString(dictionary, "ID", string.Empty);
                if (string.IsNullOrEmpty(text))
                {
                    Debug.LogWarning("LeaderVFXDataManager: Skipping entry with empty ID.");
                    continue;
                }

                if (leaderVFXData == null || text != "^")
                {
                    leaderVFXData = new LeaderVFXData(text);
                    if (!Database.ContainsKey(text))
                    {
                        Database.Add(text, leaderVFXData);
                    }
                    else
                    {
                        Debug.LogWarning($"LeaderVFXDataManager: Duplicate ID found: {text}. Overwriting existing entry.");
                        Database[text] = leaderVFXData;
                    }
                    DatabaseArray.Add(leaderVFXData);
                }

                LeaderVFXEntry entry = new LeaderVFXEntry(dictionary);
                leaderVFXData.AddEntry(entry);
            }
            catch (Exception ex)
            {
                Debug.LogError($"LeaderVFXDataManager: Error parsing row: {ex.Message}");
                Debug.LogError($"Stack trace: {ex.StackTrace}");
                if (item is Dictionary<string, object> dict)
                {
                    Debug.LogError($"Problematic data keys: {string.Join(", ", dict.Keys)}");
                }
                // Continue with the next item instead of throwing
            }
        }
    }
}
