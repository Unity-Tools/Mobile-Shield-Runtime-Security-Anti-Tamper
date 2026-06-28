# Mobile Shield v1.0.0
**Private Package — Laila Omran**
**lailaahmed18@outlook.com**

---

## Overview

Mobile Shield is a Runtime Application Self-Protection (RASP) solution for Unity Android. It detects common security threats at runtime and shuts down your app before it can be tampered with, reverse engineered, or run in an unsafe environment.

Built from scratch after a full penetration test cycle on a production Unity mobile application. Zero external dependencies, no Java bridge, and no public bypass scripts.

---

## What It Detects

| Threat | Description |
|--------|-------------|
| Root & Jailbreak | Detects rooted devices using multiple signals |
| Runtime hooking & code injection | Detects dynamic instrumentation tools injected into your app's process |
| Active debugger | Detects a debugger attached to the running app |
| Emulator & simulator | Detects Android emulators and virtual devices |
| APK tampering & repackaging | Detects if your APK was modified and re-signed |
| Unofficial install source | Detects installs from outside official app stores |
| Developer Options / USB Debugging | Detects devices with ADB enabled |
| Native function hooking | Detects if the security checks themselves have been tampered with |

**29+ individual detection signals across 8 threat categories.**

All checks run on Android device builds only — they are automatically skipped in the Unity Editor.

---

## Architecture

```
MobileShieldManager.cs       (C# — attach to your first scene)
         │
         ▼
libnative_security.so        (native library — performs the actual checks)
```

`MobileShieldManager` is the only script you interact with. It drives all checks automatically and loads your security alert scene when a threat is found. The native library handles the low-level detection internally — no configuration needed.

---

## Files

```
MobileShield/
├── Scripts/
│   └── MobileShieldManager.cs        ← Attach to a GameObject in your FIRST scene only
├── Demo/Scripts/
│   └── SecurityAlertController.cs    ← Pre-attached in SecurityAlert.unity
├── Demo/Scene/
│   └── SecurityAlert.unity           ← Pre-built alert scene (add to Build Settings)
├── Plugins/Android/
│   └── libnative_security.so         ← Prebuilt native library (do not modify)
├── Editor/
│   └── MobileShieldSetup.cs          ← Tools > Mobile Shield > Setup Wizard
├── Documentation/
│   └── README.md                     ← This file
├── package.json
├── CHANGELOG.md
└── LICENSE.md
```

---

## Setup

### Step 1 — Add MobileShieldManager to Your First Scene

Go to **Tools → Mobile Shield → Setup Wizard**

Or manually: create an empty GameObject in your startup scene, name it `MobileShieldManager`, and attach the `MobileShieldManager` script.

> Place it in your **first/startup scene only**. The script persists across all scene loads automatically. Never place it in the SecurityAlert scene.

### Step 2 — Configure Inspector Fields

| Field | Description |
|-------|-------------|
| Security Scene Name | Scene to load when a threat is detected (default: `SecurityAlert`) |
| Signing Cert Hash | Your release APK signing hash — leave empty to skip tamper check |
| Block Unofficial Install | Toggle — blocks APKs not installed from official app stores |
| Block Developer Options | Toggle — blocks devices with USB Debugging / ADB enabled |
| Start Delay | Seconds to wait before checks run (default: `1.5`) |
| Enable Logs | Enables `[MSH]` logcat output — disable before releasing to production |

### Step 3 — Add Scenes to Build Settings

Go to **File → Build Settings → Scenes In Build** and add both scenes:

```
Index 0 — your startup/main scene     ← must contain MobileShieldManager
Index 1 — SecurityAlert.unity         ← Assets/MobileShield/MobileShield/Demo/Scene/
```

> `SecurityAlert` must be in the build or the alert scene will never load. It must not be the first scene — your main scene needs to run first so checks can execute.

### Step 4 — Import TextMeshPro Essential Resources

`SecurityAlertController` uses TextMeshPro for all on-screen text. Without this step, all text will be invisible in device builds.

