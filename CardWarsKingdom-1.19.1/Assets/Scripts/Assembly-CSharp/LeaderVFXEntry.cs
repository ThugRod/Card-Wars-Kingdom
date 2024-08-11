using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderVFXEntry
{
    private string _AnimStateName;
    private string _VFXName;
    private string _AttachNode;
    private float _StartOffsetTime;

    public string AnimStateName => _AnimStateName;
    public string VFXName => _VFXName;
    public string AttachNode => _AttachNode;
    public float StartOffsetTime => _StartOffsetTime;

    public LeaderVFXEntry(Dictionary<string, object> dict)
    {
        if (dict == null)
        {
            Debug.LogError("LeaderVFXEntry: Constructor received a null dictionary.");
            return;
        }

        try
        {
            _VFXName = TFUtils.LoadString(dict, "VFXName", string.Empty);
            _AnimStateName = TFUtils.LoadString(dict, "AnimStateName", string.Empty);
            _AttachNode = TFUtils.LoadString(dict, "AttachNode", string.Empty);
            _StartOffsetTime = TFUtils.LoadFloat(dict, "StartOffsetFrame", 0f) / 30f;

            // Log warnings for empty required fields
            if (string.IsNullOrEmpty(_VFXName))
                Debug.LogWarning("LeaderVFXEntry: VFXName is empty.");
            if (string.IsNullOrEmpty(_AnimStateName))
                Debug.LogWarning("LeaderVFXEntry: AnimStateName is empty.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"LeaderVFXEntry: Error in constructor: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }
    }
}
