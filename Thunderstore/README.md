# Description

## A simple `client side mod` that will show you which mods you have that might be out of date, based on age alone.

It will show you what version you have installed, the last time it updated (in your local time), as well as the latest version available on https://valheim.thunderstore.io/. Mods listed in the UI are potential mods that you might need to remove due to age. Some mods can be older than the latest game update without issues. Keep that in mind and only use this as a guide in finding updates or bugs with your mods.

`Note: The configuration values for this mod should be input based on your local time. The time the mod was uploaded to Thunderstore is in UTC but converted and displayed to you in your local time on the UI. The converted local time is what the configuration values are being compared to. The default configuration values are in EST of shortly after the update. I will change this default configuration value depending on which update breaks the most mods.`  

---

![https://i.imgur.com/VgqH93a.png](https://i.imgur.com/VgqH93a.png)


## ModAge Plugin Configuration

v1.0.4 configuration file set for EST time zone for update 0.217.38.

Reminder: The configuration values for this mod should be input based on your local time. The default configuration values are in EST of shortly after the update. I will change this default configuration value depending on which update breaks the most mods.

#### ShowAllMods
- **Description:** Choose whether to show all mods or only the outdated ones.
- **Type:** Toggle (On/Off)
- **Default Value:** Off
- **Acceptable Values:** Off, On
- **Configuration Line:** `ShowAllMods = Off`

#### YearToTarget
- **Description:** Set the target year for comparing mod updates.
- **Type:** Int32
- **Default Value:** 2023
- **Configuration Line:** `YearToTarget = 2023`

#### MonthToTarget
- **Description:** Set the target month for comparing mod updates.
- **Type:** Int32
- **Default Value:** 12
- **Configuration Line:** `MonthToTarget = 12`

#### DayToTarget
- **Description:** Set the target day for comparing mod updates.
- **Type:** Int32
- **Default Value:** 14
- **Configuration Line:** `DayToTarget = 14`

#### HourToTarget
- **Description:** Set the target hour for comparing mod updates. It's advised to set this slightly after the expected update time for accuracy.
- **Type:** Int32
- **Default Value:** 4
- **Configuration Line:** `HourToTarget = 4`

#### MinuteToTarget
- **Description:** Set the target minute for comparing mod updates. It's advised to set this slightly after the expected update time for accuracy.
- **Type:** Int32
- **Default Value:** 0
- **Configuration Line:** `MinuteToTarget = 0`

#### SecondToTarget
- **Description:** Set the target second for comparing mod updates. It's advised to set this slightly after the expected update time for accuracy.
- **Type:** Int32
- **Default Value:** 0
- **Configuration Line:** `SecondToTarget = 0`

---

**Plugin GUID:** Azumatt.ModAge



<details>
<summary><b>Installation Instructions</b></summary>

***You must have BepInEx installed correctly! I can not stress this enough.***

### Manual Installation

`Note: (Manual installation is likely how you have to do this on a server, make sure BepInEx is installed on the server correctly)`

1. **Download the latest release of BepInEx.**
2. **Extract the contents of the zip file to your game's root folder.**
3. **Download the latest release of ModAge from Thunderstore.io.**
4. **Extract the contents of the zip file to the `BepInEx/plugins` folder.**
5. **Launch the game.**

### Installation through r2modman or Thunderstore Mod Manager

1. **Install [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/)
   or [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager).**

   > For r2modman, you can also install it through the Thunderstore site.
   ![](https://i.imgur.com/s4X4rEs.png "r2modman Download")

   > For Thunderstore Mod Manager, you can also install it through the Overwolf app store
   ![](https://i.imgur.com/HQLZFp4.png "Thunderstore Mod Manager Download")
2. **Open the Mod Manager and search for "ModAge" under the Online
   tab. `Note: You can also search for "Azumatt" to find all my mods.`**

   `The image below shows VikingShip as an example, but it was easier to reuse the image.`

   ![](https://i.imgur.com/5CR5XKu.png)

3. **Click the Download button to install the mod.**
4. **Launch the game.**

</details>

<br>
<br>

# Special Thanks

I would like to say thank you to Margmas for listening to me complain about the Thunderstore API not really being able
to map mods to it very well. His [ValheimModVersionCheck bot](https://github.com/MSchmoecker/ValheimModVersionCheck) is
something widely used in most Valheim modding discords.
He added an [API](https://mod-version-check.eu/api/docs#) so that I might leech off of the information he gathers. It
bridges the gap where Thunderstore's API
fell short. Thank you Margmas!

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)

`Feel free to reach out to me on discord if you need manual download assistance.`

Source code can be found here: [https://github.com/AzumattDev/ModAge](https://github.com/AzumattDev/ModAge)

# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>