Go to **Window → TextMeshPro → Import TMP Essential Resources** and import. This only needs to be done once per project.

### Step 5 — Get Your APK Signing Hash

Leave **Signing Cert Hash** empty on your first build. Install and run the APK on a device, then run:

```
adb logcat -s Unity | findstr "[MSH]"
```

Find this line in the output:
```
[MSH] APK signature hash: <your_hash_here>
```

Copy that value into the **Signing Cert Hash** field in the Inspector and rebuild. From this point on, any repackaged or tampered APK will be detected and blocked.

### Step 6 — Security Alert Scene

The `SecurityAlert.unity` scene is ready to use out of the box. `SecurityAlertController` is already attached and wired up — it reads the threat details from `MobileShieldManager` automatically, shows a 5-second countdown, and closes the app.

You can customize the look of the scene freely. The four wired references in the Inspector are:

| Field | Content shown |
|-------|---------------|
| Title Text | "Security Alert" |
| Message Text | The detected threat description |
| Countdown Text | "Closing in Ns" |
| Background Panel | Red background (color configurable via `Alert Color`) |

---

## Execution Flow

```
App launches
    ↓
MobileShieldManager initializes — persists across all scenes
    ↓  (waits Start Delay seconds)
All security checks run
    ↓
No threats found → app continues normally
    ↓
Threat found → SecurityAlert scene loads
    ↓
Threat message displayed — 5-second countdown
    ↓
App is closed
```

---

## Callbacks

You can subscribe to events anywhere in your project if you want to handle threats yourself instead of relying on the automatic scene load:

```csharp
// Called once for each threat as it is detected
MobileShieldManager.Instance.OnThreatDetected += (message) =>
{
    Debug.Log("Threat: " + message);
};

// Called once when all checks finish
// hasThreats = true if any threat was found
MobileShieldManager.Instance.OnChecksComplete += (hasThreats) =>
{
    if (hasThreats) { /* handle it */ }
};
```

To disable the automatic scene load and handle everything yourself, leave **Security Scene Name** empty in the Inspector.

---

## Logs Reference

All logs are prefixed with `[MSH]` and can be toggled via **Enable Logs** in the Inspector.

```
[MSH] Initialized.
[MSH] Starting checks in 1.5s...
[MSH] All checks passed. Device is clean.
```

When a threat is found:
```
[MSH] Security breach detected:
[MSH] Loading security scene: SecurityAlert
```

Filter in logcat:
```
adb logcat -s Unity | findstr "[MSH]"
```

Always disable **Enable Logs** before releasing to production.

---

## Compatibility

| Unity Version | Status |
|---------------|--------|
| Unity 6 (6000.x) | ✅ Built and tested |
| Unity 2022 LTS | ✅ Supported |
| Unity 2021 LTS | ✅ Supported |
| Unity 2020.3 LTS | ✅ Minimum supported |

- Android API 23+ (Android 6.0 Marshmallow)
- arm64-v8a only
- IL2CPP recommended (Mono supported)
- 16KB page size compatible (Android 15+)
- TextMeshPro required
- Zero external dependencies
- No internet required

---

## Native Function Reference

The native library exports 5 obfuscated symbols via P/Invoke. These are intentionally named to prevent symbol-based hooking.

| Symbol | What It Does |
|--------|--------------|
| `z1a2b3c4` | Root detection |
| `q9w8e7r6` | Hooking / instrumentation detection |
| `m5n4p3o2` | Debugger detection |
| `k7j6h5g4` | Emulator detection |
| `x3y2z1w0` | Combined bitmask + self-integrity check on all 4 above |

Each function returns `0` (clean) or `1` (threat detected). `x3y2z1w0` returns a bitmask where each bit maps to one check, and returns `0xF` (all bits set) if any of the exported functions have been hooked at the native level.

These are called internally by `MobileShieldManager` — you do not call them directly.

---

## Contact

Laila Omran
lailaahmed18@outlook.com
