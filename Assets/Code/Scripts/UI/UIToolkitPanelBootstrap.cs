using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIToolkitPanelBootstrap
{
    private static readonly Type PanelTextSettingsType =
        Type.GetType("UnityEngine.UIElements.PanelTextSettings, UnityEngine.UIElementsModule");

    private static readonly PropertyInfo TextSettingsProperty =
        typeof(PanelSettings).GetProperty("textSettings", BindingFlags.Instance | BindingFlags.Public);

    public static void EnsureTextSettings(UIDocument uiDocument)
    {
        if (uiDocument == null)
        {
            return;
        }

        PanelSettings panelSettings = uiDocument.panelSettings;
        if (panelSettings == null || TextSettingsProperty == null || PanelTextSettingsType == null)
        {
            return;
        }

        if (TextSettingsProperty.GetValue(panelSettings) != null)
        {
            return;
        }

        ScriptableObject runtimeTextSettings = ScriptableObject.CreateInstance(PanelTextSettingsType);
        runtimeTextSettings.name = "RuntimePanelTextSettings";
        runtimeTextSettings.hideFlags = HideFlags.DontSave;
        TextSettingsProperty.SetValue(panelSettings, runtimeTextSettings);
    }
}
