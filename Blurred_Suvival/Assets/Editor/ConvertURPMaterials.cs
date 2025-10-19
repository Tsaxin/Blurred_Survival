using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class ConvertURPToBuiltInFull : EditorWindow
{
    [MenuItem("Tools/Convert URP → Built-in (Full Project)")]
    public static void ConvertURPToBuiltIn()
    {
        int changedMats = 0;
        int changedRenderers = 0;

        // --- STEP 1: Convert all material assets ---
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) continue;

            if (FixMaterialShader(mat))
                changedMats++;
        }

        // --- STEP 2: Convert all prefabs in project ---
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) continue;

            if (FixGameObjectRenderers(prefab))
                changedRenderers++;
        }

        // --- STEP 3: Convert all open scenes ---
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                if (FixGameObjectRenderers(root))
                    changedRenderers++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Conversion complete!\n• {changedMats} material assets fixed\n• {changedRenderers} renderers/prefabs updated");
    }

    // --- Fix shader on a material ---
    private static bool FixMaterialShader(Material mat)
    {
        string name = mat.shader != null ? mat.shader.name : "";
        Shader newShader = null;

        if (name.Contains("Universal Render Pipeline/2D") ||
            name.Contains("Sprite-Lit") ||
            name.Contains("Sprite-Unlit"))
        {
            newShader = Shader.Find("Sprites/Default");
        }
        else if (name.Contains("Universal Render Pipeline/Lit") ||
                 name.Contains("Universal Render Pipeline/Unlit"))
        {
            newShader = Shader.Find("Standard");
        }

        if (newShader != null)
        {
            mat.shader = newShader;
            EditorUtility.SetDirty(mat);
            return true;
        }
        return false;
    }

    // --- Fix renderers on a GameObject ---
    private static bool FixGameObjectRenderers(GameObject go)
    {
        bool changed = false;

        // SpriteRenderers
        foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.sharedMaterial && FixMaterialShader(sr.sharedMaterial))
                changed = true;

            // Some inline materials can’t be edited directly — force reset
            if (sr.sharedMaterial == null || sr.sharedMaterial.shader.name.Contains("Universal"))
            {
                sr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                changed = true;
            }
        }

        // MeshRenderers
        foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
        {
            var mats = mr.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] && FixMaterialShader(mats[i]))
                    changed = true;
            }
        }

        // UI Images
        foreach (var img in go.GetComponentsInChildren<Image>(true))
        {
            if (img.material && FixMaterialShader(img.material))
                changed = true;
        }

        if (changed)
            EditorUtility.SetDirty(go);

        return changed;
    }
}
