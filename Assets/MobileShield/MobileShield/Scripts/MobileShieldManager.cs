using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System.Collections;

/// <summary>
/// Mobile Shield v1.0.0
/// Runtime Application Self-Protection for Mobile.
///
/// SETUP:
/// 1. Attach this script to a GameObject in your FIRST scene only
/// 2. Set Security Scene Name to the scene shown on threat detection
/// 3. In your security scene read:
///      MobileShieldManager.Instance.ThreatMessage
/// 4. Set your APK signing hash in the Inspector
///
/// CALLBACKS — subscribe anytime before checks complete:
///
///   // Called for each individual threat as it is detected
///   MobileShieldManager.Instance.OnThreatDetected += (msg) => { };
///
///   // Called once when all checks are done, clean or not
///   MobileShieldManager.Instance.OnChecksComplete += (hasThreats) => { };
///
/// Singleton — survives scene loads. Only one instance allowed.
/// </summary>
public class MobileShieldManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────
    public static MobileShieldManager Instance { get; private set; }

    // Security scene reads this to display the threat reason
    public string ThreatMessage { get; private set; } = "";

    // ── Callbacks ─────────────────────────────────────────────────

    /// <summary>
    /// Fired for each individual threat as it is detected.
    /// string = threat description.
    /// </summary>
    public event System.Action<string> OnThreatDetected;

    /// <summary>
    /// Fired once when all checks are complete.
    /// bool = true if any threat was found, false if device is clean.
    /// </summary>
    public event System.Action<bool> OnChecksComplete;

    // ── Inspector ─────────────────────────────────────────────────

    [Header("Response")]
    [Tooltip("Scene to load when a threat is detected.\n" +
             "Leave empty to suppress auto scene load\n" +
             "and handle via OnChecksComplete instead.")]
    [SerializeField] private string securitySceneName = "SecurityAlert";

    [Header("Integrity")]
    [Tooltip("Your release APK signing certificate hash (Base64 SHA-256).\n" +
             "Leave empty to skip signature check.\n" +
             "Get it: build release APK, run app, then:\n" +
             "adb logcat -s Unity | findstr \"[MSH]\"")]
    [SerializeField] private string signingCertHash = "";

    [Tooltip("Block apps installed outside official stores.")]
    [SerializeField] private bool blockUnofficialInstall = false;

    [Tooltip("Block devices with Developer Options / USB Debugging enabled.")]
    [SerializeField] private bool blockDeveloperOptions = false;

    [Header("Timing")]
    [Tooltip("Seconds to wait before running checks.")]
    [SerializeField] private float startDelay = 1.5f;

    [Header("Logging")]
    [Tooltip("Enable detailed logs. Disable in production.")]
    [SerializeField] private bool enableLogs = true;

#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("native_security")] private static extern int z1a2b3c4();
    [DllImport("native_security")] private static extern int q9w8e7r6();
    [DllImport("native_security")] private static extern int m5n4p3o2();
    [DllImport("native_security")] private static extern int k7j6h5g4();
    [DllImport("native_security")] private static extern int x3y2z1w0();
#endif

    private const int _f0 = 1 << 0;
    private const int _f1 = 1 << 1;
    private const int _f2 = 1 << 2;
    private const int _f3 = 1 << 3;

    private const string _tag = "[MSH]";

    // ── Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            _Log("Duplicate instance destroyed.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
        _Log("Initialized.");
    }

    private void Start()
    {
        ThreatMessage = "";
        _Log($"Starting checks in {startDelay}s...");
        StartCoroutine(_Delay());
    }

    private IEnumerator _Delay()
    {
        yield return new WaitForSeconds(startDelay);
        _Complete();
    }

    private void _Complete()
    {
        _Log("Running all security checks...");
        _RunAll();

        bool _hasThreats = !string.IsNullOrEmpty(ThreatMessage);

        if (_hasThreats)
        {
            _LogWarn($"Security breach detected:\n{ThreatMessage}");

            if (!string.IsNullOrEmpty(securitySceneName))
            {
                _Log($"Loading security scene: {securitySceneName}");
                SceneManager.LoadScene(securitySceneName);
            }
            else
            {
                _Log("No security scene set — handle via OnChecksComplete.");
            }
        }
        else
        {
            _Log("All checks passed. Device is clean.");
        }

        OnChecksComplete?.Invoke(_hasThreats);
    }

    // ── Checks ────────────────────────────────────────────────────

    private void _RunAll()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _N();
        _P();
        _I();
        _U();
        _D();
#else
        _Log("Editor mode — all checks skipped.");
#endif
    }

    private void _N()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _Log("Running native checks (root / hooking / debugger / emulator)...");
        try
        {
            int _a  = z1a2b3c4();
            var _b  = Time.realtimeSinceStartup;
            int _c  = q9w8e7r6();
            var _e  = Application.version;
            int _g  = m5n4p3o2();
            var _h  = SystemInfo.deviceModel;
            int _j  = k7j6h5g4();
            int _kv = x3y2z1w0();

            _Log($"Native results: a={_a} c={_c} g={_g} j={_j} combined={_kv}");

            bool _t0 = (_a != 0) || ((_kv & _f0) != 0);
            bool _t1 = (_c != 0) || ((_kv & _f1) != 0);
            bool _t2 = (_g != 0) || ((_kv & _f2) != 0);
            bool _t3 = (_j != 0) || ((_kv & _f3) != 0);
            bool _t4 = _kv == 0xF;

            if (_t4 || _t0) _Add("Root or Jailbreak detected. This poses a high risk.");
            if (_t1)        _Add("Runtime code injection or hooking detected.");
            if (_t2)        _Add("Active debugger detected.");
            if (_t3)        _Add("Running on an emulator or simulator.");

            if (!_t4 && !_t0 && !_t1 && !_t2 && !_t3)
                _Log("Native checks: clean.");
        }
        catch (System.Exception _ex)
        {
            _LogWarn($"Native checks failed: {_ex.Message}");
        }
