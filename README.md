# Unity-UGUI-Emoji解决方案

## 参考资料

参考博客：

https://virtualcast.jp/blog/2019/10/emoji/

https://forum.unity.com/threads/full-emoji-support-api-emoji-sequen.660310/#post-4420162

Emoji图片资源：

https://github.com/iamcal/emoji-data

Emoji测试：

https://getemoji.com/

最全的Emoji：

https://unicode.org/emoji/charts/full-emoji-list.html

## 测试工程说明

在EmojiAssets下，存放了已经生成好的表情资源，目前覆盖度应该有90%以上。

该工具是基于TextMeshPro做的扩展，操作方式如下：

- 菜单栏点击Window-->TextMeshPro-->Sprite Emoji Inporter

<img src="MarkDownPic/emoji_1.png" alt="emoji_1" style="zoom:50%;" />

- Import Format选择Texture Packer Json Array，Sprite Data Source选择TexturePacker导出json，Sprite Texture Atlas选择TexturePacker导出的图集，点击Create Sprite Asset，下面会显示导入了多少个表情，点击Save Sprite Asset将配置文件保存在本地。

  <img src="MarkDownPic/emoji_2.png" alt="emoji_2" style="zoom:50%;" />

- 在工程的TextMesh Pro文件夹下找到Resources里面有个TMP Settings，找到Default Sprite Asset，选择上面保存的配置文件

  <img src="MarkDownPic/emoji_3.png" alt="emoji_3" style="zoom:50%;" />

- 将场景里面的TextMeshPro脚本替换成TMP_Emoji Text UGUI

  <img src="MarkDownPic/emoji_4.png" alt="emoji_4" style="zoom:50%;" />

- 在输入框输入表情，进行测试。



## 注意事项

- 生成的图集Texture Type改成Defaut，Alpha Is Transparency勾选，Generate Mip Maps不勾选，如果图集大于2048，记得将Max Szie改成对应的尺寸。

  <img src="MarkDownPic/emoji_5.png" alt="emoji_5" style="zoom:50%;" />

- TextureMeshPro的版本要用2.1.0
  - "com.unity.textmeshpro": "2.1.0",
- 插件版本要用1.0.8
  - "com.kyub.emojisearch": "https://gitlab-ci-token:zF563oCqvXdJmiDuMShq@gitlab.com/KyubInteractive/kyublibs.git#com.kyub.emojisearch-1.0.8",