using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace PETweaks
{
    [BepInPlugin("com.dsh.petweaks", "Project Exhibitionist Tweaks", "1.6.0")]
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

            Logger.LogInfo("PETweaks v1.6.0 loaded (persistent runner attached)");
        }

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
            try { Inertia.Tick(Plugin.InertiaKick.Value); } catch { }
            try { ClothSlots.Tick(Plugin.ExtraClothSlots.Value); } catch { }
            try { FaceCam.Tick(Plugin.FaceCamOffsetY.Value); } catch { }
            try { SideRandom.Tick(Plugin.SideRandomInterval.Value, Plugin.SideRandomize.Value, Plugin.BodyAsymmetryMul.Value); } catch { }

            if (Time.unscaledTime < _nextApply) return;
            _nextApply = Time.unscaledTime + 5f;
            try { Plugin.Apply("tick"); } catch { }
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

        internal static void Tick(int extra)
        {
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

        internal static void Tick(float offsetY)
        {
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
            try { _prev = _selectedField?.GetValue(__instance); } catch { _prev = null; }
        }

        private static void Postfix(object __instance, object slot)
        {
            try
            {
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

}