#endif
    }

    private void _P()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _Log("Checking for privileged/root apps...");
        try
        {
            string[] _pkgs = {
                "com.topjohnwu.magisk",       "com.noshufou.android.su",
                "eu.chainfire.supersu",        "com.koushikdutta.superuser",
                "com.thirdparty.superuser",    "com.zachspong.temprootremovejb",
                "com.ramdroid.appquarantine",  "com.kingroot.kinguser",
                "com.kingo.root",              "com.smedialink.oneclickroot"
            };
            using (var _up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var _ac = _up.GetStatic<AndroidJavaObject>("currentActivity");
                var _pm = _ac.Call<AndroidJavaObject>("getPackageManager");
                foreach (var _pkg in _pkgs)
                {
                    try
                    {
                        _pm.Call<AndroidJavaObject>("getPackageInfo", _pkg, 0);
                        _LogWarn($"Root app found: {_pkg}");
                        _Add("Root or Jailbreak detected. This poses a high risk.");
                        return;
                    }
                    catch { }
                }
            }
            _Log("Privileged app check: clean.");
        }
        catch (System.Exception _ex)
        {
            _LogWarn($"Privileged app check failed: {_ex.Message}");
        }
#endif
    }

    private void _I()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _Log("Checking app integrity (signature)...");
        try
        {
            using (var _up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var _ac  = _up.GetStatic<AndroidJavaObject>("currentActivity");
                var _pm  = _ac.Call<AndroidJavaObject>("getPackageManager");
                var _pkg = _ac.Call<string>("getPackageName");
                var _pi  = _pm.Call<AndroidJavaObject>("getPackageInfo", _pkg, 64);
                var _sg  = _pi.Get<AndroidJavaObject[]>("signatures");

                if (_sg == null || _sg.Length == 0)
                {
                    _LogWarn("No signatures found in APK.");
                    _Add("Application integrity violation detected (Tampered).");
                    return;
                }

                byte[] _sb = _sg[0].Call<byte[]>("toByteArray");
                using (var _sha = System.Security.Cryptography.SHA256.Create())
                {
                    string _hv = System.Convert.ToBase64String(_sha.ComputeHash(_sb));
                    _Log($"APK signature hash: {_hv}");

                    if (string.IsNullOrEmpty(signingCertHash))
                        _Log("No expected hash set — skipping signature validation.");
                    else if (_hv != signingCertHash)
                    {
                        _LogWarn("Signature mismatch — APK tampered.");
                        _Add("Application integrity violation detected (Tampered).");
                    }
                    else
                        _Log("Integrity check: clean.");
                }
            }
        }
        catch (System.Exception _ex)
        {
            _LogWarn($"Integrity check failed: {_ex.Message}");
        }
#endif
    }

    private void _U()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!blockUnofficialInstall)
        {
            _Log("Unofficial install check: disabled (enable in Inspector).");
            return;
        }

        _Log("Checking install source...");
        try
        {
            using (var _up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var _ac  = _up.GetStatic<AndroidJavaObject>("currentActivity");
                var _pm  = _ac.Call<AndroidJavaObject>("getPackageManager");
                var _pkg = _ac.Call<string>("getPackageName");

                string[] _os = {
                    "com.android.vending",
                    "com.huawei.appmarket",
                    "com.samsung.android.app.store"
                };

                string _inst = "";
                try { _inst = _pm.Call<string>("getInstallerPackageName", _pkg) ?? ""; }
                catch { }

                _Log($"Installer package: '{_inst}'");

                bool _ok = false;
                foreach (var _s in _os)
                    if (_inst == _s) { _ok = true; break; }

                if (!_ok)
                {
                    _LogWarn("App not installed from an official store.");
                    _Add("Application installed from an unofficial source.");
                }
                else
                    _Log("Install source check: clean.");
            }
        }
        catch (System.Exception _ex)
        {
            _LogWarn($"Install source check failed: {_ex.Message}");
        }
#endif
    }

    private void _D()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!blockDeveloperOptions)
        {
            _Log("Developer options check: disabled (enable in Inspector).");
            return;
        }

        _Log("Checking developer options...");
        try
        {
            using (var _up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var _ac = _up.GetStatic<AndroidJavaObject>("currentActivity");
                var _cr = _ac.Call<AndroidJavaObject>("getContentResolver");
                using (var _st = new AndroidJavaClass("android.provider.Settings$Global"))
                {
                    int _v = _st.CallStatic<int>("getInt", _cr, "adb_enabled", 0);
                    if (_v == 1)
                    {
                        _LogWarn("Developer Options / ADB enabled.");
                        _Add("Developer Options enabled (e.g., USB Debugging).");
                    }
                    else
                        _Log("Developer options check: clean.");
                }
            }
        }
        catch (System.Exception _ex)
        {
            _LogWarn($"Developer options check failed: {_ex.Message}");
        }
#endif
    }

    // ── Helpers ───────────────────────────────────────────────────

    private void _Add(string _msg)
    {
        ThreatMessage = string.IsNullOrEmpty(ThreatMessage)
            ? $"Security Risk Detected:\n- {_msg}"
            : ThreatMessage + $"\n- {_msg}";

        OnThreatDetected?.Invoke(_msg);
    }

    private void _Log(string _msg)
    {
        if (enableLogs) Debug.Log($"{_tag} {_msg}");
    }

    private void _LogWarn(string _msg)
    {
        if (enableLogs) Debug.LogWarning($"{_tag} {_msg}");
    }
}