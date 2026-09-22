#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptFinder
{
    [MenuItem("Tools/Debug/Find Missing Scripts")]
    private static void FindMissingScripts()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        int count = 0;

        foreach (GameObject root in roots)
        {
            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform transform in transforms)
            {
                GameObject go = transform.gameObject;

                int missingCount =
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

                if (missingCount > 0)
                {
                    Debug.LogError(
                        $"[Missing Script] {GetPath(go)} : {missingCount}°³",
                        go);

                    count += missingCount;
                }
            }
        }

        Debug.Log($"Missing Script ÃÑ {count}°³");
    }

    private static string GetPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}

#endif