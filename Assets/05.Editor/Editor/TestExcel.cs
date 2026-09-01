using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using JetBrains.Annotations;
using System.Linq;
using System.Reflection;
using Codice.Client.BaseCommands.Download;
using Unity.VisualScripting;
public static class ExcelTest 
{ 
    [MenuItem("Tools/ExcelTest/1. Read File")] 
    public static void ReadFile() 
    { 
        string path = "Assets/04.Data/Test/Excel/Test.csv";
        string text = File.ReadAllText(path);

        string[] lines = text.Split('\n'); 
        string[] values;

        string[] headers = lines[0].Trim('\r').Split(",");    

        List<TestData> datas = new List<TestData>(); 
        
        
        for (int i = 1; i < lines.Length; i++) 
        {
            if (string.IsNullOrEmpty(lines[i])) continue;
            lines[i] = lines[i].TrimEnd('\r');

            values = lines[i].Split(',');

            Type listType = typeof(TestData);
            Type dataType = listType.BaseType.GetGenericArguments()[0];

            BaseData data = (BaseData)Activator.CreateInstance(dataType);

           
            
            for (int col = 0; col < headers.Length; col++)
            {
                FieldInfo field = dataType.GetField(headers[col]);
                if (field == null) continue;

                object value = Convert.ChangeType(values[col], field.FieldType);

                field.SetValue(data, value);

                if (col > 100)
                {
                    Debug.LogError("Loop guard triggered");
                    break;
                }
            }
            


            datas.Add(data);
            
        }

        
        string listPath = "Assets/04.Data/Test/TestData.asset";
        TestList testList = AssetDatabase.LoadAssetAtPath<TestList>(listPath);

        testList.baseList = datas;

        EditorUtility.SetDirty(testList);
        AssetDatabase.SaveAssets();
        
        
    }


    private static void ReadData()
    {

    }






    [MenuItem("Tools/ExcelTest/1. Read File2")]
    public static void ReadFile2()
    {
        List<DataEntry> entries = new List<DataEntry>();
        Type baseDataType = typeof(BaseData);
        var dataTypes = TypeCache.GetTypesDerivedFrom(baseDataType);



        foreach (Type dataType in dataTypes)
        {
            if(dataType.IsAbstract) continue;                      
            if(dataType == null) continue;

            entries.Add(new DataEntry(dataType));
        }

        entries = entries.OrderBy(x=>x.DataType.FullName).ToList();


    }


    public class DataEntry
    {
       
        public Type DataType { get; }

        public DataEntry(Type dataType)
        {            
            DataType = dataType;
        }
    }
}