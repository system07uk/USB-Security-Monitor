# USB Security Monitor

[![VB.NET](https://img.shields.io/badge/Language-VB.NET-blue?style=flat&logo=visualbasic)](https://learn.microsoft.com/en-us/dotnet/visual-basic/) [![Windows](https://img.shields.io/badge/Platform-Windows-green?style=flat&logo=windows)](https://www.microsoft.com/windows)

## 소개
이 프로그램은 **PC의 무단 사용과 USB 드라이브 삽입을 제한**하기 위해 설계된 보안 도구입니다. 부팅 후 자동으로 패스워드를 요구하여 몰래 사용을 막고, USB 삽입 시 옵션으로 자동 포맷을 실행합니다. 공유 PC나 중요한 워크스테이션에서 데이터 유출과 무단 접근을 방지하는 데 적합합니다.

### 주요 취지
- **무단 사용 방지**: 다른 사람이 PC를 몰래 사용하지 못하게 함.
- **USB 보안**: USB 드라이브 삽입 시 자동 포맷으로 악성 코드/데이터 유입 차단.
- **긴급 대응**: 패스워드 오류 시 알림(Discord 웹훅)과 shutdown, 하지만 합법 사용자(긴급 패스워드)에게는 취소 옵션 제공.

이 프로그램은 VB.NET으로 작성된 Windows Forms 앱으로, 시작 프로그램 등록으로 부팅 시 자동 실행됩니다. (현재 버전: 1.0, 2025년 11월 28일 기준)

## 기능
- **부팅 후 자동 패스워드 확인**:
  - 프로그램 시작 후 30초 대기 (부팅 지연 방지).
  - 일반 패스워드 입력 요구 (1분 타임아웃).
  - **정상**: "시스템 정상" 메시지, 프로그램 지속 실행.
  - **오류**: Discord 웹훅으로 알림(스크린샷 + 웹캠 캡처 + IP/호스트 정보) 전송. 2분 후 자동 shutdown.
  - **긴급 패스워드**: shutdown 2분 내 입력 시 취소 ("시스템 종료가 취소되었습니다.").

- **USB 드라이브 감지 및 포맷**:
  - "Format USB" 체크박스가 활성화된 상태에서 USB 삽입 시 즉시 자동 포맷 (NTFS, Quick/Yes 옵션).
  - 포맷 후 패스워드 확인 (위와 동일 로직: 오류 → 알림 + shutdown, 긴급 패스워드로 취소).
  - 체크박스 비활성화 시 USB 삽입 감지만 (포맷 생략).

- **설정 관리**:
  - **첫 실행**: 설정 파일 없으면 자동으로 일반 패스워드, 비상 패스워드, Discord 웹훅 URL 입력 대화상자 띄움.
  - **설정 변경**: "설정" 버튼 클릭 시 일반 패스워드 확인 후 입력 대화상자. 파일 잊으면 `%APPDATA%\HUK_CHK.NFO` 삭제로 초기화.

- **알림 및 캡처**:
  - 패스워드 오류/USB 이벤트 시 Discord 웹훅으로 실시간 알림 (Embed 형식: 추가/제거/현재 프로세스 목록, 색상 구분).
  - 스크린샷(PNG) + 웹캠 캡처(JPEG) 자동 첨부.
  - 헤더: "Event rec: [IP]: [호스트명]: [사용자명]".

- **기타**:
  - 웹캠 사용 여부: Check_cam 체크박스 (폼 UI).
  - 설정 파일: `%APPDATA%\HUK_CHK.NFO` (일반 패스워드, 비상 패스워드, Discord 웹훅 URL ? 3줄).

## 설치 및 사용법
### 요구사항
- **OS**: Windows 10/11 (.NET Framework 4.7.2+).
- **라이브러리**: Emgu.CV (웹캠), Newtonsoft.Json (Discord JSON).
- **관리자 권한**: 포맷/shutdown 위해 필요 (UAC 팝업 발생).

### 설치 단계
1. **빌드 및 실행**:
   - Visual Studio에서 프로젝트 열기 → Build → Rebuild Solution.
   - `bin\Release\YourApp.exe` 실행 (관리자 권한으로).

2. **부팅 자동 실행 등록**:
   - 시작 프로그램 폴더(Win+R → `shell:startup`)에 EXE 숏컷 추가.
   - 또는 레지스트리: `reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "USBSecurity" /t REG_SZ /d "C:\Path\To\YourApp.exe" /f`.

3. **첫 실행 설정**:
   - 프로그램 실행 시 설정 파일 없으면 자동 대화상자: 일반 패스워드, 비상 패스워드, Discord 웹훅 URL 입력.
   - Discord 웹훅: 서버 설정 > 통합 > 웹훅 생성.

4. **테스트**:
   - USB 드라이브 꽂기: "Format USB" 체크 확인 후 포맷 + 패스워드 대화상자.
   - 패스워드 오류: Discord 채널 알림 + shutdown 카운트다운.
   - 긴급 패스워드: 취소 확인.
   - 설정 변경: "설정" 버튼 클릭 → 패스워드 확인 → 입력.

### UI 요소
- **Check_cam**: 웹캠 캡처 활성화/비활성화 체크박스.
- **Check_usb**: USB 포맷 활성화