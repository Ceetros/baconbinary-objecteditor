# BSUIT File Format Specification (.meta and .asset)

This document describes the architecture of the `.meta` and `.asset` file formats, designed to replace `.dat` and `.spr` formats with a focus on performance, extensibility, and security.

## Security and Encryption

All `.meta` and `.asset` files support native encryption.

### Encrypted File Structure

To ensure the file can be identified before decryption, the initial header remains in plain text.

| Field            | Type      | Description                                                               |
|------------------|-----------|---------------------------------------------------------------------------|
| `Signature`      | `char[]`  | "BSUIT" (5 bytes) or "BASSET" (6 bytes).                                  |
| `FormatVersion`  | `byte`    | File structure version.                                                   |
| `EncryptionType` | `byte`    | Encryption type: `0x00` = None, `0x01` = AES-256-CBC.                     |
| `IV`             | `byte[16]`| Initialization Vector (IV) for AES. Randomly generated upon saving.       |
| `EncryptedData`  | `byte[]`  | The rest of the file (Lookup Table + Data Blocks), encrypted.             |

### Encryption Algorithm (AES-256-CBC)

- **Algorithm:** AES (Advanced Encryption Standard).
- **Mode:** CBC (Cipher Block Chaining).
- **Padding:** PKCS7.
- **Key:** 32 bytes (256 bits). Provided by the user.
- **IV:** 16 bytes (128 bits). Stored in the file header (plain text).

---

## 1. Metadata File (`project.meta`)

Replaces `.dat`. Contains all object information except pixel data.

### File Body (Post-Decryption)

| Field                 | Type          | Description                                                            |
|-----------------------|---------------|------------------------------------------------------------------------|
| **Internal Header** |               |                                                                        |
| `ClientVersionMajor`  | `byte`        | Major client version (e.g., 10 for 10.98).                             |
| `ClientVersionMinor`  | `byte`        | Minor client version (e.g., 98 for 10.98).                             |
| `FeatureFlags`        | `byte`        | Bitmask of global features (see table below).                          |
| **Lookup Table** |               |                                                                        |
| `ItemCount`           | `uint`        | Total count of Items.                                                  |
| `OutfitCount`         | `uint`        | Total count of Outfits.                                                |
| `EffectCount`         | `uint`        | Total count of Effects.                                                |
| `MissileCount`        | `uint`        | Total count of Missiles.                                               |
| **Asset Mapping** |               |                                                                        |
| `AssetFileCount`      | `uint`        | Number of generated .asset files.                                      |
| *Loop (AssetFileCount)*|              |                                                                        |
| `FileIndex`           | `byte`        | File index (0, 1, 2...).                                               |
| `StartSpriteID`       | `uint`        | ID of the first sprite in this file.                                   |
| `EndSpriteID`         | `uint`        | ID of the last sprite in this file.                                    |
| **Thing Addresses** |               |                                                                        |
| `ItemAddresses`       | `uint[]`      | Array containing absolute offsets of each Item in the decrypted body.  |
| `OutfitAddresses`     | `uint[]`      | Array containing absolute offsets of each Outfit.                      |
| `EffectAddresses`     | `uint[]`      | Array containing absolute offsets of each Effect.                      |
| `MissileAddresses`    | `uint[]`      | Array containing absolute offsets of each Missile.                     |
| **Data Blocks** | `byte[]`      | Concatenation of data blocks for each `Thing`.                         |

#### `FeatureFlags` Definition

| Bit | Flag | Description |
|---|---|---|
| 0 | `Extended` | Sprites are `uint` (4 bytes) instead of `ushort` (2 bytes). |
| 1 | `Transparency` | Sprites use alpha channel (RGBA). |
| 2 | `FrameDurations` | Animated objects have custom frame durations. |
| 3 | `FrameGroups` | Outfits use multiple animation groups. |

---

### `Thing` Block Structure

Each object in the file follows this structure:

| Field             | Type     | Description                                                                                                   |
|-------------------|----------|---------------------------------------------------------------------------------------------------------------|
| `BlockSize`       | `uint`   | **(4 bytes)** Total size of the remaining block in bytes. Allows skipping to the next object quickly.         |
| `CommonFlags`     | `uint`   | 32-bit Bitmask for properties common to all types.                                                            |
| `ItemFlags`       | `uint`   | 32-bit Bitmask for properties exclusive to Items. Will be 0 if not an item.                                   |
| `ConditionalData` | `byte[]` | Block containing only data for flags that are set, written in the order defined below.                        |
| `GroupCount`      | `byte`   | Number of `FrameGroup`s this object possesses.                                                                |
| `FrameGroups`     | `byte[]` | Concatenation of data blocks for each `FrameGroup`.                                                           |

#### Flags Definition and Conditional Data Order

