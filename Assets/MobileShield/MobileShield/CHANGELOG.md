# Mobile Shield — Changelog

## v1.0.0 (April 2026)
- Initial release
- Native C++ root detection (su binaries, root manager tools, test-keys, /system write)
- Native hooking-framework detection (port 27042, /proc/maps, fd scan)
- Native debugger detection (TracerPid, ptrace)
- Native emulator detection (qemu files, system properties)
- Self-integrity check (detects hooks on own native functions)
- Java-layer root app package detection
- APK signature verification (tamper detection)
- Unofficial store detection
- Developer Options / ADB detection
- 16KB page size compatible (Android 15+)
- Supports Unity 2020.3 and higher
- arm64-v8a architecture
- Zero external dependencies
- No internet required
