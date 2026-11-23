<div align="center">
<h1>Discord Editor Presence</h1>
<p>Unity Editorで開いているプロジェクトをDiscordのRich Presenceに表示できる、Windows向けのUnityパッケージです。Unity 2022.3以降を対象にしています。</p>

[インストール](#インストール) · [表示と共有](#表示と共有) · [困ったとき](#困ったとき) · [ライセンスと貢献](#ライセンスと貢献)

[日本語](README.md) · [English](README.en.md) · [한국어](README.ko.md)

</div>

## インストール

1. Gitをインストールし、コマンドラインで `git --version` が実行できることを確認します。Unityの `Window > Package Manager` を開き、`+` → `Add package from git URL…` を選びます。次のURLを貼り付け、`Add` を押します。 [UnityのGit URL導入手順](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-giturl.html)

   ```text
   https://github.com/Harukotokoto/unity-discord-presence.git#main
   ```
2. Discordデスクトップ版にログインし、ユーザー設定でアクティビティ共有をオンにします。[Discordの共有設定](https://support.discord.com/hc/en-us/articles/7931156448919-Activity-Sharing-on-Discord-FAQ)
3. Unityの `Window > Discord Editor Presence` で **このプロジェクトのアクティビティを共有する** をオンにします。

配布用の `.tgz` を使う場合は `Add package from tarball…`、手元のソースを使う場合は `Add package from disk…` で `package.json` を選びます。 [tarballの導入手順](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-tarball.html)

配布版にはApplication IDが含まれています。自分のDiscordアプリ名で表示したい場合は、**独自の Application ID** に自分のIDを入力して Enter を押してください。空欄にすると配布版のIDに戻ります。[Discord Developer Portal](https://discord.com/developers/applications)

## 表示と共有

- プロジェクトのフォルダ名と、現在アクティブなシーンの名前。設定から個別に非表示にできます。
- 編集中・再生中・一時停止中の状態とUnityのバージョン。
- 共有を始めてからの経過時間。スクリプトを再コンパイルしても引き継ぎます。

共有は初期状態でオフです。共有のオン・オフと名前の表示設定はプロジェクトごとに保存します。独自のApplication IDは同じPCのUnity Editorで共通です。共有をオフにするかUnityを終了すると、現在のアクティビティが解除されます。

## 困ったとき

- 「Discordデスクトップ版を待っています。」が続く場合は、デスクトップ版を起動してログインしてください。パッケージは自動で再接続します。
- 「Discordに共有中です。」と表示されてもプロフィールで見えない場合は、Discordの共有設定を確認してください。表示の更新に数秒かかる場合があります。

## ライセンスと貢献

Apache License 2.0で配布します。変更履歴と開発への参加方法は次のファイルを参照してください。

<div align="center">

[ライセンス](LICENSE) · [変更履歴](CHANGELOG.md) · [コントリビュートガイド](CONTRIBUTING.md)

</div>
