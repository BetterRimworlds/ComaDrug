# Better Rimworlds: Medical Coma Drug for RimWorld

A www.GlitterWorlds.dev Project by the [**Better Rimworlds org**](https://github.com/BetterRimworlds/).

[**Steam Workshop**](https://steamcommunity.com/sharedfiles/filedetails/?id=3442966730)

## Overview
This mod adds the capability to induce medical comas in pawns through a specialized drug. Medical comas can be used to
keep injured pawns in a stable state, prevent them from experiencing pain, and potentially increase healing efficiency.

![ComaDrug Image](https://images.steamusercontent.com/ugc/15304813754967414/91DC405327DA05F80A6C129C6AEFFFC6A3821769/?imw=637&imh=358&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=true)

## Features
- **Medical Coma Drug**: Administer to pawns to induce a controlled medical coma.
- **Duration**: The coma will last 5.55 in-game days.
- **Reduced Consciousness**: Pawns in a coma have their consciousness reduced to 10%.
- **Complete Immobilization**: Affected pawns cannot move or manipulate objects (0% capacity).
- **Medical Benefits**: Ideal for critically injured pawns or during complex surgeries.
- **Reduced Food Need**: Like in the real world, pawns' hunger need is only 15% of normal (1 meal per treatment).
- **Reduced Luciferium Need**: The pawn's Luciferium need reduces 75% slower than normal.
- **Provides Baseline Luciferium**: Luciferium addicts will never fall below 5% of the need.
- **Rapid Recovery**: Pawns will recover 2x faster from diseases.
- **Controlled Wakeup**: When administered Wake-Up, the pawn will immediately awaken.
- **Greater Survival Odds**: By massively reducing the pawn's dietary and Luciferium needs, it greatly increases their
    survival odds, especially during times of famine and extreme injury and disease.

## Installation
1. Download the latest release from the [GitHub repository](https://github.com/BetterRimWorlds/ComaDrug/releases)
2. Extract the contents into your RimWorld/Mods folder
3. Enable the mod in the game's Mod menu before starting a new game or loading a save

## Usage
1. **Research**: Complete the "Medical Comas" research project (requires Medicine Production).
2. **Production**: Craft the coma-inducing drug at a drug lab (requires medicine, devilstrand, and 1 luciferium).
3. **Administration**: Select the pawn, go to the Health tab, and select "Administer coma drug".
5. **Recovery**: Monitor pawns coming out of a coma as they may be disoriented.

The drug is balanced to:
- Provide medical benefits for injured pawns.
- Not cause addiction or tolerance.
- Have a controlled duration.
- Colonists need to be a Crafting skill of level 8 and an Intellectual skill of level 10.
- Reduces the hunger need by 85% to match real-world conditions.

## Compatibility
- Compatible with RimWorld v1.2-v1.6.
- Should work alongside most other mods.
- Not dependent on any other mods.
- May have interactions with mods that significantly alter consciousness or health systems.

![ComaDrug Sequence Diagram](https://github.com/user-attachments/assets/808f3810-2ef9-4b58-98dd-658d54f7c1e5)

## Future Plans
- Add effects of medical comas when the patient is in a hospital bed with a Vitals Monitor.


## Change Log

**v2.0.0: 2025-07-09**
- Added a mechanism on waking up the pawn with the Wake-Up drug.
- Increased the drug's immunity gain speed by 200%.
- Added a mechanism for reducing any Need at a constant slower or faster rate.
- Added a mechanism for keeping any Need at a constant minimum state and froze Luciferium need.
- Added support for Rimworld v1.6.
- Added support for Rimworld v1.3-v1.5.
- Majorly improved ./build.sh to handle XML changes as well.

**v1.0.1: 2025-03-12**
* **[2025-03-11 23:10:29 CDT]** Fixes to the research config.
* **[2025-03-11 23:10:38 CDT]** Reduce hunger by 85% to match real-world conditions.

**v1.0.0: 2025-03-11**
* Initial Release

## Better Rimworlds Stargate Mods

1. [**Stargate**](https://github.com/BetterRimworlds/Stargate) — Send Pawns and Items to other Savegames on the same computer.
2. [**CryoRegenesis**](https://github.com/BetterRimworlds/CryoRegenesis) — Forever Young Glittertech (a Rimworld take on the Goa'uld Sarcophagus).
3. [**ZPM**](https://github.com/BetterRimworlds/ZPM) — Build your own or buy an Archotech Zero-Point Module (Stargate Atlantis).
4. [**ZatGun**](https://github.com/BetterRimworlds/ZatGun) — An actual Zat'nik'tel, from the Stargate Universe. One shot stuns. Two shots kills.

## Other Better Rimworlds Mods

1. [**WakeUp Implant**](https://github.com/BetterRimworlds/WakeUpImplant) — Installs a brain implant that gives the effects of a permanent wakeup high.
2. [**Savegame Shrinker**](https://github.com/BetterRimworlds/RimworldSavegameShrinker) — Cleans up unnecessary data from long-running Savegames.
3. [**DeMaterializer**](https://github.com/BetterRimworlds/DeMaterializer) — Precursor to teleportation. Late-stage raid defense system

## Contributors

[Theodore R. Smith](https://github.com/hopeseekr/]) <hopeseekr@gmail.com>  
GPG Fingerprint: D8EA 6E4D 5952 159D 7759  2BB4 EEB6 CE72 F441 EC41  
WhatsApp / Signal: +1 832-303-9477

## License

**CC-BY-ND-4.0**
Creative Commons NoDerivations v4.0: Please see the [license file](LICENSE.md) for more information.

**YOU MAY FORK THIS PROJECT.**

**YOU MAY NOT PUBLISH ANY DERIVATION of this project to either your own website or a third-party host.**

**YOU MAY NOT PUBLISH ANY DERIVATION ON STEAM WORKSHOP WITHOUT EXPLICIT APPROVAL.**
