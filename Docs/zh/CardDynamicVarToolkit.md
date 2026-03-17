# 卡牌动态变量工具包

本文介绍 RitsuLib 提供的卡牌动态变量创建方式、Tooltip 绑定规则及其在卡牌悬停时的注入机制，并附使用示例。

---

## 概览

游戏的 `DynamicVar` 系统让卡牌在运行时携带可变数值。RitsuLib 在此基础上提供：

- 便捷构造器（`ModCardVars`）
- 每个变量可独立绑定 Tooltip（`DynamicVarExtensions`）
- 卡牌悬停时自动注入所有已绑定 Tooltip（Patch 实现，无需额外配置）

---

## 变量构造

通过 `ModCardVars` 创建变量，并在卡牌的 `DynamicVarSet` 中使用：

```csharp
public class MyCard : ModCardTemplate(1, CardType.Attack, CardRarity.Common, TargetType.SingleEnemy)
{
    private static readonly DynamicVar _charges =
        ModCardVars.Int("charges", amount: 3)
            .WithSharedTooltip("my_mod_charges");

    private static readonly DynamicVar _label =
        ModCardVars.String("flavor", value: "wine");

    public override DynamicVarSet CreateDynamicVars() =>
        new DynamicVarSet().Add(_charges).Add(_label);
}
```

| 方法 | 说明 |
|---|---|
| `ModCardVars.Int(name, amount)` | 创建数值变量（`decimal`） |
| `ModCardVars.String(name, value)` | 创建字符串变量 |

RitsuLib 不为这些变量赋予任何玩法语义，变量的具体含义完全由内容作者定义。

---

## Tooltip 绑定

在变量定义时通过扩展方法链式绑定 Tooltip：

### 绑定共享 Tooltip（推荐）

从 `static_hover_tips` 表读取 Key：

```csharp
var myVar = ModCardVars.Int("my_var", 2)
    .WithSharedTooltip("my_mod_my_var");
// 解析：
//   static_hover_tips["my_mod_my_var.title"]
//   static_hover_tips["my_mod_my_var.description"]
```

### 绑定指定 Table/Key

```csharp
var myVar = ModCardVars.Int("my_var", 2)
    .WithTooltip(
        titleTable: "card_keywords",
        titleKey:   "my_mod_my_var.title",    // description 默认为 .title → .description
        iconPath:   "res://MyMod/art/kw.png"  // 可选
    );
```

### 绑定自定义工厂

```csharp
var myVar = ModCardVars.Int("my_var", 2)
    .WithTooltip(var => new HoverTip(
        new LocString("my_table", "my_var.title"),
        new LocString("my_table", "my_var.description")
    ));
```

---

## 本地化示例

若使用 `WithSharedTooltip("my_mod_charges")`，需在 `static_hover_tips` 本地化文件中提供：

```json
{
  "my_mod_charges.title": "充能",
  "my_mod_charges.description": "累积的充能层数，造成额外伤害。"
}
```

RitsuLib 不提供内置本地化词条，若使用 `WithSharedTooltip`，词条须由内容作者自行提供。

---

## 卡牌 Tooltip 注入

RitsuLib 的 Patch 会在卡牌悬停时自动将 `CardModel.DynamicVars` 中所有已绑定 Tooltip 的变量追加到提示序列末尾，无需任何额外配置。

---

## 克隆行为

调用 `DynamicVar.Clone()` 时，绑定在原变量上的 Tooltip 元数据会一并复制到克隆对象。战斗中升级或复制卡牌时行为正确，无需额外处理。

---

## 运行时读取变量值

通过 `DynamicVarExtensions` 扩展方法方便地读取变量值：

```csharp
// 读取 int 值（默认 0）
int charges = card.DynamicVars.GetIntOrDefault("charges");

// 读取 decimal 值
decimal val = card.DynamicVars.GetValueOrDefault("charges");

// 检查是否为正值
bool active = card.DynamicVars.HasPositiveValue("charges");
```

---

## 相关文档

- [内容注册规则](ContentAuthoringToolkit.md)
- [快速入门](GettingStarted.md)
