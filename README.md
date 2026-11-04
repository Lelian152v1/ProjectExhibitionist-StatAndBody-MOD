# PETweaks 1.7.0 + 地图种子查看器

> 一个包解决两件事：
> 1. **PETweaks 插件** —— 运行时调整游戏数值（速度、掉落、磨损、体力、身体物理、服装槽、HUD 等）
> 2. **地图种子查看器** —— 输入游戏里的种子，在浏览器里复现整张程序化城市地图（含真实地块贴图）

---

## 目录

- [包内容](#包内容)
- [安装](#安装)
- [插件功能与默认值](#插件功能与默认值)
- [地图查看器](#地图查看器)
- [地图生成算法（完整）](#地图生成算法完整)
- [游戏内默认数据](#游戏内默认数据)
- [常见问题](#常见问题)
- [版本记录](#版本记录)
- [许可](#许可)

---

## 包内容

```
PETweaks-1.7.0/
├─ winhttp.dll                ← BepInEx 5.4.23.6 加载器
├─ doorstop_config.ini
├─ .doorstop_version
├─ BepInEx/
│  ├─ core/                   ← BepInEx 运行库
│  ├─ plugins/PETweaks.dll    ← 插件本体
│  └─ config/com.dsh.petweaks.cfg
├─ map-viewer/                ← 地图种子查看器（可单独拷走）
│  ├─ index.html
│  └─ assets/
│     ├─ tiles/               ← 45 张游戏真实地块贴图
│     └─ map_ui.png           ← 游戏地图 UI 背景（可选）
├─ source/                    ← 插件源码
└─ README.md
```

---

## 安装

### 插件

把 `winhttp.dll`、`doorstop_config.ini`、`.doorstop_version`、`BepInEx/` 一起放到游戏根目录（与 `Project Exhibitionist.exe` 同级）。

进游戏玩一会儿退出，`BepInEx/LogOutput.log` 里出现这一行即成功：

```
PETweaks v1.7.0 loaded (persistent runner attached)
```

**卸载**：删掉上面四项即可，数值立刻恢复原版（存档里已生成的物品不会回滚）。

### 地图查看器

直接双击 `map-viewer/index.html`（Chrome / Edge 均可，离线可用）。
注意不要单独移动 `index.html`——它靠同级的 `assets/tiles/` 读贴图。

---

## 插件功能与默认值

配置文件：`BepInEx/config/com.dsh.petweaks.cfg`，改完**重启游戏**生效。

### `[1. General]`

| 键 | 默认 | 说明 |
|---|---|---|
| `Enabled` | true | 总开关；false 时所有功能（含 Harmony 补丁）都不生效 |

### `[2. Speed]`

| 键 | 原版值 | 默认倍率 |
|---|---|---|
| `WalkMultiplier` | 2 | 1.5 |
| `RunningMultiplier` | 4 | 1.5 |
| `SprintMultiplier` | 6 | 1.5 |
| `CrouchMultiplier` | 2 | 1 |

### `[3. Loot / Rarity]`

| 键 | 原版值 | 默认 |
|---|---|---|
| `Override` | — | true |
| `RollWeightBase` | 80 | 60 |
| `RollWeightPlusOne` | 17 | 30 |
| `RollWeightPlusTwo` | 3 | 10 |

概率 = 自身 / 三者之和。

### `[4. Loot / Items]`

| 键 | 默认 | 说明 |
|---|---|---|
| `WeightMultiplier` | 1 | 物品池权重等比缩放 |

### `[7. Wear]`

| 键 | 原版值 | 默认 |
|---|---|---|
| `ClothingWearMultiplier` | `wearDuration` = 60 秒 | 1 |
| `ItemDurabilityMultiplier` | `maxDurability` = 100 | 1 |
| `UnlimitedWear` | — | false |

### `[8. Trash]`

| 键 | 原版值 | 默认 |
|---|---|---|
| `MinItems` | 1 | 1 |
| `MaxItems` | 2 | 2 |

### `[9. Stamina]`

| 键 | 原版值 | 默认倍率 |
|---|---|---|
| `MaxStaminaMultiplier` | 200 | 1 |
| `RecoveryMultiplier` | 1.2/秒 | 1 |
| `SprintCostMultiplier` | 30 | 1 |
| `JumpCostMultiplier` | 30 | 1 |
| `RollCostMultiplier` | 25 | 1 |
| `IgnoreConsumption` | — | false |

### `[10. Body Physics]`

游戏自带弹簧：

| 键 | 默认 | 说明 |
|---|---|---|
| `BreastJiggleMultiplier` | 1 | 乘 `kick` / `walkKick`（不含上限） |
| `ButtJiggleMultiplier` | 1 | 乘 `kick` / `kickBack` / `squash` |
| `DampingMultiplier` | 1 | 阻尼倍率 |
| `FrequencyMultiplier` | 1 | 弹簧频率倍率 |
| `SwayMultiplier` | 1 | 左右横向响应 |
| `AsymmetryMultiplier` | 1 | 左右不同步 |
| `CounterKickMultiplier` | 1 | 反向回弹 |
| `KickRandomMultiplier` | 1 | 抖动随机性 |
| `GainMultiplier` | 1 | 位移→旋转增益（最直接的幅度旋钮） |
| `MaxAngleDegrees` | 0（=原版 32°） | 上下俯仰上限；横向是它的 0.5 倍，**没有独立横向上限** |
| `StepBounce` | true | false 去掉走路脚步弹动 |

插件自己加的横向惯性（Harmony 补丁）：

| 键 | 默认 | 说明 |
|---|---|---|
| `JiggleMode` | Spring | Spring = 物理弹簧 / Simple = 旧版角度映射 |
| `JiggleStiffness` | 40 | 刚度（20~120） |
| `JiggleDamping` | 8 | 阻尼（4~20） |
| `YawOffsetMeters` | 0.02 | 横向最大位移（**过大网格会被拉长**） |
| `JiggleRotationScale` | 0.5 | 位移→倾斜比例 |
| `JiggleArmMeters` | 0.12 | 力臂 |
| `YawLagMaxDegrees` | 60 | 惯性倾斜角上限（建议 5~8） |
| `YawLag` | 0 | Simple 模式的惯性强度 |
| `SideRandomize` | true | 左右随机活跃侧 |
| `SideRandomInterval` | 0.5 | 切换间隔（秒） |
| `InertiaKick` | 0 | 旧版转向冲量，保留兼容 |

### `[11. Inventory]`

| 键 | 默认 | 说明 |
|---|---|---|
| `ExtraClothSlots` | 0 | 服装槽额外数量（原版 6）；建议 ≤4 |

实现：`maxClothSlots` + 补 4 个并行列表 + 克隆 HUD 格子 + 克隆面板格子（第二行容器）。

### `[12. HUD]`

| 键 | 默认 | 说明 |
|---|---|---|
| `FaceCamOffsetY` | 0 | 角色头像窗口上移像素 |
| `FaceCamScale` | 1 | 窗口缩放 |
| `FaceCamDistance` | 1 | 相机退远倍率 |
| `FaceCamFovScale` | 1 | 视野倍率 |
| `FaceCamAspect` | 0 | 宽高比（0 = 方形，0.78 = 竖长） |

### `[13. QoL]`

| 键 | 默认 | 说明 |
|---|---|---|
| `PanelClothClick` | Double | I 面板点服装：Off / Double / Single |

### `[14. Map]`

| 键 | 默认 | 说明 |
|---|---|---|
| `ShowMapSeed` | true | 左上角显示基础信息：种子 / 所在格 / 街区 / 区块 / 旋转 / 坐标与距离 |
| `DumpMapData` | false | 进图导出区块/街区/地块权重到日志（调试用） |
| `MapSyncEnabled` | true | 开启地图实时同步（只监听 127.0.0.1 的临时动态端口） |
| `MapSyncAutoOpen` | true | 进图后自动用默认浏览器打开查看器 |
| `MapSyncIntervalMs` | 500 | 同步轮询间隔（毫秒） |
| `MapSyncViewerDir` | 空 | 查看器目录；留空 = 自动查找 `PETweaks/map-viewer`、`plugins/map-viewer`、`<游戏根>/map-viewer` |

**地图实时同步**（默认开）：

- 插件在 `127.0.0.1` 上开一个**系统自动分配端口**的只限本机服务，把游戏**实际生成**的区块（区块名 / 旋转 / 每个槽位的地块）提供给查看器；
- **不需要管理员、不写注册表、不改防火墙**（只绑回环地址，外部不可达）；
- 进图后自动打开 `http://127.0.0.1:<端口>/`，查看器每 0.5 秒增量拉取，**边走边点亮**；
- 连上后**自动把种子填进输入框并应用**；
- 传回角色当前格，查看器**自动跟随并居中**（工具栏「跟随角色」可关）；
- 数据源优先取**实际生成的区块**（`spawnedZones`），能反映运行时的 POI 替换，而不是仅取计划；实测数据在查看器里用**青色描边**标出，区块旋转取游戏真实值，与"确定性推算"的区域一眼可分；
- 关掉浏览器后刷新标签页即可重连；日志里会打印当前端口。

左上角显示内容（默认开启）：

```
SEED -1483479323
CELL (6, 3)  [Residential]
MonoChunkData - 1x1 1x1 rot180°
POS (386.2, 151.8)  HUD 420 m  (原点 415 m)
Z: City10
```

---

## 地图查看器

打开 `map-viewer/index.html`。

### 功能

- **种子输入**：填入游戏里的 `SEED`，点「应用」
- **坐标跳转**：输入 `x, y`（格坐标）或「世界坐标」模式（米），点「跳转」
- **出生点标记**：`(0,0)` 绿圈；**滑出视野时，屏幕边缘出现绿色箭头 + 距离标签**（`出生点 1.2 km`）
- **悬停详情**：格坐标、世界坐标、街区、区块 + 尺寸 + 旋转、每个槽位的地块名
- **缩放**：10 ~ **350 px/格**（滚轮以鼠标为中心）
- **图例 / 概率面板**：街区权重、8 种 POI 占比与平均间距
- **只看 POI**、**地铁 2 格间距圈**、**街区底色**、**圆形小地图框**
- **Classic 限制**（红色开关，默认关）：**仅 0~4 级**排位/普通模式需要开启——按 `RankPoiPolicy` 禁用 泳池/商场/地铁/公园 并移除地铁区块；Rank 05 或管理员**不要开**（限制已解除）
- **模拟干旱 POI**（默认关）：按节奏标记游戏可能强插的便利店/泳池/美术馆，用橙色虚线框示意——**仅演示，不代表真实位置**（见工具栏开关旁说明）
- **模式说明横幅**：页面顶部固定提示"本视图 = Free 模式确定性布局"
- **实时同步**：插件自动打开 `http://127.0.0.1:<端口>/` 时，查看器每 0.5 秒增量拉取游戏**实际生成结果**，用青色描边标出；`file://` 打开时可用「加载实际结果」手动导入 JSON

### 坐标约定

```
逻辑坐标 = 引擎坐标 + WorldOrigin.Offset
格 = round(逻辑坐标 / 60)
世界坐标（米）= 格 × 60
```

游戏内 Y 轴向上，所以查看器里 `(x, y)` 的街区画在屏幕 `(x, -y)` 方向（上下与游戏一致）。

---

## 地图生成算法（完整）

> 以下全部来自反编译 `Assembly-CSharp.dll` 并在游戏内实测验证（7×7 共 49 格，区块/旋转/地块 **49/49 一致**）。

### 0. 先搞清楚：地图不是纯函数

游戏的地图生成分**两层**：

1. **确定性基础布局**（本文档描述的、查看器复现的）——由 `(seed, 格坐标)` 唯一决定；
2. **有状态的 POI 补偿系统**（`PoiTravelDroughtScheduler` + `ResolveDroughtRelief`）——**取决于你走过的路径和最近看过哪些 POI**，任何种子都算不出来。

补偿系统做的事：

- 玩家走路累计 **120 米**没碰到任何 POI → 进入"待补充"；**420 米**是硬上限；
- 满足时，在你**前进方向前方**的合格格子里**强制塞入**便利店 / 泳池 / 美术馆；
- 每种 POI 还有"近期见过就降权"的倍率（最近见过的 ×0.15、第二近 ×0.35、第三近 ×0.65）；
- 强制插入的特殊建筑会登记 **±2 格的间距区**，于是它附近原本确定性布局里的特殊建筑会被**降级成普通街区**（反过来也一样，谁先被生成谁占住）。

#### 影响"查看器 vs 游戏"的完整因素清单（反编译核对）

| # | 因素 | 是否影响确定性布局 | 说明 |
|---|---|---|---|
| 1 | **运行模式** | ✅ 决定是否启用补偿 | `Free 且非地图浏览` → `HasSeededFreeWorld=true` → **纯确定性，查看器完全一致**；`Classic/Guided/LongWayHome/地图浏览` → 补偿系统生效 |
| 2 | **POI 干旱/近期补偿** | ✅（仅非 Free） | 强插 Kombini/Pool/ArtGallery + 按近期见过的类型降权重抽 + 前进走廊过滤 + 大型 POI 冷却（2000 米） |
| 3 | **RankPoiPolicy 限制** | ✅（仅 Classic/Guided 且非管理员） | rank 不是 `hornyslut_declared` 且非管理员时，**禁止 Pool/Mall/Subway/Park**，并移除地铁区块 |
| 4 | **travelOffset** | ✅ | 解析用 `cell + travelOffset`；平时 `(0,0)`，远距离世界原点重设后会变，且会清缓存 |
| 5 | **地图光照质量（目录）** | ❌ 已核对 | `Balanced/Original/Performance` 6 个 MapCatalog 的**区块权重与地铁标志完全一致**，只影响渲染，不影响布局 |

因此：

> **查看器 = "自由模式（Free，非地图浏览）下的确定性基础布局"**。
> 在 Classic / 地图浏览模式里，POI 位置会随行走方向、近期所见 POI、以及等级限制而漂移或消失——这不是查看器的 bug。
> 对照时请用 **Free 模式**，并确认 `travel=(0,0)`（插件日志里可看）。

### 1. 哈希

```js
n = seed
n ^= imul(x, -1640531535)
n = rotl(n, 13); n = imul(n, 1540483477)
n ^= imul(y, -2048144777)
n = rotl(n, 17); n = imul(n, 2654435761)
n ^= n >>> 16; n = imul(n, 73244475)
return n ^ (n >>> 16)

advance(h):  h ^= h >>> 16; h = imul(h, 73244475); h ^= h >>> 16
hashToFloat(h) = (h & 0xFFFFFF) / 16777216
```

同一 `(seed, x, y)` 永远同一结果。

### 2. 街区（Biome）—— Voronoi 抖动网格

```
districtSize = 3
if x² + y² <= 4: return Residential          // 出生点 2 格内固定

d  = 3
di = floorDiv(x, d); dj = floorDiv(y, d)
for i,j in [-1..1]:
    nx = di + i; ny = dj + j
    h  = hash(seed ^ 0x9E3779B9, nx, ny)
    cx = (nx + hashToFloat(h)) * d
    h  = advance(h)
    cy = (ny + hashToFloat(h)) * d
    h  = advance(h)
    if (cx-x)² + (cy-y)² < best:
        best = ...
        chosen = weightedPickBiome(hashToFloat(h))
return chosen
```

街区权重：`Residential 40 / Suburban 35 / City 25`。

### 3. 区块（Chunk）

```
h0 = hash(seed, x, y)
h1 = advance(h0)                 // 组内抽签 + 旋转都用 h1
rotation = (h1 % 4) * 90°

small = 所有 1×1 区块（含地铁）
large = 所有 >1×1 区块
W_S = Σ small.weight（210.56）  W_L = Σ large.weight（1.68）

if hashToFloat(h0) * (W_S + W_L) < W_S:
    chunk = weightedPick(small, hashToFloat(h1))
else:
    chunk = weightedPick(large, hashToFloat(h1))
```

> ⚠️ 三个容易写错的点：地铁属于 **small 组**；组内用 **advance(h)** 且用**组内总权重**；旋转用 **advance(h) % 4**。

### 4. 地铁间距

地铁区块之间至少隔 **1 格**（±1 邻域，含对角）：

```
if 本格是地铁:
    p1 = SubwayPriority(x, y) = hash(seed ^ 0x6D2B79F5, x, y)
    对 i,j ∈ [-1..1]（跳过 0,0）:
        nx = x+i, ny = y+j
        若 SelectBaseChunkForSpacing(nx, ny) 是地铁:      // 邻居的原始区块
            p2 = SubwayPriority(nx, ny)
            若 p2 > p1 或 (p2 == p1 且 ComesBefore(邻居, 本格)):
                本格 = SelectNonSubwayReplacement(本格, Advance(selectionHash ^ 0xA511E9B3))
                // 从同尺寸的非地铁区块里按权重抽一个
```

### 5. 地块（Zone）

```
h0 = hash(seed, x, y); h1 = advance(h0)
槽位 i 使用 advance^(i+2)(h0)

每个槽位六步回退（occupancy 始终是硬条件）：
1. 区块池 ∩ 街区池，未用过
2. 区块池 ∩ 回退街区池，未用过
3. 区块池 ∩ 街区池，忽略已用
4. 区块池 ∩ 回退街区池，忽略已用
5. 区块池，未用过
6. 区块池
```

抽到 `Kombini` 时还有门禁：

```
if (advance(h ^ 0xC2B2AE35) % 3) != 0:   // 2/3 概率被拒
    以「排除特殊地块」重走上面六步
```

### 6. 特殊建筑间距（POI）

算出区块和地块后，还要做一次"特殊建筑间距"裁决（`EnforceSpecialSpacing`）：

```
if 本格不含特殊地块（且区块不是"永远特殊"）: 保持原样
else:
    if SpecialCandidateWins(本格, 原始结果): 保持原样
    else: 降级
```

`SpecialCandidateWins` 用 `SpecialPriority(cell) = hash(seed ^ -1255572915, x, y)`，
扫描一个**不对称窗口** `i,j ∈ [-(maxLarge-1+2) .. (本格尺寸-1+2)]`（1×1 时为 `[-5..2]`），
对每个窗口内、足迹间距 ≤2、且本身含特殊的邻居做优先级比较。

**降级规则（`BuildNonSpecialReplacement`）**：

- 区块**不是**"永远特殊"（如 Mono 1x1）→ 同一区块、同一哈希，**排除特殊地块重抽**；
- 区块**是**"永远特殊"（含地铁的区块、3×3、4×4）→ 用 `h2 = hash(seed ^ -776648139, x, y)` 从小区块里抽一个替换，再排除特殊地块重抽。

> "永远特殊"的判定：`containsSubway == true`，或该区块池里**所有**地块都是特殊地块。
> 所以 **Mono 3x3 / Mono 4x4 落败时会整个变成一个 1×1 普通区块**。

### 7. 放置

按「到原点距离」由近到远扫描，已被覆盖的格子跳过；大区块会占满自己的足迹，因此不会重叠。

### 8. 槽位位置（重要）

槽位坐标**不是** `gridPosition`（生成器留空），而是区块 prefab 里 `Base*` 子物体的**层级顺序**：

```
X 区块：槽0 (+2.5,+2.5) 右-上   槽1 (-2.5,-2.5) 左-下
        槽2 (-2.5,+2.5) 左-上   槽3 (+2.5,-2.5) 右-下
T 区块：槽0 左半(2×1)            槽1 (+2.5,+2.5) 右-上   槽2 (+2.5,-2.5) 右-下
```

---

## 游戏内默认数据

### 区块

| 区块 | 权重 | 尺寸 | 槽位 |
|---|---|---|---|
| Mono 1x1 | 10 | 1×1 | Full ×1 |
| X 1x1 | 100 | 1×1 | Quarter ×4 |
| T 1x1 | 100 | 1×1 | Half + Quarter ×2 |
| TChunkWithSubway | 0.56 | 1×1 | Half + Quarter ×2 |
| Mono 3x3 | 1.47 | 3×3 | Full ×1 |
| Mono 4x4 | 0.21 | 4×4 | Full ×1 |

**街区倍率**：`City` 把 `Mono 1x1` 权重 **×20**（10 → 200）。

### 区块自带地块池（顺序即游戏序列化顺序）

**Mono 1x1（Full）**

| 地块 | 权重 |
|---|---|
| Suburban_Kombini | 11 |
| City11, City8 | 各 15 |
| City1 ~ City7 | 各 15 |
| City9, City10 | 各 15 |
| Art Gallery | 8.5 |
| Pool | 10 |

**X / T / Subway（Quarter 池，13 个）**

| 地块 | 权重 |
|---|---|
| Residential_1 | 20 |
| Suburban_5 | 25 |
| Suburban_4 | 50 |
| Residential_2 ~ Residential_6 | 各 50 |
| Suburban_2, Suburban_3 | 各 50 |
| Suburban_Sushi_1 | 15 |
| Suburban_6, Suburban_7 | 各 50 |

T / Subway 额外有 Half 池：`Residential_2(Half) 100`、`Residential_3(Half) 100`（Subway 区块换成 `Subway 30`）。

**Mono 3x3（Full）**：`Park 0.21`、`University 0.42`、`Mall 0.84`
**Mono 4x4（Full）**：`Stadium 0.21`

### 街区

| 街区 | 权重 | 回退 |
|---|---|---|
| Residential | 40 | Suburban |
| Suburban | 35 | Residential |
| City | 25 | Suburban |

### 特殊目标点（POI）

由「区块自带地块池」里的地块名判定：

| POI | 判定关键字 | 所在区块 |
|---|---|---|
| 便利店 Kombini | `Kombini` | Mono 1x1 |
| 美术馆 ArtGallery | `Art Gallery` | Mono 1x1 |
| 泳池 Pool | `Pool` | Mono 1x1 |
| 地铁 Subway | `Subway` | TChunkWithSubway |
| 公园 Park | `PARK - 3x3` / `Park` | Mono 3x3 |
| 大学 University | `University` | Mono 3x3 |
| 商场 Mall | `Mall` | Mono 3x3 |
| 体育场 Stadium | `Stadium` | Mono 4x4 |

排除项：名字含 `CityCross`、`SuburbanMisc` 的地块不会出现。

### 实测概率

| 区块 | 占比 |
|---|---|
| X 1x1 | 47.1% |
| T 1x1 | 47.1% |
| Mono 1x1 | 4.7% |
| Mono 3x3 | 0.69%（占面积约 6.7%） |
| Mono 4x4 | 0.10% |
| 地铁 | 0.26% |

| POI | 占比 | 平均间距（1 格 = 60 米） |
|---|---|---|
| 商场 | 0.40% | ~15 km |
| 便利店 | 0.27% | ~23 km |
| 地铁 | 0.26% | ~23 km |
| 泳池 | 0.24% | ~25 km |
| 美术馆 | 0.21% | ~29 km |
| 大学 | 0.20% | ~30 km |
| 公园 | 0.10% | ~61 km |
| 体育场 | 0.10% | ~61 km |

---

## 常见问题

**Q：地图和游戏里不一样？**
分两种情况：

- **区块 / 旋转 / 普通地块不一致** → 先检查三件事：种子一致、查看器用了同目录的 `assets/tiles`、没有改错种子。这部分已用 49 格逐格校验。
- **POI（游泳馆/美术馆/便利店/地铁等）位置不一样** → 正常现象。查看器显示的是确定性基础布局；游戏会在你走 120 米没碰到 POI 时**强制插入**便利店/泳池/美术馆，并把它附近 ≤2 格内的其它特殊建筑降级。这跟行走路径有关，无法靠种子复现。

**Q：找不到地铁？**
占比 0.26%，平均 23 km（约 380 格）。可以用查看器输入种子后搜索「地铁」标记再前往。

**Q：插件改了没反应？**
配置只在启动时读取，改完要重启游戏；`Enabled=false` 时所有功能（含补丁）都不生效。

**Q：服装槽多了排版乱？**
`ExtraClothSlots` 建议 ≤4；过多时第二行容器可能重叠，改回 0 重启即可恢复。

**Q：胸部网格被拉长？**
降 `YawOffsetMeters`（0.02 → 0.015）与 `JiggleRotationScale`（0.12 → 0.06），不要靠加 `MaxAngleDegrees` 补。

---

## 版本记录

| 版本 | 内容 |
|---|---|
| 1.0.0 | 速度 / 稀有度 / 物品池 |
| 1.2.0 | 磨损、耐久、垃圾桶 |
| 1.4.0 | 体力、身体物理 |
| 1.5.0 | 横向惯性补丁、左右随机、弹簧模式 |
| 1.6.0 | 服装槽扩容、头像窗口调整、面板点击穿脱 |
| 1.6.1 | 修复 `Enabled=false` 未关闭补丁的问题 |
| **1.7.0** | 清理游戏内调试输出（保留基础信息叠加层 + `Z:` 地块行）；内置地图种子查看器与真实地块贴图；**新增地图实时同步**（回环动态端口 + 增量 JSON + 自动开浏览器，零配置/零系统改动）；新增 **Classic 限制**开关与干旱模拟；同步数据源改为实际生成的区块；文档补齐生成算法、默认值、特殊建筑间距、POI 干旱补偿、等级限制与同步协议 |

---

## 许可

| 组件 | 许可 |
|---|---|
| BepInEx 5.4.23.6 | LGPL-2.1 |
| PETweaks | 附源码（`source/`） |
| 地图贴图 | 来自游戏资源，仅用于本地查看器 |

---

*文档版本 2026-11-04 · 对应 PETweaks 1.7.0*
