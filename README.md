# MotionFramework
MotionFramework是一个基于Unity3D引擎的游戏框架。该框架由MotionEngine和MotionGame组成。
MotionEngine是框架的基础，MotionGame是在前者的基础上做了业务逻辑上的扩展。

## 支持的Unity版本
Unity2018.4

## MotionEngine.Runtime
由多个模块组成，每个模块互相独立，开发者可以灵活选择需要的模块。

#### Base 核心部分

#### Engine.AI AI模块：有限状态机。
#### Engine.Event 事件模块
#### Engine.IO IO模块
#### Engine.Net 网络模块：异步IOCP SOCKET支持高并发，自定义协议解析器。
#### Engine.Patch 补丁模块
#### Engine.Res 资源模块：基于引用计数的资源系统，基于面向对象的资源加载方式。
#### Engine.Utility 工具模块

## MotionEngine.Editor
扩展的相关工具

#### AssetBrowser 资源对象总览工具
#### AssetBuilder 资源打包工具
#### AssetImporter 资源导入工具
#### AssetSearch 资源引用搜索工具

## MotionGame.Runtime
内含AudioManager, CfgManager, EventManager, FsmManager, NetManager, PoolManager, ResManager，ILRManager。
其中新引入了ILRuntime库，来支持C#编写业务逻辑并实现热更新。协议解析器使用protobuf库来做包体序列化，并可以和ET 5.0服务器通信。
