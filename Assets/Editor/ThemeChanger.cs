using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ThemeChanger : MonoBehaviour
{
    [MenuItem("ThemeChanger/Toggle Theme")]
    static void ToggleTheme()
    {
        InternalEditorUtility.SwitchSkinAndRepaintAllViews();
    }
}
