ModifierRarity
===
___
To make a new ModifierRarity, simply make a new class that inherits the class `ModifierRarity` in the `Loot.System` namespace.

#### Primary functions
___
The primary use of a rarity is to provide some representation of how strong a rolled item's modifiers are. For most rarities, you'll just want to be overriding these properties: (example is Common Rarity)
```csharp
public class CommonRarity : ModifierRarity
{
    public override string Name => "Common";
    public override Color Color => Color.White;
    public override float RequiredRarityLevel => 0f;
}
```

A rarity is always rolled by its RequiredRarityLevel, and the highest matching rarity is always selected. A higher required level indicates a more powerful item, so the rarity should signify that.

#### Extra functions
___
A rarity can add a prefix and suffix to an item name, as well as change the color of it in the tooltip.
```csharp
    public virtual Color? OverrideNameColor => null;
    public virtual string ItemPrefix => null;
    public virtual string ItemSuffix => null;
```
        
#### Saving and loading data
___     
You can save and load data exactly like how you can do it for a Modifier (see above)