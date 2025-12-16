# Especificação dos Formatos de Arquivo BSUIT (.meta e .asset)

Este documento descreve a arquitetura dos formatos de arquivo `.meta` e `.asset`, projetados para substituir os formatos `.dat` e `.spr` com foco em performance, extensibilidade e segurança.

## Segurança e Criptografia

Todos os arquivos `.meta` e `.asset` suportam criptografia nativa.

### Estrutura do Arquivo Criptografado

Para garantir que o arquivo possa ser identificado antes da descriptografia, o cabeçalho inicial permanece em texto claro.

| Campo            | Tipo      | Descrição                                                                 |
|------------------|-----------|---------------------------------------------------------------------------|
| `Signature`      | `char[]`  | "BSUIT" (5 bytes) ou "BASSET" (6 bytes).                                  |
| `FormatVersion`  | `byte`    | Versão da estrutura do arquivo.                                           |
| `EncryptionType` | `byte`    | Tipo de criptografia: `0x00` = Nenhuma, `0x01` = AES-256-CBC.             |
| `IV`             | `byte[16]`| Vetor de Inicialização (IV) para o AES. Gerado aleatoriamente no salvamento.|
| `EncryptedData`  | `byte[]`  | O restante do arquivo (Lookup Table + Data Blocks), criptografado.        |

### Algoritmo de Criptografia (AES-256-CBC)

- **Algoritmo:** AES (Advanced Encryption Standard).
- **Modo:** CBC (Cipher Block Chaining).
- **Padding:** PKCS7.
- **Chave (Key):** 32 bytes (256 bits). Fornecida pelo usuário.
- **IV:** 16 bytes (128 bits). Armazenado no cabeçalho do arquivo (texto claro).

---

## 1. Arquivo de Metadados (`project.meta`)

Substitui o `.dat`. Contém todas as informações dos objetos, exceto os dados de pixel.

### Corpo do Arquivo (Pós-Descriptografia)

| Campo                 | Tipo          | Descrição                                                              |
|-----------------------|---------------|------------------------------------------------------------------------|
| **Header Interno**    |               |                                                                        |
| `ClientVersionMajor`  | `byte`        | Versão principal do cliente (ex: 10 para 10.98).                       |
| `ClientVersionMinor`  | `byte`        | Versão secundária do cliente (ex: 98 para 10.98).                      |
| `FeatureFlags`        | `byte`        | Bitmask de features globais (ver tabela abaixo).                       |
| **Lookup Table**      |               |                                                                        |
| `ItemCount`           | `uint`        | Contagem total de Itens.                                               |
| `OutfitCount`         | `uint`        | Contagem total de Outfits.                                             |
| `EffectCount`         | `uint`        | Contagem total de Effects.                                             |
| `MissileCount`        | `uint`        | Contagem total de Missiles.                                            |
| **Asset Mapping**     |               |                                                                        |
| `AssetFileCount`      | `uint`        | Número de arquivos .asset gerados.                                     |
| *Loop (AssetFileCount)*|              |                                                                        |
| `FileIndex`           | `byte`        | Índice do arquivo (0, 1, 2...).                                        |
| `StartSpriteID`       | `uint`        | ID do primeiro sprite neste arquivo.                                   |
| `EndSpriteID`         | `uint`        | ID do último sprite neste arquivo.                                     |
| **Thing Addresses**   |               |                                                                        |
| `ItemAddresses`       | `uint[]`      | Array com os offsets absolutos de cada Item no corpo descriptografado. |
| `OutfitAddresses`     | `uint[]`      | Array com os offsets absolutos de cada Outfit.                         |
| `EffectAddresses`     | `uint[]`      | Array com os offsets absolutos de cada Effect.                         |
| `MissileAddresses`    | `uint[]`      | Array com os offsets absolutos de cada Missile.                        |
| **Data Blocks**       | `byte[]`      | Concatenação dos blocos de dados de cada `Thing`.                      |

#### Definição de `FeatureFlags`

| Bit | Flag | Descrição |
|---|---|---|
| 0 | `Extended` | Sprites são `uint` (4 bytes) em vez de `ushort` (2 bytes). |
| 1 | `Transparency` | Sprites usam canal alfa (RGBA). |
| 2 | `FrameDurations` | Objetos animados têm durações de frame customizadas. |
| 3 | `FrameGroups` | Outfits usam múltiplos grupos de animação. |

