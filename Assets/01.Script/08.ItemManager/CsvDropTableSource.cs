using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Assets/04.Data/Test/Excel/DropTable.csv → IDropTableSource.
/// itemId는 Generated ItemId(int)와 동일.
/// </summary>
public sealed class CsvDropTableSource : IDropTableSource
{
    public const string DefaultCsvAssetPath = "Assets/04.Data/Test/Excel/DropTable.csv";

    readonly Dictionary<int, DropTableEntry[]> tables;

    CsvDropTableSource(Dictionary<int, DropTableEntry[]> tables)
    {
        this.tables = tables;
    }

    public bool TryGetEntries(int dropTableId, out IReadOnlyList<DropTableEntry> entries)
    {
        if (tables.TryGetValue(dropTableId, out DropTableEntry[] found))
        {
            entries = found;
            return true;
        }

        entries = null;
        return false;
    }

    public static bool TryLoadFromAssetPath(string assetPath, out CsvDropTableSource source, out string error)
    {
        source = null;
        error = null;

        if (string.IsNullOrWhiteSpace(assetPath))
        {
            error = "CSV path is empty.";
            return false;
        }

        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string fullPath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(fullPath))
        {
            error = $"DropTable CSV not found: {fullPath}";
            return false;
        }

        string text;
        try
        {
            using FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new StreamReader(stream);
            text = reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }

        if (!TryParse(text, out Dictionary<int, List<DropTableEntry>> grouped, out error))
            return false;

        var built = new Dictionary<int, DropTableEntry[]>(grouped.Count);
        foreach (KeyValuePair<int, List<DropTableEntry>> pair in grouped)
            built[pair.Key] = pair.Value.ToArray();

        source = new CsvDropTableSource(built);
        return true;
    }

    static bool TryParse(string csvText, out Dictionary<int, List<DropTableEntry>> grouped, out string error)
    {
        grouped = new Dictionary<int, List<DropTableEntry>>();
        error = null;

        if (string.IsNullOrWhiteSpace(csvText))
        {
            error = "DropTable CSV is empty.";
            return false;
        }

        string[] lines = csvText.Split('\n');
        if (lines.Length < 2)
        {
            error = "DropTable CSV has no data rows.";
            return false;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim('\r', ' ', '\t');
            if (string.IsNullOrEmpty(line))
                continue;

            string[] cols = line.Split(',');
            if (cols.Length < 5)
            {
                error = $"DropTable CSV line {i + 1}: expected 5 columns.";
                return false;
            }

            if (!int.TryParse(cols[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int dropTableId))
            {
                error = $"DropTable CSV line {i + 1}: invalid dropTableId.";
                return false;
            }

            if (!int.TryParse(cols[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int itemId))
            {
                error = $"DropTable CSV line {i + 1}: invalid itemId.";
                return false;
            }

            if (!float.TryParse(cols[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float chance))
            {
                error = $"DropTable CSV line {i + 1}: invalid chance.";
                return false;
            }

            if (!int.TryParse(cols[3].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int minAmount))
            {
                error = $"DropTable CSV line {i + 1}: invalid minAmount.";
                return false;
            }

            if (!int.TryParse(cols[4].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxAmount))
            {
                error = $"DropTable CSV line {i + 1}: invalid maxAmount.";
                return false;
            }

            if (!Enum.IsDefined(typeof(ItemId), itemId))
            {
                error = $"DropTable CSV line {i + 1}: itemId {itemId} is not in ItemId enum.";
                return false;
            }

            var entry = new DropTableEntry((ItemId)itemId, chance, minAmount, maxAmount);
            if (!grouped.TryGetValue(dropTableId, out List<DropTableEntry> list))
            {
                list = new List<DropTableEntry>(4);
                grouped[dropTableId] = list;
            }

            list.Add(entry);
        }

        if (grouped.Count == 0)
        {
            error = "DropTable CSV: no valid rows parsed.";
            return false;
        }

        return true;
    }
}
