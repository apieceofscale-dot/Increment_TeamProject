using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

//이 파일은 Asset폴더의 Editor안에서만 작동하니, 멋대로 이동 금지. Ediotor파일 통채로 옮기던가.

// 지금 호출하는 함수가 2개고, 반드시 첫 함수가 끝나고, 컴파일이 끝난 뒤 두 번째 함수가 로드 되어야 하니
// SessionState + [DidReloadScripts]를 쓰면 더할 나위 없이 좋겠으나 생략함. 시간 남으면 하기. 

public class DataManagerGenerator
{
    //파일 저장 경로
    private const string GeneratedPath = "Assets/01.Script/02.DataManager/DataManager.g.cs";

    //attribute 를 통한 딸깍기능
    //'Generate DataManager.g.cs' 가 최종적으로 실행할 함수.
    [MenuItem("Tools/Data/Excute this First!/Generate DataManager.g.cs")]
    public static void GnerateDataManager()
    {
        List <DataAndListEntry> entries = new List <DataAndListEntry>();

        Type baseListType = typeof(BaseList<>);
        var listTypes = TypeCache.GetTypesDerivedFrom(baseListType); //반환형이 TypeCache.TypeCollection. 인 조회용 collection여기에 여러 Type이 들어있다.
                
        foreach(Type listType in listTypes)
        {
            if(listType.IsAbstract) continue;//추상함수는 뭔가를 가지면 안되므로 막음.

            Type dataType = GetBaseListDataType(listType);
            if (dataType == null) continue; //널이면 들어가면 안됨.
            
            entries.Add(new DataAndListEntry(listType, dataType));          
        }
        entries = entries.OrderBy(x => x.DataType.FullName).ToList(); //foreach의 순회는 순서 보장이 안되므로 항상 같은 결과로 보이게 정렬해줌. 미적요소임.
        /*
        foreach(var entry in entries)
        {
            Debug.Log($"{entry.ListType}, {entry.DataType}");
        }        
        */
        foreach (var group in entries.GroupBy(x => x.DataType)) //설마 같은 데이터를 2개 넣는 사람은 없을 거라 생각하긴 하지만...
        {
            if (group.Count() <= 1)
            {
                continue;
            }
            Debug.LogError($"{group.Key.Name} 용 리스트가 DataManager에 여러개 있음.");
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("//자동생성됨. 수정금지.");
        builder.AppendLine("using UnityEngine;");
        builder.AppendLine("public partial class DataManager");
        builder.AppendLine("{");

        //직렬화 필드 생성 [SerializeField] private MonsterList monsterList;
        foreach (var entry in entries)
        {
            string fieldType = entry.ListType.Name;
            string fieldName = GetFieldName(entry.ListType);

            builder.AppendLine($"    [SerializeField] private {fieldType} {fieldName};");
        }
        builder.AppendLine();
        //데이터 리포지토리 생성
        foreach (DataAndListEntry entry in entries)
        {
            string dataTypeName = GetTypeName(entry.DataType);
            string repositoryname = GetRepositoryName(entry.DataType);

            builder.AppendLine($"    private readonly DataRepositary<{dataTypeName}> {repositoryname} =  new DataRepositary<{dataTypeName}>();");
            builder.AppendLine();
        }
        builder.AppendLine();

        //LoadAllData 함수 조립
        builder.AppendLine("    partial void LoadAllOfDataGenerated()");
        builder.AppendLine("    {");
        foreach (DataAndListEntry entry in entries)
        {
            string repositoryName = GetRepositoryName(entry.DataType);
            string fieldName = GetFieldName(entry.ListType);

            builder.AppendLine($"        LoadData({repositoryName}, {fieldName}.baseList);");
        }

        builder.AppendLine("    }");

        builder.AppendLine("}");

        string generatedCode = builder.ToString();

        string directory = Path.GetDirectoryName(GeneratedPath);

        //c에서도 공부했었던 IO임. 스크립트만 만드는 거라 별거 없으니 걍 문서 한번 쭉 보면 됨.

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        //파일 드디어 생성
        File.WriteAllText(GeneratedPath, generatedCode);

        AssetDatabase.ImportAsset(GeneratedPath);//유니티 api

        Debug.Log($"Data Repository {entries.Count}개 생성 완료.");

    }


    [MenuItem("Tools/Data/Excute this 2nd/InjectDataManagerAsset")]
    public static void InjectDataManagerAsset()
    {
        DataManager manager = UnityEngine.Object.FindFirstObjectByType<DataManager>();
        SerializedObject so = new SerializedObject(manager);

        List<DataAndListEntry> entries = new List<DataAndListEntry>();
        FieldInfo[] fields = typeof(DataManager).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            Type dataType = GetBaseListDataType(field.FieldType);
            if (dataType == null) continue;
            entries.Add(new DataAndListEntry(field.FieldType, dataType));
        }

        entries = entries.OrderBy(x => x.DataType.FullName).ToList();

        foreach (var group in entries.GroupBy(x => x.DataType)) //설마 같은 데이터를 2개 넣는 사람은 없을 거라 생각하긴 하지만...
        {
            if (group.Count() <= 1) { continue; }
            Debug.LogError($"{group.Key.Name} 용 리스트가 DataManager에 여러개 있음.");
            return;
        }
        foreach (var entry in entries)
        {
            string fieldType = entry.ListType.Name;
            string fieldName = GetFieldName(entry.ListType);
            // Debug.Log($"{fieldType}, {fieldName}");
            string[] guids = AssetDatabase.FindAssets($"t:{fieldType}");
            if (guids.Length == 0)
            {
                Debug.LogError($"{fieldType} 에셋을 찾지 못했습니다.");
                continue;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            UnityEngine.Object listAsset = AssetDatabase.LoadAssetAtPath(path, entry.ListType);

            
            SerializedProperty property = so.FindProperty(fieldName);
            property.objectReferenceValue = listAsset;
            so.ApplyModifiedProperties();
        }
    }


    private static Type GetBaseListDataType(Type type)
    {
        Type current = type;
        //반복을 통해 부모의 제네릭을 찾는 코드
        while (current != null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(BaseList<>))
            {
                return current.GetGenericArguments()[0];
            }

            current = current.BaseType;
        }

        return null;
    }


    private static string GetTypeName(Type type)
    {
        string fullName = type.FullName ?? type.Name; //?? 병합 연산자. 앞에가 널이면 뒤에걸 쓴다.
        fullName = fullName.Replace("+", ".");
        return fullName;
    }

    private static string GetFieldName(Type type)
    {
        string name = type.Name;
        name = char.ToLowerInvariant(name[0]) + name.Substring(1);
        return name;
    }


    private static string GetRepositoryName(Type type)
    {
        string name = type.Name;

        //ex) MonsterData->Monster
        if(name.EndsWith("Data"))
        {
            name = name.Substring(0, name.Length - "Data".Length);
        }
        //Monster->monster
        name = char.ToLowerInvariant(name[0]) + name.Substring(1);

        //monster ->monsterRepository
        return name + "Repository";      

    }

    private class DataEntries
    {
        public string ListFieldName { get; }
        public Type DataType { get; }

        public DataEntries(string listFieldName, Type dataType)
        {
            ListFieldName = listFieldName;
            DataType = dataType;
        }
    }

    private class DataAndListEntry
    {
        public Type ListType { get; }
        public Type DataType { get; }

        public DataAndListEntry(Type listType, Type dataType)
        {
            ListType = listType;
            DataType = dataType;
        }
    }



    



}

