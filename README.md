# VTS_XYPlugin
用于Bilibili的VTube Studio的插件

## 下载
[releases][1]

## 插件说明
获取B站直播数据，并根据礼物播放对应动作，根据礼物掉落图片等，可以绑定多种动作，可以显示多条日志。

## 安装说明
1. 从[releases][1]下载最新版本的压缩包
2. 将压缩包内的文件全部解压到VTubeStudio根目录，注意不要缺少文件
3. 从steam启动VTubeStudio（不可以从快捷方式或者文件夹中启动）

## 功能建议
可以直接提Issues或者在B站私信我，我有空的时候会研究。

## 引用的库
[VTS-Sharp][2]

[blivedm][3]

## 参考的代码
[VTSBilibili][4]

## 修改说明
由于使用了资源商店的插件，所以git上的源文件并不是完整的文件。
如果你需要修改并编译，需要在package manager中导入odin和easy save 3两个插件。

[1]:https://github.com/xiaoye97/VTS_XYPlugin/releases
[2]:https://github.com/FomTarro/VTS-Sharp
[3]:https://github.com/xfgryujk/blivedm
[4]:https://github.com/pierpan2/VTSBilibili