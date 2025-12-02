# USB-Security-Monitor

[![VB.NET](https://img.shields.io/badge/Language-VB.NET-blue?style=flat&logo=visualbasic)](https://learn.microsoft.com/en-us/dotnet/visual-basic/) [![Windows](https://img.shields.io/badge/Platform-Windows-green?style=flat&logo=windows)](https://www.microsoft.com/windows)

## Overview
**USB-Security-Monitor** is a lightweight Windows security tool designed to protect your PC from unauthorized access and risky USB insertions. It acts as a boot-time sentinel, enforcing password verification and automated responses to threats. Perfect for shared environments, kiosks, or personal paranoia-proofing!

### Key Purposes
- **Prevent Unauthorized Use**: Ensures others cannot secretly use your PC.
- **USB Security**: Automatically formats inserted USB drives to block malware or data infiltration.
- **Emergency Response**: On password errors, sends alerts (via Discord webhook) and initiates shutdown, but provides a cancel option for legitimate users (emergency password).

This program is a VB.NET-based Windows Forms app that runs automatically on boot via startup registration. (Current version: v1.0, as of November 28, 2025)

## Key Features
- **Boot-Time Password Lock**: After 30 seconds of startup, prompts for a normal password. Wrong entry triggers a 2-minute shutdown countdown.
- **Emergency Override**: During shutdown, enter the emergency password within 2 minutes to cancel the shutdown.
- **USB Drive Protection**: If enabled, automatically formats any inserted USB drive (NTFS, quick format) and restarts the password prompt + shutdown sequence.
- **Stealth Monitoring**: Captures screenshots, webcam photos (if enabled), running app titles, and sends alerts via Discord webhook for remote logging.
- **Easy Configuration**: On first run (or if config file is missing), sets up normal/emergency passwords and Discord webhook. Config stored securely in `%APPDATA%\HUK_CHK.NFO` (delete this file to reset).
- **No Bloat**: Built with VB.NET and Emgu.CV for webcam support. Minimal dependencies.

| Feature | Description | Enabled By Default? |
|---------|-------------|---------------------|
| Password Prompt | 30s post-boot | Yes |
| USB Auto-Format | Formats on insert | UI Checkbox |
| Webcam Capture | Snapshot on alert | UI Checkbox |
| Discord Alerts | Sends logs/screenshots | Config via webhook URL |

## Installation & Setup
### Requirements
- **OS**: Windows 10/11 (.NET Framework 4.7.2+).
- **Libraries**: Emgu.CV (webcam), Newtonsoft.Json (Discord JSON).
- **Admin Privileges**: Required for formatting/shutdown (UAC popup may appear).

### Installation Steps
1. **Download**: Download `USB-Security-Monitor-v1.0.zip` from the GitHub release.
2. **Extract ZIP**:
   - Unzip to a dedicated folder, e.g., `C:\Program Files\USB-Security-Monitor` (create if needed). Avoid spaces in the path for better compatibility.
3. **Add to Startup**:
   - Press `Win + R`, type `shell:startup`, and press Enter to open the Startup folder.
   - Right-click `USB-Security-Monitor.exe` in the extracted folder > Create Shortcut.
   - Copy the shortcut to the Startup folder.
   - Alternatively, via registry: `reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "USBSecurity" /t REG_SZ /d "C:\Path\To\USB-Security-Monitor.exe" /f`.
4. **First-Time Setup**:
   - Restart your PC – it will prompt for setup on first boot.
   - Enter a strong **normal password** (for daily use).
   - Set an **emergency password** (for shutdown cancellation).
   - (Optional) Add a Discord webhook URL (e.g., from Discord channel settings > Integrations > Webhooks).
5. **Enable Options**: Launch the app (or via config button) to toggle USB formatting and webcam capture.
6. **Reset Config**: Forgot passwords? Delete `%APPDATA%\HUK_CHK.NFO` and relaunch.

**Pro Tip**: Test in a virtual machine first! If issues arise, kill the process via Task Manager.

### UI Elements
- **Check_cam**: Checkbox to enable/disable webcam capture.
- **Check_usb**: Checkbox to enable/disable USB formatting.
- **Button_config**: Settings change button (verifies normal password before dialog).
- **Button_OK**: Emergency cancel button (during shutdown sequence).

## Usage Notes
- **Shutdown Behavior**: Uses Windows `shutdown /s /t 120` command. Emergency cancel with `/a`.
- **USB Formatting**: Irreversible! Only enable if you're sure – it runs `format /FS:NTFS /Q /Y` silently.
- **Dependencies**: Requires .NET Framework 4.7+ and Emgu.CV (bundled). Webcam needs a compatible camera.
- **Logging**: All alerts include IP, hostname, username, timestamp, and app snapshots.

### Testing
- Insert USB drive: Confirm "Format USB" checkbox, then check for format + password dialog.
- Password error: Discord channel alert + shutdown countdown.
- Emergency password: Verify cancellation.
- Settings change: Click "Settings" button > Verify password > Input new values.

## Warnings & Disclaimer
- **⚠️ High Risk**: This tool can **permanently delete data** (USB format) or **force shutdowns**. Use responsibly – not for production without backups.
- **Legal/Ethical**: Intended for personal/shared PC protection. Do not deploy on others' systems without consent.

## Changelog
- **v1.0 (Initial Release)**: Full implementation with password dialogs, USB watcher, Discord integration, and startup registration support.
- Fixed: Config file handling for `%APPDATA%` path.
- Added: Webcam initialization checks and error handling.


===============================================


# USB-Security-Monitor

[![VB.NET](https://img.shields.io/badge/Language-VB.NET-blue?style=flat&logo=visualbasic)](https://learn.microsoft.com/en-us/dotnet/visual-basic/) [![Windows](https://img.shields.io/badge/Platform-Windows-green?style=flat&logo=windows)](https://www.microsoft.com/windows)

## 개요
**USB-Security-Monitor**는 무단 접근과 위험한 USB 삽입으로부터 PC를 보호하기 위한 가벼운 Windows 보안 도구입니다. 부팅 시 센티널 역할을 하며, 비밀번호 검증과 위협에 대한 자동 응답을 강제합니다. 공유 환경, 키오스크, 또는 개인적인 편집증 방지용으로 완벽합니다!

### 주요 취지
- **무단 사용 방지**: 다른 사람이 PC를 몰래 사용하지 못하게 함.
- **USB 보안**: USB 드라이브 삽입 시 자동 포맷으로 악성 코드/데이터 유입 차단.
- **긴급 대응**: 패스워드 오류 시 알림(Discord 웹훅)과 shutdown, 하지만 합법 사용자(긴급 패스워드)에게는 취소 옵션 제공.

이 프로그램은 VB.NET으로 작성된 Windows Forms 앱으로, 시작 프로그램 등록으로 부팅 시 자동 실행됩니다. (현재 버전: v1.0, 2025년 11월 28일 기준)

## 주요 기능
- **부팅 시 비밀번호 잠금**: 부팅 후 30초 후, 일반 비밀번호를 입력하라는 프롬프트가 나타납니다. 잘못된 입력 시 2분 셧다운 카운트다운이 시작됩니다.
- **비상 오버라이드**: 셧다운 중 2분 이내에 비상 비밀번호를 입력하면 셧다운을 취소할 수 있습니다.
- **USB 드라이브 보호**: 활성화된 경우, 삽입된 USB 드라이브를 자동으로 포맷(NTFS, 빠른 포맷)하고 비밀번호 프롬프트 + 셧다운 시퀀스를 재시작합니다.
- **스텔스 모니터링**: 스크린샷, 웹캠 사진(활성화된 경우), 실행 중인 앱 제목을 캡처하고 Discord 웹훅을 통해 원격 로깅을 위해 알림을 보냅니다.
- **쉬운 구성**: 첫 실행(또는 설정 파일 누락 시), 일반/비상 비밀번호와 Discord 웹훅을 설정합니다. 설정은 `%APPDATA%\HUK_CHK.NFO`에 안전하게 저장됩니다(이 파일을 삭제하면 재설정).
- **블로트 없음**: VB.NET과 Emgu.CV로 웹캠 지원. 최소 의존성.

| 기능 | 설명 | 기본 활성화? |
|------|------|-------------|
| 비밀번호 프롬프트 | 부팅 후 30초 | 예 |
| USB 자동 포맷 | 삽입 시 포맷 | UI 체크박스 |
| 웹캠 캡처 | 알림 시 스냅샷 | UI 체크박스 |
| Discord 알림 | 로그/스크린샷 전송 | 웹훅 URL 구성 |

## 설치 & 설정
### 요구사항
- **OS**: Windows 10/11 (.NET Framework 4.7.2+).
- **라이브러리**: Emgu.CV (웹캠), Newtonsoft.Json (Discord JSON).
- **관리자 권한**: 포맷/shutdown 위해 필요 (UAC 팝업 발생).

### 설치 단계
1. **다운로드**: GitHub 릴리즈에서 `USB-Security-Monitor-v1.0.zip`을 다운로드하세요.
2. **ZIP 압축 해제**:
   - 전용 폴더로 압축을 해제하세요, 예: `C:\Program Files\USB-Security-Monitor`(폴더가 없으면 생성). 경로에 공백이 없도록 하는 게 호환성에 좋습니다.
3. **시작 프로그램 추가**:
   - `Win + R` 누르고 `shell:startup` 입력 후 Enter로 시작 폴더 열기.
   - 압축 해제된 폴더의 `USB-Security-Monitor.exe` 우클릭 > 바로가기 만들기.
   - 생성된 바로가기를 시작 폴더로 복사.
   - 또는 레지스트리: `reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "USBSecurity" /t REG_SZ /d "C:\Path\To\USB-Security-Monitor.exe" /f`.
4. **첫 설정**:
   - PC 재시작 – 첫 부팅 시 설정 프롬프트가 나타납니다.
   - 강력한 **일반 비밀번호** 입력(일상 사용).
   - **비상 비밀번호** 설정(셧다운 취소).
   - (선택) Discord 웹훅 URL 추가(예: Discord 채널 설정 > 통합 > 웹훅).
5. **옵션 활성화**: 앱 실행(또는 구성 버튼)으로 USB 포맷과 웹캠 캡처 토글.
6. **설정 재설정**: 비밀번호 잊음? `%APPDATA%\HUK_CHK.NFO` 삭제 후 재실행.

**프로 팁**: 먼저 가상 머신에서 테스트하세요! 문제가 생기면 작업 관리자로 프로세스 종료.

### UI 요소
- **Check_cam**: 웹캠 캡처 활성화/비활성화 체크박스.
- **Check_usb**: USB 포맷 활성화/비활성화 체크박스.
- **Button_config**: 설정 변경 버튼 (일반 패스워드 확인 후 대화상자).
- **Button_OK**: 긴급 취소 버튼 (shutdown 시퀀스 중).

## 사용 노트
- **셧다운 동작**: Windows `shutdown /s /t 120` 명령 사용. 비상 취소 `/a`.
- **USB 포맷**: 되돌릴 수 없음! 확신할 때만 활성화 – `format /FS:NTFS /Q /Y` 무음 실행.
- **의존성**: .NET Framework 4.7+ 및 Emgu.CV(번들) 필요. 웹캠은 호환 카메라 필요.
- **로깅**: 모든 알림에 IP, 호스트명, 사용자명, 타임스탬프, 앱 스냅샷 포함.

### 테스트
- USB 드라이브 꽂기: "Format USB" 체크 확인 후 포맷 + 패스워드 대화상자.
- 패스워드 오류: Discord 채널 알림 + shutdown 카운트다운.
- 긴급 패스워드: 취소 확인.
- 설정 변경: "설정" 버튼 클릭 → 패스워드 확인 → 입력.

## 경고 & 면책
- **⚠️ 고위험**: 이 도구는 **영구 데이터 삭제**(USB 포맷) 또는 **강제 셧다운**을 유발할 수 있습니다. 책임 있게 사용 – 백업 없이 프로덕션에 사용 금지.
- **법적/윤리적**: 개인/공유 PC 보호용. 타인 시스템에 동의 없이 배포 금지.

## 변경 로그
- **v1.0 (초기 릴리즈)**: 비밀번호 대화상자, USB 감시자, Discord 통합, 시작 등록 지원 완전 구현.
- 수정: `%APPDATA%` 경로 설정 파일 처리.
- 추가: 웹캠 초기화 검사 및 오류 처리.

- 
