<div align="center">
<h1>Discord Editor Presence</h1>
<p>Unity 에디터의 작업 상태를 Discord Rich Presence에 표시합니다. Windows의 Unity 2022.3 이상을 대상으로 합니다.</p>

[설치](#설치) · [활동 표시 및 공유](#활동-표시-및-공유) · [문제 해결](#문제-해결) · [라이선스 및 기여](#라이선스-및-기여)

[日本語](README.md) · [English](README.en.md) · [한국어](README.ko.md)

</div>

## 설치

1. Git을 설치하고 명령줄에서 `git --version`이 실행되는지 확인합니다. Unity의 `Window > Package Manager`에서 `+` → `Add package from git URL…`을 선택합니다. 다음 URL을 붙여넣고 `Add`를 누릅니다. [Unity의 Git URL 설치 안내](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-giturl.html)

   ```text
   https://github.com/Harukotokoto/unity-discord-presence.git#main
   ```
2. Discord 데스크톱 앱에 로그인하고 사용자 설정에서 활동 공유를 켭니다. [Discord 활동 공유 설정](https://support.discord.com/hc/en-us/articles/7931156448919-Activity-Sharing-on-Discord-FAQ)
3. Unity의 `Window > Discord Editor Presence`에서 **このプロジェクトのアクティビティを共有する**(이 프로젝트의 활동 공유)를 켭니다.

배포용 `.tgz` 파일을 설치하려면 `Add package from tarball…`을 선택합니다. 로컬 소스에서 설치하려면 `Add package from disk…`에서 `package.json`을 선택합니다. [tarball 설치 안내](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-tarball.html)

패키지에 Application ID가 포함되어 있습니다. 자신의 Discord 앱 이름으로 표시하려면 **独自の Application ID**(사용자 지정 Application ID)에 ID를 입력하고 Enter 키를 누릅니다. 필드를 비우면 패키지에 포함된 ID를 다시 사용합니다. [Discord Developer Portal](https://discord.com/developers/applications)

## 활동 표시 및 공유

- 프로젝트 폴더 이름과 활성 씬 이름을 표시합니다. 설정에서 각각 숨길 수 있습니다.
- 편집 중, 재생 중, 일시 정지 상태와 Unity 버전을 표시합니다.
- 공유 시작 후 경과한 시간을 표시합니다. 스크립트를 다시 컴파일해도 시간은 유지됩니다.

공유는 기본적으로 꺼져 있습니다. 공유 여부와 이름 표시 설정은 프로젝트별로 저장됩니다. 사용자 지정 Application ID는 같은 PC의 Unity 프로젝트에 공통으로 적용됩니다. 공유를 끄거나 Unity를 닫으면 현재 활동 표시가 해제됩니다.

## 문제 해결

- Discord 데스크톱 앱을 기다린다는 메시지가 계속 표시되면 데스크톱 앱을 열고 로그인합니다. 패키지가 자동으로 다시 연결합니다.
- 공유 중이라고 표시되는데 프로필에서 활동이 보이지 않으면 Discord의 활동 공유 설정을 확인합니다. 표시가 갱신되기까지 몇 초가 걸릴 수 있습니다.

## 라이선스 및 기여

Apache License 2.0으로 배포합니다. 변경 사항과 기여 방법은 아래 문서를 참고하세요.

<div align="center">

[라이선스](LICENSE) · [변경 이력](CHANGELOG.md) · [기여 안내](CONTRIBUTING.md)

</div>
