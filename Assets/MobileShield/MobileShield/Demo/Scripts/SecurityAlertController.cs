using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mobile Shield v1.0.0 — Security Alert Scene Controller
///
/// Place this in your security alert scene.
/// Reads ThreatMessage directly from MobileShieldManager.Instance,
/// shows a countdown, then kills the application.
/// </summary>
public class SecurityAlertController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Image backgroundPanel;

    [Header("Settings")]
    [SerializeField] private float countdownSeconds = 5f;
    [SerializeField] private Color alertColor = new Color(0.8f, 0.1f, 0.1f, 1f);

    private void Start()
    {
    
        if (backgroundPanel != null)
            backgroundPanel.color = alertColor;

        if (titleText != null)
            titleText.text = "Security Alert";

        // Read directly from the singleton
        if (messageText != null)
        {
            string threat = MobileShieldManager.Instance?.ThreatMessage;
            messageText.text = !string.IsNullOrEmpty(threat)
                ? threat
                : "Security violation detected.";
        }

        StartCoroutine(_Countdown());
    }

    private IEnumerator _Countdown()
    {
        float _r = countdownSeconds;
        while (_r > 0)
        {
            if (countdownText != null)
                countdownText.text = $"Closing in {Mathf.CeilToInt(_r)}s";
            yield return new WaitForSeconds(1f);
            _r--;
        }

        if (countdownText != null)
            countdownText.text = "Closing...";

        _End();
    }

    private void _End()
    {
        Application.Quit();
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var _p = new AndroidJavaClass("android.os.Process"))
                _p.CallStatic("killProcess", _p.CallStatic<int>("myPid"));
        }
        catch { }
#endif
    }
}