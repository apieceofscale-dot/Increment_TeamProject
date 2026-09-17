using System;
using UnityEngine;

/// <summary>
/// 강화 1회 비용 (임시). 재화 연동 전 UI 표시용.
/// </summary>
public sealed class ItemUpgradeCostProvider
{
    public const long BaseCost = 100;
    public const double GrowthRate = 1.2;

    public static readonly ItemUpgradeCostProvider Default = new ItemUpgradeCostProvider();

    public long GetCost(int currentUpgradeLevel)
    {
        int level = Mathf.Max(0, currentUpgradeLevel);
        return (long)(BaseCost * Math.Pow(GrowthRate, level));
    }
}
