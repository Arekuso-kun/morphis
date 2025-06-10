using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MeshUtility
{
    private const string LevelsRoot = "Assets/Resources/Levels";
    private const string ResourcesRoot = "Levels";

    public static void SaveLevel(string levelName, Stack<ObjectState> states, Stack<Mesh> meshes)
    {
#if UNITY_EDITOR
        List<ObjectState> stateList = new(states);
        List<Mesh> meshList = new(meshes);

        string levelFolder = Path.Combine(LevelsRoot, levelName);

        if (Directory.Exists(levelFolder))
        {
            string[] existingFiles = Directory.GetFiles(levelFolder);
            foreach (string file in existingFiles)
            {
                if (file.EndsWith(".meta")) continue;
                AssetDatabase.DeleteAsset(file);
            }
        }
        else
        {
            Directory.CreateDirectory(levelFolder);
        }

        for (int i = 0; i < stateList.Count; i++)
        {
            string meshFileName = (i == stateList.Count - 1) ? "goal.asset" : $"hint_{i}.asset";
            string meshPath = Path.Combine(levelFolder, meshFileName);

            Mesh savedMesh = Object.Instantiate(meshList[i]);
            AssetDatabase.CreateAsset(savedMesh, meshPath);

            stateList[i].meshFileName = meshFileName;
            stateList[i].isGoal = (i == stateList.Count - 1);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string json = JsonUtility.ToJson(new ObjectStateWrapper { states = stateList }, true);
        string jsonPath = Path.Combine(levelFolder, "data.json");
        File.WriteAllText(jsonPath, json);

        Debug.Log($"Saved level to: {levelFolder}");
#endif
    }

    public static List<ObjectState> LoadHints(string levelName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"{ResourcesRoot}/{levelName}/data");
        if (jsonFile == null)
        {
            Debug.LogError($"State data not found for level {levelName}");
            return null;
        }

        ObjectStateWrapper wrapper = JsonUtility.FromJson<ObjectStateWrapper>(jsonFile.text);
        return wrapper?.states;
    }

    public static Mesh LoadMesh(string levelName, string meshFileName)
    {
        string path = $"{ResourcesRoot}/{levelName}/{meshFileName.Replace(".asset", "")}";
        Debug.Log($"Loading mesh from path: {path}");
        return Resources.Load<Mesh>(path);
    }
}
