#if (UNITY_EDITOR) 
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ModelManager))]
[CanEditMultipleObjects]
public class ModelManagerEditor : Editor {

    ModelManager ModelManager;

    SerializedProperty ModelRoot;
    SerializedProperty NodePrefab;
    SerializedProperty xmlFile;
    SerializedProperty LineMaterial;
    SerializedProperty ColorNegocio;
    SerializedProperty ColorEstrategia;
    SerializedProperty ColorAplicacion;
    SerializedProperty ColorTecnologia;
    SerializedProperty AutoLoadModel;
    
    const float buttonWidth = 90.0f;
    
    private void OnEnable()
    {
        ModelManager = this.target as ModelManager;
        ModelRoot = serializedObject.FindProperty("ModelRoot");
        NodePrefab = serializedObject.FindProperty("NodePrefab");
        xmlFile = serializedObject.FindProperty("xmlFile");
        LineMaterial = serializedObject.FindProperty("LineMaterial");
        ColorNegocio = serializedObject.FindProperty("ColorNegocio");
        ColorEstrategia = serializedObject.FindProperty("ColorEstrategia");
        ColorAplicacion = serializedObject.FindProperty("ColorAplicacion");
        ColorTecnologia = serializedObject.FindProperty("ColorTecnologia");
        AutoLoadModel = serializedObject.FindProperty("AutoLoadModel");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!ModelManager.AutoLoadModel)
        {
            GUILayout.Label("Actions", EditorStyles.boldLabel);
            if (GUILayout.Button("Load Model", GUILayout.Width(buttonWidth)))
            {
                ModelManager.LoadModel();
            }
            if (GUILayout.Button("RecalculateRelations", GUILayout.Width(buttonWidth)))
            {
                ModelManager.RecalculateRelations();
            }
        }
    }
}
#endif