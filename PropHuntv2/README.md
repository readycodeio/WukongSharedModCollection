# Commands

## prop [option]
Game state management (command available only to the host).
* **start** - Starts a new game (requires at least 2 players).
* **end** - Forces the end of the active round.

## propConfig [option] [value as number]
Configuration of game rules and parameters (command available only to the host when a round is not active).
* **maxhiderhp** - Sets the maximum health points for Hiders.
* **maxseekerhp** - Sets the maximum health points for Seekers.
* **maxhunter** - Sets the maximum number of Seekers in a round.
* **minhunter** - Sets the minimum number of Seekers in a round.
* **preparationtime** - Sets the duration of the preparation phase (in seconds).
* **gametime** - Sets the maximum duration of the actual game round (in seconds).
* **destroytamers** - Removes enemies/monsters from the game world for the duration of the round (value > 0 enables, 0 disables).
* **hpbarsvisible** - Enables or disables the visibility of health bars above players (value > 0 enables, 0 disables).
---

# Controls

* **Right Arrow** - Rotate prop to the right.
* **Left Arrow** - Rotate prop to the left.
* **Shift + F2** - Reroll prop (transform into a different object).
* **Shift + F3** - Taunt (plays a sound from the Hider's location).
* **Shift + F4** - Place Decoy (spawns a fake prop in the game world).