---

### Estrutura de um Bloco de `Thing`

Cada objeto no arquivo segue esta estrutura:

| Campo           | Tipo     | Descrição                                                                                             |
|-----------------|----------|-------------------------------------------------------------------------------------------------------|
| `BlockSize`     | `uint`   | **(4 bytes)** Tamanho total do restante do bloco em bytes. Permite pular para o próximo objeto rapidamente. |
| `CommonFlags`   | `uint`   | Bitmask de 32 bits para propriedades comuns a todos os tipos.                                         |
| `ItemFlags`     | `uint`   | Bitmask de 32 bits para propriedades exclusivas de Items. Será 0 se não for um item.                  |
| `ConditionalData` | `byte[]` | Bloco contendo apenas os dados das flags que estão ativadas, escritos na ordem definida abaixo.       |
| `GroupCount`    | `byte`   | Número de `FrameGroup`s que este objeto possui.                                                       |
| `FrameGroups`   | `byte[]` | Concatenação dos blocos de dados de cada `FrameGroup`.                                              |

#### Definição das Flags e Ordem dos Dados Condicionais

**CommonFlags (uint):**
| Bit | Flag | Dados Condicionais (Tipo) |
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
| Bit | Flag | Dados Condicionais (Tipo) |
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

### Estrutura de um Bloco de `FrameGroup`

| Campo         | Tipo     | Descrição                                                              |
|---------------|----------|------------------------------------------------------------------------|
| `Width`       | `byte`   | Largura do grupo em tiles.                                             |
| `Height`      | `byte`   | Altura do grupo em tiles.                                              |
| `Layers`      | `byte`   | Número de camadas.                                                     |
| `PatternX`    | `byte`   | Número de variações no eixo X.                                         |
| `PatternY`    | `byte`   | Número de variações no eixo Y.                                         |
| `PatternZ`    | `byte`   | Número de variações no eixo Z.                                         |
| `Frames`      | `byte`   | Número de quadros de animação.                                         |
| `AnimData`    | `byte[]` | **Se Frames > 1:** `byte Mode`, `uint LoopCount`, `byte StartFrame`, `uint[Frames*2] Durations`. |
| `SpriteCount` | `uint`   | Contagem total de sprites neste grupo.                                 |
| `SpriteIDs`   | `uint[]` | Array com os IDs dos sprites.                                          |

---

## 2. Arquivo de Assets (`project.asset`)

Substitui o `.spr`. Contém apenas os dados de pixel, otimizados para acesso aleatório.

### Corpo do Arquivo (Pós-Descriptografia)

| Campo             | Tipo     | Descrição                                                                                             |
|-------------------|----------|-------------------------------------------------------------------------------------------------------|
| **Header Interno**|          |                                                                                                       |
| `CompressionType` | `byte`   | Enum: 0=Nenhum, 1=LZ4, 2=Zstd.                                                                        |
| `SpriteCount`     | `uint`   | Contagem total de sprites **neste arquivo**.                                                          |
| **Lookup Table**  |          |                                                                                                       |
| `SpriteAddresses` | `uint[]` | Array com os offsets de cada sprite no arquivo. O endereço 0 indica um sprite vazio.                |
| **Data Blocks**   | `byte[]` | Concatenação dos blocos de dados de cada sprite.                                                      |

### Estrutura de um Bloco de Sprite

- **Com Compressão (LZ4/Zstd):**
  - `int CompressedSize`: Tamanho do bloco comprimido.
  - `byte[] CompressedData`: Dados comprimidos.
- **Sem Compressão:**
  - `byte[] PixelData`: Pixels RGBA brutos (32x32x4 = 4096 bytes).

### Divisão em Múltiplos Arquivos (`.asset0`, `.asset1`, ...)

- O sistema divide automaticamente os assets em múltiplos arquivos se o número de sprites exceder 1.000.000.
- O arquivo `.meta` contém a tabela `Asset Mapping` que informa qual intervalo de IDs está em qual arquivo.
- Exemplo:
  - `project.asset0`: Sprites 1 a 1.000.000
  - `project.asset1`: Sprites 1.000.001 a 2.000.000
