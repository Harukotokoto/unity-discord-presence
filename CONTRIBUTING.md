# コントリビュートについて

IssueとPull Requestを歓迎します。

## 開発環境

- Windows
- Unity Editor 2022.3以降と、Editorスクリプトを編集できるC#開発環境
- 表示確認用のDiscordデスクトップ版

## 提出前の確認

Unityのテスト用プロジェクトに `Add package from disk…` で `package.json` を追加し、Consoleにコンパイルエラーが出ないことを確認します。変更した機能について、Discordデスクトップ版で、共有の有効化、編集・再生・一時停止、シーンの切り替え、共有の解除を確認してください。Application IDの設定を変更した場合は、空欄で配布用IDが使われることと、独自IDで上書きできることも確認します。

TODO: 自動テストの実行手順は導入後に追記。

## バグ報告

次の情報があると調査しやすくなります。

- パッケージ、Unity Editor、Discordデスクトップ版のバージョン
- 再現手順、期待した動作、実際の表示
- Unityの `Window > Discord Editor Presence` に表示された接続状態
- 配布版のApplication IDと独自のApplication IDのどちらで再現したか

## セキュリティ上の問題

セキュリティに関わる問題は公開Issueではなく、GitHubのSecurity Advisory(Report a vulnerability)から非公開で報告してください。

## ライセンス

コントリビュートされた内容は、本プロジェクトと同じApache License 2.0で提供されるものとします。
