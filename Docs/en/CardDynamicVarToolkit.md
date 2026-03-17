# Card Dynamic Var Toolkit

This document covers dynamic variable construction, tooltip binding, and automatic card hover injection.

---

## Overview

The game's `DynamicVar` system lets cards carry runtime-variable values. RitsuLib adds:

- Convenience constructors via `ModCardVars`
- Per-variable tooltip binding via `DynamicVarExtensions`
- Automatic tooltip injection into the card hover sequence (via Patch)

---

## Variable Construction

Create variables with `ModCardVars` and include them in a card's `DynamicVarSet`:

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

| Method | Description |
|---|---|
| `ModCardVars.Int(name, amount)` | Creates a numeric variable (`decimal`) |
| `ModCardVars.String(name, value)` | Creates a string variable |

RitsuLib does not assign gameplay semantics to these variables. Their meaning is entirely defined by the content author.

---

## Tooltip Binding

Bind tooltips to variables at definition time via extension methods:

### Shared tooltip (recommended)

Reads from the `static_hover_tips` localization table:

```csharp
var myVar = ModCardVars.Int("my_var", 2)
    .WithSharedTooltip("my_mod_my_var");
// Resolves:
//   static_hover_tips["my_mod_my_var.title"]
//   static_hover_tips["my_mod_my_var.description"]
```

### Explicit table / key

```csharp
var myVar = ModCardVars.Int("my_var", 2)
    .WithTooltip(
        titleTable: "card_keywords",
        titleKey:   "my_mod_my_var.title",   // description defaults to .title → .description
        iconPath:   "res://MyMod/art/kw.png" // optional
    );
```

### Custom factory

```csharp
var myVar = ModCardVars.Int("my_var", 2)
    .WithTooltip(var => new HoverTip(
        new LocString("my_table", "my_var.title"),
        new LocString("my_table", "my_var.description")
    ));
```

---

## Localization

If using `WithSharedTooltip("my_mod_charges")`, provide these entries in `static_hover_tips`:

```json
{
  "my_mod_charges.title": "Charges",
  "my_mod_charges.description": "Accumulated charges that deal extra damage."
}
```

RitsuLib does not provide built-in localization entries for dynamic variables.

---

## Card Hover Injection

RitsuLib's patch automatically appends all registered dynamic-variable tooltips from `CardModel.DynamicVars` to the card hover-tip sequence. No additional setup is required; simply bind a tooltip to the variable.

---

## Clone Behavior

Tooltip metadata is copied to clones when `DynamicVar.Clone()` is called. This means upgraded or copied cards in combat carry the correct tooltips without any extra handling.

---

## Reading Var Values at Runtime

`DynamicVarExtensions` provides convenience methods for reading variable values:

```csharp
// Read int value (default 0)
int charges = card.DynamicVars.GetIntOrDefault("charges");

// Read decimal value
decimal val = card.DynamicVars.GetValueOrDefault("charges");

// Check for a positive value
bool active = card.DynamicVars.HasPositiveValue("charges");
```

---

## Related Documents

- [ContentAuthoringToolkit.md](ContentAuthoringToolkit.md)
- [GettingStarted.md](GettingStarted.md)
