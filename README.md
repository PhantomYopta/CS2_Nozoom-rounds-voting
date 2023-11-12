# CS2_Nozoom-rounds-voting
With this plugin, players will be able to vote for the game without scopes.
# Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

2. Download NZ

3. Unzip the archive and upload it to the game server
# Configs
```
  "NzNeed": 0.6, // Percentage of all players required to start the game without sights.
  "NzRounds": 4, //The number of rounds without sighting after a successful vote. 
  "NzCooldownRounds": 4, // Number of rounds between votes.
  "DisableDeagle": true // Is the digil disabled for rounds without scope?
```
# Commands
`css_nz` `!nz` - vote for the game without sights
