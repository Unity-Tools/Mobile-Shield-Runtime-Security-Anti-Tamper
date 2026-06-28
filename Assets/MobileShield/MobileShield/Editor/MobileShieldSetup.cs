using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

/// <summary>
/// Mobile Shield — Editor Setup Wizard
/// Tools > Mobile Shield > Setup Wizard
/// </summary>
public class MobileShieldSetup : EditorWindow
{
    private string _sceneName = "SecurityAlert";
    private string _certHash = "";
    private bool _blockUnofficial = false;
    private bool _blockDevOptions = false;

    [MenuItem("Tools/Mobile Shield/Setup Wizard")]
    public static void ShowWindow()
    {
        var w = GetWindow<MobileShieldSetup>("MSH Setup");
        w.minSize = new Vector2(420, 400);
    }

    [MenuItem("Tools/Mobile Shield/Documentation")]
    public static void OpenDocs()
    {
        string[] guids = AssetDatabase.FindAssets("README", new[] { "Assets" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("MobileShield") && path.Contains("Documentation"))
            {
                string fullPath = Path.GetFullPath(path);
                Application.OpenURL("file:///" + fullPath.Replace("\\", "/"));
                return;
            }
        }

        EditorUtility.DisplayDialog(
            "Documentation Not Found",
            "Could not locate the README file.\n" +
            "Please check: Assets/MobileShield/Documentation/README.md\n\n" +
            "Contact: lailaahmed18@outlook.com",
            "OK");
    }

    [MenuItem("Tools/Mobile Shield/Contact Support")]
    public static void ContactSupport()
    {
        try
        {
            // Use Process.Start which correctly opens mailto: on all platforms
            Process.Start(new ProcessStartInfo
            {
                FileName = "mailto:lailaahmed18@outlook.com" +
                                  "?subject=Native%20Security%20Kit%20Support",
                UseShellExecute = true
            });
        }
        catch
        {
            // Fallback — copy email to clipboard and show dialog
            EditorGUIUtility.systemCopyBuffer = "lailaahmed18@outlook.com";
            EditorUtility.DisplayDialog(
                "Contact Support",
                "Email address copied to clipboard:\n\nlailaahmed18@outlook.com",
                "OK");
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Mobile Shield v1.0.0",
            EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Runtime Application Self-Protection for Mobile",
            EditorStyles.miniLabel);

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "These settings will be applied to the MobileShieldManager " +
            "component added to your first scene.",
            MessageType.Info);

        GUILayout.Space(10);
        _sceneName = EditorGUILayout.TextField("Security Scene Name", _sceneName);

        GUILayout.Space(5);
        _certHash = EditorGUILayout.TextField("APK Signing Hash", _certHash);
        EditorGUILayout.HelpBox(
            "Get your signing hash: build a release APK, install it, run the app, then:\n" +
            "adb logcat -s Unity | findstr \"[MSH]\"\n" +
            "Copy the hash from the line showing 'APK signature hash:'",
            MessageType.Info);

        GUILayout.Space(10);
        _blockUnofficial = EditorGUILayout.Toggle(
            "Block Unofficial Installs", _blockUnofficial);
        _blockDevOptions = EditorGUILayout.Toggle(
            "Block Developer Options", _blockDevOptions);

        GUILayout.Space(20);
        if (GUILayout.Button("Add to Current Scene", GUILayout.Height(35)))
            AddToScene();

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Documentation"))
            OpenDocs();
        if (GUILayout.Button("Contact Support"))
            ContactSupport();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);
        EditorGUILayout.LabelField(
            "Mobile Shield — lailaahmed18@outlook.com",
            EditorStyles.centeredGreyMiniLabel);
    }

    private void AddToScene()
    {
        var existing = FindObjectOfType<MobileShieldManager>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog(
                "Already Exists",
                "MobileShieldManager already exists in this scene.",
                "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        var go = new GameObject("MobileShield");
        var mgr = go.AddComponent<MobileShieldManager>();

        var so = new SerializedObject(mgr);
        so.FindProperty("securitySceneName").stringValue = _sceneName;
        so.FindProperty("signingCertHash").stringValue = _certHash;
        so.FindProperty("blockUnofficialInstall").boolValue = _blockUnofficial;
        so.FindProperty("blockDeveloperOptions").boolValue = _blockDevOptions;
        so.ApplyModifiedProperties();

        Selection.activeGameObject = go;
        Undo.RegisterCreatedObjectUndo(go, "Add MobileShield");

        EditorUtility.DisplayDialog(
            "Setup Complete",
            "MobileShield added to scene.\n\n" +
            "Assign your APK signing hash in the Inspector " +
            "to enable tamper detection.",
            "OK");
    }
}