#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainScene(01.MainScene)에 MonsterItem 파이프라인에 필요한 매니저·팩토리·풀을 한 번에 점검/연결합니다.
/// </summary>
public static class MonsterItemMainSceneSetup
{
    const string MainScenePath = "Assets/00.Scenes/01.MainScene/01.MainScene.unity";
    const string ItemDataPath = "Assets/04.Data/Test/ItemData.asset";
    const string MonsterDataPath = "Assets/04.Data/Test/MonsterData.asset";
    const string PlayerDataPath = "Assets/04.Data/Test/PlayerData.asset";
    const string StageDataPath = "Assets/04.Data/Test/StageData.asset";
    const string TestDataPath = "Assets/04.Data/Test/TestData.asset";
    const string ItemPrefabPath = "Assets/02.Prefab/03.Item/Item.prefab";
    const string MonsterPrefabPath = "Assets/02.Prefab/02.Monster/Monster 0.prefab";

    [MenuItem("Tools/MonsterItem/1. Validate Main Scene (report only)")]
    public static void ValidateMainScene()
    {
        if (!EnsureMainSceneOpen(out Scene scene))
            return;

        Debug.Log(BuildReport(scene));
    }

    [MenuItem("Tools/MonsterItem/2. Wire Main Scene (MonsterItem pipeline)")]
    public static void WireMainScene()
    {
        if (!EnsureMainSceneOpen(out Scene scene))
            return;

        Transform manager = FindChild(scene, "Manager");
        if (manager == null)
        {
            Debug.LogError("[MonsterItem] Manager 오브젝트를 찾을 수 없습니다.");
            return;
        }

        EnsureDataManager(manager);
        EnsureObjectPools(manager);
        EnsureFactories(manager);
        EnsureItemDropFacade(manager);
        EnsureStageComponents(manager);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[MonsterItem] MainScene 연결 완료. File → Save 또는 Ctrl+S 로 씬 저장하세요.");
        Debug.Log(BuildReport(scene));
    }

    [MenuItem("Tools/MonsterItem/3. Import CSVs (Item + Monster + Player)")]
    public static void ImportGameCsvs()
    {
        ExcelImporter.ImportCsv("Assets/04.Data/Test/Excel/Item.csv");
        ExcelImporter.ImportCsv("Assets/04.Data/Test/Excel/Monster.csv");
        ExcelImporter.ImportCsv("Assets/04.Data/Test/Excel/Player.csv");
        if (CsvDropTableSource.TryLoadFromAssetPath(CsvDropTableSource.DefaultCsvAssetPath, out _, out string error))
            Debug.Log("[MonsterItem] DropTable.csv OK.");
        else
            Debug.LogWarning("[MonsterItem] DropTable: " + error);
    }

    static bool EnsureMainSceneOpen(out Scene scene)
    {
        scene = EditorSceneManager.GetActiveScene();
        if (scene.path != MainScenePath)
        {
            if (!EditorUtility.DisplayDialog(
                    "MonsterItem Setup",
                    "01.MainScene을 열어야 합니다. 지금 열까요?",
                    "열기",
                    "취소"))
                return false;

            scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }

        return true;
    }

