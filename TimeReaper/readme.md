QA
## 命名规则
### XAML命名规则
使用 页面+位置+作用+特殊
MainLeftFramework

### 实现技术
计时器实现：https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.dispatchertimer
    具体的时钟实现：https://www.cnblogs.com/mengnan/p/5069405.html
    倒计时的实现：
    https://www.cnblogs.com/siazon/p/6636253.html
    [倒计时中的异步调用](https://docs.microsoft.com/en-us/uwp/api/windows.ui.core.coredispatcher.tryrunasync#Windows_UI_Core_CoreDispatcher_TryRunAsync_Windows_UI_Core_CoreDispatcherPriority_Windows_UI_Core_DispatchedHandler_)

### 逻辑
```
IsPressButton 判断用户是否按了按钮
NeedPress 判断是否可以按这个按钮
NeedPress = false
番茄钟的逻辑
第一次计时开始
    每次计时时判断是否达到指定时间
        达到指定时间
            弹出通知，提示休息
            NeedPress = true,IsPressButton = false;
此后一直空过直到用户点击按钮
IsPressButton = true
用户点击按钮之后，开始新的一轮（休息/工作）
NeedPress = false;
```
### 遇坑警告
 1. String的比较不能用=，而应该使用String.Equals方法。
使用=是比较两个String是否属于同一个对象，使用Equals才是比较两个String的内容是否相同。

### UI更新 2018.5.31 3:34

### 更新了AdaptiveUI和LiveTiles 2018.6.1 19:40