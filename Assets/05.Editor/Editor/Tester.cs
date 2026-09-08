using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class Tester
{
    [MenuItem("Tools/Test/test1")]
    public static void test1()
    {
        string[] guids = AssetDatabase.FindAssets("t:TestList");  
        string path = path = AssetDatabase.GUIDToAssetPath(guids[0]);

        TestList testList = AssetDatabase.LoadAssetAtPath<TestList>(path);
        DataManager manager = UnityEngine.Object.FindFirstObjectByType<DataManager>();   
        SerializedObject so = new SerializedObject( manager );   
        SerializedProperty property = so.FindProperty("testList");          
        property.objectReferenceValue = testList;

        so.ApplyModifiedProperties();
    }


    [MenuItem("Tools/Test/test2")]
    public static void test2()
    {

        string[] guids = AssetDatabase.FindAssets("t:MonsterList");
        if (guids.Length == 0)
        {
            Debug.Log("MonsterList를 찾지 못했습니다.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        MonsterList monsterList = AssetDatabase.LoadAssetAtPath<MonsterList>(path);

        Debug.Log($"경로 : {path}");
        Debug.Log($"에셋 : {monsterList.name}");





    }
}


