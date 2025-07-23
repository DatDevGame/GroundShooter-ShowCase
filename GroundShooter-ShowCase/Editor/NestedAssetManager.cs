using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NestedAssetManager : EditorWindow
{
    private Object targetAsset;
    private string newAssetName = "NewNestedAsset";
    private Vector2 scrollPosition;
    private List<Object> nestedAssets = new List<Object>();
    private Dictionary<Object, string> renamingAssets = new Dictionary<Object, string>();

    [MenuItem("Tools/Nested Asset Manager")]
    public static void ShowWindow()
    {
        GetWindow<NestedAssetManager>("Nested Asset Manager");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Nested Asset Manager", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        targetAsset = EditorGUILayout.ObjectField("Target Asset", targetAsset, typeof(Object), false);
        if (EditorGUI.EndChangeCheck())
        {
            RefreshNestedAssets();
        }

        if (targetAsset == null)
        {
            EditorGUILayout.HelpBox("Select a target asset to manage its nested assets.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        DrawCreateAssetSection();
        EditorGUILayout.Space(10);
        DrawNestedAssetsSection();
    }

    private void DrawCreateAssetSection()
    {
        EditorGUILayout.LabelField("Create New Nested Asset", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            newAssetName = EditorGUILayout.TextField("New Asset Name", newAssetName);

            if (GUILayout.Button("Create Asset", GUILayout.Height(30)))
            {
                Vector2 mousePosition = Event.current.mousePosition;
                ShowCreateAssetMenu(mousePosition);
            }
        }
    }

    private void ShowCreateAssetMenu(Vector2 position)
    {
        var menu = new GenericMenu();

        // Add ScriptableObject-based assets
        var scriptableObjectTypes = TypeCache.GetTypesDerivedFrom<ScriptableObject>();
        foreach (var type in scriptableObjectTypes)
        {
            if (type.IsAbstract || type.IsInterface) continue;

            var menuPath = GetAssetMenuPath(type);
            if (string.IsNullOrEmpty(menuPath)) continue;

            menu.AddItem(new GUIContent(menuPath), false, () => CreateNestedAssetOfType(type));
        }

        // Add Material
        menu.AddItem(new GUIContent("Material"), false, () => CreateNestedMaterial());

        // Add other built-in asset types as needed
        menu.AddItem(new GUIContent("Animation/AnimationClip"), false, () => CreateNestedAnimationClip());

        menu.ShowAsContext();
    }

    private string GetAssetMenuPath(System.Type type)
    {
        var attr = System.Attribute.GetCustomAttribute(type, typeof(CreateAssetMenuAttribute)) as CreateAssetMenuAttribute;
        if (attr != null)
        {
            return attr.menuName;
        }
        return null;
    }

    private void CreateNestedAssetOfType(System.Type type)
    {
        var asset = ScriptableObject.CreateInstance(type);
        AddNestedAsset(asset);
    }

    private void CreateNestedMaterial()
    {
        var material = new Material(Shader.Find("Standard"));
        AddNestedAsset(material);
    }

    private void CreateNestedAnimationClip()
    {
        var clip = new AnimationClip();
        AddNestedAsset(clip);
    }

    private void AddNestedAsset(Object asset)
    {
        if (asset == null || targetAsset == null) return;

        string parentPath = AssetDatabase.GetAssetPath(targetAsset);
        if (string.IsNullOrEmpty(parentPath))
        {
            EditorUtility.DisplayDialog("Error", "Parent asset must be saved first!", "OK");
            return;
        }

        Undo.RegisterCreatedObjectUndo(asset, "Create Nested Asset");

        asset.name = newAssetName;
        AssetDatabase.AddObjectToAsset(asset, targetAsset);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.SetDirty(targetAsset);
        RefreshNestedAssets();
    }

    private void DrawNestedAssetsSection()
    {
        EditorGUILayout.LabelField("Current Nested Assets", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (nestedAssets.Count == 0)
            {
                EditorGUILayout.LabelField("No nested assets found", EditorStyles.boldLabel);
            }

            for (int i = 0; i < nestedAssets.Count; i++)
            {
                if (nestedAssets[i] == null) continue;

                EditorGUILayout.BeginHorizontal();

                // Show asset field
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(nestedAssets[i], nestedAssets[i].GetType(), false);
                EditorGUI.EndDisabledGroup();

                // Rename field
                if (!renamingAssets.ContainsKey(nestedAssets[i]))
                {
                    renamingAssets[nestedAssets[i]] = nestedAssets[i].name;
                }

                // Name text field
                renamingAssets[nestedAssets[i]] = EditorGUILayout.TextField(renamingAssets[nestedAssets[i]], GUILayout.Width(120));

                // Rename button
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(renamingAssets[nestedAssets[i]]) ||
                                           renamingAssets[nestedAssets[i]] == nestedAssets[i].name);
                if (GUILayout.Button("Rename", GUILayout.Width(60)))
                {
                    RenameNestedAsset(nestedAssets[i], renamingAssets[nestedAssets[i]]);
                }
                EditorGUI.EndDisabledGroup();

                // Remove button
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RemoveNestedAsset(nestedAssets[i]);
                    RefreshNestedAssets();
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void RenameNestedAsset(Object asset, string newName)
    {
        if (asset == null || string.IsNullOrEmpty(newName)) return;

        Undo.RecordObject(asset, "Rename Nested Asset");
        asset.name = newName;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    private void RemoveNestedAsset(Object assetToRemove)
    {
        if (assetToRemove == null) return;

        Undo.RecordObject(targetAsset, "Remove Nested Asset");

        // Remove from rename dictionary
        renamingAssets.Remove(assetToRemove);

        // Destroy the nested asset
        Undo.DestroyObjectImmediate(assetToRemove);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void RefreshNestedAssets()
    {
        nestedAssets.Clear();
        renamingAssets.Clear();

        if (targetAsset == null) return;

        string assetPath = AssetDatabase.GetAssetPath(targetAsset);
        if (string.IsNullOrEmpty(assetPath)) return;

        // Get all assets at the path
        Object[] assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object asset in assetsAtPath)
        {
            // Skip the main asset
            if (asset == targetAsset) continue;

            // Add nested assets to our list
            if (asset != null)
            {
                nestedAssets.Add(asset);
                renamingAssets[asset] = asset.name;
            }
        }
    }
}