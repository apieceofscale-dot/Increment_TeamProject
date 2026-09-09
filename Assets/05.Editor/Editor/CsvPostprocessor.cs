using UnityEditor;
using UnityEngine;

public class CsvPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
       string[] importedAssets,
       string[] deletedAssets,
       string[] movedAssets,
       string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (!path.EndsWith(".csv"))
                continue;

            Debug.Log($"CSV 변경 감지: {path}");
            string csvPath = path;

            EditorApplication.delayCall += () =>
            {
                ExcelImporter.ImportCsv(csvPath);
            };
        }

    }
}
