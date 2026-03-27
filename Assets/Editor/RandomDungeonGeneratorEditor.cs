using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class RandomDungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        AbstractDungeonGenerator generator = (AbstractDungeonGenerator)target;

        if (GUILayout.Button("Create Dungeon"))
        {
            GenerateDungeonWithUndo(generator);
        }
    }

    private void GenerateDungeonWithUndo(AbstractDungeonGenerator generator)
    {
        if (generator == null)
            return;

        Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Create Dungeon");

        Tilemap[] tilemaps = generator.GetComponentsInChildren<Tilemap>(true);
        for (int i = 0; i < tilemaps.Length; i++)
        {
            Undo.RegisterFullObjectHierarchyUndo(tilemaps[i].gameObject, "Create Dungeon");
        }

        generator.GenerateDungeon();

        EditorUtility.SetDirty(generator);

        for (int i = 0; i < tilemaps.Length; i++)
        {
            EditorUtility.SetDirty(tilemaps[i]);
        }

        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
        }
    }
}