# 生命周期事件参考

本文列出 RitsuLib 提供的全部生命周期事件，介绍订阅方式及可重放事件的行为。

---

## 订阅事件

**按类型订阅（推荐）：**

```csharp
// 持有返回的 IDisposable 以便稍后取消订阅
var sub = RitsuLibFramework.SubscribeLifecycle<GameReadyEvent>(evt =>
{
    Logger.Info($"游戏已就绪：{evt.Game}");
});

// 取消订阅
sub.Dispose();
```

**通过 `ILifecycleObserver` 订阅多种事件：**

```csharp
public class MyObserver : ILifecycleObserver
{
    public void OnEvent(IFrameworkLifecycleEvent evt)
    {
        if (evt is CombatStartingEvent combat)
            HandleCombatStart(combat);
        else if (evt is RunEndedEvent run)
            HandleRunEnd(run);
    }
}

RitsuLibFramework.SubscribeLifecycle(new MyObserver());
```

> **可重放事件（`IReplayableFrameworkLifecycleEvent`）：** 若在事件已发生后才订阅，框架会立即以已存储的事件回调，无需关心订阅时机。

---

## 框架事件

在框架初始化和 Profile 服务初始化阶段触发。

| 事件 | 可重放 | 携带数据 |
|---|---|---|
| `FrameworkInitializingEvent` | — | `FrameworkModId`、`FrameworkVersion` |
| `FrameworkInitializedEvent` | ✓ | `FrameworkModId`、`IsActive` |
| `ProfileServicesInitializingEvent` | — | — |
| `ProfileServicesInitializedEvent` | ✓ | `ProfileId` |

---

## 游戏引导事件

在游戏启动流程中依次触发，覆盖 Model 注册到游戏就绪全程。

| 事件 | 可重放 | 携带数据 |
|---|---|---|
| `EssentialInitializationStartingEvent` | — | — |
| `EssentialInitializationCompletedEvent` | ✓ | — |
| `DeferredInitializationStartingEvent` | — | — |
| `DeferredInitializationCompletedEvent` | ✓ | — |
| `ContentRegistrationClosedEvent` | ✓ | `Reason` |
| `ModelRegistryInitializingEvent` | — | — |
| `ModelRegistryInitializedEvent` | ✓ | `RegisteredModelTypeCount` |
| `ModelIdsInitializingEvent` | — | — |
| `ModelIdsInitializedEvent` | ✓ | — |
| `ModelPreloadingStartingEvent` | — | — |
| `ModelPreloadingCompletedEvent` | ✓ | — |
| `GameTreeEnteredEvent` | ✓ | `Game` |
| `GameReadyEvent` | ✓ | `Game` |

```csharp
// ModelId 初始化完成后，可安全读取 ModelId
RitsuLibFramework.SubscribeLifecycle<ModelIdsInitializedEvent>(_ =>
{
    var id = ModelDb.GetId<MyCard>();
});
```

---

## 跑局事件

| 事件 | 可重放 | 携带数据 |
|---|---|---|
| `RunStartedEvent` | — | `RunState`、`IsMultiplayer`、`IsDaily` |
| `RunLoadedEvent` | — | `RunState`、`IsMultiplayer`、`IsDaily` |
| `RunEndedEvent` | — | `Run`、`IsVictory`、`IsAbandoned` |

---

## 房间与章节事件

| 事件 | 携带数据 |
|---|---|
| `RoomEnteringEvent` | `RunState`、`Room` |
| `RoomEnteredEvent` | `RunState`、`Room` |
| `RoomExitedEvent` | `RunManager`、`Room` |
| `ActEnteringEvent` | `RunManager`、`TargetActIndex`、`DoTransition` |
| `ActEnteredEvent` | `RunState`、`CurrentActIndex` |
| `RewardsScreenContinuingEvent` | `RunManager` |

---

## 战斗事件

| 事件 | 携带数据 |
|---|---|
| `CombatStartingEvent` | `RunState`、`CombatState?` |
| `CombatEndedEvent` | `RunState`、`CombatState?`、`Room` |
| `CombatVictoryEvent` | `RunState`、`CombatState?`、`Room` |
| `SideTurnStartingEvent` | `CombatState`、`Side` |
| `SideTurnStartedEvent` | `CombatState`、`Side` |
| `CardPlayingEvent` | `CombatState`、`CardPlay` |
| `CardPlayedEvent` | `CombatState`、`CardPlay` |
| `CardDrawnEvent` | `CombatState`、`Card`、`FromHandDraw` |
| `CardDiscardedEvent` | `CombatState`、`Card` |
| `CardExhaustedEvent` | `CombatState`、`Card`、`CausedByEthereal` |
| `CardMovedBetweenPilesEvent` | `RunState`、`CombatState?`、`Card`、`PreviousPile`、`Source` |

```csharp
RitsuLibFramework.SubscribeLifecycle<CardDrawnEvent>(evt =>
{
    if (evt.Card is MyCard myCard)
        myCard.OnDrawn(evt.CombatState);
});
```

---

## 存档与持久化事件

由 `ModDataStore` 内部使用，也可供 Mod 监听存档状态变化。

| 事件 | 说明 |
|---|---|
| `ProfileDataReady` | 存档数据加载完毕，可安全读写 |
| `ProfileDataChanged` | 存档数据发生变更 |
| `ProfileDataInvalidated` | 存档数据失效（如切换档案） |

---

## 相关文档

- [快速入门](GettingStarted.md)
- [内容注册规则](ContentAuthoringToolkit.md)
