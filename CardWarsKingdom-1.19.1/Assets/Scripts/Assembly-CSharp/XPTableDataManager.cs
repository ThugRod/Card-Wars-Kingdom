using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class XPTableDataManager : DataManager<XPTableData>
{
	private static XPTableDataManager _instance;

	public static XPTableDataManager Instance
	{
		get
		{
			if (_instance == null)
			{
				string path = Path.Combine(SQSettings.CDN_URL, "Blueprints", "db_XPTables.json");
				_instance = new XPTableDataManager(path);
			}
			return _instance;
		}
	}

	public XPTableDataManager(string path)
	{
		base.FilePath = path;
	}

protected override void ParseRows(List<object> jlist)
{
    if (jlist == null || jlist.Count == 0)
    {
        Debug.LogWarning("XPTableDataManager: Empty or null jlist received.");
        return;
    }

    List<XPTableData> list = new List<XPTableData>();

    try
    {
        Dictionary<string, object> dictionary = jlist[0] as Dictionary<string, object>;
        if (dictionary == null)
        {
            Debug.LogError("XPTableDataManager: First item in jlist is not a Dictionary<string, object>.");
            return;
        }

        foreach (KeyValuePair<string, object> item in dictionary)
        {
            if (item.Key != "Level")
            {
                list.Add(new XPTableData(item.Key));
            }
        }

        foreach (object item2 in jlist)
        {
            Dictionary<string, object> dictionary2 = item2 as Dictionary<string, object>;
            if (dictionary2 == null)
            {
                Debug.LogWarning("XPTableDataManager: Skipping non-dictionary item in jlist.");
                continue;
            }

            int num = 0;
            int count = list.Count;

            foreach (KeyValuePair<string, object> item3 in dictionary2)
            {
                if (item3.Key == "Level")
                {
                    continue;
                }

                if (item3.Value == null || (item3.Value is string stringValue && string.IsNullOrEmpty(stringValue)))
                {
                    num++;
                    continue;
                }

                for (int i = num; i < count && list[i].ID != item3.Key; i++)
                {
                    num++;
                }

                if (num < count)
                {
                    int xpToReach;
                    if (int.TryParse(item3.Value.ToString(), out xpToReach))
                    {
                        list[num].AddLevel(xpToReach);
                    }
                    else
                    {
                        Debug.LogWarning($"XPTableDataManager: Unable to parse XP value '{item3.Value}' for key '{item3.Key}'.");
                    }
                }
                else
                {
                    Debug.LogWarning($"XPTableDataManager: Unable to find matching XPTableData for key '{item3.Key}'.");
                }

                num++;
            }
        }

        foreach (XPTableData item4 in list)
        {
            if (!string.IsNullOrEmpty(item4.ID) && !Database.ContainsKey(item4.ID))
            {
                Database.Add(item4.ID, item4);
                DatabaseArray.Add(item4);
            }
            else
            {
                Debug.LogWarning($"XPTableDataManager: Duplicate or invalid ID '{item4.ID}'. Skipping this item.");
            }
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"XPTableDataManager.ParseRows error: {ex.Message}");
        Debug.LogError($"Stack trace: {ex.StackTrace}");
        throw; // Re-throw the exception to be caught by the outer try-catch
    }
}
}