**CommonFlags (uint):**
| Bit | Flag | Conditional Data (Type) |
|---|---|---|
| 0 | AnimateAlways | - |
| 1 | LyingObject | - |
| 2 | TopEffect | - |
| 3 | Translucent | - |
| 4 | HasOffset | `short OffsetX`, `short OffsetY` |
| 5 | HasElevation | `ushort Elevation` |
| 6 | IsRotatable | - |
| 7 | IsHangable | - |
| 8 | IsHookSouth | - |
| 9 | IsHookEast | - |
| 10 | HasLight | `ushort LightLevel`, `ushort LightColor` |
| 11 | DontHide | - |
| 12 | IsMiniMap | `ushort MiniMapColor` |
| 13 | IsLensHelp | `ushort LensHelp` |
| 14 | IgnoreLook | - |
| 15 | IsCloth | `ushort ClothSlot` |
| 16 | HasDefaultAction | `ushort DefaultAction` |
| 17 | IsWrappable | - |
| 18 | IsUnwrappable | - |
| 19 | IsUsable | - |
| 20 | IsVertical | - |
| 21 | IsHorizontal | - |

**ItemFlags (uint):**
| Bit | Flag | Conditional Data (Type) |
|---|---|---|
| 0 | IsGround | `ushort GroundSpeed` |
| 1 | IsGroundBorder | - |
| 2 | IsOnBottom | - |
| 3 | IsOnTop | - |
| 4 | IsContainer | - |
| 5 | IsStackable | - |
| 6 | ForceUse | - |
| 7 | IsMultiUse | - |
| 8 | IsWritable | `ushort MaxTextLength` |
| 9 | IsReadable | `ushort MaxTextLength` |
| 10 | IsFluidContainer | - |
| 11 | IsFluid | - |
| 12 | IsSplash | - |
| 13 | IsUnpassable | - |
| 14 | IsUnmoveable | - |
| 15 | BlockMissile | - |
| 16 | BlockPathfind | - |
| 17 | NoMoveAnimation | - |
| 18 | IsPickupable | - |
| 19 | IsFullGround | - |
| 20 | IsMarketItem | `ushort Category`, `ushort TradeAs`, `ushort ShowAs`, `ushort NameLen`, `string Name`, `ushort Profession`, `ushort Level` |

---

### `FrameGroup` Block Structure

| Field         | Type     | Description                                                                                            |
|---------------|----------|--------------------------------------------------------------------------------------------------------|
| `Width`       | `byte`   | Group width in tiles.                                                                                  |
| `Height`      | `byte`   | Group height in tiles.                                                                                 |
| `Layers`      | `byte`   | Number of layers.                                                                                      |
| `PatternX`    | `byte`   | Number of variations on the X axis.                                                                    |
| `PatternY`    | `byte`   | Number of variations on the Y axis.                                                                    |
| `PatternZ`    | `byte`   | Number of variations on the Z axis.                                                                    |
| `Frames`      | `byte`   | Number of animation frames.                                                                            |
| `AnimData`    | `byte[]` | **If Frames > 1:** `byte Mode`, `uint LoopCount`, `byte StartFrame`, `uint[Frames*2] Durations`.       |
| `SpriteCount` | `uint`   | Total count of sprites in this group.                                                                  |
| `SpriteIDs`   | `uint[]` | Array containing sprite IDs.                                                                           |

---

## 2. Asset File (`project.asset`)

Replaces `.spr`. Contains only pixel data, optimized for random access.

### File Body (Post-Decryption)

| Field             | Type     | Description                                                                                           |
|-------------------|----------|-------------------------------------------------------------------------------------------------------|
| **Internal Header**|          |                                                                                                       |
| `CompressionType` | `byte`   | Enum: 0=None, 1=LZ4, 2=Zstd.                                                                          |
| `SpriteCount`     | `uint`   | Total sprite count **in this file**.                                                                  |
| **Lookup Table** |          |                                                                                                       |
| `SpriteAddresses` | `uint[]` | Array containing offsets for each sprite in the file. Address 0 indicates an empty sprite.            |
| **Data Blocks** | `byte[]` | Concatenation of data blocks for each sprite.                                                         |

### Sprite Block Structure

- **With Compression (LZ4/Zstd):**
  - `int CompressedSize`: Size of the compressed block.
  - `byte[] CompressedData`: Compressed data.
- **Without Compression:**
  - `byte[] PixelData`: Raw RGBA pixels (32x32x4 = 4096 bytes).

### Splitting into Multiple Files (`.asset0`, `.asset1`, ...)

- The system automatically splits assets into multiple files if the number of sprites exceeds 1,000,000.
- The `.meta` file contains the `Asset Mapping` table which indicates which ID range is in which file.
- Example:
  - `project.asset0`: Sprites 1 to 1,000,000
  - `project.asset1`: Sprites 1,000,001 to 2,000,000
