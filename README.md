# 切西瓜（Fruit Slash）

竖屏切水果小游戏：划开水果得分，躲开炸弹。美术与音效均为程序生成的占位资源，可直接在本地试玩或改造成自己的作品。

包名 `com.fruitslash.game`，竖屏，最低 Android 7.1（API 25）。

## 功能

- 多点触控划切、抛物线抛出、切开飞散
- 水果：西瓜 / 苹果 / 橙子 / 香蕉 / 菠萝，以及炸弹
- 模式：
  - **经典**：3 条命，漏水果或切炸弹扣命
  - **街机**：60 秒，切炸弹扣 8 秒
  - **禅**：90 秒，无炸弹
- 连击倍率、结算金币、本地商店（刀光 / 背景）、本机排行榜
- 刀光拖尾、果汁粒子、震动（真机）、占位音效与 BGM

## 环境

推荐 **团结引擎 1.10.x**（基于 Unity 2022.3 LTS）。国际版 Unity 2022.3 一般也能打开本工程。

1. 安装 [团结引擎 Hub](https://www.tuanjie.cn/) 或 Unity Hub，并安装带 **Android Build Support** 的编辑器。
2. 用个人版许可登录（免费）。
3. 用编辑器打开**本仓库根目录**（含 `Assets`、`ProjectSettings` 的目录）。
4. 若场景缺失，菜单 **Fruit Slash → 配置工程**。
5. 打开 `Assets/Scenes/Boot.scene`，点 Play。编辑器里鼠标拖拽相当于手指滑动。

> 中国大陆访问 `download.unity3d.com` 可能失败，可改用团结引擎安装包。

## 玩法速览

短时间连续切开可叠连击。结算按得分发金币，商店解锁刀光与主题。排行榜与存档写在设备本地，无联网。

## 导出 Android

菜单：

| 菜单项 | 产物 |
| --- | --- |
| **Fruit Slash → 导出 Android APK** | `Builds/FruitSlash.apk` |
| **Fruit Slash → 导出 Android AAB** | `Builds/FruitSlash.aab` |

命令行示例（把编辑器路径换成你的安装位置）：

```bash
Tuanjie.exe -batchmode -nographics -quit \
  -projectPath "本仓库根目录" \
  -executeMethod FruitSlash.EditorTools.ProjectSetup.BuildApkBatch \
  -logFile Builds/apk.log
```

打包约定：

- 脚本后端 **IL2CPP**
- CPU：**ARM64 + x86_64**（模拟器可用 x86_64）
- 图形接口固定 **OpenGL ES 3**（部分模拟器上 Vulkan 会卡住）
- SDK / NDK：优先使用编辑器自带的 Android 工具目录
- JDK：请在 Hub / Preferences 中配置 **JDK 17+**，或设置环境变量 `JAVA_HOME` 指向 JDK 17。团结引擎自带的 OpenJDK 11 可能无法出包

上架请自行准备正式签名 keystore，**不要把密钥提交到 Git**。本仓库默认使用编辑器调试签名。

## 仓库说明

应提交：

- `Assets/`、`Packages/`、`ProjectSettings/`
- `README.md`、`LICENSE`、`.gitignore`

不要提交：

- `Library/`、`Temp/`、`Logs/`、`UserSettings/`、`Builds/`
- `*.apk` / `*.aab`、本地 JDK/SDK 下载目录、`*.keystore` / `*.ulf` 等许可与签名文件

克隆后第一次打开会由编辑器重建 `Library`。

## 第三方资源

| 资源 | 许可 |
| --- | --- |
| [Source Han Sans CN](https://github.com/adobe-fonts/source-han-sans)（`Assets/Resources/Fonts`） | SIL Open Font License 1.1 |

游戏逻辑、程序化贴图与占位音频为原创内容，按本仓库 **MIT** 许可分发。

## 许可

本项目源码采用 [MIT License](LICENSE)。
