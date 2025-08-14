# Polymorph Spell
*Erm, technically this spell requires a caterpillar cocoon* ☝️🤓

A spell which turns other mages into animals,
rendering them unable to communicate or use spells until the effect wares off, or they die.

## Spell Information
* **Name**: "Polymorph"
* **Cooldown**: 60 seconds
* **Duration**: 5 seconds + 1 per spell level
* **Range**: 20 meters maximum
* **Cone of Vision**: 45°
* **Team Chest**: Can spawn in team chest

## Installation

1. **Prerequisites**:
   - BepInEx 5.4.21
   - ModSync
   - BlackMagicAPI

2. **Installation**:
- After downloading the mod, place the files in the following locations:

   ```
   MageArena/
   └── BepInEx/
       └── plugins/
           └── PolymorphSpell/
			   └── PolymorphSpell.dll
			   └── AssetBundles/
			       └── polymorph
			   └── Sounds/
			       └── Polymorph_Cast.wav
			       └── Polymorph_Drop.wav
			       └── Polymorph_Equip.wav
			       └── Polymorph_Subside.wav
			   └── Sprites/
			       └── Polymorph_Emission.png
			       └── Polymorph_Main.png
			       └── Polymorph_Ui.png
   ```
## Configuration
Edit `BepInEx/config/com.YeahThatsMJ.PolymorphSpell.cfg` to adjust settings.

Compatible with [MageConfigurationAPI by D1GQ](https://thunderstore.io/c/mage-arena/p/D1GQ/MageConfigurationAPI/)!

The following settings can be adjusted:
* Cooldown
* Range
* Team chest spawning
* Spell duration length
* Duration increase per spell level

## Changelog
* **0.11.0**: Add cow and sheep as alternate polymorph

* **0.10.0**: Add penguin as alternate polymorph

* **0.8.0**: Fix polymorph victim's item still being visible in the air and the victim's item not being visible client-side upon un-polymorphing

* **0.7.0**: Polymorph animation is synced between all clients

* **0.6.0**: Polymorph has its own health and damage carries over to player

* **0.5.0**: Mute player upon polymorph

* **0.4.1**: Can't target already polymorphed players

* **0.4.0**: Add chicken sounds

* **0.3.1**: Fix polymorph ending immediately

* **0.3.0**: Add controllable polymorph

* **0.2.0**: Add casting/subsiding sounds and spell duration configuration

* **0.1.0**: Add spell page with visuals and pickup/drop sounds

## Credits
* **Polymorph Icon**: "Baldurs Gate 3" by Larian Studios - https://bg3.wiki/wiki/Polymorph
* **Casting sound**: "Magic spell (small positive)" by Nakhas - https://freesound.org/people/Nakhas/sounds/506939/
* **Chicken, Penguin models**: "Animals FREE - Animated Low Poly 3D Models" by ithappy - https://assetstore.unity.com/packages/3d/characters/animals/animals-free-animated-low-poly-3d-models-260727
* **Star explosion effect**: "Magic Effects FREE" by Hovl Studio - https://assetstore.unity.com/packages/vfx/particles/spells/magic-effects-free-247933
* **Chicken sounds**: "chicken clucking type 3" by RibhavAgrawal - https://pixabay.com/sound-effects/chicken-cluking-type-3-293320/
* **Penguin sounds**: "Gunter" by Adventure Time - https://en.wikipedia.org/wiki/Adventure_Time
* **Sheep model**: "Animated Goat and Sheep- 3D low poly-FREE" by UrsaAnimations - https://assetstore.unity.com/packages/3d/characters/animals/animated-goat-and-sheep-3d-low-poly-free-251910
* **Sheep sounds**: "sheep baaing type 01" by RibhavAgrawal - https://pixabay.com/sound-effects/sheep-baaing-type-01-293306/
* **Cow model**: "LowPoly Animated Animals" by Quaternius - https://quaternius.itch.io/lowpoly-animated-animals
* **Cow sounds**: "cow mooing type 02" by RibhavAgrawal - https://pixabay.com/sound-effects/cow-mooing-type-02-293304/
