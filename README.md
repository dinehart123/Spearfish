# Spearfish

## Overview

A first-person survival/spearfishing game where the player lives out of a boat and dives into the ocean to hunt fish, earn money, upgrade equipment, and survive increasingly dangerous sea creatures.

The player begins with very basic equipment and gradually progresses from catching fish by hand to using equipment such as knives, spears, and eventually spearguns.

The main gameplay loop is:

**Dive → Hunt → Collect → Return to Boat → Sell → Upgrade → Repeat**

The longer the player stays underwater and the more they collect, the greater the potential reward—but also the greater the risk. The longer you spend on a dive the more XP you get. This will prevent people from catching one fish and trying to cash out asap. Also increase the odds of rarer/more expensive fish spawning the longer the dive. Encourage a better balance of longer dives and faster sells.

---

## Core Gameplay Loop

### 1. Prepare
The player starts on their boat and chooses their equipment before entering the water.

### 2. Dive
Explore underwater areas while managing:

- Oxygen
- Health
- Weight
- Temperature
- Dangerous sea creatures

### 3. Hunt & Collect
Catch or kill fish and collect anything valuable found underwater.

Different fish have different values based on their rarity and difficulty.

No crosshair to make it more immersive and more difficult. 

### 4. Decide When to Return
The player must decide whether to:

- Continue diving for more valuable fish
- Return because inventory weight is becoming dangerous
- Return because oxygen/health is low
- Return because the boat is full

Staying underwater longer offers greater potential rewards but increases the chance of dying.

### 5. Sell
Return to the boat and sell the catch.

Fish lose value over time, so **fresh fish are worth more money**. This encourages the player to balance long dives against returning frequently to sell their catch.

### 6. Upgrade
Money can be spent on equipment that allows the player to dive longer, move faster, carry more, and hunt more dangerous/valuable creatures.

---

# Player Stats

The player has four primary survival stats:

### Health
Represents the player's physical condition.

Health can be lost from:

- Dangerous sea creatures
- Running out of oxygen
- Environmental hazards

### Oxygen
Decreases while underwater.

Oxygen is consumed faster when:

- Sprint swimming
- Carrying excessive weight
- Potentially using certain equipment

When oxygen becomes critically low:

- Vision begins to blur
- Health begins decreasing

### Weight
Represents the total weight the player is carrying.

Fish, equipment, and collected items contribute to weight.

Higher weight causes:

- Slower ascent
- Faster descent
- Increased oxygen consumption
- Potential movement penalties

### Temperature
Represents the player's body temperature.

Cold water gradually decreases temperature. Equipment such as better wetsuits can reduce temperature loss.

---

# Player Progression

Players earn **XP by eating fish**.

XP can be used to improve permanent player stats.

Upgradeable stats include:

- **Health** — increases maximum health
- **Carry Capacity** — increases how much weight can be carried before penalties begin
- **Oxygen** — increases maximum oxygen
- **Swim Speed** — increases underwater movement speed
- **Damage** — increases damage dealt to sea creatures

These upgrades remain after death.

---

# Equipment Progression

Players use money from selling fish to purchase better equipment.

### Weapons

Possible weapon progression:

**Hands → Knife → Spear → Speargun → Upgraded Spearguns**

Better weapons allow the player to hunt larger and more valuable creatures.

### Diving Equipment

Possible upgrades include:

- Goggles / masks
- Wetsuits
- Fins
- Diving weights / belts
- Oxygen/scuba tanks
- Improved spearfishing equipment

Equipment can improve stats such as:

- Oxygen capacity
- Swim speed
- Temperature resistance
- Carry capacity
- Damage

### Boat Upgrades

The player can eventually purchase larger/better boats.

Boat upgrades could provide:

- More fish storage
- More equipment storage
- Longer trips
- Additional upgrade/equipment options

---

# Death

Death should have a significant penalty.

When the player dies:

- All currently carried fish/items are lost
- Potentially lose equipped gear depending on balancing
- Money/progression can remain
- Permanent player stat upgrades remain

This creates a risk/reward decision between continuing to hunt and safely returning to the boat.

---

# Fish & Sea Creatures

The ocean should contain a mixture of common fish, valuable fish, and dangerous creatures.

### Common Creatures

Use free/simple assets where possible:

- Small fish
- Medium fish
- Lobsters
- Crabs

These make up most of the player's early-game catches.

### Rare / Valuable Creatures

Examples:

- Sailfish
- Eels
- Stingrays
- Octopus
- Large/rare fish

These should be harder to find or kill but sell for significantly more money.

### Dangerous Creatures

Examples:

- Sharks
- Eels
- Jellyfish
- Other aggressive predators

Some creatures may attack naturally while others could become aggressive when provoked.

### Other Wildlife

Non-hostile wildlife could make the ocean feel more alive:

- Sea turtles
- Seals
- Rays
- Schools of fish

---

# Blood / Predator System

Killing or injuring creatures can release blood into the water.

Blood could temporarily increase the chance of attracting predators such as sharks.

This creates another risk/reward mechanic:

**Killing valuable fish → Blood in water → Predators attracted → Player must decide whether to stay or escape**

---

# Day / Night System

If time allows, the game can include a day/night cycle.

### Day

- Better visibility
- Safer diving
- More common fish
- Easier exploration

### Night

- Significantly reduced visibility
- More dangerous creatures
- Increased predator activity
- Rare/night-exclusive fish
- Potentially higher-value catches

Night creatures could include:

- Anglerfish
- Bioluminescent fish
- Other deep-sea creatures

This makes nighttime diving **high-risk/high-reward**.

---

# Ocean Conditions

Ocean conditions can vary between dives/days.

One major variable is **visibility**.

Some days may have:

- Clear water
- Medium visibility
- Very poor visibility

Poor visibility makes finding fish harder and dangerous creatures more difficult to detect.

Future environmental variables could include:

- Water temperature
- Currents
- Weather
- Waves

---

# Controls

| Input | Action |
|---|---|
| `WASD` | Swim/move |
| `Mouse` | Look around |
| `Space` | Ascend |
| `Alt` | Descend |
| `Shift` | Sprint swim |
| `Left Click` | Attack/use weapon |

Additional controls can be added as equipment and inventory systems are implemented.

---

# Core Risk vs. Reward

The game should constantly make the player choose between **returning safely** and **pushing their luck**.

For example:

> The player catches several valuable fish and is carrying a lot of weight. Their oxygen is getting low, their ascent speed has decreased, and blood from a recently killed fish may attract sharks. They can return to the boat and secure their money or continue deeper looking for an even rarer fish.

The goal is for these systems to interact so that even a simple fishing trip can become increasingly dangerous as the player collects more valuable items.

---

# Development Priorities

Since the initial version needs to remain achievable, development should focus on the core gameplay before optional systems.

### MVP / Must Have

- First-person swimming
- Ascending/descending
- Oxygen system
- Health system
- Basic fish AI
- Fish hunting/damage
- Fish collection/inventory
- Basic weapon
- Boat/safe area
- Selling fish
- Money
- Basic equipment upgrades
- Player death/reset

### Secondary

- Weight system
- Multiple weapons
- Multiple fish species
- Hostile creatures
- Equipment progression
- Player stat upgrades
- Fish freshness/value system

### Stretch Goals

- Day/night cycle
- Night-exclusive creatures
- Dynamic visibility
- Temperature system
- Blood attracting predators
- Boat upgrades
- Advanced scuba equipment
- Weather/ocean conditions
- Large variety of rare creatures
  
---

# Team Spearfish
### Members:
- Ethan Dinehart: environment & ambience
- Ryan Theliji: player
- Pranav : enemies
