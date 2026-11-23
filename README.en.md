<div align="center">
<h1>Discord Editor Presence</h1>
<p>Show your Unity Editor activity in Discord Rich Presence. This package targets Unity 2022.3 and later on Windows.</p>

[Install](#install) · [Activity and sharing](#activity-and-sharing) · [Troubleshooting](#troubleshooting) · [License and contributing](#license-and-contributing)

[日本語](README.md) · [English](README.en.md) · [한국어](README.ko.md)

</div>

## Install

1. Install Git and verify that `git --version` works from the command line. In Unity, open `Window > Package Manager`, then choose `+` → `Add package from git URL…`. Paste the following URL and click `Add`. [Unity's Git URL installation guide](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-giturl.html)

   ```text
   https://github.com/Harukotokoto/unity-discord-presence.git#main
   ```
2. Sign in to Discord desktop and enable activity sharing in User Settings. [Discord activity sharing settings](https://support.discord.com/hc/en-us/articles/7931156448919-Activity-Sharing-on-Discord-FAQ)
3. In Unity, open `Window > Discord Editor Presence` and enable **このプロジェクトのアクティビティを共有する** (Share activity for this project).

To install a release `.tgz`, choose `Add package from tarball…`. For a local source checkout, choose `Add package from disk…` and select `package.json`. [Tarball installation guide](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-tarball.html)

The package includes an Application ID. To display activity under your own Discord app name, enter your ID in **独自の Application ID** (Custom Application ID) and press Enter. Clear the field to restore the bundled ID. [Discord Developer Portal](https://discord.com/developers/applications)

## Activity and sharing

- The project folder name and active scene name. You can hide either one in the settings.
- Editing, playing, or paused state and the Unity version.
- Time since sharing began. The timer survives script recompilation.

Sharing starts off. The sharing and name visibility preferences are saved per project. A custom Application ID applies across Unity projects on the same PC. Turning sharing off or closing Unity clears the active presence.

## Troubleshooting

- If the package keeps waiting for Discord desktop, open the desktop app and sign in. The package reconnects automatically.
- If the package reports that it is sharing but your profile does not show it, check Discord's activity sharing settings. Updates can take a few seconds.

## License and contributing

Distributed under Apache License 2.0. See the release changes and how to contribute in the files below.

<div align="center">

[License](LICENSE) · [Changelog](CHANGELOG.md) · [Contributing guide](CONTRIBUTING.md)

</div>