    static void EnsureDataManager(Transform manager)
    {
        DataManager dm = Object.FindFirstObjectByType<DataManager>(FindObjectsInactive.Include);
        if (dm == null)
        {
            GameObject go = new GameObject("00.DataManager");
            Undo.RegisterCreatedObjectUndo(go, "Add DataManager");
            go.transform.SetParent(manager, false);
            dm = go.AddComponent<DataManager>();
        }

        SerializedObject so = new SerializedObject(dm);
        AssignList(so, "itemList", ItemDataPath);
        AssignList(so, "monsterList", MonsterDataPath);
        AssignList(so, "playerList", PlayerDataPath);
        AssignList(so, "stageList", StageDataPath);
        AssignList(so, "testList", TestDataPath);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AssignList(SerializedObject dm, string propertyName, string assetPath)
    {
        Object list = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (list == null)
        {
            Debug.LogWarning("[MonsterItem] SO not found: " + assetPath);
            return;
        }

        SerializedProperty prop = dm.FindProperty(propertyName);
        if (prop != null)
            prop.objectReferenceValue = list;
    }

    static void EnsureObjectPools(Transform manager)
    {
        Transform poolRoot = FindChild(manager, "01.ObjectPoolManager");
        if (poolRoot == null)
        {
            GameObject go = new GameObject("01.ObjectPoolManager");
            Undo.RegisterCreatedObjectUndo(go, "Add ObjectPool root");
            go.transform.SetParent(manager, false);
            poolRoot = go.transform;
        }

        EnsureComponentOnChild(poolRoot, "ItemObjectPoolManager", typeof(ItemObjectPoolManager));
        EnsureComponentOnChild(poolRoot, "MonsterObjectPoolManager", typeof(MonsterObjectPoolManager));
    }

    static void EnsureFactories(Transform manager)
    {
        Transform factoryRoot = FindChild(manager, "02.Factory");
        if (factoryRoot == null)
        {
            Debug.LogError("[MonsterItem] 02.Factory 가 없습니다.");
            return;
        }

        ItemObjectPoolManager itemPool = Object.FindFirstObjectByType<ItemObjectPoolManager>(FindObjectsInactive.Include);
        MonsterObjectPoolManager monsterPool = Object.FindFirstObjectByType<MonsterObjectPoolManager>(FindObjectsInactive.Include);
        ItemController itemPrefab = AssetDatabase.LoadAssetAtPath<ItemController>(ItemPrefabPath);
        MonsterController monsterPrefab = AssetDatabase.LoadAssetAtPath<MonsterController>(MonsterPrefabPath);

        ItemFactory itemFactory = EnsureComponentOnChild(factoryRoot, "02.ItemFactory", typeof(ItemFactory)) as ItemFactory;
        MonsterFactory monsterFactory = EnsureComponentOnChild(factoryRoot, "03.MonsterFactory", typeof(MonsterFactory)) as MonsterFactory;

        if (itemFactory != null)
        {
            SerializedObject so = new SerializedObject(itemFactory);
            so.FindProperty("defaultPrefab").objectReferenceValue = itemPrefab;
            so.FindProperty("poolManager").objectReferenceValue = itemPool;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        if (monsterFactory != null)
        {
            SerializedObject so = new SerializedObject(monsterFactory);
            so.FindProperty("defaultPrefab").objectReferenceValue = monsterPrefab;
            so.FindProperty("poolManager").objectReferenceValue = monsterPool;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static void EnsureItemDropFacade(Transform manager)
    {
        ItemDropManager drop = Object.FindFirstObjectByType<ItemDropManager>(FindObjectsInactive.Include);
        if (drop == null)
        {
            Debug.LogWarning("[MonsterItem] ItemDropManager가 없습니다.");
            return;
        }

        if (drop.GetComponent<ItemDropFacade>() == null)
            Undo.AddComponent<ItemDropFacade>(drop.gameObject);
    }

    static void EnsureStageComponents(Transform manager)
    {
        StageController stage = Object.FindFirstObjectByType<StageController>(FindObjectsInactive.Include);
        if (stage == null)
        {
            Debug.LogWarning("[MonsterItem] StageController가 없습니다.");
            return;
        }

        if (stage.GetComponent<MonsterSpawner>() == null)
            Undo.AddComponent<MonsterSpawner>(stage.gameObject);
        if (stage.GetComponent<StageFacade>() == null)
            Undo.AddComponent<StageFacade>(stage.gameObject);
    }

    static Component EnsureComponentOnChild(Transform parent, string childName, System.Type type)
    {
        Transform child = FindChild(parent, childName);
        if (child == null)
        {
            GameObject go = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(go, "Add " + childName);
            go.transform.SetParent(parent, false);
            child = go.transform;
        }

        Component c = child.GetComponent(type);
        if (c == null)
            c = Undo.AddComponent(child.gameObject, type);
        return c;
    }

    static Transform FindChild(Transform root, string name)
    {
        if (root.name == name)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChild(root.GetChild(i), name);
            if (found != null)
                return found;
        }

        return null;
    }

    static Transform FindChild(Scene scene, string name)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindChild(roots[i].transform, name);
            if (found != null)
                return found;
        }

        return null;
    }

    static string BuildReport(Scene scene)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[MonsterItem] Scene: {scene.path}");
        sb.AppendLine(DataManager.instance != null || Object.FindFirstObjectByType<DataManager>(FindObjectsInactive.Include) != null
            ? "  OK  DataManager"
            : "  MISSING  DataManager");
        sb.AppendLine(FindType<ItemFactory>() != null ? "  OK  ItemFactory" : "  MISSING  ItemFactory");
        sb.AppendLine(FindType<MonsterFactory>() != null ? "  OK  MonsterFactory" : "  MISSING  MonsterFactory");
        sb.AppendLine(FindType<ItemObjectPoolManager>() != null ? "  OK  ItemObjectPoolManager" : "  MISSING  ItemObjectPoolManager");
        sb.AppendLine(FindType<MonsterObjectPoolManager>() != null ? "  OK  MonsterObjectPoolManager" : "  MISSING  MonsterObjectPoolManager");
        sb.AppendLine(FindType<ItemDropFacade>() != null ? "  OK  ItemDropFacade" : "  MISSING  ItemDropFacade (on ItemDropManager)");
        sb.AppendLine(FindType<MonsterSpawner>() != null ? "  OK  MonsterSpawner" : "  MISSING  MonsterSpawner (on StageManager)");
        sb.AppendLine(FindType<StageFacade>() != null ? "  OK  StageFacade" : "  MISSING  StageFacade (on StageManager)");
        return sb.ToString();
    }

    static T FindType<T>() where T : Object => Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
}
#endif
