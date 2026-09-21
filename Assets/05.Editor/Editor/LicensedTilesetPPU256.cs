using UnityEditor;
using UnityEngine;

public static class LicensedTilesetPPU256
{
    private const string TargetFolder =
        "Assets/03.Asset/Licensed/2D Platformer Tileset/Sprites";

    private const float TargetPPU = 256f;

    [MenuItem("Tools/Tileset/Set Licensed Tileset PPU 256")]
    public static void SetPPU()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:Texture2D",
            new[] { TargetFolder }
        );

        int changedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            TextureImporter importer =
                AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer == null)
                continue;

            // Sprite가 아닌 일반 Texture는 건드리지 않음
            if (importer.textureType != TextureImporterType.Sprite)
                continue;

            // 이미 256이면 재임포트하지 않음
            if (Mathf.Approximately(importer.spritePixelsPerUnit, TargetPPU))
                continue;

            importer.spritePixelsPerUnit = TargetPPU;
            importer.SaveAndReimport();

            changedCount++;

            Debug.Log($"[PPU 변경] {assetPath} -> {TargetPPU}");
        }

        Debug.Log(
            $"[Tileset PPU 설정 완료] 총 {changedCount}개의 Sprite를 {TargetPPU} PPU로 변경했습니다."
        );
    }
}