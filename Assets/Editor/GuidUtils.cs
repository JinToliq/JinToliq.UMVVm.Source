using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Misc
{
  public static class GuidUtils
  {
    [MenuItem("Assets/Generate new GUID", true)]
    private static bool GenerateNewGuidValidation()
    {
      // Validate if an asset is selected
      return Selection.activeObject != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    [MenuItem("Assets/Generate new GUID", false)]
    public static void GenerateNewGuid()
    {
      var selectedObject = Selection.activeObject;
      if (selectedObject == null)
      {
        Debug.LogError("No asset selected");
        return;
      }

      var assetPath = AssetDatabase.GetAssetPath(selectedObject);
      if (string.IsNullOrEmpty(assetPath))
      {
        Debug.LogError("Failed to get asset path");
        return;
      }

      var oldGuid = AssetDatabase.AssetPathToGUID(assetPath);
      var newGuid = GUID.Generate().ToString();
      var metaFilePath = assetPath + ".meta";
      if (!File.Exists(metaFilePath))
      {
        Debug.LogError("Meta file not found.");
        return;
      }

      var metaFileLines = File.ReadAllLines(metaFilePath);
      for (var i = 0; i < metaFileLines.Length; i++)
      {
        if (!metaFileLines[i].StartsWith("guid:"))
          continue;

        metaFileLines[i] = "guid: " + newGuid;
        break;
      }

      File.WriteAllLines(metaFilePath, metaFileLines);
      AssetDatabase.Refresh();
      UpdateGuidReferences(oldGuid, newGuid);
      Debug.Log($"Generated new GUID for asset: {assetPath}\nOld GUID:{oldGuid}\nNew GUID: {newGuid}");
    }

    [MenuItem("Assets/Generate new GUIDs for all assets", false)]
    public static void GenerateNewGuidsForAll()
    {
      var allAssets = AssetDatabase
        .GetAllAssetPaths()
        .ToArray();

      foreach (var assetPath in allAssets)
      {
        if (Path.GetExtension(assetPath) == ".meta")
          continue;
        if (!assetPath.StartsWith("Assets"))
          continue;
        var metaFilePath = assetPath + ".meta";
        if (!File.Exists(metaFilePath))
          continue;

        var oldGuid = AssetDatabase.AssetPathToGUID(assetPath);
        var newGuid = GUID.Generate().ToString();

        var metaFileLines = File.ReadAllLines(metaFilePath);
        for (var i = 0; i < metaFileLines.Length; i++)
        {
          if (!metaFileLines[i].StartsWith("guid:"))
            continue;

          metaFileLines[i] = "guid: " + newGuid;
          break;
        }

        File.WriteAllLines(metaFilePath, metaFileLines);
        AssetDatabase.Refresh();
        UpdateGuidReferences(oldGuid, newGuid);
        Debug.Log($"Generated new GUID for asset: {assetPath}\nOld GUID:{oldGuid}\nNew GUID: {newGuid}");
      }
    }

    private static void UpdateGuidReferences(string oldGuid, string newGuid)
    {
      var allAssetPaths = AssetDatabase.GetAllAssetPaths();
      foreach (var path in allAssetPaths)
      {
        if (Path.GetExtension(path) == ".meta")
          continue;
        if (path == "Assets")
          continue;
        if (!path.StartsWith("Assets"))
          continue;
        if (AssetDatabase.IsValidFolder(path))
          continue;

        var content = File.ReadAllText(path);
        if (!content.Contains(oldGuid))
          continue;

        content = content.Replace(oldGuid, newGuid);
        File.WriteAllText(path, content);
        Debug.Log($"Updated GUID in: {path}");
      }

      AssetDatabase.Refresh();
    }
  }
}