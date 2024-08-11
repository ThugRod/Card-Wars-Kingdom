using System;
using System.Collections.Generic;
using UnityEngine;

public class XPTableData : ILoadableData
{
    private string _ID;
    private List<int> _XPToReachLevel = new List<int>();

    public string ID => _ID;

    public XPTableData(string tableId)
    {
        if (string.IsNullOrEmpty(tableId))
        {
            Debug.LogWarning("XPTableData: Initialized with null or empty ID.");
        }
        _ID = tableId;
    }

    public int GetXpToReachLevel(int level)
    {
        if (level < 1 || level > _XPToReachLevel.Count)
        {
            Debug.LogWarning($"XPTableData: Invalid level {level} requested. Valid range is 1 to {_XPToReachLevel.Count}.");
            return -1;
        }
        return _XPToReachLevel[level - 1];
    }

    public void AddLevel(int xpToReach)
    {
        if (xpToReach < 0)
        {
            Debug.LogWarning($"XPTableData: Attempted to add negative XP value {xpToReach}. Ignoring.");
            return;
        }
        _XPToReachLevel.Add(xpToReach);
    }

    public void Populate(Dictionary<string, object> dict)
    {
        // This method is not used in the current implementation,
        // but it's required by the ILoadableData interface.
        // You might want to implement it if you need to populate data from a dictionary in the future.
        Debug.LogWarning("XPTableData: Populate method called but not implemented.");
    }

    public int GetCurrentLevel(int currentXP, int levelCap)
    {
        if (currentXP < 0)
        {
            Debug.LogWarning($"XPTableData: Negative current XP {currentXP}. Returning level 1.");
            return 1;
        }

        for (int i = 1; i <= levelCap; i++)
        {
            if (i > _XPToReachLevel.Count || currentXP < GetXpToReachLevel(i))
            {
                return i - 1;
            }
        }
        return levelCap;
    }

    public XPLevelData GetLevelData(int currentXP, int levelCap = -1)
    {
        if (levelCap == -1)
        {
            levelCap = _XPToReachLevel.Count;
        }
        levelCap = Math.Min(levelCap, _XPToReachLevel.Count);

        XPLevelData xPLevelData = new XPLevelData();
        xPLevelData.mCurrentXP = currentXP;
        xPLevelData.mCurrentLevel = GetCurrentLevel(currentXP, levelCap);
        xPLevelData.mXPToReachCurrentLevel = GetXpToReachLevel(xPLevelData.mCurrentLevel);
        xPLevelData.mMaxLevel = levelCap;
        xPLevelData.mIsAtMaxLevel = xPLevelData.mCurrentLevel >= levelCap;

        if (xPLevelData.mIsAtMaxLevel)
        {
            xPLevelData.mXPToPassCurrentLevel = int.MaxValue;
            xPLevelData.mRemainingXPToLevelUp = int.MaxValue;
            xPLevelData.mTotalXPInCurrentLevel = xPLevelData.mXPToReachCurrentLevel - GetXpToReachLevel(Math.Max(1, xPLevelData.mCurrentLevel - 1));
            xPLevelData.mXPEarnedWithinCurrentLevel = xPLevelData.mTotalXPInCurrentLevel;
            xPLevelData.mPercentThroughCurrentLevel = 1f;
        }
        else
        {
            xPLevelData.mXPToPassCurrentLevel = GetXpToReachLevel(xPLevelData.mCurrentLevel + 1);
            xPLevelData.mTotalXPInCurrentLevel = xPLevelData.mXPToPassCurrentLevel - xPLevelData.mXPToReachCurrentLevel;
            xPLevelData.mRemainingXPToLevelUp = Math.Max(0, xPLevelData.mXPToPassCurrentLevel - currentXP);
            xPLevelData.mXPEarnedWithinCurrentLevel = Math.Max(0, currentXP - xPLevelData.mXPToReachCurrentLevel);
            xPLevelData.mPercentThroughCurrentLevel = (float)xPLevelData.mXPEarnedWithinCurrentLevel / Math.Max(1, xPLevelData.mTotalXPInCurrentLevel);
            xPLevelData.mPercentThroughCurrentLevel = Mathf.Clamp01(xPLevelData.mPercentThroughCurrentLevel);
        }

        return xPLevelData;
    }
}
