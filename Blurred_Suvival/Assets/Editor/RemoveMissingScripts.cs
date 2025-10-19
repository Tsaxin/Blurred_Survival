using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class RemoveMissingScripts
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts From Current Scene")]
    public static void RemoveMissingScript()
    {
        int totalRemoved = 0;

        // Loop through all open scenes
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            // Loop through all root GameObjects
            foreach (var root in scene.GetRootGameObjects())
            {
                totalRemoved += RemoveMissingScriptsRecursive(root);
            }

            // Mark scene as dirty and save
            if (totalRemoved > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Removed {totalRemoved} missing scripts from the current scene(s).");
    }

    // Recursive function to remove missing scripts from a GameObject and its children
    private static int RemoveMissingScriptsRecursive(GameObject go)
    {
        int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        // Recursively check children
        foreach (Transform child in go.transform)
        {
            removedCount += RemoveMissingScriptsRecursive(child.gameObject);
        }

        return removedCount;
    }
}
