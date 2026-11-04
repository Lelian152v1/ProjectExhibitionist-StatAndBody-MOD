# PETweaks 1.7.0 + 地图种子查看器

适用于 Project Exhibitionist v1.0.0（Unity 2022 / Mono / 64 位）的运行时调整插件。  
🔴注意：测试时游戏版本为1.0.0，不保证其他游戏版本能正常运行

> 一个包解决两件事：
> 1. **PETweaks 插件** —— 运行时调整游戏数值（速度、掉落、磨损、体力、身体物理、服装槽、HUD 等），**只改内存**，不写资源、不动存档、不联网；
> 2. **地图种子查看器** —— 输入游戏里的种子，在浏览器里复现整张程序化城市地图（含真实地块贴图），并可与游戏实时同步。

---

## 目录

- [包内容](#包内容)
- [安装](#安装)
- [插件功能与默认值](#插件功能与默认值)
- [预设（推荐收藏）](#预设推荐收藏)
- [手感与界面调优](#手感与界面调优)
- [地图查看器](#地图查看器)
- [地图生成算法（完整）](#地图生成算法完整)
- [游戏内默认数据](#游戏内默认数据)
- [常见问题](#常见问题)
- [日志说明](#日志说明)
- [实现原理](#实现原理)
- [已知限制](#已知限制)
- [卸载](#卸载)
- [版本记录](#版本记录)
- [许可](#许可)

---

## 这是什么

一个 BepInEx 插件，在游戏运行时修改内存里的数值。**只改内存**：不写游戏资源、不动存档结构、不联网。

删掉插件（或 `Enabled = false`）立即恢复原版。

| 模块 | 内容 |
|---|---|
| 移动 | 走 / 跑 / 冲刺 / 蹲 速度倍率 |
| 掉落 | 稀有度权重、物品池权重、垃圾桶出货数量 |
| 装备 | 服装磨损、道具耐久 |
| 体力 | 上限、恢复速度、冲刺/跳跃/翻滚消耗 |
| 身体物理 | 胸/臀弹簧参数、左右随机、步态弹跳开关 |
| 惯性 | 转身时胸部的横向甩动（Harmony 补丁，走游戏自己的应用通道） |
| 服装槽 | **新增**：扩容服装槽（数据 + HUD + 面板 UI 一起扩） |
| 界面 | **新增**：角色头像窗口移位 / 缩放 / 退远 / 加宽 / 改宽高比 |
| 操作 | **新增**：I 键面板里双击服装格直接聚焦/穿脱 |

Mod版胸部物理与原版对比：  
<img width="384" height="360" alt="MOD效果 00_00_00-00_00_06" src="https://github.com/user-attachments/assets/5d443858-71e7-44f4-bbe0-9b280b626886" /><img width="376" height="360" alt="原版 00_00_00-00_00_30" src="https://github.com/user-attachments/assets/40180ab3-39f3-4762-bd2e-9cfed8def1fa" />

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

调参：编辑 `BepInEx/config/com.dsh.petweaks.cfg` → **重启游戏**生效。

> ⚠️ BepInEx 在运行中/退出时可能重写该文件，注释会被替换成插件内置说明。**预设请以本文档为准。**

### 地图查看器

直接双击 `map-viewer/index.html`（Chrome / Edge 均可，离线可用）。
注意不要单独移动 `index.html`——它靠同级的 `assets/tiles/` 读贴图。

**卸载**：删掉插件四项即可，数值立刻恢复原版（存档里已生成的物品不会回滚）。

---

## 插件功能与默认值

配置文件：`BepInEx/config/com.dsh.petweaks.cfg`，改完**重启游戏**生效。

### `[1. General]`总开关

| 键 | 默认 | 说明 |
|---|---|---|
| `Enabled` | true | 总开关；false 时所有功能（含 Harmony 补丁）都不生效 |

### `[2. Speed]`速度

| 键 | 原版值 | 默认倍率 |
|---|---|---|
| `WalkMultiplier` | 2 | 1.5 |
| `RunningMultiplier` | 4 | 1.5 |
| `SprintMultiplier` | 6 | 1.5 |
| `CrouchMultiplier` | 2 | 1 |

### `[3. Loot / Rarity]`各稀有度出率比

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
| `WeightMultiplier` | 1 | 物品池权重等比缩放，不改相对比例 |

### `[7. Wear]`服装/道具磨损

| 键 | 原版值 | 默认 |
|---|---|---|
| `ClothingWearMultiplier` | `wearDuration` = 60 秒 | 1 |
| `ItemDurabilityMultiplier` | `maxDurability` = 100 | 1 |
| `UnlimitedWear` | — | false |

### `[8. Trash]`单个垃圾桶产出物品数

| 键 | 原版值 | 默认 |
|---|---|---|
| `MinItems` | 1 | 1 |
| `MaxItems` | 2 | 2 |

### `[9. Stamina]`体力消耗

| 键 | 原版值 | 默认倍率 |
|---|---|---|
| `MaxStaminaMultiplier` | 200 | 1 |
| `RecoveryMultiplier` | 1.2/秒 | 1 |
| `SprintCostMultiplier` | 30 | 1 |
| `JumpCostMultiplier` | 30 | 1 |
| `RollCostMultiplier` | 25 | 1 |
| `IgnoreConsumption` | — | false |

### `[10. Body Physics]`身体物理（胸和臀）

游戏自带弹簧（替换Anime 风格）：

| 键 | 默认 | 说明 |
|---|---|---|
| `BreastJiggleMultiplier` | 1 | 胸部冲量倍率（× `kick` / `walkKick`），越大越晃，2~3 已明显 |
| `ButtJiggleMultiplier` | 1 | 臀部冲量倍率（× `kick` / `kickBack` / `squash`） |
| `DampingMultiplier` | 1 | 阻尼倍率：小 = 晃得久，大 = 更快停 |
| `FrequencyMultiplier` | 1 | 弹簧频率倍率：小 = 更慢更软，大 = 更弹 |
| `SwayMultiplier` | 1 | 左右横向响应，想更"活"就调高 |
| `AsymmetryMultiplier` | 1 | 左右不同步程度，高 = 两侧不再镜像 |
| `CounterKickMultiplier` | 1 | 反向回弹强度，高 = 落地后摆更明显 |
| `KickRandomMultiplier` | 1 | 每次抖动的随机性，高 = 动作不重复 |
| `GainMultiplier` | 1 | 位移→旋转增益（**最直接的幅度旋钮**，可 <1） |
| `MaxAngleDegrees` | 0（=原版 32°） | 胸【上下/俯仰】上限；横向是它的 0.5 倍，**没有独立横向上限** |
| `StepBounce` | true | false = 去掉走路一顿一顿的弹动 |

> ⚠️ 游戏里没有单独的"横向角度上限"变量：代码写死 `maxAngle × 0.5`，`MaxAngleDegrees` 同时决定上下和左右。

插件自己加的横向惯性（Harmony 补丁）：

| 键 | 默认 | 说明 |
|---|---|---|
| `JiggleMode` | Spring | Spring = 物理弹簧（线加速度+离心力驱动，推荐）；Simple = 旧版角度映射。 |
| `JiggleStiffness` | 40 | 刚度 k（20~120），越大越紧 |
| `JiggleDamping` | 8 | 阻尼 c（4~20），越大越快停 |
| `YawOffsetMeters` | 0.02 | **横向最大位移（米）★最易造成网格拉长** |
| `JiggleRotationScale` | 0.5 | 位移→倾斜角比例；0 = 只平移不倾斜 |
| `JiggleArmMeters` | 0.12 | 力臂长度 |
| `YawLagMaxDegrees` | 60 | 惯性倾斜角上限，建议 5~8 |
| `YawLag` | 0 | Simple 模式下的惯性强度 |
| `SideRandomize` | true | 定期翻转 `asymmetry` 符号，随机决定哪侧更活跃 |
| `SideRandomInterval` | 0.5 | 左右切换间隔（秒） |
| `InertiaKick` | 0 | 旧版转向冲量，保留兼容 |

### `[11. Inventory]` 服装槽扩容（实验性）

| 键 | 默认 | 说明 |
|---|---|---|
| `ExtraClothSlots` | 0 | 在原版 6 个基础上额外增加 |

实现（三层一起改）：`maxClothSlots` + 补并行列表 + 克隆 HUD 格子 + 克隆面板格子（第二行容器）。只影响运行时，不写存档、不改 prefab；出问题改回 0 重启即恢复。

### `[12. HUD]` 角色头像窗口

| 键 | 默认 | 说明 |
|---|---|---|
| `FaceCamOffsetY` | 0 | 窗口整体上移像素（正数=上移） |
| `FaceCamScale` | 1 | 窗口缩放，0.85 = 85% |
| `FaceCamDistance` | 1 | 相机退远倍率（1.45 = 能看到上半身） |
| `FaceCamFovScale` | 1 | 视野倍率（配退远用，1.25） |
| `FaceCamAspect` | 0 | 宽高比（宽/高）：0 = 方形，0.78 = 竖长 |

**原理**：`PlayerFaceCam` 渲染到 `800×800` 的 `CielFaceCam` 渲染纹理，窗口是显示该纹理的 `RawImage`。位移/缩放改窗口 `RectTransform`；取景改 `offset.z` 与 `fieldOfView`；竖长重建渲染纹理为 `800×(800/aspect)`。

### `[13. QoL]`双击聚焦指定服装

| 键 | 默认 | 说明 |
|---|---|---|
| `PanelClothClick` | Double | I 面板点击服装格：Off 只选中 / Double 双击 / Single 单击 |

**注意**：衣服是"可交互对象"，双击对非 `WearableItemSO` 服装走 `SelectCloth()`（聚焦）分支——所以双击是让 HUD 聚焦到该件服装，而非直接穿脱，这是原版设计。

### `[14. Map]`地图同步

| 键 | 默认 | 说明 |
|---|---|---|
| `ShowMapSeed` | true | 左上角显示基础信息：种子 / 所在格 / 街区 / 区块 / 旋转 / 坐标与距离 |
| `DumpMapData` | false | 进图导出区块/街区/地块权重到日志（调试用） |
| `MapSyncEnabled` | true | 开启地图实时同步（只监听 127.0.0.1 的临时动态端口） |
| `MapSyncAutoOpen` | true | 进图后自动用默认浏览器打开查看器 |
| `MapSyncIntervalMs` | 500 | 同步轮询间隔（毫秒） |
| `MapSyncViewerDir` | 空 | 查看器目录；留空 = 自动查找 `PETweaks/map-viewer`、`plugins/map-viewer`、`<游戏根>/map-viewer` |

**地图实时同步**（默认开）：

- 插件在 `127.0.0.1` 上开一个**系统自动分配端口**的只限本机服务，把游戏**实际生成**的区块提供给查看器；
- **不需要管理员、不写注册表、不改防火墙**（只绑回环地址，外部不可达）；
- 进图后自动打开 `http://127.0.0.1:<端口>/`，查看器每 0.5 秒增量拉取，**边走边点亮**；
- 连上后**自动把种子填进输入框并应用**；传回角色当前格，查看器**自动跟随并居中**（工具栏「跟随角色」可关）；
- 数据源优先取**实际生成的区块**（`spawnedZones`），能反映运行时的 POI 替换；实测数据用**青色描边**标出，区块旋转取游戏真实值；
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

## 胸部晃动预设

> 配置文件可能被程序重写，**请以本节为准**。

### 幅度档

| 档位 | 键值 |
|---|---|
| **偏软（日常）** | `BreastJiggleMultiplier=2.2` `ButtJiggleMultiplier=1.6` `GainMultiplier=0.7` `SwayMultiplier=1.2` `YawOffsetMeters=0.02` `JiggleRotationScale=0.12` `MaxAngleDegrees=45` |
| **更小** | `GainMultiplier=0.55` `YawOffsetMeters=0.015` `JiggleRotationScale=0.06` |
| **更大（会拉长网格）** | `GainMultiplier=1.0` `YawOffsetMeters=0.035` `JiggleRotationScale=0.3` `MaxAngleDegrees=60` |

### 回复档（只影响收敛速度）

| 档位 | 键值 | 手感 |
|---|---|---|
| **正常** | `JiggleStiffness=55` `JiggleDamping=11` | 收得快，跑停后几乎无余摆 |
| **偏软** | `JiggleStiffness=35` `JiggleDamping=6.5` | 跑停后还有 2~3 次余摆 |
| **更软** | `JiggleStiffness=25` `JiggleDamping=4` | 明显晃动、过冲更大 |

### 界面档

| 目标 | 键值 |
|---|---|
| 头像窗口竖长（能看上半身） | `FaceCamDistance=1.45` `FaceCamFovScale=1.25` `FaceCamAspect=0.78` `FaceCamScale=0.85` |
| 再远一点 | `FaceCamDistance=1.7` |
| 放大 | `FaceCamScale=1.0`、`FaceCamAspect=0.9` |
| 服装 +2 格 | `ExtraClothSlots=2` |

---

## 手感与界面调优

| 现象 | 原因 | 处理 |
|---|---|---|
| **运动时胸部被拉长** | 位移把骨骼推离锚点，蒙皮被拽过去 | 先降 `YawOffsetMeters`，再降 `JiggleRotationScale`；不要靠加 `MaxAngleDegrees` 弥补 |
| **跑停后没有余摆** | 阻尼/刚度过大 | 降到「回复档 偏软」 |
| **晃得太夸张、撞上限** | 冲量过大 | 降 `BreastJiggleMultiplier` 与 `GainMultiplier` |
| **走路一顿一顿弹** | 脚步冲量 | `StepBounce=false` |
| **只有乳头在动、根不动** | 模型只有两根兄弟骨，根部靠位移驱动 | 见「实现原理」，属模型限制 |
| **服装格超出屏幕** | 面板行宽固定 | 插件会自动放第二行；还是挤就减少 `ExtraClothSlots` |
| **头像窗口挡住 HUD** | 窗口默认在右下 | 调 `FaceCamOffsetY` 上移 |

---

## 地图查看器

打开 `map-viewer/index.html`。

### 功能

- **种子输入**：填入游戏里的 `SEED`，点「应用」
- **坐标跳转**：输入 `x, y`（格坐标）或「世界坐标」模式（米），点「跳转」
- **出生点标记**：`(0,0)` 绿圈；滑出视野时，屏幕边缘出现绿色箭头 + 距离标签
- **悬停详情**：格坐标、世界坐标、街区、区块 + 尺寸 + 旋转、每个槽位的地块名
- **缩放**：10 ~ 350 px/格（滚轮以鼠标为中心）
- **图例 / 概率面板**：街区权重、8 种 POI 占比与平均间距
- **只看 POI**、**地铁 2 格间距圈**、**街区底色**、**圆形小地图框**
- **Classic 限制**（红色开关，默认关）：仅 0~4 级排位/普通模式需要开启——按 `RankPoiPolicy` 禁用 泳池/商场/地铁/公园 并移除地铁区块；Rank 05 或管理员不要开
- **模拟干旱 POI**（默认关）：按节奏标记游戏可能强插的便利店/泳池/美术馆，用橙色虚线框示意（仅演示）
- **模式说明横幅**：页面顶部固定提示"本视图 = Free 模式确定性布局"
- **实时同步**：插件打开时自动拉取游戏实际生成结果，青色描边标出；`file://` 打开时可用「加载实际结果」手动导入 JSON

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

### 0. 地图不是纯函数

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
| 5 | **地图光照质量（目录）** | ❌ 已核对 | 6 个 MapCatalog 的**区块权重与地铁标志完全一致**，只影响渲染，不影响布局 |

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
        若 SelectBaseChunkForSpacing(nx, ny) 是地铁:
            p2 = SubwayPriority(nx, ny)
            若 p2 > p1 或 (p2 == p1 且 ComesBefore(邻居, 本格)):
                本格 = SelectNonSubwayReplacement(本格, Advance(selectionHash ^ 0xA511E9B3))
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

```
if 本格不含特殊地块（且区块不是"永远特殊"）: 保持原样
else:
    if SpecialCandidateWins(本格, 原始结果): 保持原样
    else: 降级
```

`SpecialCandidateWins` 用 `SpecialPriority(cell) = hash(seed ^ -1255572915, x, y)`，扫描一个**不对称窗口** `i,j ∈ [-(maxLarge-1+2) .. (本格尺寸-1+2)]`（1×1 时为 `[-5..2]`）。

**降级规则（`BuildNonSpecialReplacement`）**：

- 区块**不是**"永远特殊"（如 Mono 1x1）→ 同一区块、同一哈希，**排除特殊地块重抽**；
- 区块**是**"永远特殊"（含地铁的区块、3×3、4×4）→ 用 `h2 = hash(seed ^ -776648139, x, y)` 从小区块里抽一个替换，再排除特殊地块重抽。

> "永远特殊"的判定：`containsSubway == true`，或该区块池里**所有**地块都是特殊地块。所以 **Mono 3x3 / Mono 4x4 落败时会整个变成一个 1×1 普通区块**。

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
- **POI（游泳馆/美术馆/便利店/地铁等）位置不一样** → 正常现象。查看器显示的是确定性基础布局；游戏会在你走 120 米没碰到 POI 时强制插入便利店/泳池/美术馆，并把它附近 ≤2 格内的其它特殊建筑降级。这跟行走路径有关，无法靠种子复现。

**Q：找不到地铁？**
占比 0.26%，平均 23 km（约 380 格）。可以用查看器输入种子后搜索「地铁」标记再前往。

**Q：插件改了没反应？**
配置只在启动时读取，改完要重启游戏；`Enabled=false` 时所有功能（含补丁）都不生效。

**Q：服装槽多了排版乱？**
`ExtraClothSlots` 建议 ≤4；过多时第二行容器可能重叠，改回 0 重启即可恢复。

**Q：胸部网格被拉长？**
降 `YawOffsetMeters`（0.02 → 0.015）与 `JiggleRotationScale`（0.12 → 0.06），不要靠加 `MaxAngleDegrees` 补。

---

## 日志说明

`BepInEx/LogOutput.log` 每 5 秒输出：

```text
[tick] speed=2 rarity=1 lootTables=0 wear=63 durability=88 trash=3 stamina=1 body=14
```

| 字段 | 含义 |
|---|---|
| `speed` | 玩家控制器数量（菜单 0，进场景 1~2） |
| `rarity` | 稀有度设置对象（通常 1） |
| `lootTables` | 物品池数量（`WeightMultiplier=1` 时为 0 正常） |
| `wear` / `durability` | 被改写的服装 / 道具数量 |
| `trash` | 垃圾桶档位数（通常 3） |
| `stamina` | 被改写的角色数 |
| `body` | 物理参数组数（最多 14） |

功能性日志：

```text
clothslots: expanded 6 -> 8 (ui 8, step ...)
facecam: offset.z x1.45 -> -1.160
clothclick installed (mode=Double)
mapinfo: seed=... cellSize=60 travel=(0,0) catalog=MapCatalog_PerformanceOriginal
mapinfo: mode=Classic rank=... admin=... restrictions=...
mapsync: http://127.0.0.1:端口/
```

出现 `EXCEPTION` 说明某一项反射失败，其它项不受影响。

---

## 实现原理

- **加载**：BepInEx 5 通过 `winhttp.dll` 注入，插件挂在常驻 GameObject 上；
- **数值**：全部通过反射写入运行时对象，不修改 `.assets`；
- **惯性甩动**：Harmony 后置补丁挂在 `AnimeBodyPhysics.SwingBreast(...)`，在游戏算完后叠加，由游戏自己应用，衣物/布料全部同步；
- **服装槽**：数据列表补齐 + 克隆 HUD 格子 + 克隆面板格子（第二行容器）；
- **头像窗口**：按渲染纹理 `CielFaceCam` 反查 `RawImage`，改 `RectTransform` 与相机参数；
- **点击穿脱**：Harmony 补丁在 `InventoryDialogController.OnSlotClicked` 之后调用 `UseSelectedSlot()`；
- **地图同步**：从 `ChunkSpawnResolver` / `ChunkManager` 读真实结果，走回环 TcpListener 提供给浏览器。

### 为什么"根部"很难动

每侧胸只有两根兄弟骨：

```text
c_breast_01.l  ← 父 c_spine_02_bend.x，无子骨
c_breast_02.l  ← 父 c_spine_02_bend.x，子骨 c_breast_02.l_end
```

- 旋转只带动远端（乳头区域）；
- 整块胸的"整体晃动"是靠**骨骼位移**实现；
- 靠近胸口的顶点权重偏向脊椎，位移过大会被拉长——模型限制。

---

## 已知限制

- 仅支持 64 位 Mono 版；
- 游戏大版本更新后字段/方法名变化会让对应功能失效（日志记 `EXCEPTION`，游戏不受影响）；
- **服装槽扩容是实验性功能**：依赖 prefab 结构，UI 布局是手工摆放的，增加过多可能排版不佳；
- 横向甩动受两根胸骨限制，位移过大会拉长网格；
- 头像窗口的取景依赖 `PlayerFaceCam` 与 `CielFaceCam` 渲染纹理，若游戏改结构会失效；
- **地图查看器复现的是确定性基础布局**，POI 位置随行走路径漂移属正常现象（见"地图不是纯函数"）；
- **地图同步**依赖回环端口，极少数杀软可能对"游戏进程监听端口"误报（只绑 127.0.0.1，风险很低）；
- 与 ReShade（`dxgi.dll` 代理）共存无冲突，已实测；
- 不联网、不写存档、不读注册表（仅地图同步会在 127.0.0.1 回环监听）。

---

## 卸载

删除以下四项即可完全还原：

```text
winhttp.dll
doorstop_config.ini
.doorstop_version
BepInEx/
```

卸载后数值回到原版，**已生成在存档里的物品/耐久不会回滚**。地图查看器（`map-viewer/`）是独立网页，可单独保留。

---

## 版本记录

| 版本 | 内容 |
|---|---|
| 1.0.0 | 速度 / 稀有度 / 物品池 |
| 1.2.0 | 服装磨损、道具耐久、垃圾桶数量 |
| 1.4.0 | 体力；身体物理（幅度、阻尼、频率、sway、不对称、回弹、随机） |
| 1.5.0 | 横向惯性甩动（Harmony）、左右随机、步态弹跳开关、弹簧模式（Spring/Simple）、完整中文注释 |
| 1.6.0 | 服装槽扩容（数据 + HUD + 面板第二行）<br />头像窗口移位/缩放/退远/加宽/宽高比、I 面板双击聚焦/穿脱；<br />修复物理参数判定遗漏与弹簧首帧爆冲 |
| 1.6.1 | 修复 `Enabled=false` 未关闭补丁的问题 |
| **1.7.0** | 清理游戏内调试输出（保留基础信息叠加层 + `Z:` 地块行）；<br />内置地图种子查看器与真实地块贴图；<br />**新增地图实时同步**（回环动态端口 + 增量 JSON + 自动开浏览器，零配置/零系统改动）； |

---

## 许可

| 组件 | 许可 |
|---|---|
| BepInEx 5.4.23.6 | LGPL-2.1 · <https://github.com/BepInEx/BepInEx> |
| PETweaks | 本包内附源码（`source/`），可用 `dotnet build -c Release` 自行编译核对 |
| 地图贴图 | 来自游戏资源，仅用于本地查看器 |

---

*文档版本 2026-11-04 · 对应 PETweaks 1.7.0*
