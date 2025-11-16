# RaidExtractor JSON Schema Documentation

Version: 1.0.0

This document describes the JSON export format produced by RaidExtractor v1.0.0.

## File Overview

The extractor produces four JSON files per extraction:

- `roster.json` - Champion roster data
- `artifacts.json` - Artifact/gear data
- `account.json` - Account-level data
- `metadata.json` - Extraction metadata

All files use UTF-8 encoding with camelCase naming convention.

---

## roster.json

Contains all champion data including base stats, skills, masteries, and equipped artifacts.

### Schema

```json
{
  "champions": [
    {
      "championId": <integer>,
      "name": <string>,
      "rarity": <string>,
      "role": <string>,
      "fraction": <string>,
      "element": <string>,
      "grade": <string>,
      "level": <integer>,
      "experience": <integer>,
      "fullExperience": <integer>,
      "awakenLevel": <integer>,
      "locked": <boolean>,
      "inStorage": <boolean>,
      "marker": <string>,
      "stats": {
        "health": <float>,
        "attack": <float>,
        "defense": <float>,
        "speed": <float>,
        "accuracy": <float>,
        "resistance": <float>,
        "criticalChance": <float>,
        "criticalDamage": <float>,
        "criticalHeal": <float>
      },
      "skills": [
        {
          "id": <integer>,
          "typeId": <integer>,
          "level": <integer>
        }
      ],
      "masteries": [<integer>, ...],
      "artifacts": [<integer>, ...]
    }
  ]
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `championId` | integer | Unique champion instance ID |
| `name` | string | Champion name |
| `rarity` | string | Champion rarity (Common, Uncommon, Rare, Epic, Legendary) |
| `role` | string | Champion role (Attack, Defense, HP, Support) |
| `fraction` | string | Champion faction |
| `element` | string | Champion affinity (Magic, Force, Spirit, Void) |
| `grade` | string | Star rating (1-6) |
| `level` | integer | Current level (1-60) |
| `experience` | integer | Current experience points |
| `fullExperience` | integer | Total experience accumulated |
| `awakenLevel` | integer | Ascension level (0-6) |
| `locked` | boolean | Whether champion is locked from being used as food |
| `inStorage` | boolean | Whether champion is in vault storage |
| `marker` | string | User-assigned marker/tag |
| `stats.health` | float | Total HP including gear bonuses |
| `stats.attack` | float | Total ATK including gear bonuses |
| `stats.defense` | float | Total DEF including gear bonuses |
| `stats.speed` | float | Total SPD including gear bonuses |
| `stats.accuracy` | float | Total ACC including gear bonuses |
| `stats.resistance` | float | Total RES including gear bonuses |
| `stats.criticalChance` | float | Total C.RATE including gear bonuses |
| `stats.criticalDamage` | float | Total C.DMG including gear bonuses |
| `stats.criticalHeal` | float | Total C.HEAL including gear bonuses |
| `skills` | array | List of champion skills |
| `skills[].id` | integer | Skill instance ID |
| `skills[].typeId` | integer | Skill type ID (static reference) |
| `skills[].level` | integer | Skill level (0-5) |
| `masteries` | array | List of mastery IDs unlocked |
| `artifacts` | array | List of artifact IDs equipped on this champion |

### Example

```json
{
  "champions": [
    {
      "championId": 12345,
      "name": "Kael",
      "rarity": "Rare",
      "role": "Attack",
      "fraction": "Dark Elves",
      "element": "Magic",
      "grade": "6",
      "level": 60,
      "experience": 1547230,
      "fullExperience": 1547230,
      "awakenLevel": 6,
      "locked": true,
      "inStorage": false,
      "marker": "Default",
      "stats": {
        "health": 15450.0,
        "attack": 1542.0,
        "defense": 958.0,
        "speed": 105.0,
        "accuracy": 0.15,
        "resistance": 0.30,
        "criticalChance": 0.85,
        "criticalDamage": 2.10,
        "criticalHeal": 0.10
      },
      "skills": [
        {
          "id": 1001,
          "typeId": 5001,
          "level": 5
        }
      ],
      "masteries": [101, 102, 201],
      "artifacts": [9001, 9002, 9003, 9004, 9005, 9006]
    }
  ]
}
```

---

## artifacts.json

Contains all artifact/gear data including bonuses and upgrade levels.

### Schema

```json
{
  "artifacts": [
    {
      "artifactId": <integer>,
      "set": <string>,
      "kind": <string>,
      "rank": <string>,
      "rarity": <string>,
      "level": <integer>,
      "isActivated": <boolean>,
      "isSeen": <boolean>,
      "requiredFraction": <string | null>,
      "sellPrice": <integer>,
      "price": <integer>,
      "failedUpgrades": <integer>,
      "primaryBonus": {
        "kind": <string>,
        "isAbsolute": <boolean>,
        "value": <float>
      },
      "secondaryBonuses": [
        {
          "kind": <string>,
          "isAbsolute": <boolean>,
          "value": <float>,
          "enhancement": <float>,
          "level": <integer>
        }
      ]
    }
  ]
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `artifactId` | integer | Unique artifact instance ID |
| `set` | string | Artifact set name |
| `kind` | string | Artifact slot (Weapon, Helmet, Shield, Gloves, Chest, Boots, Ring, Amulet, Banner) |
| `rank` | string | Artifact rank (1-6 stars) |
| `rarity` | string | Artifact rarity (Common, Uncommon, Rare, Epic, Legendary) |
| `level` | integer | Upgrade level (0-16) |
| `isActivated` | boolean | Whether artifact is equipped |
| `isSeen` | boolean | Whether artifact has been viewed by player |
| `requiredFraction` | string/null | Faction requirement (null if no requirement) |
| `sellPrice` | integer | Silver value when sold |
| `price` | integer | Purchase/upgrade cost |
| `failedUpgrades` | integer | Number of failed upgrade attempts |
| `primaryBonus.kind` | string | Primary stat type |
| `primaryBonus.isAbsolute` | boolean | Whether bonus is flat (true) or percentage (false) |
| `primaryBonus.value` | float | Primary stat value (0.0-1.0 for percentages) |
| `secondaryBonuses` | array | List of substats |
| `secondaryBonuses[].kind` | string | Substat type |
| `secondaryBonuses[].isAbsolute` | boolean | Whether substat is flat or percentage |
| `secondaryBonuses[].value` | float | Substat value |
| `secondaryBonuses[].enhancement` | float | Enhancement value from rolls |
| `secondaryBonuses[].level` | integer | Level at which substat unlocked |

### Example

```json
{
  "artifacts": [
    {
      "artifactId": 9001,
      "set": "Speed",
      "kind": "Boots",
      "rank": "5",
      "rarity": "Epic",
      "level": 16,
      "isActivated": true,
      "isSeen": true,
      "requiredFraction": null,
      "sellPrice": 45000,
      "price": 12000,
      "failedUpgrades": 2,
      "primaryBonus": {
        "kind": "Speed",
        "isAbsolute": true,
        "value": 45.0
      },
      "secondaryBonuses": [
        {
          "kind": "Attack",
          "isAbsolute": false,
          "value": 0.08,
          "enhancement": 0.05,
          "level": 4
        },
        {
          "kind": "CriticalRate",
          "isAbsolute": false,
          "value": 0.06,
          "enhancement": 0.03,
          "level": 8
        }
      ]
    }
  ]
}
```

---

## account.json

Contains account-level data including arena ranking, shards, great hall bonuses, and battle presets.

### Schema

```json
{
  "arenaLeague": <string>,
  "shards": {
    "<shardType>": {
      "count": <integer>,
      "summonData": [
        {
          "rarity": <string>,
          "pullCount": <integer>,
          "lastHeroId": <integer>
        }
      ]
    }
  },
  "greatHall": {
    "<element>": {
      "<statType>": <integer>
    }
  },
  "stagePresets": {
    "<stageId>": [<championId>, ...]
  }
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `arenaLeague` | string | Current arena league/tier |
| `shards` | object | Shard inventory and pity counter data |
| `shards.<type>.count` | integer | Number of shards of this type owned |
| `shards.<type>.summonData` | array | Pity system data per rarity |
| `shards.<type>.summonData[].rarity` | string | Rarity tier tracked |
| `shards.<type>.summonData[].pullCount` | integer | Pulls since last champion of this rarity |
| `shards.<type>.summonData[].lastHeroId` | integer | Last champion ID pulled at this rarity |
| `greatHall` | object | Great Hall stat bonuses by element |
| `greatHall.<element>.<stat>` | integer | Bonus level for this stat/element combination |
| `stagePresets` | object | Saved team presets for stages |
| `stagePresets.<stageId>` | array | Array of champion IDs in preset |

### Example

```json
{
  "arenaLeague": "Gold4",
  "shards": {
    "Ancient": {
      "count": 23,
      "summonData": [
        {
          "rarity": "Legendary",
          "pullCount": 157,
          "lastHeroId": 4523
        }
      ]
    }
  },
  "greatHall": {
    "Magic": {
      "Attack": 10,
      "Defense": 8,
      "Health": 10
    }
  },
  "stagePresets": {
    "12001": [12345, 12346, 12347, 12348, 12349]
  }
}
```

---

## metadata.json

Contains extraction metadata for tracking and versioning.

### Schema

```json
{
  "extractionTimestamp": <string>,
  "extractorVersion": <string>,
  "exportPath": <string>
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `extractionTimestamp` | string | ISO 8601 timestamp of extraction (UTC) |
| `extractorVersion` | string | RaidExtractor version used |
| `exportPath` | string | Absolute path to export directory |

### Example

```json
{
  "extractionTimestamp": "2025-11-16T14:32:15.0000000Z",
  "extractorVersion": "1.0.0",
  "exportPath": "C:\\RaidKiller\\exports"
}
```

---

## error.json

Generated when extraction fails. Only present on error.

### Schema

```json
{
  "error": <string>,
  "timestamp": <string>
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `error` | string | Error message describing failure |
| `timestamp` | string | ISO 8601 timestamp of error (UTC) |

### Example

```json
{
  "error": "RAID client not detected",
  "timestamp": "2025-11-16T14:32:15.0000000Z"
}
```

---

## Data Type Reference

| Type | Description | Example Values |
|------|-------------|----------------|
| `<integer>` | 32-bit signed integer | `123`, `-1`, `0` |
| `<float>` | 32-bit floating point | `1.5`, `0.85`, `15450.0` |
| `<boolean>` | Boolean value | `true`, `false` |
| `<string>` | UTF-8 text string | `"Kael"`, `"Attack"` |
| `<string\|null>` | String or null | `"Dark Elves"`, `null` |
| `<array>` | JSON array | `[1, 2, 3]`, `[]` |
| `<object>` | JSON object | `{"key": "value"}` |

---

## Schema Stability

This schema is **frozen at version 1.0.0**.

- No breaking changes will be introduced
- New optional fields may be added in future versions
- Parsers should ignore unknown fields for forward compatibility
- All existing fields will maintain their types and semantics

## Integration Notes

### Linking Data

- Champions reference artifacts via `champions[].artifacts[]` array containing artifact IDs
- Match these IDs to `artifacts[].artifactId` in `artifacts.json`
- Stage presets reference champions via `stagePresets.<id>[]` containing champion IDs
- Match these IDs to `champions[].championId` in `roster.json`

### Percentage Values

- Percentage values are represented as decimals (0.0 to 1.0)
- Example: 85% critical rate = `0.85`
- To display as percentage: multiply by 100

### Null Values

- Optional fields may be `null`
- Empty arrays are represented as `[]`, not `null`
- Missing optional object properties may be omitted entirely

### Character Encoding

All JSON files are UTF-8 encoded. Ensure your parser uses UTF-8 decoding.
