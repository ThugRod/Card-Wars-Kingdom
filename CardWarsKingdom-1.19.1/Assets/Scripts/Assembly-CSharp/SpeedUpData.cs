using System.Collections.Generic;
using UnityEngine;

public class SpeedUpData : ILoadableData
{
    public string ID { get; private set; }
    public int Minutes { get; private set; }
    public int Price { get; private set; }

    public void Populate(Dictionary<string, object> dict)
    {
        if (dict == null)
        {
            Debug.LogWarning("SpeedUpData.Populate received a null dictionary.");
            return;
        }

        ID = TFUtils.LoadString(dict, "ID", string.Empty);
        if (string.IsNullOrEmpty(ID))
        {
            Debug.LogWarning("SpeedUpData: ID is missing or empty.");
        }

        Minutes = TFUtils.LoadInt(dict, "SpeedupMinutes", 1);
        Price = TFUtils.LoadInt(dict, "Price", 1);

        // Log warning if any required field is missing or invalid
        if (Minutes <= 0)
        {
            Debug.LogWarning($"SpeedUpData: Invalid or missing SpeedupMinutes for ID {ID}. Using default value 1.");
        }
        if (Price <= 0)
        {
            Debug.LogWarning($"SpeedUpData: Invalid or missing Price for ID {ID}. Using default value 1.");
        }
    }
}