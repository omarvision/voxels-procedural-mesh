
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainHeightTexture))]
public class TerrainHeightTextureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TerrainHeightTexture script = (TerrainHeightTexture)target;

        GUI.backgroundColor = Color.yellow;

        if (GUILayout.Button("GenerateHeight") == true)
        {
            script.heightparam.depth = Random.Range(0.1f, 1.5f);
            script.heightparam.wavesX = Random.Range(0.25f, 8.0f);
            script.heightparam.wavesZ = Random.Range(0.25f, 8.0f);
            script.heightparam.perlinOffsetX = Random.Range(0.0f, 512.0f);
            script.heightparam.perlinOffsetZ = Random.Range(0.0f, 512.0f);
            script.GenerateHeight();
        }

        if (GUILayout.Button("GenerateTexture") == true)
        {
            script.GenerateTexture();
        }        
    }
}
