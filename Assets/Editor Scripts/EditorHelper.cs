using UnityEditor;
using UnityEngine;

public static class InspectorLockToggle
{
    [MenuItem("Tools/Toggle Inspector Lock %l")] // Ctrl + L (Cmd + L on Mac)
    private static void ToggleInspectorLock()
    {
        var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        var window = EditorWindow.GetWindow(inspectorType);

        var isLockedProperty = inspectorType.GetProperty("isLocked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        bool currentLock = (bool)isLockedProperty.GetValue(window);
        isLockedProperty.SetValue(window, !currentLock);
        isLockedProperty.SetValue(window, currentLock);

        window.Repaint();
    }
}
