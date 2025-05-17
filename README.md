# Description

## Always points your camera in the direction of the boss location after interacting with a Vegvisir, even when no map mode is not turned on. Does not add a pin for bosses and does not open the map.

`Client or Server mod. Server installation is optional, but when installed on a server, it will sync the Azumatt.BossDirections.Offerings.yml file to all clients.`

Mod made at the request of Mofker in the Odin Plus Discord.

ServerSync and Offering added at the request of Majestic.

---

<details>
<summary><b>Installation Instructions</b></summary>

***You must have BepInEx installed correctly! I can not stress this enough.***

### Manual Installation

`Note: (Manual installation is likely how you have to do this on a server, make sure BepInEx is installed on the server correctly)`

1. **Download the latest release of BepInEx.**
2. **Extract the contents of the zip file to your game's root folder.**
3. **Download the latest release of BossDirections from Thunderstore.io.**
4. **Extract the contents of the zip file to the `BepInEx/plugins` folder.**
5. **Launch the game.**

### Installation through r2modman or Thunderstore Mod Manager

1. **Install [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/)
   or [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager).**

   > For r2modman, you can also install it through the Thunderstore site.
   ![](https://i.imgur.com/s4X4rEs.png "r2modman Download")

   > For Thunderstore Mod Manager, you can also install it through the Overwolf app store
   ![](https://i.imgur.com/HQLZFp4.png "Thunderstore Mod Manager Download")
2. **Open the Mod Manager and search for "BossDirections" under the Online
   tab. `Note: You can also search for "Azumatt" to find all my mods.`**

   `The image below shows VikingShip as an example, but it was easier to reuse the image.`

   ![](https://i.imgur.com/5CR5XKu.png)

3. **Click the Download button to install the mod.**
4. **Launch the game.**

</details>

## Features

- **Instant Look-Direction**  
  After using a Vegvisir, your camera smoothly rotates to face the boss’s spawn point.

- **Custom “Offerings”**  
  Define items you burn at a fireplace to reveal or point at a boss location via the
  `Azumatt.BossDirections.Offerings.yml`.

- **Hot-reload & ServerSync**
    - **Solo / Client-only:** if you run the mod just on your client, it will load and hot-reload your local YAML in
      memory.
    - **Server-hosted YAML:** if your server also has BossDirections installed, it will serve its
      `Azumatt.BossDirections.Offerings.yml` down to all connecting clients automatically (via ServerSync).

---

## YAML Configuration

The default `Azumatt.BossDirections.Offerings.yml` is **embedded** in the mod and will be generated in your
`BepInEx/config` folder on first run if it’s missing. You can edit it in one of two ways:

1. **Solo / Client-only play:**
    - Edit `BepInEx/config/Azumatt.BossDirections.Offerings.yml` on your local machine.
    - The mod detects file changes and re-loads the offerings **in memory**.

2. **ServerSync mode (optional):**
    - Install BossDirections on your dedicated server and put the YAML in `BepInEx/config/` (or boot the server once
      with the mod to generate the default file).
    - Clients with the mod will receive the server’s copy on connect, overriding their local in-memory data.
    - Clients never write to disk; they simply use the synced data in memory.

### Embedded Default YAML

```yaml
# Azumatt.BossDirections.Offerings.yml
# Defines boss offerings using prefab names for items.
offerings:

  - location: StartTemple
    name: Stone Altar
    addname: false
    quotes:
      - "Begin again where the first light rose."
      - "Every journey starts beneath unyielding rock."
      - "Let these stones guide you back to your roots."
      - "In every ending lies the seed of a new start."
      - "Return now, and let the path reveal itself."
    items:
      Stone: 10      # prefab name: Stone

  - location: Vendor_BlackForest
    name: Haldor
    addname: true
    quotes:
      - "Wanderer, my wares await beneath ancient boughs."
      - "Coins jingling? Come, let me show you something special."
      - "Step into my shop—dwarven craftsmanship at its finest."
      - "You won’t regret this purchase, I guarantee it."
      - "Masks off, deals on—welcome, friend of the forge."
    items:
      Coins: 100     # prefab name: Coins

  - location: Hildir_camp
    name: Hildir
    addname: true
    quotes:
      - "Gems for the gallant—what riches do you seek?"
      - "Welcome, traveler! My jewels sparkle just for you."
      - "A ruby for courage, an amber for valor—choose wisely."
      - "Step closer; I have treasures you’ve only dreamed of."
      - "Bright stones for dark nights—take what you need."
    items:
      Ruby: 5   # prefab name: Ruby
      Amber: 20   # prefab name: Amber
      AmberPearl: 10   # prefab name: AmberPearl


  - location: Eikthyrnir
    name: Eikthyr
    addname: true
    quotes:
      - "Mortal, you dare summon my grace?"
      - "Hear my charge upon the winds of these plains."
      - "Bow or be swept aside by my antlers’ might."
      - "By hoof and horn, I call you forth."
      - "Stand tall, challenger, and face the storm."
    items:
      TrophyDeer: 1   # prefab name: TrophyDeer


  - location: GDKing
    name: Elder
    addname: true
    quotes:
      - "Roots run deep; what brings you to my ancient grove?"
      - "One seed sown is a forest reborn."
      - "Offer the old ways, and listen to the earth’s song."
      - "Let growth guide you through shadowed woods."
      - "Whisper your intent to the roots below."
    items:
      AncientSeed: 1  # prefab name: AncientSeed


  - location: Bonemass
    name: Bonemass
    addname: true
    quotes:
      - "Bones crumble, but my hunger endures."
      - "Swallow the marsh’s curse—or become it."
      - "Your flesh will nourish my endless appetite."
      - "Rise, intruder, and feed my legion of decay."
      - "I am the rot that never sleeps."
    items:
      WitheredBone: 10  # prefab name: WitheredBone


  - location: Dragonqueen
    name: Moder
    addname: true
    quotes:
      - "Frost bites deeper than steel. Prepare yourself."
      - "An egg’s chill heralds the queen’s return."
      - "Ice and flame dance at my whim."
      - "Feel the bite of winter’s heart."
      - "No mortal warmth can thaw my resolve."
    items:
      DragonEgg: 1     # prefab name: DragonEgg


  - location: GoblinKing
    name: Yagluth
    addname: true
    quotes:
      - "My totem stands as testament to goblin fury."
      - "Offer this, and I might spare your bones."
      - "Shadows hunger for your fear—feed them well."
      - "Gaze upon my power, worm."
      - "Your essence will strengthen my reign."
    items:
      GoblinTotem: 1   # prefab name: GoblinTotem


  - location: Mistlands_DvergrBossEntrance1
    name: Queen
    addname: true
    quotes:
      - "Hornets swarm for their queen—bring me proof."
      - "Your bravery will not save you from my hive."
      - "Steel yourself, for the sting is sweet."
      - "Betray the swarm, and taste its wrath."
      - "My children feast on interlopers—join them?"
    items:
      TrophySeekerBrute: 1  # prefab name: TrophySeekerBrute


  - location: FaderLocation
    name: Fader
    addname: true
    quotes:
      - "A father noble and proud, he soared through skies of fire."
      - "The madness lowered its shroud, and warped his heart's desire."
    items:
      BellFragment: 1  # prefab name: BellFragment
      MorgenHeart: 3  # prefab name: MorgenHeart

```

| Field      | Type              | Description                                                                                |
|------------|-------------------|--------------------------------------------------------------------------------------------|
| `location` | `string`          | Zone key from Valheim’s zone list (see link below).                                        |
| `name`     | `string`          | Display name for messaging.                                                                |
| `addname`  | `bool`            | Prepend `[BossName]` to your quote.                                                        |
| `quotes`   | `string[]`        | One or more messages—chosen at random when your offering is consumed.                      |
| `items`    | `map[string]int]` | Prefab or shared-name (no leading `$`, case-insensitive) → stack size required to trigger. |

## Zone Keys

- Use Valheim’s internal zone names for location, e.g.:
    - https://valheim-modding.github.io/Jotunn/data/zones/location-list.html

<br>
<br>

`Feel free to reach out to me on discord if you need manual download assistance.`

# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>