# Project Exhibitionist Tweaks (PETweaks) v1.5.0

> 适用于 **Project Exhibitionist V1.00**（Unity 2022 / Mono / 64 位）的运行时数值调整插件。
> 内置 **BepInEx 5.4.23.6 (x64)**，解压即用，无需额外安装。
> 此文档由大肥鱼deepseek生成

---

## 目录

- [这是什么](#这是什么)
- [快速开始](#快速开始)
- [功能一览](#功能一览)
- [配置详解](#配置详解)
- [预设（推荐收藏）](#预设推荐收藏)
- [手感调试指南](#手感调试指南)
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
| 其它 | 强制语言 |

---

## 快速开始

### 1. 安装

关闭游戏 → 把压缩包**解压到游戏根目录**（与 `Project Exhibitionist.exe` 同级）→ 启动游戏。

解压后结构：

```
Project Exhibitionist/
├─ Project Exhibitionist.exe
├─ winhttp.dll              ← 新增（BepInEx 加载器）
├─ doorstop_config.ini      ← 新增
├─ .doorstop_version        ← 新增
└─ BepInEx/                 ← 新增
   ├─ core/
   ├─ plugins/PETweaks.dll
   └─ config/com.dsh.petweaks.cfg
```

### 2. 验证

进游戏玩一会儿，退出，打开 `BepInEx/LogOutput.log`，看到这两行即成功：

```text
PETweaks v1.5.0 loaded (persistent runner attached)
[tick] speed=2 rarity=1 lootTables=0 wear=63 durability=87 trash=3 stamina=1 body=14
```

### 3. 调参

编辑 `BepInEx/config/com.dsh.petweaks.cfg` → **重启游戏**生效。

> ⚠️ BepInEx 在运行中/退出时可能重写该文件，注释会被替换成插件内置说明（也是中文）。
> **预设请以本文档为准**，配置文件里的注释只作参考。

---

## 功能一览

| 分类 | 键 | 默认 | 说明 |
|---|---|---|---|
| 总开关 | `Enabled` | `true` | false = 全部不生效 |
| 速度 | `WalkMultiplier` | 1.5 | 原版 2 |
| | `RunningMultiplier` | 1.5 | 原版 4 |
| | `SprintMultiplier` | 1.5 | 原版 6 |
| | `CrouchMultiplier` | 1 | 原版 2 |
| 稀有度 | `Override` | true | false = 保留原版 80/17/3 |
| | `RollWeightBase` | 60 | 基础权重，原版 80 |
| | `RollWeightPlusOne` | 30 | 升一档，原版 17 |
| | `RollWeightPlusTwo` | 10 | 升两档，原版 3 |
| 物品池 | `WeightMultiplier` | 1 | 等比缩放，不改相对比例 |
| 磨损 | `ClothingWearMultiplier` | 1 | × `wearDuration`（原版 60 秒） |
| | `ItemDurabilityMultiplier` | 1 | × `maxDurability`（原版 100） |
| | `UnlimitedWear` | false | true = 服装永不磨损 |
| 垃圾桶 | `MinItems` / `MaxItems` | 1 / 2 | 每桶件数 |
| 体力 | `MaxStaminaMultiplier` | 1 | 原版 200 |
| | `RecoveryMultiplier` | 1 | 原版 1.2/秒 |
| | `SprintCostMultiplier` | 1 | 原版 30 |
| | `JumpCostMultiplier` | 1 | 原版 30 |
| | `RollCostMultiplier` | 1 | 原版 25 |
| | `IgnoreConsumption` | false | true = 完全不耗体力 |
| 语言 | `ForceLocale` | `zh-Hans` | 留空 = 不改 |

---

## 配置详解

### `[10. Body Physics]` 身体物理

#### 游戏自带弹簧（Anime 风格）

| 键 | 默认 | 作用 | 调大 / 调小 |
|---|---|---|---|
| `BreastJiggleMultiplier` | 1 | 胸部冲量倍率（× `kick` / `walkKick` 等力项，不含上限） | 越大越晃；2~3 已很明显 |
| `ButtJiggleMultiplier` | 1 | 臀部冲量倍率（× `kick` / `kickBack` / `squash`） | 同上 |
| `DampingMultiplier` | 1 | 阻尼倍率 | 小 = 晃得久；大 = 更快停 |
| `FrequencyMultiplier` | 1 | 弹簧频率倍率 | 小 = 更慢更软；大 = 更弹 |
| `SwayMultiplier` | 1 | 左右横向响应倍率 | 想让动作更"活"就调高 |
| `AsymmetryMultiplier` | 1 | 左右不同步程度 | 高 = 两侧不再镜像 |
| `CounterKickMultiplier` | 1 | 反向回弹强度 | 高 = 落地后来回摆更明显 |
| `KickRandomMultiplier` | 1 | 每次抖动的随机性 | 高 = 动作不重复 |
| `GainMultiplier` | 1 | **游戏"位移→旋转"增益**，最直接的幅度旋钮 | 可小于 1，用来整体降幅 |
| `MaxAngleDegrees` | 0 | 胸部【上下/俯仰】摆动上限；0 = 原版 32° | — |
| `StepBounce` | true | false = 去掉走路时"一顿一顿弹" | — |

> ⚠️ **游戏里没有单独的"横向角度上限"变量**：代码写死为 `maxAngle × 0.5`。
> `MaxAngleDegrees` 同时影响上下（±X）和左右（±X/2）。

#### 本插件的横向惯性（Harmony 补丁）

| 键 | 默认 | 作用 |
|---|---|---|
| `JiggleMode` | `Spring` | `Spring` = 物理弹簧；`Simple` = 旧版角度映射 |
| `JiggleStiffness` | 40 | 刚度 k（20~120），越大越紧、回得越快 |
| `JiggleDamping` | 8 | 阻尼 c（4~20），越大越快停 |
| `YawOffsetMeters` | 0.02 | **横向最大位移（米）★最容易造成网格拉长** |
| `JiggleRotationScale` | 0.5 | 位移→倾斜角比例；0 = 只平移不倾斜 |
| `JiggleArmMeters` | 0.12 | 力臂长度，位移换算倾斜角用 |
| `YawLagMaxDegrees` | 60 | 惯性倾斜角上限，建议 5~8 |
| `YawLag` | 0 | `Simple` 模式下的惯性强度（`Spring` 模式不用） |

#### 其它

| 键 | 默认 | 作用 |
|---|---|---|
| `SideRandomize` | true | 定期翻转 `asymmetry` 符号，随机决定哪侧更活跃 |
| `SideRandomInterval` | 0.5 | 左右切换间隔（秒），0.2 = 很随性 |
| `InertiaKick` | 0 | 旧版转向冲量，保留兼容，0 = 关 |

---

## 预设（推荐收藏）

> 配置文件可能被程序重写，**请以本节为准**。
> 只需填写的键，未列出的保持默认即可。

### 幅度档

| 档位 | 键值 |
|---|---|
| **偏软（推荐日常）** | `BreastJiggleMultiplier=2.2` `ButtJiggleMultiplier=1.6`<br>`GainMultiplier=0.7` `SwayMultiplier=1.2`<br>`YawOffsetMeters=0.02` `JiggleRotationScale=0.12`<br>`MaxAngleDegrees=45` |
| **更小** | `GainMultiplier=0.55` `YawOffsetMeters=0.015` `JiggleRotationScale=0.06` |
| **更大（会开始拉长网格）** | `GainMultiplier=1.0` `YawOffsetMeters=0.035`<br>`JiggleRotationScale=0.3` `MaxAngleDegrees=60` |

### 回复档（只影响收敛速度，不影响幅度）

| 档位 | 键值 | 手感 |
|---|---|---|
| **正常** | `JiggleStiffness=55` `JiggleDamping=11` | 收得快，跑停后几乎无余摆 |
| **偏软** | `JiggleStiffness=35` `JiggleDamping=6.5` | 跑停后还有 2~3 次余摆 |
| **更软** | `JiggleStiffness=25` `JiggleDamping=4` | 明显晃动、过冲更大 |

### 其它常用

| 目标 | 键值 |
|---|---|
| 横向更明显 | `SwayMultiplier=1.4~1.6`，`JiggleRotationScale=0.2` |
| 左右更随机 | `SideRandomInterval=0.2` |
| 完全关闭甩动 | `JiggleStiffness=0`（补丁自动跳过） |

---

## 手感调试指南

| 现象 | 原因 | 处理 |
|---|---|---|
| **运动时胸部被拉长 / 拉丝** | 位移把骨骼推离锚点，蒙皮被拽过去 | 先降 `YawOffsetMeters`（0.02 → 0.015），再降 `JiggleRotationScale`（0.12 → 0.06）；**不要**靠加 `MaxAngleDegrees` 弥补 |
| **跑停后没有余摆** | 阻尼/刚度过大 | 降到「回复档 偏软」 |
| **晃得太夸张、撞角度上限** | 冲量过大 | 降 `BreastJiggleMultiplier` 与 `GainMultiplier`，而不是只降 `MaxAngleDegrees` |
| **走路一顿一顿地弹** | 脚步冲量 | `StepBounce=false` |
| **只有乳头那块在动、根部不动** | 模型只有两根兄弟骨，且游戏用位移而非旋转驱动根部 | 见「实现原理」，属于模型限制 |

---

## 日志说明

`BepInEx/LogOutput.log` 每 5 秒输出一行：

```text
[tick] speed=2 rarity=1 lootTables=0 wear=63 durability=87 trash=3 stamina=1 body=14
```

| 字段 | 含义 |
|---|---|
| `speed` | 找到的玩家控制器数量（菜单里为 0，进场景后 1~2） |
| `rarity` | 稀有度设置对象（通常 1） |
| `lootTables` | 物品池数量（`WeightMultiplier=1` 时跳过，显示 0 正常） |
| `wear` | 被改写的服装数量（倍率=1 且未开 `UnlimitedWear` 时为 0） |
| `durability` | 被改写的道具数量（倍率=1 时为 0） |
| `trash` | 垃圾桶档位配置数（通常 3） |
| `stamina` | 被改写的角色数量（全 1 且未开 `IgnoreConsumption` 时为 0） |
| `body` | 物理参数组数（胸/臀 × 各参数，最多 14） |

出现 `EXCEPTION` 说明某一项反射失败，其它项不受影响。

---

## 实现原理

- **加载**：BepInEx 5 通过 `winhttp.dll` 注入，插件挂在常驻 GameObject 上（`DontDestroyOnLoad`）；
- **数值**：全部通过反射写入运行时对象（`ScriptableObject` / 组件），不修改任何 `.assets` 资源；
- **惯性甩动**：Harmony 后置补丁挂在
  `AnimeBodyPhysics.SwingBreast(Breast, Vector3, float, BreastSettings)`，
  在游戏算完 `TargetPosition` / `TargetRotation` 之后叠加横向位移与倾斜，
  **由游戏自己应用到骨骼** —— 所以衣物、MagicaCloth、`baseFollow` 都能同步，不会撕裂；
- **缓存**：每个对象只记录一次原始值，重复写入不会累乘。

### 为什么"根部"很难动

游戏里每侧胸只有 **两根兄弟骨**：

```text
c_breast_01.l  ← 父 c_spine_02_bend.x，无子骨
c_breast_02.l  ← 父 c_spine_02_bend.x，子骨 c_breast_02.l_end
```

- 旋转它们只会带动远端（乳头区域）；
- 整块胸的"整体晃动"是靠**骨骼位移**（`Spring.SetLocalPositionAndRotation`）实现的；
- 靠近胸口的顶点权重偏向脊椎，因此位移过大会被拉长——这是模型限制。

---

## 已知限制

- 仅支持 64 位 Mono 版（本游戏即是）；
- 游戏大版本更新后，字段/方法名变化会让对应功能失效（日志记 `EXCEPTION`，游戏本身不受影响）；
- 横向甩动受两根胸骨限制，位移过大会拉长网格；
- 与 ReShade（`dxgi.dll` 代理）共存无冲突，已实测；
- 不联网、不写文件、不读注册表、不改存档。

---

## 卸载

删除以下四项即可完全还原：

```text
winhttp.dll
doorstop_config.ini
.doorstop_version
BepInEx/
```

卸载后数值回到原版，但**已经生成在存档里的物品/耐久不会回滚**。

---

## 版本记录

| 版本 | 内容 |
|---|---|
| v1.0.0 | 速度 / 稀有度 / 物品池 |
| v1.2.0 | 服装磨损、道具耐久、垃圾桶数量 |
| v1.4.0 | 体力；身体物理（幅度、阻尼、频率、sway、不对称、回弹、随机） |
| v1.5.0 | 横向惯性甩动（Harmony 补丁）、左右随机、步态弹跳开关、弹簧模式（Spring/Simple）、完整中文注释<br>修复：物理参数判定遗漏 `MaxAngle`/`StepBounce`；弹簧模式首帧未初始化导致的一次性爆冲 |

---

## 许可

| 组件 | 许可 |
|---|---|
| BepInEx 5.4.23.6 | LGPL-2.1 · <https://github.com/BepInEx/BepInEx> |
| PETweaks | 本包内附源码（`source/`），可用 `dotnet build -c Release` 自行编译核对 |

---

*文档版本：2026-11-02 · 对应 PETweaks v1.5.0*
