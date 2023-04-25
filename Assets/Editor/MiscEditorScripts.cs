using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class MiscEditorScripts : MonoBehaviour
{
    [MenuItem("Misc/CreateLineMaterial")]
    static void CreateLineMaterial()
    {
        // simple colored things.
        var shader = Shader.Find("Hidden/Internal-Colored");
        var lineMaterial = new Material(shader);
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);
        AssetDatabase.CreateAsset(lineMaterial, "Assets/Materials/LineMaterial.mat");

        AssetDatabase.SaveAssets();
    }
    [MenuItem("Misc/ToggleTheme")]
    static void ToggleTheme()
    {
        InternalEditorUtility.SwitchSkinAndRepaintAllViews();
    }
}
