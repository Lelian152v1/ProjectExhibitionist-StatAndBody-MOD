# Project Exhibitionist Tweaks (PETweaks) v1.6.0

适用于 **Project Exhibitionist v1.0.0**（Unity 2022 / Mono / 64 位）的运行时调整插件。  
🔴<font color="red">注意：测试时游戏版本为1.0.0，不保证其他游戏版本正常运行</font>
> 内置 **BepInEx 5.4.23.6 (x64)**，解压即用，无需额外安装。  
> 此文档为大肥鱼deepseek所写

---

## 目录

- [这是什么](#这是什么)
- [快速开始](#快速开始)
- [功能一览](#功能一览)
- [配置详解](#配置详解)
- [预设（推荐收藏）](#预设推荐收藏)
- [手感与界面调优](#手感与界面调优)
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
| 服装槽 | **新增**：扩容服装槽（数据 + HUD + 面板 UI 一起扩） |
| 界面 | **新增**：角色头像窗口移位 / 缩放 / 退远 / 加宽 / 改宽高比 |
| 操作 | **新增**：I 键面板里双击服装格直接聚焦/穿脱 |

---

## 快速开始

### 1. 安装

关闭游戏 → 把压缩包**解压到游戏根目录**（与 `Project Exhibitionist.exe` 同级）→ 启动游戏。

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

进游戏玩一会儿，退出，打开 `BepInEx/LogOutput.log`，看到这行即成功：

```text
PETweaks v1.6.0 loaded (persistent runner attached)
```

### 3. 调参

编辑 `BepInEx/config/com.dsh.petweaks.cfg` → **重启游戏**生效。

> ⚠️ BepInEx 在运行中/退出时可能重写该文件，注释会被替换成插件内置说明。
> **预设请以本文档为准。**

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
| 服装槽 | `ExtraClothSlots` | 0 | 额外服装槽（原版 6），实验性 |
| 头像窗口 | `FaceCamOffsetY` | 0 | 上移像素 |
| | `FaceCamScale` | 1 | 窗口缩放 |
| | `FaceCamDistance` | 1 | 退远倍率 |
| | `FaceCamFovScale` | 1 | 视野倍率 |
| | `FaceCamAspect` | 0 | 宽高比，0 = 方形 |
| 操作 | `PanelClothClick` | `Double` | `Off` / `Double` / `Single` |

---

## 配置详解

### `[10. Body Physics]` 身体物理

#### 游戏自带弹簧（Anime 风格）

| 键 | 默认 | 作用 | 调大 / 调小 |
|---|---|---|---|
| `BreastJiggleMultiplier` | 1 | 胸部冲量倍率（× `kick` / `walkKick`） | 越大越晃；2~3 已明显 |
| `ButtJiggleMultiplier` | 1 | 臀部冲量倍率 | 同上 |
| `DampingMultiplier` | 1 | 阻尼倍率 | 小 = 晃得久；大 = 更快停 |
| `FrequencyMultiplier` | 1 | 弹簧频率倍率 | 小 = 更慢更软；大 = 更弹 |
| `SwayMultiplier` | 1 | 左右横向响应倍率 | 想让动作更"活"就调高 |
| `AsymmetryMultiplier` | 1 | 左右不同步程度 | 高 = 两侧不再镜像 |
| `CounterKickMultiplier` | 1 | 反向回弹强度 | 高 = 落地后摆更明显 |
| `KickRandomMultiplier` | 1 | 每次抖动的随机性 | 高 = 动作不重复 |
| `GainMultiplier` | 1 | 游戏"位移→旋转"增益 | **最直接的幅度旋钮**，可 <1 |
| `MaxAngleDegrees` | 0 | 胸【上下/俯仰】上限；0 = 原版 32° | — |
| `StepBounce` | true | false = 去掉走路一顿一顿弹 | — |

> ⚠️ 游戏里**没有单独的"横向角度上限"变量**：代码写死 `maxAngle × 0.5`。
> `MaxAngleDegrees` 同时决定上下（±X）和左右（±X/2）。

#### 本插件的横向惯性（Harmony 补丁）

| 键 | 默认 | 作用 |
|---|---|---|
| `JiggleMode` | `Spring` | `Spring` = 物理弹簧；`Simple` = 旧版角度映射 |
| `JiggleStiffness` | 40 | 刚度 k（20~120），越大越紧 |
| `JiggleDamping` | 8 | 阻尼 c（4~20），越大越快停 |
| `YawOffsetMeters` | 0.02 | **横向最大位移（米）★最易造成网格拉长** |
| `JiggleRotationScale` | 0.5 | 位移→倾斜角比例；0 = 只平移 |
| `JiggleArmMeters` | 0.12 | 力臂长度 |
| `YawLagMaxDegrees` | 60 | 惯性倾斜角上限，建议 5~8 |
| `YawLag` | 0 | `Simple` 模式下的惯性强度 |

#### 其它

| 键 | 默认 | 作用 |
|---|---|---|
| `SideRandomize` | true | 定期翻转 `asymmetry` 符号，随机决定哪侧更活跃 |
| `SideRandomInterval` | 0.5 | 左右切换间隔（秒） |
| `InertiaKick` | 0 | 旧版转向冲量，保留兼容 |

### `[11. Inventory]` 服装槽扩容（实验性）

| 键 | 默认 | 作用 |
|---|---|---|
| `ExtraClothSlots` | 0 | 在原版 6 个基础上额外增加；建议 ≤4，过多会影响排版 |

**实现方式**（三层一起改）：

1. **数据**：`PlayerInventoryManager.maxClothSlots` +6→+N，并给 `clothSlots` / `clothDurability` / `clothRarities` / `clothVariants` 四个并行列表补元素；
2. **HUD**：`ClothHUDController._slots` 是 prefab 写死的 6 个，插件克隆 `ClothSlotUI` 补齐，并按原间距往后排；
3. **面板**：`InventoryDialogController.clothSlots` 同样是 prefab 写死，插件克隆 `InventoryPanelSlotUI` 并塞进**新建的第二行容器** `ClothRow_auto`（原行下面），避免超出屏幕。

> 只影响运行时，不写存档、不改 prefab。
> 出问题就把 `ExtraClothSlots` 改回 0 重启，完全恢复。

### `[12. HUD]` 角色头像窗口

| 键 | 默认 | 作用 |
|---|---|---|
| `FaceCamOffsetY` | 0 | 窗口整体上移像素（正数=上移） |
| `FaceCamScale` | 1 | 窗口缩放，0.85 = 85% |
| `FaceCamDistance` | 1 | 相机退远倍率（1.45 = 能看到上半身） |
| `FaceCamFovScale` | 1 | 视野倍率（配退远用，1.25） |
| `FaceCamAspect` | 0 | 宽高比（宽/高）：0 = 方形，0.78 = 竖长 |

**原理**：`PlayerFaceCam` 相机渲染到 `800×800` 的 `CielFaceCam` 渲染纹理，窗口是一个显示该纹理的 `RawImage`。

- 位移/缩放：改窗口 `RectTransform`；
- 取景：改 `PlayerFaceCam.offset.z`（距离）与 `camera.fieldOfView`；
- 竖长：重建渲染纹理为 `800×(800/aspect)`，重新赋给相机和 `RawImage`，窗口同步拉长。

### `[13. QoL]` 操作便利

| 键 | 默认 | 作用 |
|---|---|---|
| `PanelClothClick` | `Double` | I 面板点击服装格：`Off` 只选中 / `Double` 双击 / `Single` 单击 |

**注意**：游戏里衣服是"可交互对象"，`UseSelectedSlot()` 对非 `WearableItemSO` 的服装走的是 `SelectCloth()`（聚焦）分支，所以**双击的效果是让 HUD 聚焦到该件服装**，而不是直接穿脱 —— 这是原版设计，不是插件限制。

---

## 预设（推荐收藏）

> 配置文件可能被程序重写，**请以本节为准**。

### 幅度档

| 档位 | 键值 |
|---|---|
| **偏软（日常）** | `BreastJiggleMultiplier=2.2` `ButtJiggleMultiplier=1.6`<br>`GainMultiplier=0.7` `SwayMultiplier=1.2`<br>`YawOffsetMeters=0.02` `JiggleRotationScale=0.12`<br>`MaxAngleDegrees=45` |
| **更小** | `GainMultiplier=0.55` `YawOffsetMeters=0.015` `JiggleRotationScale=0.06` |
| **更大（会拉长网格）** | `GainMultiplier=1.0` `YawOffsetMeters=0.035`<br>`JiggleRotationScale=0.3` `MaxAngleDegrees=60` |

### 回复档（只影响收敛速度）

| 档位 | 键值 | 手感 |
|---|---|---|
| **正常** | `JiggleStiffness=55` `JiggleDamping=11` | 收得快，跑停后几乎无余摆 |
| **偏软** | `JiggleStiffness=35` `JiggleDamping=6.5` | 跑停后还有 2~3 次余摆 |
| **更软** | `JiggleStiffness=25` `JiggleDamping=4` | 明显晃动、过冲更大 |

### 界面档

| 目标 | 键值 |
|---|---|
| 头像窗口竖长（能看上半身） | `FaceCamDistance=1.45` `FaceCamFovScale=1.25`<br>`FaceCamAspect=0.78` `FaceCamScale=0.85` |
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
clothslots panel: expanded to 8 (step ...)
facecam ui: [0] ... [1] ...        ← 窗口层级与尺寸
facecam: offset.z x1.45 -> -1.160
facecam: rt 800x800 -> 800x1025 (aspect 0.78)
clothclick installed (mode=Double)
```

出现 `EXCEPTION` 说明某一项反射失败，其它项不受影响。

---

## 实现原理

- **加载**：BepInEx 5 通过 `winhttp.dll` 注入，插件挂在常驻 GameObject 上；
- **数值**：全部通过反射写入运行时对象，不修改 `.assets`；
- **惯性甩动**：Harmony 后置补丁挂在 `AnimeBodyPhysics.SwingBreast(...)`，在游戏算完 `TargetPosition` / `TargetRotation` 后叠加，**由游戏自己应用**，衣物/布料全部同步；
- **服装槽**：数据列表补齐 + 克隆 HUD 格子 + 克隆面板格子（第二行容器）；
- **头像窗口**：按渲染纹理 `CielFaceCam` 反查 `RawImage`，改 `RectTransform` 与相机参数；
- **点击穿脱**：Harmony 补丁在 `InventoryDialogController.OnSlotClicked` 之后调用 `UseSelectedSlot()`。

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

卸载后数值回到原版，**已生成在存档里的物品/耐久不会回滚**。

---

## 版本记录

| 版本 | 内容 |
|---|---|
| v1.0.0 | 速度 / 稀有度 / 物品池 |
| v1.2.0 | 服装磨损、道具耐久、垃圾桶数量 |
| v1.4.0 | 体力；身体物理（幅度、阻尼、频率、sway、不对称、回弹、随机） |
| v1.5.0 | 横向惯性甩动（Harmony）、左右随机、步态弹跳开关、弹簧模式（Spring/Simple）、完整中文注释 |
| **v1.6.0** | **服装槽扩容（数据 + HUD + 面板第二行）、头像窗口移位/缩放/退远/加宽/宽高比、I 面板双击聚焦/穿脱**<br>修复：物理参数判定遗漏 `MaxAngle`/`StepBounce`；弹簧首帧未初始化导致的一次性爆冲 |

---

## 许可

| 组件 | 许可 |
|---|---|
| BepInEx 5.4.23.6 | LGPL-2.1 · <https://github.com/BepInEx/BepInEx> |
| PETweaks | 本包内附源码（`source/`），可用 `dotnet build -c Release` 自行编译核对 |

---

*文档版本：2026-11-02 · 对应 PETweaks v1.6.0*
