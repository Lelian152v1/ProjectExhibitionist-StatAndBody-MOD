using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace PETweaks
{
    [BepInPlugin("com.dsh.petweaks", "Project Exhibitionist Tweaks", "1.7.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ConfigEntry<bool>   CfgEnabled;
        internal static ConfigEntry<float>  WalkMul, RunMul, SprintMul, CrouchMul;
        internal static ConfigEntry<bool>   RarityOverride;
        internal static ConfigEntry<float>  RarityBase, RarityPlusOne, RarityPlusTwo;
        internal static ConfigEntry<float>  LootWeightMul;
        internal static ConfigEntry<bool>   Verbose;
        internal static ConfigEntry<string> ForceLocale;
        internal static ConfigEntry<float>  ClothingWearMul;
        internal static ConfigEntry<float>  ItemDurabilityMul;
        internal static ConfigEntry<bool>   UnlimitedWear;
        internal static ConfigEntry<int>    TrashMinItems;
        internal static ConfigEntry<int>    TrashMaxItems;
        internal static ConfigEntry<float>  MaxStaminaMul;
        internal static ConfigEntry<float>  StaminaRecoveryMul;
        internal static ConfigEntry<float>  SprintCostMul;
        internal static ConfigEntry<float>  JumpCostMul;
        internal static ConfigEntry<float>  RollCostMul;
        internal static ConfigEntry<bool>   IgnoreStaminaConsumption;
        internal static ConfigEntry<float>  BreastJiggleMul;
        internal static ConfigEntry<float>  ButtJiggleMul;
        internal static ConfigEntry<float>  BodyDampingMul;
        internal static ConfigEntry<float>  BodyFrequencyMul;
        internal static ConfigEntry<float>  BodySwayMul;
        internal static ConfigEntry<float>  BodyAsymmetryMul;
        internal static ConfigEntry<float>  BodyCounterKickMul;
        internal static ConfigEntry<float>  BodyGainMul;
        internal static ConfigEntry<float>  InertiaKick;
        internal static ConfigEntry<float>  MaxAngleDegrees;
        internal static ConfigEntry<bool>   StepBounce;
        internal static ConfigEntry<float>  YawLag;
        internal static ConfigEntry<float>  YawLagMaxDegrees;
        internal static ConfigEntry<float>  YawOffsetMeters;
        internal static ConfigEntry<string> JiggleMode;
        internal static ConfigEntry<float>  JiggleStiffness;
        internal static ConfigEntry<float>  JiggleDamping;
        internal static ConfigEntry<float>  JiggleArmMeters;
        internal static ConfigEntry<float>  JiggleRotationScale;
        internal static ConfigEntry<int>    ExtraClothSlots;
        internal static ConfigEntry<float>  FaceCamOffsetY;
        internal static ConfigEntry<float>  FaceCamScale;
        internal static ConfigEntry<float>  FaceCamDistance;
        internal static ConfigEntry<float>  FaceCamFovScale;
        internal static ConfigEntry<float>  FaceCamAspect;
        internal static ConfigEntry<string> PanelClothClick;
        internal static ConfigEntry<bool>   ShowMapSeed;
        internal static ConfigEntry<bool>   DumpMapData;
        internal static ConfigEntry<bool>   MapSyncEnabled;
        internal static ConfigEntry<bool>   MapSyncAutoOpen;
        internal static ConfigEntry<int>    MapSyncIntervalMs;
        internal static ConfigEntry<string> MapSyncViewerDir;
        internal static ConfigEntry<bool>   SideRandomize;
        internal static ConfigEntry<float>  SideRandomInterval;
        internal static ConfigEntry<float>  BodyKickRandomMul;
        internal static ManualLogSource     LogSource;

        private void Awake()
        {
            LogSource = Logger;

            CfgEnabled = Config.Bind("1. General", "Enabled", true,
                "总开关。false = 下面所有功能都不生效（插件仍加载）。");

            WalkMul   = Config.Bind("2. Speed", "WalkMultiplier",    1.5f, "\u8d70\u8def\u500d\u7387\uff08\u539f\u7248 walkSpeed = 2\uff09");
            RunMul    = Config.Bind("2. Speed", "RunningMultiplier", 1.5f, "\u8dd1\u6b65\u500d\u7387\uff08\u539f\u7248 runningSpeed = 4\uff09");
            SprintMul = Config.Bind("2. Speed", "SprintMultiplier",  1.5f, "\u51b2\u523a\u500d\u7387\uff08\u539f\u7248 sprintSpeed = 6\uff09");
            CrouchMul = Config.Bind("2. Speed", "CrouchMultiplier",  1f,   "\u8e72\u884c\u500d\u7387\uff08\u539f\u7248 crouchSpeed = 2\uff09");

            RarityOverride = Config.Bind("3. Loot / Rarity", "Override", true,
                "是否写下面三个稀有度权重；false = 保留游戏原值 80/17/3。");
            RarityBase    = Config.Bind("3. Loot / Rarity", "RollWeightBase",    60f, "\u57fa\u7840\u7a00\u6709\u5ea6\u6743\u91cd\uff08\u539f\u7248 80\uff09");
            RarityPlusOne = Config.Bind("3. Loot / Rarity", "RollWeightPlusOne", 30f, "\u5347\u4e00\u6863\u6743\u91cd\uff08\u539f\u7248 17\uff09\uff0c\u8c03\u9ad8\u66f4\u5bb9\u6613\u51fa\u9ad8\u7ea7\u8d27");
            RarityPlusTwo = Config.Bind("3. Loot / Rarity", "RollWeightPlusTwo", 10f, "\u5347\u4e24\u6863\u6743\u91cd\uff08\u539f\u7248 3\uff09\uff0c\u8c03\u9ad8\u66f4\u5bb9\u6613\u51fa\u4f20\u5947");

            LootWeightMul = Config.Bind("4. Loot / Items", "WeightMultiplier", 1f,
                "物品池所有条目权重的统一倍率。1=不改；只等比缩放，不改变各物品相对概率。");

            Verbose = Config.Bind("5. Advanced", "Verbose", true, "\u662f\u5426\u628a\u6bcf\u6b21\u5199\u5165\u7ed3\u679c\u6253\u5230\u65e5\u5fd7");

            ForceLocale = Config.Bind("6. Language", "ForceLocale", "zh-Hans",
                "强制语言，留空=不改。可用值 zh-Hans / en / ja 等。");

            ClothingWearMul = Config.Bind("7. Wear", "ClothingWearMultiplier", 1f,
                "服装耐久倍率（乘 WearableItemSO.wearDuration，原版 60 秒）。2 = 耐穿一倍。");
            ItemDurabilityMul = Config.Bind("7. Wear", "ItemDurabilityMultiplier", 1f,
                "道具耐久倍率（乘 InventoryItemSO.maxDurability，原版 100）。");
            UnlimitedWear = Config.Bind("7. Wear", "UnlimitedWear", false,
                "true = 所有服装不再磨损（直接关掉磨损标记，比倍率更彻底）。");

            TrashMinItems = Config.Bind("8. Trash", "MinItems", 1, "\u6bcf\u4e2a\u5783\u573e\u6876\u6700\u5c11\u51fa\u51e0\u4ef6\uff08\u539f\u7248 1\uff09");
            TrashMaxItems = Config.Bind("8. Trash", "MaxItems", 2, "\u6bcf\u4e2a\u5783\u573e\u6876\u6700\u591a\u51fa\u51e0\u4ef6\uff08\u539f\u7248 2\uff09");

            MaxStaminaMul = Config.Bind("9. Stamina", "MaxStaminaMultiplier", 1f,
                "体力上限倍率（原版 maxStamina=200）。");
            StaminaRecoveryMul = Config.Bind("9. Stamina", "RecoveryMultiplier", 1f,
                "体力恢复速度倍率（原版 1.2/秒）。调高 = 回得更快。");
            SprintCostMul = Config.Bind("9. Stamina", "SprintCostMultiplier", 1f,
                "冲刺消耗倍率（原版 sprintStamina=30）。调低 = 更省体力。");
            JumpCostMul = Config.Bind("9. Stamina", "JumpCostMultiplier", 1f, "\u8df3\u8dc3\u6d88\u8017\u500d\u7387\uff08\u539f\u7248 30\uff09");
            RollCostMul = Config.Bind("9. Stamina", "RollCostMultiplier", 1f, "\u7ffb\u6eda\u6d88\u8017\u500d\u7387\uff08\u539f\u7248 25\uff09");
            IgnoreStaminaConsumption = Config.Bind("9. Stamina", "IgnoreConsumption", false,
                "true = 冲刺/跳跃/翻滚完全不消耗体力。");

            BreastJiggleMul = Config.Bind("10. Body Physics", "BreastJiggleMultiplier", 1f,
                "胸部冲量倍率（乘 kick / walkKick 等力项，不乘上限）。2~3 已很明显；过大容易撞限位。");
            ButtJiggleMul = Config.Bind("10. Body Physics", "ButtJiggleMultiplier", 1f,
                "臀部冲量倍率（乘 kick / kickBack / squash）。");
            BodyDampingMul = Config.Bind("10. Body Physics", "DampingMultiplier", 1f,
                "阻尼倍率。调小 = 晃得久、更软；调大 = 更快停、更硬（0.5~2）。");
            BodyFrequencyMul = Config.Bind("10. Body Physics", "FrequencyMultiplier", 1f,
                "弹簧频率倍率。调小 = 更慢更软；调大 = 更弹更急（0.7~1.5）。");
            BodySwayMul = Config.Bind("10. Body Physics", "SwayMultiplier", 1f,
                "左右横向响应倍率（游戏把左右位移转成绕前轴的侧倾）。想更活泼就调高（1~3）。");
            BodyAsymmetryMul = Config.Bind("10. Body Physics", "AsymmetryMultiplier", 1f,
                "左右不同步程度。调高 = 两侧不再镜像（1~2.5）。");
            BodyCounterKickMul = Config.Bind("10. Body Physics", "CounterKickMultiplier", 1f,
                "反向回弹强度。调高 = 落地后来回摆更明显。");
            BodyKickRandomMul = Config.Bind("10. Body Physics", "KickRandomMultiplier", 1f,
                "每次抖动的随机性。调高 = 动作不重复（1.5~3）。");

            BodyGainMul = Config.Bind("10. Body Physics", "GainMultiplier", 1f,
                "弹簧响应增益（乘 gain）。比幅度更敏感，一般保持 1。");

            MaxAngleDegrees = Config.Bind("10. Body Physics", "MaxAngleDegrees", 0f,
                "【上下/俯仰】摆动角度上限（游戏自带 clamp，绕 root.right 轴）。0=原版 32°，60=放宽到 60°。");
            StepBounce = Config.Bind("10. Body Physics", "StepBounce", true,
                "走路时脚步带来的那一下弹动。false = 把 kick/walkKick 置 0，去掉一顿一顿的弹，保留其它晃动。");
            YawLag = Config.Bind("10. Body Physics", "YawLag", 0f,
                "【Simple 模式】转身惯性强度（0=关，1~2=开）。只在 JiggleMode=Simple 时生效。");
            SideRandomInterval = Config.Bind("10. Body Physics", "SideRandomInterval", 0.5f,
                "左右切换间隔（秒）。0.2 = 很随性；1~2 = 更稳定。");

            SideRandomize = Config.Bind("10. Body Physics", "SideRandomize", true,
                "左右随机：每隔 SideRandomInterval 随机翻转 asymmetry 符号，决定哪一侧更活跃。");

            YawOffsetMeters = Config.Bind("10. Body Physics", "YawOffsetMeters", 0.02f,
                "横向甩动的【最大位移】（米）。这是最容易造成拉长的参数：0.02~0.04 比较安全，0.1 会明显拉丝。");

            JiggleMode = Config.Bind("10. Body Physics", "JiggleMode", "Spring",
                "甩动算法：Spring = 物理弹簧（线加速度+离心力驱动，推荐）；Simple = 旧版角度映射。");
            JiggleStiffness = Config.Bind("10. Body Physics", "JiggleStiffness", 40f,
                "弹簧刚度 k。越大越紧、回得越快；越小越软、晃得越久（20~120）。");
            JiggleDamping = Config.Bind("10. Body Physics", "JiggleDamping", 8f,
                "阻尼 c。越大越快停下；越小越容易过冲、晃得久（4~20）。");
            JiggleArmMeters = Config.Bind("10. Body Physics", "JiggleArmMeters", 0.12f,
                "力臂长度（米）。位移换算成倾斜角用；越小，同样位移产生的倾斜越大、越容易拉长。");
            FaceCamDistance = Config.Bind("12. HUD", "FaceCamDistance", 1f,
                "面部摄像机离角色的距离倍率：1 = 原版，1.4 = 退远 40%（能看到更多身体）");
            FaceCamFovScale = Config.Bind("12. HUD", "FaceCamFovScale", 1f,
                "面部摄像机视野倍率：1 = 原版，1.3 = 视野更广（配合退远用）");
            FaceCamAspect = Config.Bind("12. HUD", "FaceCamAspect", 0f,
                "窗口宽高比（宽/高）：0 = 保持原版方形；0.75 = 竖长 3:4，能装下上半身");

            FaceCamScale = Config.Bind("12. HUD", "FaceCamScale", 1f,
                "面部摄像机窗口的缩放倍率，1 = 原始大小，0.8 = 缩小到 80%");

            ShowMapSeed = Config.Bind("14. Map", "ShowMapSeed", true,
                "在屏幕左上角显示当前地图种子、所在格、街区和区块（类似 MC 的 F3）");
            DumpMapData = Config.Bind("14. Map", "DumpMapData", false,
                "进图后把真实的区块/街区/地块权重导出到日志，供地图可视化使用");

            MapSyncEnabled = Config.Bind("14. Map", "MapSyncEnabled", true,
                "启用地图实时同步：在 127.0.0.1 上开一个只限本机的临时端口，把游戏实际生成的区块提供给查看器（不需要管理员，不改系统设置）。");
            MapSyncAutoOpen = Config.Bind("14. Map", "MapSyncAutoOpen", true,
                "进图后自动用默认浏览器打开查看器页面。");
            MapSyncIntervalMs = Config.Bind("14. Map", "MapSyncIntervalMs", 500,
                "同步轮询间隔（毫秒）。500 = 每 0.5 秒检查一次新提交的区块。");
            MapSyncViewerDir = Config.Bind("14. Map", "MapSyncViewerDir", "",
                "查看器目录（含 index.html）。留空 = 自动在插件目录的 PETweaks/map-viewer 或 map-viewer 下查找。");

            PanelClothClick = Config.Bind("13. QoL", "PanelClothClick", "Double",
                "I 键面板里点击服装格的行为：Off = 只选中，Double = 双击穿脱，Single = 单击穿脱");

            FaceCamOffsetY = Config.Bind("12. HUD", "FaceCamOffsetY", 0f,
                "把面部摄像机窗口往上/下移动多少像素（正数=上移）。0 = 不动");

            ExtraClothSlots = Config.Bind("11. Inventory", "ExtraClothSlots", 0,
                "服装槽额外数量（原版 6）。实验性：会在运行时补数据并在 HUD 上克隆格子，0 = 不改");

            JiggleRotationScale = Config.Bind("10. Body Physics", "JiggleRotationScale", 0.5f,
                "位移→倾斜角的比例。0 = 只平移不倾斜（最不容易拉长）；1 = 完全按力臂换算。");

            YawLagMaxDegrees = Config.Bind("10. Body Physics", "YawLagMaxDegrees", 60f,
                "【横向】甩动角度上限（度）。建议 5~8；再大会把胸部网格扭歪。");

            InertiaKick = Config.Bind("10. Body Physics", "InertiaKick", 0f,
                "旧版转向冲量（0=关）。已被 JiggleMode 取代，保留兼容。");

            // BepInEx's manager object is destroyed on the first scene change,
            // so host our own persistent runner instead of using its coroutines.
            var go = new GameObject("PETweaks_Runner");
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Runner>();

            try { ClothClick.Install(); } catch (Exception e) { Logger.LogWarning("cloth click install failed: " + e.Message); }
            try { YawPatch.Install(); }
            catch (Exception e) { Logger.LogWarning("yaw patch install failed: " + e.Message); }

            Logger.LogInfo("PETweaks v1.7.0 loaded (persistent runner attached)");
        }

        private void OnApplicationQuit() { try { MapSync.Shutdown(); } catch { } }

        internal static void Apply(string reason)
        {
            if (!CfgEnabled.Value) return;

            int speed  = Safe(Tweaks.ApplySpeed, reason);
            int rarity = RarityOverride.Value ? Safe(Tweaks.ApplyRarity, reason) : 0;
            int loot   = Safe(Tweaks.ApplyLoot, reason);
            int wear   = Safe(Tweaks.ApplyWear, reason);
            int dura   = Safe(Tweaks.ApplyItemDurability, reason);
            int trash  = Safe(Tweaks.ApplyTrash, reason);
            int stam   = Safe(Tweaks.ApplyStamina, reason);
            int body   = Safe(Tweaks.ApplyBodyPhysics, reason);
            Safe(Tweaks.ApplyLocale, reason);

            if (Verbose.Value)
                Log($"[{reason}] speed={speed} rarity={rarity} lootTables={loot} wear={wear} durability={dura} trash={trash} stamina={stam} body={body}");
        }

        private static int Safe(Func<int> fn, string reason)
        {
            try { return fn(); }
            catch (Exception e) { Log($"[{reason}] EXCEPTION {e.GetType().Name}: {e.Message}"); return -1; }
        }

        internal static void Log(string msg) { try { LogSource?.LogInfo(msg); } catch { } }
    }

    /// <summary>Persistent host: pushes the configured values into live objects every few seconds.</summary>
    public class Runner : MonoBehaviour
    {
        private float _nextApply;

        private void Awake() { UnityEngine.Object.DontDestroyOnLoad(gameObject); }

        private void Update()
        {
            try { if (Plugin.CfgEnabled.Value) Inertia.Tick(Plugin.InertiaKick.Value); } catch { }
            try { ClothSlots.Tick(Plugin.ExtraClothSlots.Value); } catch { }
            try { FaceCam.Tick(Plugin.FaceCamOffsetY.Value); } catch { }
            try { MapInfo.Tick(); } catch { }
            try { MapSync.Tick(); } catch { }
            try { SideRandom.Tick(Plugin.SideRandomInterval.Value, Plugin.SideRandomize.Value && Plugin.CfgEnabled.Value, Plugin.BodyAsymmetryMul.Value); } catch { }

            if (Time.unscaledTime < _nextApply) return;
            _nextApply = Time.unscaledTime + 5f;
            try { Plugin.Apply("tick"); } catch { }
        }

        private void OnGUI()
        {
            try { MapInfo.Draw(); } catch { }
        }
    }

    /// <summary>
    /// Optional extra: injects a sideways impulse into the breast springs when the
    /// character turns quickly, so the motion is not purely vertical.
    /// Off by default (InertiaKick = 0).
    /// </summary>
    internal static class Inertia
    {
        private class State { public float Yaw; public Vector3 Pos; public bool Has; }

        private static Type _bodyType;
        private static FieldInfo _leftBreast, _rightBreast, _massField, _rootField;
        private static MethodInfo _kick;
        private static readonly List<UnityEngine.Object> _bodies = new List<UnityEngine.Object>();
        private static readonly Dictionary<int, State> _states = new Dictionary<int, State>();
        private static float _nextScan;

        internal static void Tick(float strength)
        {
            if (strength <= 0f) return;
            if (Time.unscaledTime >= _nextScan) { _nextScan = Time.unscaledTime + 2f; Scan(); }
            if (_bodies.Count == 0 || _kick == null) return;

            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) return;

            foreach (UnityEngine.Object o in _bodies)
            {
                if (o == null) continue;
                Component comp = o as Component;
                if (comp == null) continue;
                // 用游戏自己的 _root 坐标系（SwingBreast 里用的就是它）
                Transform tf = (_rootField != null ? _rootField.GetValue(o) as Transform : null) ?? comp.transform;
                if (tf == null) continue;
                int id = o.GetInstanceID();

                if (!_states.TryGetValue(id, out State st)) { _states[id] = new State { Yaw = tf.eulerAngles.y, Pos = tf.position, Has = true }; continue; }

                float yawRate = Mathf.DeltaAngle(st.Yaw, tf.eulerAngles.y) / dt;  // deg/s
                st.Yaw = tf.eulerAngles.y;
                st.Pos = tf.position;

                float abs = Mathf.Abs(yawRate);
                if (abs < 25f) continue;                       // ignore small look adjustments

                float speed = Mathf.Clamp(abs * 0.0015f * strength, 0f, 2.5f);
                Vector3 dir = tf.right * Mathf.Sign(yawRate);
                Kick(o, _leftBreast, dir, speed);
                Kick(o, _rightBreast, dir, speed);
            }
        }

        private static void Scan()
        {
            _bodyType ??= Tweaks.FindType("Features.BodyPhysics.AnimeBodyPhysics");
            if (_bodyType == null) return;

            _leftBreast ??= _bodyType.GetField("_leftBreast", BindingFlags.NonPublic | BindingFlags.Instance);
            _rightBreast ??= _bodyType.GetField("_rightBreast", BindingFlags.NonPublic | BindingFlags.Instance);
            _rootField ??= _bodyType.GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);

            _bodies.Clear();
            foreach (UnityEngine.Object o in Resources.FindObjectsOfTypeAll(_bodyType))
            {
                if (o == null) continue;
                Component c = o as Component;
                if (c == null) continue;
                if (!c.gameObject.scene.IsValid()) continue;   // 排除 prefab 模板
                _bodies.Add(o);
            }

            if (_massField == null && _leftBreast != null)
            {
                Type bt = _leftBreast.FieldType;
                _massField = bt.GetField("Mass", BindingFlags.Public | BindingFlags.Instance)
                          ?? bt.GetField("mass", BindingFlags.Public | BindingFlags.Instance);
                Type mt = _massField?.FieldType;
                _kick = mt?.GetMethod("Kick", new[] { typeof(Vector3), typeof(float) });
            }
        }

        private static void Kick(object body, FieldInfo holder, Vector3 dir, float speed)
        {
            if (holder == null || _massField == null || _kick == null) return;
            object spring = holder.GetValue(body);
            if (spring == null) return;
            object mass = _massField.GetValue(spring);
            if (mass == null) return;
            _kick.Invoke(mass, new object[] { dir, speed });
        }
    }

    internal static class Tweaks
    {
        private static readonly string[] SpeedNames = { "walkSpeed", "runningSpeed", "sprintSpeed", "crouchSpeed" };
        private static readonly Dictionary<int, float[]> SpeedBase = new Dictionary<int, float[]>();
        private static readonly Dictionary<int, float[]> LootBase = new Dictionary<int, float[]>();
        private static readonly Dictionary<int, float> WearBase = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> DurabilityBase = new Dictionary<int, float>();

        // ---------- speed ----------
        internal static int ApplySpeed()
        {
            Type motor = FindType("Invector.vCharacterController.vThirdPersonMotor");
            if (motor == null) return 0;

            float[] mul = { Plugin.WalkMul.Value, Plugin.RunMul.Value, Plugin.SprintMul.Value, Plugin.CrouchMul.Value };
            int touched = 0;

            foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll(motor))
            {
                if (obj == null) continue;
                foreach (FieldInfo f in motor.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (f.FieldType == null || f.FieldType.Name != "vMovementSpeed") continue;
                    object ms = f.GetValue(obj);
                    if (ms == null) continue;

                    Type mt = ms.GetType();
                    int id = obj.GetInstanceID();
                    if (!SpeedBase.TryGetValue(id, out float[] bas))
                    {
                        bas = new float[SpeedNames.Length];
                        for (int i = 0; i < SpeedNames.Length; i++)
                            bas[i] = (float)mt.GetField(SpeedNames[i]).GetValue(ms);
                        SpeedBase[id] = bas;
                    }
                    for (int i = 0; i < SpeedNames.Length; i++)
                        mt.GetField(SpeedNames[i]).SetValue(ms, bas[i] * mul[i]);

                    // keep Invector's cached defaults in sync so ResetToDefault does not undo us
                    mt.GetMethod("Init", BindingFlags.Public | BindingFlags.Instance)?.Invoke(ms, null);
                    touched++;
                }
            }
            return touched;
        }

        // ---------- instance rarity weights ----------
        internal static int ApplyRarity()
        {
            Type t = FindType("Features.Inventory.RaritySettingsSO");
            if (t == null) return 0;

            int touched = 0;
            foreach (UnityEngine.Object so in Resources.FindObjectsOfTypeAll(t))
            {
                if (so == null) continue;
                if (SetPrivateFloat(so, t, "rollWeightBase", Plugin.RarityBase.Value))
                {
                    SetPrivateFloat(so, t, "rollWeightPlusOne", Plugin.RarityPlusOne.Value);
                    SetPrivateFloat(so, t, "rollWeightPlusTwo", Plugin.RarityPlusTwo.Value);
                    touched++;
                }
            }
            return touched;
        }

        // ---------- loot table entry weights ----------
        internal static int ApplyLoot()
        {
            if (Mathf.Approximately(Plugin.LootWeightMul.Value, 1f)) return 0;

            Type t = FindType("Features.TrashcanLoot.LootTableSO");
            if (t == null) return 0;
            FieldInfo entriesField = t.GetField("entries", BindingFlags.Public | BindingFlags.Instance);
            if (entriesField == null) return 0;

            int touched = 0;
            foreach (UnityEngine.Object so in Resources.FindObjectsOfTypeAll(t))
            {
                if (so == null) continue;
                if (!(entriesField.GetValue(so) is IList list) || list.Count == 0) continue;

                int id = so.GetInstanceID();
                if (!LootBase.TryGetValue(id, out float[] bas) || bas.Length != list.Count)
                {
                    bas = new float[list.Count];
                    for (int i = 0; i < list.Count; i++) bas[i] = GetFloat(list[i], "weight", 1f);
                    LootBase[id] = bas;
                }
                for (int i = 0; i < list.Count; i++)
                    SetFloat(list[i], "weight", bas[i] * Plugin.LootWeightMul.Value);
                touched++;
            }
            return touched;
        }

        // ---------- garment wear duration ----------
        internal static int ApplyWear()
        {
            if (Mathf.Approximately(Plugin.ClothingWearMul.Value, 1f) && !Plugin.UnlimitedWear.Value) return 0;

            Type t = FindType("Features.Inventory.Wearable.WearableItemSO");
            if (t == null) return 0;
            FieldInfo durF = t.GetField("wearDuration", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo flagF = t.GetField("hasWearDuration", BindingFlags.Public | BindingFlags.Instance);
            if (durF == null) return 0;

            int touched = 0;
            foreach (UnityEngine.Object so in Resources.FindObjectsOfTypeAll(t))
            {
                if (so == null) continue;
                int id = so.GetInstanceID();
                if (!WearBase.TryGetValue(id, out float bas)) { bas = (float)durF.GetValue(so); WearBase[id] = bas; }
                durF.SetValue(so, bas * Plugin.ClothingWearMul.Value);
                if (flagF != null && Plugin.UnlimitedWear.Value) flagF.SetValue(so, false);
                touched++;
            }
            return touched;
        }

        // ---------- item durability ----------
        internal static int ApplyItemDurability()
        {
            if (Mathf.Approximately(Plugin.ItemDurabilityMul.Value, 1f)) return 0;

            Type t = FindType("Features.Inventory.InventoryItemSO");
            if (t == null) return 0;
            FieldInfo maxF = t.GetField("maxDurability", BindingFlags.Public | BindingFlags.Instance);
            if (maxF == null) return 0;

            int touched = 0;
            foreach (UnityEngine.Object so in Resources.FindObjectsOfTypeAll(t))
            {
                if (so == null) continue;
                int id = so.GetInstanceID();
                if (!DurabilityBase.TryGetValue(id, out float bas)) { bas = (float)maxF.GetValue(so); DurabilityBase[id] = bas; }
                maxF.SetValue(so, bas * Plugin.ItemDurabilityMul.Value);
                touched++;
            }
            return touched;
        }

        // ---------- body physics (breast / butt springs) ----------
        private static readonly string[] BreastAmpFields = { "kick", "walkKick" };
        private static readonly string[] ButtAmpFields   = { "kick", "kickBack", "squash" };
        private static readonly string[] DampingFields   = { "damping", "walkDamping", "settleDamping", "restDamping" };
        private static readonly string[] FrequencyFields = { "frequency", "walkFrequency", "settleFrequency", "restFrequency" };
        private static readonly string[] KickRandomFields = { "kickRandom" };
        private static readonly string[] GainFields = { "gain" };
        private static readonly string[] CounterKickFields = { "counterKick" };
        private static readonly string[] SwayFields = { "sway" };
        private static readonly string[] AsymmetryFields = { "asymmetry" };
        private static readonly Dictionary<long, float[]> BodyBase = new Dictionary<long, float[]>();

        internal static int ApplyBodyPhysics()
        {
            bool neutral = Mathf.Approximately(Plugin.BreastJiggleMul.Value, 1f)
                        && Mathf.Approximately(Plugin.ButtJiggleMul.Value, 1f)
                        && Mathf.Approximately(Plugin.BodyDampingMul.Value, 1f)
                        && Mathf.Approximately(Plugin.BodyFrequencyMul.Value, 1f)
                        && Mathf.Approximately(Plugin.BodySwayMul.Value, 1f)
                        && Mathf.Approximately(Plugin.BodyAsymmetryMul.Value, 1f)
                        && Mathf.Approximately(Plugin.BodyCounterKickMul.Value, 1f)
                        && Mathf.Approximately(Plugin.BodyKickRandomMul.Value, 1f)
                        && Mathf.Approximately(Plugin.BodyGainMul.Value, 1f)
                        && Mathf.Approximately(Plugin.MaxAngleDegrees.Value, 0f)
                        && Plugin.StepBounce.Value;
            if (neutral) return 0;

            Type t = FindType("Features.BodyPhysics.AnimeBodyPhysicsConfigSO");
            if (t == null) return 0;

            int touched = 0;
            foreach (UnityEngine.Object so in Resources.FindObjectsOfTypeAll(t))
            {
                if (so == null) continue;
                int id = so.GetInstanceID();
                object breasts = t.GetField("breasts", BindingFlags.Public | BindingFlags.Instance)?.GetValue(so);
                object butt    = t.GetField("butt", BindingFlags.Public | BindingFlags.Instance)?.GetValue(so);

                touched += Scale(breasts, id * 10 + 1, BreastAmpFields, Plugin.BreastJiggleMul.Value);
                touched += Scale(breasts, id * 10 + 2, DampingFields,   Plugin.BodyDampingMul.Value);
                touched += Scale(breasts, id * 10 + 3, FrequencyFields, Plugin.BodyFrequencyMul.Value);
                touched += Scale(butt,    id * 10 + 4, ButtAmpFields,   Plugin.ButtJiggleMul.Value);
                touched += Scale(butt,    id * 10 + 5, DampingFields,   Plugin.BodyDampingMul.Value);
                touched += Scale(butt,    id * 10 + 6, FrequencyFields, Plugin.BodyFrequencyMul.Value);
                touched += Scale(breasts, id * 10 + 7, SwayFields,      Plugin.BodySwayMul.Value);
                touched += Scale(butt,    id * 10 + 8, SwayFields,      Plugin.BodySwayMul.Value);

                touched += Scale(breasts, id * 10 + 11, CounterKickFields, Plugin.BodyCounterKickMul.Value);
                touched += Scale(butt,    id * 10 + 12, CounterKickFields, Plugin.BodyCounterKickMul.Value);
                touched += Scale(breasts, id * 10 + 13, KickRandomFields, Plugin.BodyKickRandomMul.Value);
                touched += Scale(butt,    id * 10 + 14, KickRandomFields, Plugin.BodyKickRandomMul.Value);
                touched += Scale(breasts, id * 10 + 15, GainFields, Plugin.BodyGainMul.Value);
                if (Plugin.StepBounce.Value == false)
                {
                    touched += SetAbs(breasts, "kick", 0f);
                    touched += SetAbs(breasts, "walkKick", 0f);
                    touched += SetAbs(butt, "kick", 0f);
                    touched += SetAbs(butt, "kickBack", 0f);
                }
                if (Plugin.MaxAngleDegrees.Value > 0f)
                {
                    touched += SetAbs(breasts, "maxAngle", Plugin.MaxAngleDegrees.Value);
                }
                touched += Scale(butt,    id * 10 + 16, GainFields, Plugin.BodyGainMul.Value);
            }
            return touched;
        }

        private static int SetAbs(object settings, string field, float value)
        {
            if (settings == null) return 0;
            FieldInfo f = settings.GetType().GetField(field, BindingFlags.Public | BindingFlags.Instance);
            if (f == null || f.FieldType != typeof(float)) return 0;
            f.SetValue(settings, value);
            return 1;
        }

        private static int Scale(object settings, long key, string[] fields, float mul)
        {
            if (settings == null || Mathf.Approximately(mul, 1f)) return 0;
            Type st = settings.GetType();

            var present = new List<FieldInfo>();
            var names = new List<string>();
            foreach (string n in fields)
            {
                FieldInfo f = st.GetField(n, BindingFlags.Public | BindingFlags.Instance);
                if (f != null && f.FieldType == typeof(float)) { present.Add(f); names.Add(n); }
            }
            if (present.Count == 0) return 0;

            if (!BodyBase.TryGetValue(key, out float[] bas) || bas.Length != present.Count)
            {
                bas = new float[present.Count];
                for (int i = 0; i < present.Count; i++) bas[i] = (float)present[i].GetValue(settings);
                BodyBase[key] = bas;
            }
            for (int i = 0; i < present.Count; i++) present[i].SetValue(settings, bas[i] * mul);
            return 1;
        }

        // ---------- stamina ----------
        private static readonly Dictionary<int, float[]> StaminaBase = new Dictionary<int, float[]>();
        private static readonly string[] StaminaProps = { "maxStamina", "staminaRecovery", "sprintStamina", "jumpStamina", "rollStamina" };

        internal static int ApplyStamina()
        {
            bool neutral = Mathf.Approximately(Plugin.MaxStaminaMul.Value, 1f)
                        && Mathf.Approximately(Plugin.StaminaRecoveryMul.Value, 1f)
                        && Mathf.Approximately(Plugin.SprintCostMul.Value, 1f)
                        && Mathf.Approximately(Plugin.JumpCostMul.Value, 1f)
                        && Mathf.Approximately(Plugin.RollCostMul.Value, 1f)
                        && !Plugin.IgnoreStaminaConsumption.Value;
            if (neutral) return 0;

            Type motor = FindType("Invector.vCharacterController.vThirdPersonMotor");
            if (motor == null) return 0;

            float[] mul = { Plugin.MaxStaminaMul.Value, Plugin.StaminaRecoveryMul.Value,
                            Plugin.SprintCostMul.Value, Plugin.JumpCostMul.Value, Plugin.RollCostMul.Value };

            int touched = 0;
            foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll(motor))
            {
                if (obj == null) continue;
                int id = obj.GetInstanceID();
                if (!StaminaBase.TryGetValue(id, out float[] bas))
                {
                    bas = new float[StaminaProps.Length];
                    for (int i = 0; i < StaminaProps.Length; i++)
                    {
                        PropertyInfo p = motor.GetProperty(StaminaProps[i], BindingFlags.Public | BindingFlags.Instance);
                        bas[i] = p != null ? (float)p.GetValue(obj, null) : 1f;
                    }
                    StaminaBase[id] = bas;
                }
                for (int i = 0; i < StaminaProps.Length; i++)
                {
                    if (Mathf.Approximately(mul[i], 1f)) continue;
                    PropertyInfo p = motor.GetProperty(StaminaProps[i], BindingFlags.Public | BindingFlags.Instance);
                    if (p != null && p.CanWrite) p.SetValue(obj, bas[i] * mul[i], null);
                }
                if (Plugin.IgnoreStaminaConsumption.Value)
                {
                    FieldInfo f = motor.GetField("ignoreStaminaConsumption", BindingFlags.Public | BindingFlags.Instance);
                    if (f != null) f.SetValue(obj, true);
                }
                touched++;
            }
            return touched;
        }

        // ---------- trashcan item count ----------
        internal static int ApplyTrash()
        {
            Type t = FindType("Features.TrashcanLoot.TrashcanLootProfileSO");
            if (t == null) return 0;
            FieldInfo minF = t.GetField("minItems", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo maxF = t.GetField("maxItems", BindingFlags.Public | BindingFlags.Instance);
            if (minF == null || maxF == null) return 0;

            int lo = Mathf.Clamp(Plugin.TrashMinItems.Value, 0, 20);
            int hi = Mathf.Clamp(Plugin.TrashMaxItems.Value, 0, 20);
            if (hi < lo) hi = lo;

            int touched = 0;
            foreach (UnityEngine.Object so in Resources.FindObjectsOfTypeAll(t))
            {
                if (so == null) continue;
                minF.SetValue(so, lo);
                maxF.SetValue(so, hi);
                touched++;
            }
            return touched;
        }

        // ---------- locale ----------
        internal static int ApplyLocale()
        {
            string want = Plugin.ForceLocale.Value;
            if (string.IsNullOrWhiteSpace(want)) return 0;

            Type t = FindType("Features.Localization.GameLocalization");
            if (t == null) return 0;

            string cur = t.GetProperty("CurrentCode", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
            if (cur == want) return 0;

            object loading = t.GetProperty("IsLoading", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (loading is bool busy && busy) return 0;

            MethodInfo m = t.GetMethod("SetLocale", new[] { typeof(string), typeof(bool) })
                        ?? t.GetMethod("SetLocale", new[] { typeof(string) });
            if (m == null) return 0;

            m.Invoke(null, m.GetParameters().Length == 2 ? new object[] { want, true } : new object[] { want });
            Plugin.Log($"locale: {cur} -> {want}");
            return 1;
        }

        // ---------- helpers ----------
        private static bool SetPrivateFloat(object obj, Type t, string field, float value)
        {
            FieldInfo f = t.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) return false;
            f.SetValue(obj, value);
            return true;
        }

        private static float GetFloat(object entry, string field, float fallback)
        {
            FieldInfo f = entry?.GetType().GetField(field, BindingFlags.Public | BindingFlags.Instance);
            return f == null ? fallback : (float)f.GetValue(entry);
        }

        private static void SetFloat(object entry, string field, float v)
        {
            FieldInfo f = entry?.GetType().GetField(field, BindingFlags.Public | BindingFlags.Instance);
            f?.SetValue(entry, v);
        }

        internal static Type FindType(string fullName)
        {
            Type t = Type.GetType(fullName + ", Assembly-CSharp");
            if (t != null) return t;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }
    }

    /// <summary>Vertical-axis inertia: the breast bone lags behind the torso when the character turns.</summary>
    public class YawLagRunner : MonoBehaviour
    {
        private void Awake()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        }

        private void OnDestroy()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        }

        private void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
        {
            try { YawLag.Tick(Plugin.YawLag.Value, Plugin.YawLagMaxDegrees.Value); } catch { }
        }
    }

    internal static class YawLag
    {
        private class State { public float Angle; public float Vel; }

        private static Type _bodyType;
        private static FieldInfo _rootField, _leftBreast, _rightBreast, _springField, _baseField;
        private static readonly List<UnityEngine.Object> _bodies = new List<UnityEngine.Object>();
        private static readonly Dictionary<int, float> _prevYaw = new Dictionary<int, float>();
        private static readonly Dictionary<int, State> _states = new Dictionary<int, State>();
        private static float _nextScan;

        private static bool _logged;

        private static int _lastFrame = -1;

        internal static void Tick(float strength, float maxDeg)
        {
            if (strength <= 0f) return;
            if (Time.frameCount == _lastFrame) return;
            _lastFrame = Time.frameCount;
            if (!_logged) { _logged = true; Plugin.Log($"yawlag active (strength={strength}, max={maxDeg} deg)"); }
            if (!_loggedBodies && _bodies.Count > 0)
            {
                _loggedBodies = true;
                var sb = new System.Text.StringBuilder();
                foreach (UnityEngine.Object o in _bodies) sb.Append(((Component)o).transform.root.name).Append(' ');
                Plugin.Log($"yawlag bodies={_bodies.Count} roots=[{sb}]");
            }
            if (Time.unscaledTime >= _nextScan) { _nextScan = Time.unscaledTime + 2f; Scan(); }
            if (_bodies.Count == 0 || _springField == null) return;

            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) return;

            foreach (UnityEngine.Object o in _bodies)
            {
                if (o == null) continue;
                Component comp = o as Component;
                if (comp == null) continue;
                Transform root = (_rootField?.GetValue(o) as Transform) ?? comp.transform;
                if (root == null) continue;

                int id = o.GetInstanceID();
                float yaw = root.eulerAngles.y;
                if (!_prevYaw.TryGetValue(id, out float prev)) { _prevYaw[id] = yaw; continue; }

                float rate = Mathf.DeltaAngle(prev, yaw) / dt;   // deg per second
                _prevYaw[id] = yaw;

                float target = Mathf.Clamp(-rate * 0.35f * strength, -maxDeg, maxDeg);
                if (!_states.TryGetValue(id, out State st)) { st = new State(); _states[id] = st; }

                const float k = 120f, damp = 14f;
                st.Vel += ((target - st.Angle) * k - st.Vel * damp) * dt;
                st.Angle = Mathf.Clamp(st.Angle + st.Vel * dt, -maxDeg, maxDeg);

                Apply(o, _leftBreast, root, st.Angle);
                Apply(o, _rightBreast, root, st.Angle);
            }
        }


        /// <summary>Walk up the breast bone chain and return its root (so the whole breast swings).</summary>
        private static Transform BreastRoot(Transform a, Transform b)
        {
            Transform best = null;
            int bestDepth = -1;
            foreach (Transform candidate in new[] { a, b })
            {
                if (candidate == null) continue;
                Transform root = candidate;
                int depth = 0;
                for (Transform p = candidate.parent; p != null; p = p.parent)
                {
                    if (p.name.IndexOf("breast", StringComparison.OrdinalIgnoreCase) < 0) break;
                    root = p; depth++;
                }
                if (depth > bestDepth) { bestDepth = depth; best = root; }
            }
            return best;
        }

        private static void Apply(object body, FieldInfo holder, Transform root, float deg)
        {
            if (holder == null || Mathf.Abs(deg) < 0.01f) return;
            object breast = holder.GetValue(body);
            if (breast == null) return;

            Transform baseBone = _baseField?.GetValue(breast) as Transform;
            Transform springBone = _springField?.GetValue(breast) as Transform;
            if (baseBone == null && springBone == null) return;

            // c_breast_01 (Spring) 和 c_breast_02 (Base) 是挂在脊椎下的兄弟骨，
            // 两根一起转，整块胸才会一起摆动；只转其中一根只会动一半。
            Spin(baseBone, root, deg);
            Spin(springBone, root, deg);

            Transform bone = baseBone ?? springBone;

            if (!_dumped)
            {
                _dumped = true;
                Plugin.Log("breast chain: " + Chain(baseBone) + " | spring: " + Chain(springBone));
            }
            if (Time.unscaledTime >= _nextDiag)
            {
                _nextDiag = Time.unscaledTime + 1f;
                Plugin.Log($"yawlag: bone={bone.name} deg={deg:F1}");
            }
        }

        private static float _nextDiag;
        private static bool _dumped, _loggedBodies;

        private static void Spin(Transform bone, Transform root, float deg)
        {
            if (bone == null) return;
            Transform parent = bone.parent;
            Vector3 axis = parent != null ? parent.InverseTransformDirection(root.up) : Vector3.up;
            bone.localRotation = Quaternion.AngleAxis(deg, axis) * bone.localRotation;
        }

        private static string Chain(Transform t)
        {
            if (t == null) return "null";
            var sb = new System.Text.StringBuilder(t.name);
            Transform p = t.parent; int n = 0;
            while (p != null && n < 6) { sb.Append(" <- ").Append(p.name); p = p.parent; n++; }
            sb.Append(" | children: ");
            for (int i = 0; i < t.childCount && i < 4; i++) sb.Append(t.GetChild(i).name).Append(' ');
            return sb.ToString();
        }

        private static void Scan()
        {
            _bodyType ??= Tweaks.FindType("Features.BodyPhysics.AnimeBodyPhysics");
            if (_bodyType == null) return;

            _rootField ??= _bodyType.GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
            _leftBreast ??= _bodyType.GetField("_leftBreast", BindingFlags.NonPublic | BindingFlags.Instance);
            _rightBreast ??= _bodyType.GetField("_rightBreast", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_springField == null && _leftBreast != null)
            {
                _springField = _leftBreast.FieldType.GetField("Spring", BindingFlags.Public | BindingFlags.Instance);
                _baseField = _leftBreast.FieldType.GetField("Base", BindingFlags.Public | BindingFlags.Instance);
            }

            _bodies.Clear();
            foreach (UnityEngine.Object o in Resources.FindObjectsOfTypeAll(_bodyType))
            {
                if (o == null) continue;
                Component c = o as Component;
                if (c == null) continue;
                if (!c.gameObject.scene.IsValid()) continue;   // 排除 prefab 模板
                _bodies.Add(o);
            }
        }
    }


    /// <summary>Randomly flips which side is the livelier one (changes the sign of asymmetry).</summary>
    internal static class SideRandom
    {
        private static readonly Dictionary<long, float> _base = new Dictionary<long, float>();
        private static readonly List<UnityEngine.Object> _sos = new List<UnityEngine.Object>();
        private static float _nextScan, _nextFlip;
        private static float _sign = 1f;

        internal static void Tick(float interval, bool enabled, float mul)
        {
            if (!enabled && Mathf.Approximately(mul, 1f)) return;

            if (Time.unscaledTime >= _nextScan) { _nextScan = Time.unscaledTime + 2f; Scan(); }
            if (Time.unscaledTime >= _nextFlip)
            {
                _nextFlip = Time.unscaledTime + Mathf.Max(0.1f, interval);
                _sign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            }
            if (_sos.Count == 0) return;

            float sign = enabled ? _sign : 1f;
            foreach (UnityEngine.Object so in _sos)
            {
                if (so == null) continue;
                int id = so.GetInstanceID();
                Apply(so, "breasts", id * 2L + 0, mul * sign);
                Apply(so, "butt",    id * 2L + 1, mul * sign);
            }
        }

        private static void Apply(object so, string group, long key, float k)
        {
            object settings = so.GetType().GetField(group, BindingFlags.Public | BindingFlags.Instance)?.GetValue(so);
            if (settings == null) return;
            FieldInfo f = settings.GetType().GetField("asymmetry", BindingFlags.Public | BindingFlags.Instance);
            if (f == null || f.FieldType != typeof(float)) return;
            if (!_base.TryGetValue(key, out float bas)) { bas = (float)f.GetValue(settings); _base[key] = bas; }
            f.SetValue(settings, bas * k);
        }

        private static void Scan()
        {
            _sos.Clear();
            Type t = Tweaks.FindType("Features.BodyPhysics.AnimeBodyPhysicsConfigSO");
            if (t == null) return;
            foreach (UnityEngine.Object o in Resources.FindObjectsOfTypeAll(t))
                if (o != null) _sos.Add(o);
        }
    }



    /// <summary>
    /// Harmony postfix on AnimeBodyPhysics.SwingBreast.
    /// Mode "Spring": each breast is a damped point mass driven by torso acceleration
    /// (linear + centrifugal), then written into TargetPosition / TargetRotation so the
    /// game's own apply path keeps garments and cloth in sync.
    /// Mode "Simple": the older angle-mapped offset.
    /// </summary>
    internal static class YawPatch
    {
        private class St
        {
            public int Frame;
            public bool Init;
            public Quaternion PrevRot;
            public Vector3 PrevPos, PrevVel;
            public Vector3 Offset, Vel;      // spring mode (parent-local metres)
            public float Angle, AngleVel, PrevYaw; // simple mode
        }

        private static Type _bodyType, _breastType;
        private static FieldInfo _rootField, _springField, _targetRot, _targetBase, _targetPos;
        private static readonly Dictionary<int, St> _states = new Dictionary<int, St>();
        private static bool _installed;

        internal static void Install()
        {
            if (_installed) return;
            _bodyType = Tweaks.FindType("Features.BodyPhysics.AnimeBodyPhysics");
            if (_bodyType == null) { Plugin.Log("yaw patch: AnimeBodyPhysics not found"); return; }

            MethodInfo mi = _bodyType.GetMethod("SwingBreast", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi == null) { Plugin.Log("yaw patch: SwingBreast not found"); return; }

            _breastType = mi.GetParameters()[0].ParameterType;
            _rootField = _bodyType.GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
            _springField = _breastType.GetField("Spring", BindingFlags.Public | BindingFlags.Instance);
            _targetRot = _breastType.GetField("TargetRotation", BindingFlags.Public | BindingFlags.Instance);
            _targetBase = _breastType.GetField("TargetBaseRotation", BindingFlags.Public | BindingFlags.Instance);
            _targetPos = _breastType.GetField("TargetPosition", BindingFlags.Public | BindingFlags.Instance);

            var harmony = new Harmony("com.dsh.petweaks.yaw");
            harmony.Patch(mi, postfix: new HarmonyMethod(typeof(YawPatch).GetMethod(nameof(Postfix), BindingFlags.NonPublic | BindingFlags.Static)));
            _installed = true;
            Plugin.Log("yaw patch installed on SwingBreast (mode=" + Plugin.JiggleMode.Value + ")");
        }

        private static void Postfix(object __0, object __instance)
        {
            if (!_installed || __0 == null || __instance == null) return;
            if (!Plugin.CfgEnabled.Value) return;   // 总开关关闭时不注入任何惯性

            bool springMode = !string.Equals(Plugin.JiggleMode.Value, "Simple", StringComparison.OrdinalIgnoreCase);
            if (springMode && Plugin.JiggleStiffness.Value <= 0f) return;
            if (!springMode && Plugin.YawLag.Value <= 0f) return;

            Component comp = __instance as Component;
            if (comp == null) return;

            int id = comp.GetInstanceID();
            if (!_states.TryGetValue(id, out St st)) { st = new St(); _states[id] = st; }

            Transform root = (_rootField?.GetValue(__instance) as Transform) ?? comp.transform;
            Transform spring = _springField?.GetValue(__0) as Transform;
            Transform refTf = (spring != null && spring.parent != null) ? spring.parent : root;

            bool newFrame = st.Frame != Time.frameCount;
            if (newFrame)
            {
                st.Frame = Time.frameCount;
                if (!st.Init)
                {
                    // 首帧只记录参考状态，不做积分（否则 PrevPos=0 会产生巨大瞬时速度）
                    st.PrevPos = refTf.position;
                    st.PrevRot = refTf.rotation;
                    st.PrevYaw = refTf.eulerAngles.y;
                    st.Init = true;
                }
                else if (springMode) StepSpring(st, refTf, spring, root);
                else StepSimple(st, refTf, spring, root);
            }
            if (Mathf.Abs(st.Angle) < 0.01f && st.Offset.sqrMagnitude < 1e-8f) return;

            ApplyRotation(st, __0, spring, root);
            ApplyPosition(st, __0, spring, root);
        }

        // ---------- Simple: angle mapped to a fixed lateral offset ----------
        private static void StepSimple(St st, Transform refTf, Transform spring, Transform root)
        {
            float strength = Plugin.YawLag.Value;
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) return;

            float yaw = refTf.eulerAngles.y;
            float rate = Mathf.DeltaAngle(st.PrevYaw, yaw) / dt;
            st.PrevYaw = yaw;

            float max = Mathf.Max(0.1f, Plugin.YawLagMaxDegrees.Value);
            float target = Mathf.Clamp(-rate * 0.35f * strength, -max, max);
            const float k = 120f, damp = 14f;
            st.AngleVel += ((target - st.Angle) * k - st.AngleVel * damp) * dt;
            st.Angle = Mathf.Clamp(st.Angle + st.AngleVel * dt, -max, max);
        }

        // ---------- Spring: point mass driven by torso acceleration ----------
        private static void StepSpring(St st, Transform refTf, Transform spring, Transform root)
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f || spring == null) return;

            Vector3 pos = refTf.position;
            Quaternion rot = refTf.rotation;

            Vector3 linVel = (pos - st.PrevPos) / dt;
            Vector3 linAcc = (linVel - st.PrevVel) / dt;
            st.PrevVel = linVel;
            st.PrevPos = pos;

            Quaternion dq = rot * Quaternion.Inverse(st.PrevRot);
            dq.ToAngleAxis(out float dAng, out Vector3 dAxis);
            if (dAng > 180f) dAng -= 360f;
            Vector3 angVel = (dAng * Mathf.Deg2Rad / dt) * dAxis;
            st.PrevRot = rot;

            Vector3 r = spring.position - pos;
            Vector3 drive = -Vector3.Cross(angVel, Vector3.Cross(angVel, r)) - linAcc;
            if (drive.magnitude > 60f) drive = drive.normalized * 60f;

            Transform parent = spring.parent;
            Vector3 driveLocal = parent != null ? parent.InverseTransformDirection(drive) : drive;

            float k = Mathf.Max(1f, Plugin.JiggleStiffness.Value);
            float c = Mathf.Max(0f, Plugin.JiggleDamping.Value);
            st.Vel += (driveLocal - k * st.Offset - c * st.Vel) * dt;
            st.Offset += st.Vel * dt;

            float maxOff = Mathf.Max(0.001f, Plugin.YawOffsetMeters.Value);
            if (st.Offset.magnitude > maxOff) st.Offset = st.Offset.normalized * maxOff;

            // 用位移反推一个旋转角，让形状跟着倾斜
            float lat = parent != null ? Vector3.Dot(st.Offset, parent.InverseTransformDirection(root.right)) : st.Offset.x;
            float arm = Mathf.Max(0.01f, Plugin.JiggleArmMeters.Value);
            float deg = lat / arm * Mathf.Rad2Deg * Plugin.JiggleRotationScale.Value;
            float cap = Mathf.Max(0.1f, Plugin.YawLagMaxDegrees.Value);
            st.Angle = Mathf.Clamp(deg, -cap, cap);
        }

        private static void ApplyRotation(St st, object breast, Transform spring, Transform root)
        {
            if (Mathf.Abs(st.Angle) < 0.01f || _targetRot == null) return;
            Vector3 axis = (spring != null && spring.parent != null)
                ? spring.parent.InverseTransformDirection(root.up)
                : Vector3.up;
            Quaternion extra = Quaternion.AngleAxis(st.Angle, axis);

            _targetRot.SetValue(breast, extra * (Quaternion)_targetRot.GetValue(breast));
            if (_targetBase != null) _targetBase.SetValue(breast, extra * (Quaternion)_targetBase.GetValue(breast));
        }

        private static void ApplyPosition(St st, object breast, Transform spring, Transform root)
        {
            if (_targetPos == null) return;

            Vector3 delta;
            if (st.Offset.sqrMagnitude > 1e-8f)
            {
                delta = st.Offset;                     // spring mode: already parent-local
            }
            else if (Mathf.Abs(st.Angle) > 0.01f)
            {
                float max = Mathf.Max(0.1f, Plugin.YawLagMaxDegrees.Value);
                float t01 = Mathf.Clamp(st.Angle / max, -1f, 1f);
                Vector3 lateral = (spring != null && spring.parent != null)
                    ? spring.parent.InverseTransformDirection(root.right)
                    : Vector3.right;
                delta = lateral * (t01 * Plugin.YawOffsetMeters.Value);
            }
            else return;

            Vector3 pos = (Vector3)_targetPos.GetValue(breast);
            _targetPos.SetValue(breast, pos + delta);
        }
    }

    /// <summary>Experimental: add extra clothing slots (data lists + cloned HUD slots).</summary>
    internal static class ClothSlots
    {
        private static bool _applied, _failed;
        private static float _next;
        private static float _nextPosLog;
        private static float _nextGridDump;

        internal static void Tick(int extra)
        {
            if (!Plugin.CfgEnabled.Value) return;
            if (extra <= 0 || _applied || _failed) return;
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 2f;

            try
            {
                Type pm = Tweaks.FindType("Features.Inventory.PlayerInventoryManager");
                Type hud = Tweaks.FindType("Features.Inventory.UI.ClothHUDController");
                if (pm == null || hud == null) return;

                object inv = pm.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)
                          ?? UnityEngine.Object.FindObjectOfType(pm);
                if (inv == null) return;

                object controller = null;
                foreach (UnityEngine.Object o in Resources.FindObjectsOfTypeAll(hud))
                {
                    Component c = o as Component;
                    if (c == null || !c.gameObject.scene.IsValid()) continue;
                    controller = o; break;
                }
                if (controller == null) return;

                FieldInfo boundF = hud.GetField("_bound", BindingFlags.NonPublic | BindingFlags.Instance);
                if (boundF != null && !(bool)boundF.GetValue(controller)) return;   // 等 HUD 完成绑定

                FieldInfo slotsF = hud.GetField("_slots", BindingFlags.NonPublic | BindingFlags.Instance);
                IList uiSlots = slotsF?.GetValue(controller) as IList;
                if (uiSlots == null || uiSlots.Count == 0) return;

                FieldInfo maxF = pm.GetField("maxClothSlots", BindingFlags.NonPublic | BindingFlags.Instance);
                int oldCount = (int)maxF.GetValue(inv);
                int target = oldCount + extra;
                if (uiSlots.Count >= target) { _applied = true; return; }

                int need = target - uiSlots.Count;

                // --- 1) 数据层：上限 + 4 个并行列表 ---
                maxF.SetValue(inv, target);
                AppendList(pm, inv, "clothSlots", null);
                AppendList(pm, inv, "clothDurability", 0f);
                AppendEnumList(pm, inv, "clothRarities");
                AppendList(pm, inv, "clothVariants", -1);
                for (int i = 1; i < need; i++)
                {
                    AppendList(pm, inv, "clothSlots", null);
                    AppendList(pm, inv, "clothDurability", 0f);
                    AppendEnumList(pm, inv, "clothRarities");
                    AppendList(pm, inv, "clothVariants", -1);
                }

                // --- 2) UI：克隆格子 ---
                Component lastSlot = uiSlots[uiSlots.Count - 1] as Component;
                Component prevSlot = uiSlots.Count > 1 ? uiSlots[uiSlots.Count - 2] as Component : null;
                if (lastSlot == null) { _failed = true; Plugin.Log("clothslots: last slot is null"); return; }

                Vector2 step = Vector2.zero;
                if (prevSlot != null)
                {
                    RectTransform a = prevSlot.transform as RectTransform;
                    RectTransform b = lastSlot.transform as RectTransform;
                    if (a != null && b != null) step = b.anchoredPosition - a.anchoredPosition;
                }
                if (step.sqrMagnitude < 1f) step = new Vector2(56f, 0f);

                Transform parent = lastSlot.transform.parent;
                Type slotType = lastSlot.GetType();
                MethodInfo init = slotType.GetMethod("Init", new[] { typeof(int), hud });

                for (int k = 0; k < need; k++)
                {
                    GameObject go = UnityEngine.Object.Instantiate(lastSlot.gameObject, parent, false);
                    go.name = "ClothSlot_auto" + (uiSlots.Count + k);
                    RectTransform rt = go.transform as RectTransform;
                    RectTransform lastRt = lastSlot.transform as RectTransform;
                    if (rt != null && lastRt != null)
                        rt.anchoredPosition = lastRt.anchoredPosition + step * (k + 1);

                    Component comp = go.GetComponent(slotType);
                    init?.Invoke(comp, new object[] { uiSlots.Count + k, controller });
                    uiSlots.Add(comp);
                    IList serialized = hud.GetField("serializedSlots", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(controller) as IList;
                    serialized?.Add(comp);
                }

                // --- 3) 索引数组 + 重建轮播 ---
                FieldInfo idxF = hud.GetField("_uiToClothIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                int[] idx = new int[uiSlots.Count];
                int[] oldIdx = idxF?.GetValue(controller) as int[];
                for (int i = 0; i < idx.Length; i++) idx[i] = (oldIdx != null && i < oldIdx.Length) ? oldIdx[i] : -1;
                idxF?.SetValue(controller, idx);

                hud.GetMethod("RebuildCarousel", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(controller, null);

                _applied = true;
                Plugin.Log($"clothslots: expanded {oldCount} -> {target} (ui {uiSlots.Count}, step {step})");

                ExpandPanel(target);
            }
            catch (Exception e)
            {
                _failed = true;
                Plugin.Log("clothslots EXCEPTION " + e.GetType().Name + ": " + e.Message);
            }
        }


        /// <summary>同步扩展“物品栏”面板（I 键）里的服装格子。</summary>
        private static void ExpandPanel(int target)
        {
            try
            {
                Type dialog = Tweaks.FindType("Features.Inventory.UI.InventoryDialogController");
                if (dialog == null) { Plugin.Log("clothslots panel: dialog type not found"); return; }

                object inst = null;
                foreach (UnityEngine.Object o in Resources.FindObjectsOfTypeAll(dialog))
                {
                    Component c = o as Component;
                    if (c == null || !c.gameObject.scene.IsValid()) continue;
                    inst = o; break;
                }
                if (inst == null) { Plugin.Log("clothslots panel: no dialog instance in scene"); return; }

                FieldInfo listF = dialog.GetField("clothSlots", BindingFlags.NonPublic | BindingFlags.Instance);
                IList list = listF?.GetValue(inst) as IList;
                if (list == null || list.Count == 0) { Plugin.Log("clothslots panel: list empty"); return; }

                int need = target - list.Count;
                if (need <= 0) { Plugin.Log($"clothslots panel: already {list.Count}"); return; }

                Component last = list[list.Count - 1] as Component;
                Component prev = list.Count > 1 ? list[list.Count - 2] as Component : null;
                if (last == null) { Plugin.Log("clothslots panel: last slot null"); return; }

                Vector2 step = Vector2.zero;
                RectTransform a = prev?.transform as RectTransform;
                RectTransform b = last.transform as RectTransform;
                if (a != null && b != null) step = b.anchoredPosition - a.anchoredPosition;
                if (step.sqrMagnitude < 1f) step = new Vector2(70f, 0f);

                Type slotType = last.GetType();
                Type kindType = null;
                MethodInfo init = null;
                foreach (MethodInfo m in slotType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (m.Name != "Init") continue;
                    ParameterInfo[] ps = m.GetParameters();
                    if (ps.Length == 3 && ps[1].ParameterType.IsEnum) { init = m; kindType = ps[1].ParameterType; break; }
                }
                if (init == null || kindType == null) { Plugin.Log("clothslots panel: Init(int, enum, int) not found"); return; }
                object clothing = Enum.Parse(kindType, "Clothing");

                RectTransform rowRt = last.transform.parent as RectTransform;
                RectTransform firstRt = (list[0] as Component)?.transform as RectTransform;

                // 新建“第二行”容器，继承原行的锚点/尺寸/布局组设置
                GameObject row2 = new GameObject("ClothRow_auto", typeof(RectTransform));
                RectTransform row2Rt = row2.transform as RectTransform;
                row2Rt.SetParent(rowRt != null ? rowRt.parent : last.transform.parent, false);
                if (rowRt != null)
                {
                    row2Rt.anchorMin = rowRt.anchorMin;
                    row2Rt.anchorMax = rowRt.anchorMax;
                    row2Rt.pivot = rowRt.pivot;
                    row2Rt.sizeDelta = rowRt.sizeDelta;
                    row2Rt.anchoredPosition = rowRt.anchoredPosition + new Vector2(0f, -(rowRt.sizeDelta.y + 8f));
                    CopyLayout(rowRt.gameObject, row2);
                }

                for (int k = 0; k < need; k++)
                {
                    int idx = list.Count;
                    GameObject go = UnityEngine.Object.Instantiate(last.gameObject, row2Rt, false);
                    go.name = "ClothSlot_panel_auto" + idx;
                    RectTransform rt = go.transform as RectTransform;
                    if (rt != null && step.sqrMagnitude > 1f)
                        rt.anchoredPosition = new Vector2(step.x * k, 0f);
                    Component comp = go.GetComponent(slotType);
                    init.Invoke(comp, new object[] { inst, clothing, idx });
                    list.Add(comp);
                }

                dialog.GetMethod("RefreshAll", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(inst, null);
                Plugin.Log($"clothslots panel: expanded to {list.Count} (step {step})");
            }
            catch (Exception e)
            {
                Plugin.Log("clothslots panel EXCEPTION " + e.GetType().Name + ": " + e.Message);
            }
        }


        private static void CopyLayout(GameObject src, GameObject dst)
        {
            var h = src.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (h != null)
            {
                var d = dst.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                d.spacing = h.spacing;
                d.childAlignment = h.childAlignment;
                d.childControlWidth = h.childControlWidth;
                d.childControlHeight = h.childControlHeight;
                d.childForceExpandWidth = h.childForceExpandWidth;
                d.childForceExpandHeight = h.childForceExpandHeight;
                d.reverseArrangement = h.reverseArrangement;
                d.padding = new UnityEngine.RectOffset(h.padding.left, h.padding.right, h.padding.top, h.padding.bottom);
                return;
            }
            var v = src.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (v != null)
            {
                var d = dst.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                d.spacing = v.spacing;
                d.childAlignment = v.childAlignment;
                d.childControlWidth = v.childControlWidth;
                d.childControlHeight = v.childControlHeight;
                d.childForceExpandWidth = v.childForceExpandWidth;
                d.childForceExpandHeight = v.childForceExpandHeight;
                d.reverseArrangement = v.reverseArrangement;
                d.padding = new UnityEngine.RectOffset(v.padding.left, v.padding.right, v.padding.top, v.padding.bottom);
            }
        }

        private static void AppendList(Type pm, object inv, string field, object value)
        {
            FieldInfo f = pm.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            (f?.GetValue(inv) as IList)?.Add(value);
        }

        private static void AppendEnumList(Type pm, object inv, string field)
        {
            FieldInfo f = pm.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            IList list = f?.GetValue(inv) as IList;
            if (list == null) return;
            Type elem = f.FieldType.GetGenericArguments()[0];
            list.Add(Enum.ToObject(elem, 0));
        }
    }


    /// <summary>按 RenderTexture 反查显示角色头像的 RawImage，并整体上移。</summary>
    internal static class FaceCam
    {
        private static bool _done;
        private static float _next;
        private static float _nextPosLog;
        private static float _nextGridDump;

        internal static void Tick(float offsetY)
        {
            if (!Plugin.CfgEnabled.Value) return;
            if (_done) return;
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 3f;

            try
            {
                Type fc = Tweaks.FindType("ExhibitionistDemo.UI.PlayerFaceCam");
                Type rawT = Type.GetType("UnityEngine.UI.RawImage, UnityEngine.UI");
                if (fc == null || rawT == null) return;

                Camera cam = null;
                foreach (UnityEngine.Object o in Resources.FindObjectsOfTypeAll(fc))
                {
                    Component c = o as Component;
                    if (c == null) continue;
                    cam = c.GetComponent<Camera>();
                    if (cam != null) break;
                }
                if (cam == null || cam.targetTexture == null) return;

                PropertyInfo texProp = rawT.GetProperty("texture");
                if (texProp == null) return;

                foreach (UnityEngine.Object o in Resources.FindObjectsOfTypeAll(rawT))
                {
                    Component raw = o as Component;
                    if (raw == null || !raw.gameObject.activeInHierarchy) continue;
                    if (!ReferenceEquals(texProp.GetValue(raw), cam.targetTexture)) continue;

                    RectTransform rt = raw.transform as RectTransform;
                    if (rt == null) continue;

                    // --- 取景：退远 / 加宽视野 / 竖长画面 ---
                    float dist = Plugin.FaceCamDistance.Value;
                    if (dist > 0.05f && Mathf.Abs(dist - 1f) > 0.01f)
                    {
                        FieldInfo offF = fc.GetField("offset", BindingFlags.Public | BindingFlags.Instance);
                        Vector3 off = (Vector3)offF.GetValue(cam.GetComponent(fc));
                        off.z *= dist;
                        offF.SetValue(cam.GetComponent(fc), off);
                        Plugin.Log($"facecam: offset.z x{dist} -> {off.z:0.000}");
                    }
                    float fovScale = Plugin.FaceCamFovScale.Value;
                    if (fovScale > 0.05f && Mathf.Abs(fovScale - 1f) > 0.01f)
                    {
                        cam.fieldOfView *= fovScale;
                        Plugin.Log($"facecam: fov -> {cam.fieldOfView:0.0}");
                    }
                    float aspect = Plugin.FaceCamAspect.Value;
                    if (aspect > 0.05f && cam.targetTexture != null)
                    {
                        RenderTexture old = cam.targetTexture;
                        int w = old.width;
                        int h = Mathf.Max(1, Mathf.RoundToInt(w / aspect));
                        var rtNew = new RenderTexture(w, h, old.depth, old.format) { name = old.name };
                        rtNew.Create();
                        cam.targetTexture = rtNew;
                        texProp.SetValue(raw, rtNew);
                        RectTransform wr = (raw.transform.parent != null ? raw.transform.parent : raw.transform) as RectTransform;
                        if (wr != null) wr.sizeDelta = new Vector2(wr.sizeDelta.x, wr.sizeDelta.x / aspect);
                        Plugin.Log($"facecam: rt {old.width}x{old.height} -> {w}x{h} (aspect {aspect})");
                    }

                    // 打印 4 层父级，便于确认哪一层才是“窗口”
                    var sb = new System.Text.StringBuilder();
                    Transform cur = rt;
                    for (int i = 0; i < 4 && cur != null; i++)
                    {
                        RectTransform crt = cur as RectTransform;
                        sb.Append($"[{i}] {cur.name} pos={(crt != null ? crt.anchoredPosition.ToString() : "?")} size={(crt != null ? crt.sizeDelta.ToString() : "?")}  ");
                        cur = cur.parent;
                    }
                    Plugin.Log("facecam ui: " + sb);

                    // 上移：改 RawImage 的父节点（通常是窗口根）
                    Transform win = rt.parent != null ? rt.parent : rt;
                    RectTransform wrt = win as RectTransform;
                    if (wrt != null)
                    {
                        if (Mathf.Abs(offsetY) > 0.01f)
                        {
                            wrt.anchoredPosition = wrt.anchoredPosition + new Vector2(0f, offsetY);
                            Plugin.Log($"facecam ui: moved '{wrt.name}' by {offsetY}px -> {wrt.anchoredPosition}");
                        }
                        float scale = Plugin.FaceCamScale.Value;
                        if (scale > 0.05f && Mathf.Abs(scale - 1f) > 0.01f)
                        {
                            wrt.sizeDelta = wrt.sizeDelta * scale;
                            wrt.localScale = Vector3.one;
                            Plugin.Log($"facecam ui: scaled '{wrt.name}' x{scale} -> {wrt.sizeDelta}");
                        }
                    }
                    _done = true;
                    return;
                }
            }
            catch (Exception e)
            {
                Plugin.Log("facecam EXCEPTION " + e.GetType().Name + ": " + e.Message);
                _done = true;
            }
        }
    }


    /// <summary>I 键面板里点击服装格直接穿/脱。</summary>
    internal static class ClothClick
    {
        private static Type _dialogType;
        private static FieldInfo _selectedField;
        private static MethodInfo _use;
        private static bool _installed;
        private static object _prev;

        internal static void Install()
        {
            if (_installed) return;
            string mode = Plugin.PanelClothClick.Value;
            if (string.Equals(mode, "Off", StringComparison.OrdinalIgnoreCase)) return;

            _dialogType = Tweaks.FindType("Features.Inventory.UI.InventoryDialogController");
            if (_dialogType == null) { Plugin.Log("clothclick: dialog type not found"); return; }

            MethodInfo target = null;
            foreach (MethodInfo m in _dialogType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (m.Name == "OnSlotClicked" && m.GetParameters().Length == 1) { target = m; break; }
            }
            if (target == null) { Plugin.Log("clothclick: OnSlotClicked not found"); return; }

            _selectedField = _dialogType.GetField("_selected", BindingFlags.NonPublic | BindingFlags.Instance);
            _use = _dialogType.GetMethod("UseSelectedSlot", BindingFlags.Public | BindingFlags.Instance);

            var harmony = new Harmony("com.dsh.petweaks.clothclick");
            harmony.Patch(target,
                prefix: new HarmonyMethod(typeof(ClothClick).GetMethod(nameof(Prefix), BindingFlags.NonPublic | BindingFlags.Static)),
                postfix: new HarmonyMethod(typeof(ClothClick).GetMethod(nameof(Postfix), BindingFlags.NonPublic | BindingFlags.Static)));
            _installed = true;
            Plugin.Log("clothclick installed (mode=" + mode + ")");
        }

        private static void Prefix(object __instance)
        {
            try { _prev = Plugin.CfgEnabled.Value ? _selectedField?.GetValue(__instance) : null; } catch { _prev = null; }
        }

        private static void Postfix(object __instance, object slot)
        {
            try
            {
                if (!Plugin.CfgEnabled.Value) return;
                if (slot == null || _use == null) return;
                Type st = slot.GetType();
                object kind = st.GetProperty("Kind")?.GetValue(slot);
                if (kind == null || !string.Equals(kind.ToString(), "Clothing", StringComparison.OrdinalIgnoreCase)) return;
                if (!(bool)(st.GetProperty("HasItem")?.GetValue(slot) ?? false)) return;

                bool single = string.Equals(Plugin.PanelClothClick.Value, "Single", StringComparison.OrdinalIgnoreCase);
                if (!single && !ReferenceEquals(_prev, slot)) return;   // 双击模式：第二次点才穿脱
                _use.Invoke(__instance, null);
            }
            catch (Exception e) { Plugin.Log("clothclick EXCEPTION " + e.GetType().Name + ": " + e.Message); }
        }
    }


    /// <summary>显示地图种子/坐标/街区，并把真实地图参数导出到日志。</summary>
    internal static class MapInfo
    {
        private static Type _cmType, _resolverType;
        private static object _cm, _resolver;
        private static Component _cmComp;
        private static float _cellSize = 60f;
        private static int _seed;
        private static bool _ready, _dumped;
        private static float _next;
        private static float _nextPosLog;
        private static float _nextGridDump;
        private static string _overlay = "";
        private static GUIStyle _style, _box;

        internal static void Tick()
        {
            if (!Plugin.ShowMapSeed.Value && !Plugin.DumpMapData.Value) return;
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 1.5f;

            try
            {
                // 场景切换 / 新开一局时 ChunkManager 会被销毁重建，这里检测并重置
                if (_cmComp == null)
                {
                    _ready = false;
                    _cm = null;
                    _resolver = null;
                    _dumped = false;
                }

                if (!_ready) TryInit();
                if (!_ready) return;

                // 每次刷新都重新读一次种子（不同局种子会变）
                try
                {
                    MethodInfo trySeed = _cmType.GetMethod("TryGetSessionSeed", BindingFlags.Public | BindingFlags.Instance);
                    if (trySeed != null)
                    {
                        object[] args = { 0 };
                        if ((bool)trySeed.Invoke(_cm, args))
                        {
                            int s = (int)args[0];
                            if (s != 0 && s != _seed)
                            {
                                _seed = s;
                                Plugin.Log($"mapinfo: seed -> {_seed}");
                            }
                        }
                    }
                }
                catch { }

                // 解析器也可能在换局时被重建
                try
                {
                    FieldInfo resF = _cmType.GetField("_spawnResolver", BindingFlags.NonPublic | BindingFlags.Instance);
                    object r2 = resF?.GetValue(_cm);
                    if (r2 != null && !ReferenceEquals(r2, _resolver))
                    {
                        _resolver = r2;
                        _resolverType = r2.GetType();
                    }
                }
                catch { }

                _overlay = BuildOverlay();
                if (Plugin.DumpMapData.Value && !_dumped) { _dumped = true; Dump(); }
            }
            catch (Exception e) { Plugin.Log("mapinfo EXCEPTION " + e.GetType().Name + ": " + e.Message); }
        }

        private static void TryInit()
        {
            if (_cmType == null) _cmType = Tweaks.FindType("ChunkManager");
            if (_cmType == null) return;
            if (_cm == null)
            {
                _cmComp = UnityEngine.Object.FindObjectOfType(_cmType) as Component;
                if (_cmComp == null) return;
                _cm = _cmComp;
            }

            FieldInfo cellF = _cmType.GetField("cellSize", BindingFlags.NonPublic | BindingFlags.Instance);
            if (cellF != null) _cellSize = (float)cellF.GetValue(_cm);
            if (_cellSize <= 1f) _cellSize = 60f;

            FieldInfo resF = _cmType.GetField("_spawnResolver", BindingFlags.NonPublic | BindingFlags.Instance);
            _resolver = resF?.GetValue(_cm);
            if (_resolver == null) { _cm = null; return; }
            _resolverType = _resolver.GetType();

            try
            {
                PropertyInfo to = _resolverType.GetProperty("TravelOffset");
                if (to != null) _travel = (Vector2Int)to.GetValue(_resolver, null);
            }
            catch { }

            MethodInfo trySeed = _cmType.GetMethod("TryGetSessionSeed", BindingFlags.Public | BindingFlags.Instance);
            if (trySeed != null)
            {
                object[] args = { 0 };
                if ((bool)trySeed.Invoke(_cm, args)) _seed = (int)args[0];
            }
            _ready = true;
            float bootCs = -1f;
            try
            {
                Type gs = Tweaks.FindType("GameSystemsBootstrapper");
                object gsi = gs?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                object v = gs?.GetProperty("CellSize")?.GetValue(gsi);
                if (v is float f) bootCs = f;
            }
            catch { }
            Plugin.Log($"mapinfo: seed={_seed} cellSize={_cellSize} bootstrapperCellSize={bootCs} travel=({_travel.x},{_travel.y}) catalog={_cmType.GetProperty("ActiveMapCatalogResource")?.GetValue(_cm)}");

        }

        private static Vector2Int PlayerCell()
        {
            Type invT = Tweaks.FindType("Features.Inventory.PlayerInventoryManager");
            object inv = invT?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            Component c = inv as Component;
            if (c == null) return new Vector2Int(int.MinValue, int.MinValue);
            Vector3 pos = c.transform.position;
            try
            {
                MethodInfo w2c = _cmType.GetMethod("WorldToCell", BindingFlags.Public | BindingFlags.Instance);
                if (w2c != null) return (Vector2Int)w2c.Invoke(_cm, new object[] { pos });
            }
            catch { }
            Vector3 lp = LogicalPos(pos);
            return new Vector2Int(Mathf.RoundToInt(lp.x / _cellSize), Mathf.RoundToInt(lp.z / _cellSize));
        }

        private static Vector2Int _travel = Vector2Int.zero;
        private static PropertyInfo _offsetProp;

        private static float ReadHudDistance(float fallback)
        {
            try
            {
                if (_dtType == null) _dtType = Tweaks.FindType("ExhibitionistDemo.UI.DistanceTracker");
                if (_dtType == null) return fallback;
                if (_dt == null) _dt = UnityEngine.Object.FindObjectOfType(_dtType);
                if (_dt == null) return fallback;
                MethodInfo m2 = _dtType.GetMethod("GetDistance2D", BindingFlags.Public | BindingFlags.Instance);
                if (m2 != null) return (float)m2.Invoke(_dt, null);
                MethodInfo m1 = _dtType.GetMethod("GetDistance", BindingFlags.Public | BindingFlags.Instance);
                if (m1 != null) return (float)m1.Invoke(_dt, null);
            }
            catch { }
            return fallback;
        }

        private static Type _dtType;
        private static object _dt;

        private static Vector3 LogicalPos(Vector3 unityPos)
        {
            if (_offsetProp == null)
            {
                Type wo = Tweaks.FindType("Features.RenderingPrecision.WorldOrigin");
                if (wo != null) _offsetProp = wo.GetProperty("Offset", BindingFlags.Public | BindingFlags.Static);
            }
            if (_offsetProp == null) return unityPos;
            return unityPos + (Vector3)_offsetProp.GetValue(null);
        }

        private static string BuildOverlay()
        {
            Vector2Int cell = PlayerCell();
            if (cell.x == int.MinValue) return $"SEED {_seed}    (等待角色生成)";

            string posText = "";
            Type invT2 = Tweaks.FindType("Features.Inventory.PlayerInventoryManager");
            object inv2 = invT2?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (inv2 is Component cc)
            {
                Vector3 wp = LogicalPos(cc.transform.position);
                float originDist = Mathf.Sqrt(wp.x * wp.x + wp.z * wp.z);
                float hudDist = ReadHudDistance(originDist);
                posText = $"\nPOS ({wp.x:0.0}, {wp.z:0.0})  HUD {hudDist:0} m  (原点 {originDist:0} m)";
            }

            string biome = "?", chunk = "?", size = "", rot = "";
            bool subway = false;
            try
            {
                MethodInfo biomeM = _resolverType.GetMethod("GetBiomeForCell");
                object b = biomeM?.Invoke(_resolver, new object[] { cell });
                if (b != null) biome = (string)b.GetType().GetField("biomeName")?.GetValue(b) ?? "?";

                MethodInfo resolve = _resolverType.GetMethod("Resolve", new[] { typeof(Vector2Int) });
                object entry = resolve?.Invoke(_resolver, new object[] { cell });
                if (entry != null)
                {
                    Type et = entry.GetType();
                    object cd = et.GetField("chunkData")?.GetValue(entry);
                    if (cd != null)
                    {
                        chunk = ((UnityEngine.Object)cd).name;
                        object gs = cd.GetType().GetField("gridSize")?.GetValue(cd);
                        if (gs is Vector2Int v) size = $" {v.x}x{v.y}";
                        object sw = cd.GetType().GetField("containsSubway")?.GetValue(cd);
                        subway = sw is bool b2 && b2;
                    }
                    object r = et.GetField("rotation")?.GetValue(entry);
                    if (r is int ri) rot = $" rot{ri * 90}°";
                }
            }
            catch (Exception e) { Plugin.Log("mapinfo cell EX " + e.Message); }

            string zoneText = "";
            try
            {
                MethodInfo rz = _resolverType.GetMethod("Resolve", new[] { typeof(Vector2Int) });
                object ez = rz?.Invoke(_resolver, new object[] { cell });
                if (ez != null)
                {
                    Type et = ez.GetType();
                    object cd = et.GetField("chunkData")?.GetValue(ez);
                    int[] zi = et.GetField("zoneIndices")?.GetValue(ez) as int[];
                    if (cd != null && zi != null)
                    {
                        var list = cd.GetType().GetField("availableZones")?.GetValue(cd) as System.Collections.IList;
                        var ns = new List<string>();
                        foreach (int ix in zi)
                            ns.Add((list != null && ix >= 0 && ix < list.Count && list[ix] != null)
                                ? ((UnityEngine.Object)list[ix]).name : "-");
                        zoneText = "\nZ: " + string.Join(" / ", ns);
                    }
                }
            }
            catch { }

            return $"SEED {_seed}\nCELL ({cell.x}, {cell.y})  [{biome}]\n{chunk}{size}{rot}{(subway ? "  [地铁]" : "")}{posText}{zoneText}";
        }

        private static void Dump()
        {
            try
            {
                FieldInfo chunksF = _cmType.GetField("availableChunks", BindingFlags.NonPublic | BindingFlags.Instance);
                var chunks = chunksF?.GetValue(_cm) as Array;
                Plugin.Log($"mapdata: seed={_seed} cellSize={_cellSize} chunks={chunks?.Length ?? 0}");
                if (chunks != null)
                {
                    foreach (object c in chunks)
                    {
                        if (c == null) continue;
                        Type ct = c.GetType();
                        object gs = ct.GetField("gridSize")?.GetValue(c);
                        string sz = gs is Vector2Int v ? $"{v.x}x{v.y}" : "?";
                        object w = ct.GetField("spawnWeight")?.GetValue(c);
                        object sub = ct.GetField("containsSubway")?.GetValue(c);
                        object allowed = ct.GetField("allowedBiomes")?.GetValue(c);
                        int allowedN = (allowed as System.Collections.IList)?.Count ?? 0;
                        Plugin.Log($"mapdata chunk | {((UnityEngine.Object)c).name} | {sz} | w={w} | subway={sub} | biomes={(allowedN == 0 ? "any" : allowedN.ToString())}");

                        // 区块 prefab 里的 Base* 子物体（决定槽位实际位置）
                        try
                        {
                            object tpl0 = ct.GetField("template")?.GetValue(c);
                            object prefab = tpl0?.GetType().GetField("chunkPrefab")?.GetValue(tpl0);
                            GameObject pg = prefab as GameObject;
                            if (pg != null)
                            {
                                var trs = pg.GetComponentsInChildren<Transform>(true);
                                var parts = new List<string>();
                                foreach (Transform tr in trs)
                                {
                                    if (tr == pg.transform) continue;
                                    if (tr.name.IndexOf("Base", StringComparison.OrdinalIgnoreCase) < 0) continue;
                                    Vector3 lp = tr.localPosition;
                                    Vector3 le = tr.localEulerAngles;
                                    parts.Add($"{tr.name}@({lp.x:0.#},{lp.y:0.#},{lp.z:0.#})r{le.y:0}");
                                }
                                Plugin.Log($"mapdata   bases | {((UnityEngine.Object)c).name} | {string.Join(", ", parts)}");
                            }
                        }
                        catch (Exception ex) { Plugin.Log("mapdata bases EX " + ex.Message); }

                        // 槽位顺序（zoneSlots）
                        try
                        {
                            object tpl = ct.GetField("template")?.GetValue(c);
                            var slots = tpl?.GetType().GetField("zoneSlots")?.GetValue(tpl) as System.Collections.IList;
                            if (slots != null)
                            {
                                var parts = new List<string>();
                                foreach (object s2 in slots)
                                {
                                    if (s2 == null) { parts.Add("?"); continue; }
                                    Type st2 = s2.GetType();
                                    object occ2 = st2.GetField("occupancy")?.GetValue(s2);
                                    object pos2 = st2.GetField("gridPosition")?.GetValue(s2);
                                    object rot2 = st2.GetField("rotation")?.GetValue(s2);
                                    parts.Add($"{occ2}@{pos2}r{rot2}");
                                }
                                Plugin.Log($"mapdata   slots | {((UnityEngine.Object)c).name} | {string.Join(", ", parts)}");
                            }
                        }
                        catch (Exception ex) { Plugin.Log("mapdata slots EX " + ex.Message); }

                        var czones = ct.GetField("availableZones")?.GetValue(c) as System.Collections.IList;
                        if (czones != null && czones.Count > 0)
                        {
                            foreach (object cz in czones)
                            {
                                if (cz == null) continue;
                                Type zt2 = cz.GetType();
                                object zw2 = zt2.GetField("spawnWeight")?.GetValue(cz);
                                object occ2 = zt2.GetField("occupancy")?.GetValue(cz);
                                object tcs2 = zt2.GetField("targetChunkSize")?.GetValue(cz);
                                Plugin.Log($"mapdata   chunkzone | {((UnityEngine.Object)cz).name} | w={zw2} | {tcs2}/{occ2} | {PoiInfo(cz)}");
                            }
                        }
                    }
                }

                FieldInfo biomeF = _cmType.GetField("biomeManager", BindingFlags.NonPublic | BindingFlags.Instance);
                object bm = biomeF?.GetValue(_cm);
                var biomes = bm?.GetType().GetField("biomes")?.GetValue(bm) as System.Collections.IList;
                Plugin.Log($"mapdata: biomes={biomes?.Count ?? 0}");
                if (biomes != null)
                {
                    foreach (object b in biomes)
                    {
                        if (b == null) continue;
                        Type bt = b.GetType();
                        string name = bt.GetField("biomeName")?.GetValue(b) as string;
                        object w = bt.GetField("baseSpawnWeight")?.GetValue(b);
                        object fb = bt.GetField("fallbackBiome")?.GetValue(b);
                        string fbName = fb != null ? ((UnityEngine.Object)fb).name : "null";
                        var ovList = bt.GetField("chunkWeightOverrides")?.GetValue(b) as System.Collections.IList;
                        Plugin.Log($"mapdata biome | {name} | baseWeight={w} | fallback={fbName} | overrides={ovList?.Count ?? 0}");

                        var zones = bt.GetField("availableZones")?.GetValue(b) as System.Collections.IList;
                        if (zones != null)
                        {
                            foreach (object z in zones)
                            {
                                if (z == null) continue;
                                Type zt = z.GetType();
                                string zn = zt.GetField("zoneName")?.GetValue(z) as string;
                                object zw = zt.GetField("spawnWeight")?.GetValue(z);
                                object occ = zt.GetField("occupancy")?.GetValue(z);
                                object tcs = zt.GetField("targetChunkSize")?.GetValue(z);
                                Plugin.Log($"mapdata   zone | {zn} | w={zw} | {tcs}/{occ} | {PoiInfo(z)}");
                            }
                        }
                        var ov = bt.GetField("chunkWeightOverrides")?.GetValue(b) as System.Collections.IList;
                        if (ov != null)
                        {
                            foreach (object o in ov)
                            {
                                if (o == null) continue;
                                Type ot = o.GetType();
                                object cd = ot.GetField("chunk")?.GetValue(o);
                                object mul = ot.GetField("multiplier")?.GetValue(o);
                                Plugin.Log($"mapdata   override | {(cd != null ? ((UnityEngine.Object)cd).name : "?")} x{mul}");
                            }
                        }
                    }
                }
            }
            catch (Exception e) { Plugin.Log("mapdata EXCEPTION " + e.GetType().Name + ": " + e.Message); }
        }

        private static Type _policyType;

        private static string PoiInfo(object zone)
        {
            try
            {
                if (_policyType == null) _policyType = Tweaks.FindType("ProceduralZonePolicy");
                if (_policyType == null) return "?";
                object special = _policyType.GetMethod("IsSpecialZone")?.Invoke(null, new[] { zone });
                object kind = _policyType.GetMethod("GetPoiKind")?.Invoke(null, new[] { zone });
                bool isSpecial = special is bool b && b;
                string k = kind?.ToString() ?? "None";
                if (!isSpecial && k == "None") return "-";
                return $"special={(isSpecial ? 1 : 0)} poi={k}";
            }
            catch { return "?"; }
        }

        internal static void Draw()
        {
            if (!Plugin.ShowMapSeed.Value || string.IsNullOrEmpty(_overlay)) return;
            if (_box == null)
            {
                _box = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14 };
                _style = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true, alignment = TextAnchor.UpperLeft, wordWrap = false };
                _style.normal.textColor = Color.white;
            }
            // 按文字实际大小包住内容
            var content = new GUIContent(_overlay);
            Vector2 size = _style.CalcSize(content);
            var rect = new Rect(10f, 10f, size.x + 18f, size.y + 12f);
            GUI.Box(rect, GUIContent.none, _box);
            GUI.Label(new Rect(rect.x + 9f, rect.y + 6f, size.x, size.y), _overlay, _style);
        }
    }


    /// <summary>临时探针：记录游戏内部每次地块抽取的哈希与结果。</summary>
    internal static class ZoneProbe
    {
        internal static bool Active;
        internal static int CellX, CellY;
        private static bool _installed;

        internal static void Install()
        {
            if (_installed) return;
            Type rt = Tweaks.FindType("ChunkSpawnResolver");
            if (rt == null) { Plugin.Log("zoneprobe: resolver not found"); return; }

            MethodInfo mi = null;
            foreach (MethodInfo m in rt.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (m.Name != "SelectZoneIndex") continue;
                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length == 6 && ps[2].ParameterType == typeof(uint)) { mi = m; break; }
            }
            if (mi == null) { Plugin.Log("zoneprobe: SelectZoneIndex not found"); return; }

            var harmony = new Harmony("com.dsh.petweaks.zoneprobe");
            harmony.Patch(mi,
                prefix: new HarmonyMethod(typeof(ZoneProbe).GetMethod(nameof(Prefix), BindingFlags.NonPublic | BindingFlags.Static)),
                postfix: new HarmonyMethod(typeof(ZoneProbe).GetMethod(nameof(Postfix), BindingFlags.NonPublic | BindingFlags.Static)));
            _installed = true;
            Plugin.Log("zoneprobe installed");
        }

        private static uint _h;
        private static void Prefix(object[] __args)
        {
            if (!Active) return;
            try { _h = (uint)__args[2]; } catch { }
        }

        private static void Postfix(ref int __result)
        {
            if (!Active) return;
            try { Plugin.Log($"zoneprobe cell=({CellX},{CellY}) h={_h} h%4={_h % 4} h2f={( _h & 0xFFFFFF) / 16777216f:0.000000} chosenIndex={__result}"); } catch { }
        }
    }

    /// <summary>
    /// 地图同步：把游戏实际生成的区块通过本地回环 HTTP 服务提供给查看器。
    /// 只监听 127.0.0.1 的动态端口：不需要管理员、不写注册表、不改防火墙。
    /// </summary>
    internal static class MapSync
    {
        private sealed class CellRec
        {
            public int X, Y, Gw, Gh, Rot;
            public string Chunk = "";
            public string[] Zones = Array.Empty<string>();
        }

        private static bool _initTried, _ready;
        private static Type _cmType;
        private static object _cm;
        private static Component _cmComp;
        private static FieldInfo _planField;
        private static PropertyInfo _validProp;
        private static FieldInfo _chunkF, _rotF, _zonesF;
        private static FieldInfo _spawnedField, _ciChunkF, _ciRotF, _ciZonesF;
        private static int _seed;
        private static int _px, _py;
        private static bool _hasPlayer;
        private static float _nextPoll;
        private static bool _autoOpened;

        private static readonly object _lock = new object();
        private static readonly List<CellRec> _ordered = new List<CellRec>();
        private static readonly Dictionary<long, int> _index = new Dictionary<long, int>();
        private static readonly Dictionary<int, List<int>> _verDelta = new Dictionary<int, List<int>>();
        private static int _version;
        private static int _trimBefore = 1;

        private static TcpListener _listener;
        private static Thread _thread;
        private static int _port;
        private static string _rootDir;

        private static long Key(int x, int y) { return ((long)x << 32) | (uint)y; }

        internal static void Tick()
        {
            if (!Plugin.MapSyncEnabled.Value) return;
            try
            {

                if (!_initTried) { _initTried = true; TryInit(); }
                if (!_ready) return;
                if (Time.unscaledTime < _nextPoll) return;
                float iv = Mathf.Max(0.2f, Plugin.MapSyncIntervalMs.Value / 1000f);
                _nextPoll = Time.unscaledTime + iv;

                Poll();

                if (!_autoOpened && _seed != 0 && Plugin.MapSyncAutoOpen.Value && _rootDir != null)
                {
                    _autoOpened = true;
                    Open();
                }
            }
            catch (Exception e) { Plugin.Log("mapsync tick EX " + e.Message); }
        }

        private static void TryInit()
        {
            try
            {
                _cmType = Tweaks.FindType("ChunkManager");
                if (_cmType == null) { Plugin.Log("mapsync: ChunkManager type not found"); return; }
                _planField = _cmType.GetField("_committedSpawnPlan", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_planField == null) { Plugin.Log("mapsync: _committedSpawnPlan not found"); return; }
                Type entryType = _planField.FieldType.GetGenericArguments()[1];
                _validProp = entryType.GetProperty("IsValid");
                _chunkF = entryType.GetField("chunkData");
                _rotF = entryType.GetField("rotation");
                _zonesF = entryType.GetField("zoneIndices");

                _spawnedField = _cmType.GetField("_spawnedChunks", BindingFlags.NonPublic | BindingFlags.Instance);
                Type ciType = _spawnedField != null ? _spawnedField.FieldType.GetGenericArguments()[1] : null;
                if (ciType != null)
                {
                    _ciChunkF = ciType.GetField("chunkData");
                    _ciRotF = ciType.GetField("rotation");
                    _ciZonesF = ciType.GetField("spawnedZones");
                }

                _rootDir = FindViewerDir();
                if (_rootDir == null) Plugin.Log("mapsync: map-viewer dir not found");

                _listener = new TcpListener(IPAddress.Loopback, 0);
                _listener.Start();
                _port = ((IPEndPoint)_listener.LocalEndpoint).Port;
                _thread = new Thread(ServeLoop) { IsBackground = true, Name = "PETweaks-MapSync" };
                _thread.Start();
                _ready = true;
                Plugin.Log("mapsync: http://127.0.0.1:" + _port + "/  viewerDir=" + (_rootDir ?? "null"));
            }
            catch (Exception e) { Plugin.Log("mapsync init EX " + e.GetType().Name + ": " + e.Message); }
        }

        private static string FindViewerDir()
        {
            try
            {
                string custom = Plugin.MapSyncViewerDir.Value;
                if (!string.IsNullOrEmpty(custom) && File.Exists(Path.Combine(custom, "index.html"))) return custom;
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string[] cands =
                {
                    Path.Combine(dir, "PETweaks", "map-viewer"),
                    Path.Combine(dir, "map-viewer"),
                    Path.GetFullPath(Path.Combine(dir, "..", "..", "map-viewer")),
                };
                foreach (string c in cands) if (File.Exists(Path.Combine(c, "index.html"))) return c;
            }
            catch { }
            return null;
        }

        internal static void Open()
        {
            if (!_ready) return;
            try { Application.OpenURL("http://127.0.0.1:" + _port + "/"); } catch { }
        }

        private static void Poll()
        {
            try
            {
                if (_cmComp == null)
                {
                    _cm = null;
                    _cmComp = UnityEngine.Object.FindObjectOfType(_cmType) as Component;
                    if (_cmComp == null) return;
                    _cm = _cmComp;
                }
                int nowSeed = ReadSeed();
                if (nowSeed != 0 && nowSeed != _seed)
                {
                    _seed = nowSeed;
                    ResetData();
                }
                UpdatePlayer();

                System.Collections.IDictionary plan = _planField.GetValue(_cm) as System.Collections.IDictionary;
                System.Collections.IDictionary spawned = _spawnedField != null
                    ? _spawnedField.GetValue(_cm) as System.Collections.IDictionary
                    : null;
                if ((plan == null || plan.Count == 0) && (spawned == null || spawned.Count == 0)) return;

                List<CellRec> fresh = new List<CellRec>();
                List<KeyValuePair<int, CellRec>> updates = new List<KeyValuePair<int, CellRec>>();
                HashSet<long> seen = new HashSet<long>();

                // 1) 实际生成的区块优先：能拿到"生成时被替换后"的真实地块
                if (spawned != null)
                {
                    foreach (System.Collections.DictionaryEntry de in spawned)
                    {
                        if (!(de.Key is Vector2Int scell)) continue;
                        long k = Key(scell.x, scell.y);
                        if (!seen.Add(k)) continue;
                        object planEntry = null;
                        if (plan != null && plan.Contains(scell)) planEntry = plan[scell];
                        CellRec rec = ReadSpawned(scell, de.Value, planEntry);
                        if (rec != null) MergeRec(rec, fresh, updates);
                    }
                }
                // 2) 计划兜底：前方已决定但还没生成的格子
                if (plan != null)
                {
                    foreach (System.Collections.DictionaryEntry de in plan)
                    {
                        if (!(de.Key is Vector2Int cell)) continue;
                        long k = Key(cell.x, cell.y);
                        if (!seen.Add(k)) continue;
                        CellRec rec = ReadEntry(cell, de.Value);
                        if (rec != null) MergeRec(rec, fresh, updates);
                    }
                }
                if (fresh.Count == 0 && updates.Count == 0) return;

                lock (_lock)
                {
                    _version++;
                    List<int> idxs = new List<int>(fresh.Count + updates.Count);
                    foreach (CellRec r in fresh)
                    {
                        _index[Key(r.X, r.Y)] = _ordered.Count;
                        _ordered.Add(r);
                        idxs.Add(_ordered.Count - 1);
                    }
                    foreach (KeyValuePair<int, CellRec> u in updates)
                    {
                        if (u.Key >= 0 && u.Key < _ordered.Count)
                        {
                            _ordered[u.Key] = u.Value;
                            idxs.Add(u.Key);
                        }
                    }
                    _verDelta[_version] = idxs;
                    if (_verDelta.Count > 512)
                    {
                        List<int> keys = new List<int>(_verDelta.Keys);
                        keys.Sort();
                        int drop = keys.Count - 512;
                        for (int i = 0; i < drop; i++) _verDelta.Remove(keys[i]);
                        _trimBefore = keys[drop];
                    }
                }
            }
            catch (Exception e) { Plugin.Log("mapsync poll EX " + e.Message); }
        }

        private static void MergeRec(CellRec rec, List<CellRec> fresh, List<KeyValuePair<int, CellRec>> updates)
        {
            long k = Key(rec.X, rec.Y);
            int idx;
            if (_index.TryGetValue(k, out idx))
            {
                if (idx < 0 || idx >= _ordered.Count) return;
                if (!SameRec(_ordered[idx], rec)) updates.Add(new KeyValuePair<int, CellRec>(idx, rec));
            }
            else fresh.Add(rec);
        }

        private static bool SameRec(CellRec a, CellRec b)
        {
            if (a == null || b == null) return false;
            if (a.X != b.X || a.Y != b.Y || a.Gw != b.Gw || a.Gh != b.Gh || a.Rot != b.Rot) return false;
            if (!string.Equals(a.Chunk, b.Chunk, StringComparison.Ordinal)) return false;
            if (a.Zones == null || b.Zones == null) return ReferenceEquals(a.Zones, b.Zones);
            if (a.Zones.Length != b.Zones.Length) return false;
            for (int i = 0; i < a.Zones.Length; i++)
                if (!string.Equals(a.Zones[i], b.Zones[i], StringComparison.Ordinal)) return false;
            return true;
        }

        private static CellRec ReadSpawned(Vector2Int cell, object inst, object planEntry)
        {
            if (inst == null) return null;
            object cd = _ciChunkF != null ? _ciChunkF.GetValue(inst) : null;
            UnityEngine.Object cdo = cd as UnityEngine.Object;
            if (cdo == null) return null;
            Vector2Int gs = ReadGridSize(cd);
            int rot;
            if (planEntry != null && _rotF != null) rot = (int)_rotF.GetValue(planEntry);
            else rot = QuatToDegrees(inst);
            int[] zi = (planEntry != null && _zonesF != null) ? _zonesF.GetValue(planEntry) as int[] : null;
            return new CellRec
            {
                X = cell.x,
                Y = cell.y,
                Gw = Mathf.Max(1, gs.x),
                Gh = Mathf.Max(1, gs.y),
                Rot = (rot >= 0 && rot <= 3) ? rot * 90 : rot,
                Chunk = cdo.name,
                Zones = ReadZoneNames(cd, zi, inst)
            };
        }

        private static int QuatToDegrees(object inst)
        {
            try
            {
                object q = _ciRotF != null ? _ciRotF.GetValue(inst) : null;
                if (q is Quaternion quat)
                {
                    int deg = Mathf.RoundToInt(quat.eulerAngles.y / 90f) * 90;
                    return ((deg % 360) + 360) % 360;
                }
            }
            catch { }
            return 0;
        }

        private static string[] ReadZoneNames(object chunkData, int[] zoneIndices, object inst)
        {
            System.Collections.IList spawnedZones = _ciZonesF != null ? _ciZonesF.GetValue(inst) as System.Collections.IList : null;
            System.Collections.IList pool = null;
            try
            {
                FieldInfo af = chunkData.GetType().GetField("availableZones");
                if (af != null) pool = af.GetValue(chunkData) as System.Collections.IList;
            }
            catch { }

            // 实际生成的 zone 数量与槽位一致时，优先用真实名字
            if (zoneIndices != null && spawnedZones != null && spawnedZones.Count == zoneIndices.Length && zoneIndices.Length > 0)
            {
                string[] arr = new string[spawnedZones.Count];
                for (int i = 0; i < arr.Length; i++)
                {
                    UnityEngine.GameObject go = spawnedZones[i] as UnityEngine.GameObject;
                    arr[i] = go != null ? StripZonePrefix(go.name) : "-";
                }
                return arr;
            }
            if (zoneIndices == null && spawnedZones != null && spawnedZones.Count > 0)
            {
                string[] arr = new string[spawnedZones.Count];
                for (int i = 0; i < arr.Length; i++)
                {
                    UnityEngine.GameObject go = spawnedZones[i] as UnityEngine.GameObject;
                    arr[i] = go != null ? StripZonePrefix(go.name) : "-";
                }
                return arr;
            }
            if (zoneIndices != null && pool != null)
            {
                string[] arr = new string[zoneIndices.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    int ix = zoneIndices[i];
                    arr[i] = (ix >= 0 && ix < pool.Count && pool[ix] is UnityEngine.Object zo) ? zo.name : "-";
                }
                return arr;
            }
            return Array.Empty<string>();
        }

        private static string StripZonePrefix(string name)
        {
            if (string.IsNullOrEmpty(name)) return "-";
            return name.StartsWith("Zone_", StringComparison.Ordinal) ? name.Substring(5) : name;
        }

        private static Vector2Int ReadGridSize(object chunkData)
        {
            try
            {
                FieldInfo gf = chunkData.GetType().GetField("gridSize");
                if (gf != null) return (Vector2Int)gf.GetValue(chunkData);
            }
            catch { }
            return Vector2Int.one;
        }

        private static int ReadSeed()
        {
            try
            {
                MethodInfo m = _cmType.GetMethod("TryGetSessionSeed", BindingFlags.Public | BindingFlags.Instance);
                if (m != null)
                {
                    object[] a = { 0 };
                    if ((bool)m.Invoke(_cm, a)) return (int)a[0];
                }
                FieldInfo rf = _cmType.GetField("_spawnResolver", BindingFlags.NonPublic | BindingFlags.Instance);
                object r = rf != null ? rf.GetValue(_cm) : null;
                if (r != null)
                {
                    FieldInfo sf = r.GetType().GetField("_sessionSeed", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (sf != null) return (int)sf.GetValue(r);
                }
            }
            catch { }
            return 0;
        }

        private static void ResetData()
        {
            lock (_lock)
            {
                _ordered.Clear();
                _index.Clear();
                _verDelta.Clear();
                _version = 0;
                _trimBefore = 1;
            }
            Plugin.Log("mapsync: seed changed -> reset, seed=" + _seed);
        }

        private static void UpdatePlayer()
        {
            try
            {
                Type invT = Tweaks.FindType("Features.Inventory.PlayerInventoryManager");
                object inv = invT != null ? invT.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) : null;
                Component comp = inv as Component;
                if (comp == null) return;
                Vector3 pos = comp.transform.position;
                MethodInfo w2c = _cmType.GetMethod("WorldToCell", BindingFlags.Public | BindingFlags.Instance);
                if (w2c == null) return;
                Vector2Int cell = (Vector2Int)w2c.Invoke(_cm, new object[] { pos });
                _px = cell.x;
                _py = cell.y;
                _hasPlayer = true;
            }
            catch { }
        }

        private static CellRec ReadEntry(Vector2Int cell, object entry)
        {
            if (entry == null) return null;
            if (_validProp != null && !(bool)_validProp.GetValue(entry)) return null;
            object cd = _chunkF != null ? _chunkF.GetValue(entry) : null;
            if (cd == null) return null;
            int rot = _rotF != null ? (int)_rotF.GetValue(entry) : 0;
            int[] zi = _zonesF != null ? _zonesF.GetValue(entry) as int[] : null;
            UnityEngine.Object cdo = cd as UnityEngine.Object;
            string cname = cdo != null ? cdo.name : "?";
            Vector2Int gs = Vector2Int.one;
            try
            {
                FieldInfo gf = cd.GetType().GetField("gridSize");
                if (gf != null) gs = (Vector2Int)gf.GetValue(cd);
            }
            catch { }
            List<string> zones = new List<string>();
            try
            {
                FieldInfo af = cd.GetType().GetField("availableZones");
                System.Collections.IList list = af != null ? af.GetValue(cd) as System.Collections.IList : null;
                if (list != null && zi != null)
                {
                    foreach (int ix in zi)
                    {
                        if (ix >= 0 && ix < list.Count && list[ix] is UnityEngine.Object zo) zones.Add(zo.name);
                        else zones.Add("-");
                    }
                }
            }
            catch { }
            return new CellRec
            {
                X = cell.x,
                Y = cell.y,
                Gw = Mathf.Max(1, gs.x),
                Gh = Mathf.Max(1, gs.y),
                Rot = (rot <= 3 ? rot * 90 : rot),
                Chunk = cname,
                Zones = zones.ToArray()
            };
        }

        private static void ServeLoop()
        {
            while (true)
            {
                TcpClient client = null;
                try
                {
                    client = _listener.AcceptTcpClient();
                    HandleClient(client);
                }
                catch (Exception)
                {
                    try { if (_listener == null) return; } catch { return; }
                    Thread.Sleep(20);
                }
                finally { try { if (client != null) client.Close(); } catch { } }
            }
        }

        private static void HandleClient(TcpClient client)
        {
            try
            {
                client.ReceiveTimeout = 3000;
                client.SendTimeout = 5000;
                NetworkStream ns = client.GetStream();
                byte[] buf = new byte[8192];
                int n = ns.Read(buf, 0, buf.Length);
                if (n <= 0) return;
                string req = Encoding.ASCII.GetString(buf, 0, n);
                string line = req.Split('\n')[0].Trim();
                string[] parts = line.Split(' ');
                if (parts.Length < 2) return;
                string path = parts[1];

                byte[] body;
                string ctype;
                if (path.StartsWith("/map.json", StringComparison.Ordinal))
                {
                    int v = 0;
                    int qi = path.IndexOf("?v=", StringComparison.Ordinal);
                    if (qi >= 0) int.TryParse(path.Substring(qi + 3), out v);
                    body = Encoding.UTF8.GetBytes(BuildJson(v));
                    ctype = "application/json; charset=utf-8";
                }
                else
                {
                    string file = MapPath(path);
                    if (file == null || !File.Exists(file))
                    {
                        body = Encoding.UTF8.GetBytes("{\"error\":\"not found\"}");
                        WriteResponse(ns, 404, "Not Found", "application/json; charset=utf-8", body, true);
                        return;
                    }
                    body = File.ReadAllBytes(file);
                    ctype = ContentType(file);
                }
                WriteResponse(ns, 200, "OK", ctype, body, path.StartsWith("/map.json", StringComparison.Ordinal));
            }
            catch { }
        }

        private static void WriteResponse(NetworkStream ns, int code, string reason, string ctype, byte[] body, bool noStore)
        {
            StringBuilder head = new StringBuilder(256);
            head.Append("HTTP/1.1 ").Append(code).Append(' ').Append(reason).Append("\r\n");
            head.Append("Content-Type: ").Append(ctype).Append("\r\n");
            head.Append("Content-Length: ").Append(body.Length).Append("\r\n");
            head.Append(noStore ? "Cache-Control: no-store\r\n" : "Cache-Control: max-age=3600\r\n");
            head.Append("Access-Control-Allow-Origin: *\r\n");
            head.Append("Access-Control-Allow-Private-Network: true\r\n");
            head.Append("Connection: close\r\n\r\n");
            byte[] hb = Encoding.ASCII.GetBytes(head.ToString());
            ns.Write(hb, 0, hb.Length);
            if (body.Length > 0) ns.Write(body, 0, body.Length);
            ns.Flush();
        }

        private static string MapPath(string path)
        {
            if (_rootDir == null) return null;
            int q = path.IndexOf('?');
            if (q >= 0) path = path.Substring(0, q);
            string rel = path.TrimStart('/');
            if (rel.Length == 0) rel = "index.html";
            rel = rel.Replace('/', Path.DirectorySeparatorChar);
            if (rel.Contains("..")) return null;
            return Path.Combine(_rootDir, rel);
        }

        private static string ContentType(string file)
        {
            switch (Path.GetExtension(file).ToLowerInvariant())
            {
                case ".html":
                case ".htm": return "text/html; charset=utf-8";
                case ".js": return "application/javascript; charset=utf-8";
                case ".css": return "text/css; charset=utf-8";
                case ".json": return "application/json; charset=utf-8";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".svg": return "image/svg+xml";
                case ".ico": return "image/x-icon";
                default: return "application/octet-stream";
            }
        }

        private static string BuildJson(int clientV)
        {
            List<CellRec> cells;
            int ver;
            lock (_lock)
            {
                ver = _version;
                cells = new List<CellRec>();
                if (clientV > 0 && clientV >= _trimBefore && clientV < ver)
                {
                    HashSet<int> seen = new HashSet<int>();
                    for (int v = clientV + 1; v <= ver; v++)
                    {
                        List<int> list;
                        if (_verDelta.TryGetValue(v, out list))
                        {
                            foreach (int i in list)
                            {
                                if (seen.Add(i) && i < _ordered.Count) cells.Add(_ordered[i]);
                            }
                        }
                    }
                }
                else if (clientV != ver)
                {
                    cells.AddRange(_ordered);
                }
            }
            StringBuilder sb = new StringBuilder(1024 + cells.Count * 80);
            sb.Append("{\"v\":").Append(ver).Append(",\"seed\":").Append(_seed);
            if (_hasPlayer) sb.Append(",\"p\":[").Append(_px).Append(',').Append(_py).Append(']');
            sb.Append(",\"cells\":[");
            for (int i = 0; i < cells.Count; i++)
            {
                if (i > 0) sb.Append(',');
                CellRec c = cells[i];
                sb.Append("{\"x\":").Append(c.X).Append(",\"y\":").Append(c.Y)
                  .Append(",\"c\":\"").Append(Esc(c.Chunk))
                  .Append("\",\"w\":").Append(c.Gw).Append(",\"h\":").Append(c.Gh)
                  .Append(",\"r\":").Append(c.Rot).Append(",\"z\":[");
                for (int j = 0; j < c.Zones.Length; j++)
                {
                    if (j > 0) sb.Append(',');
                    sb.Append('"').Append(Esc(c.Zones[j])).Append('"');
                }
                sb.Append("]}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private static string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            StringBuilder sb = null;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == '"' || ch == '\\')
                {
                    if (sb == null) { sb = new StringBuilder(s.Length + 8); if (i > 0) sb.Append(s, 0, i); }
                    sb.Append('\\').Append(ch);
                }
                else if (ch < ' ')
                {
                    if (sb == null) { sb = new StringBuilder(s.Length + 8); if (i > 0) sb.Append(s, 0, i); }
                    sb.Append(' ');
                }
                else if (sb != null) sb.Append(ch);
            }
            return sb == null ? s : sb.ToString();
        }

        internal static void Shutdown()
        {
            try { if (_listener != null) _listener.Stop(); } catch { }
            _listener = null;
        }
    }

}
