# Polymorph Spell
*Erm, technically this spell requires a caterpillar cocoon* ☝️🤓

A spell spell which turns other mages into animals,
rendering them unable to communicate or use spells until the effect wares off or they die.

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
			   └── Sounds/
			       └── Polymorph_Drop.wav
			       └── Polymorph_Equip.wav
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
* **0.2.0**: Add casting/subsiding sounds and spell duration configuration

* **0.1.0**: Add spell page with visuals and pickup/drop sounds

## Credits
* **Polymorph Icon**: "Baldurs Gate 3" by Larian Studios - https://bg3.wiki/wiki/Polymorph
* **Casting sound**: "Magic spell (small positive)" by Nakhas - https://freesound.org/people/Nakhas/sounds/506939/
