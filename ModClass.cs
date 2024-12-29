using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Satchel;
using Satchel.BetterMenus;
using GlobalEnums;
using Satchel.Reflected;
using MagicUI;
using MagicUI.Elements;
using MagicUI.Core;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Floor_is_lava
{
    public class SaveData
    {
        public int Divisor_of_frames_in_kings_pass_for_frame_limit = 2;
        public double LowerNumber = 0.5;
        public bool Compass_Mode = false;
    }
    public class SaveData2
    {
        public bool HasDash = false;
        public bool HasCDash = false;
        public bool HasClaw = false;
        public bool HasWings = false;
        public bool HasSDash = false;
        public bool HasIsma = false;
        public bool Dirthmouth = false;
        public double FramesLimit = 1000000.00;
        public double Minus_frames_On_floor_per_skill = 250.00;
    }
    public class Floor_is_lava : Mod, IGlobalSettings<SaveData>, ILocalSettings<SaveData2>
    {
        public double FramesOnFloor = 0;
       
        public bool ToggleButtonInsideMenu => true;

        public static SaveData SaveData { get; set; } = new();
        public static SaveData2 SaveData2 { get; set; } = new();
        new public string GetName() => "Floor is Lava";
        public override string GetVersion() => "1.0.7.2";

        

        public override void Initialize()
        {
            // don't use update, as update doesn't always run at 60/240/400/144 fps, it can vary, while fixedupdate **always** runs at 50Hz
            // On.HeroController.Update += HeroController_Update;
            On.HeroController.FixedUpdate += HeroController_FixedUpdate;
            // not needed, we'll call it from within our hook directly above, also changed to fixedupdate
            // ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
            On.HeroController.SceneInit += HeroController_SceneInit;
            ModHooks.AfterPlayerDeadHook += ModHooks_AfterPlayerDeadHook;
        }

        private void ModHooks_AfterPlayerDeadHook()
        {
            FramesOnFloor = 0;
        }

        public void Unload()
        {
            Log("Unloading FTC");

            // On.HeroController.Update -= HeroController_Update;
            On.HeroController.FixedUpdate -= HeroController_FixedUpdate;
            // ModHooks.HeroUpdateHook -= ModHooks_HeroUpdateHook;
            On.HeroController.SceneInit -= HeroController_SceneInit;
            ModHooks.AfterPlayerDeadHook -= ModHooks_AfterPlayerDeadHook;

            text.Destroy();
            layout.Destroy();
        }

        // idea: instead of `{FOF} of {FL}`, maybe `{FOF}/{FL}` is better
        public string FloorFrames => $"{FramesOnFloor} of {FrameInt}";
        public string NoCompass => $"";
        public string Final {get; set;} = $"gaos4325";
        public int FrameInt = 1000000000;


        LayoutRoot layout;
        TextObject text;

        private void HeroController_SceneInit(On.HeroController.orig_SceneInit orig, HeroController self)
        {
            orig(self);

            Log("Creating FTC");
            layout = new LayoutRoot(false, "Floor is Lava");
            text = new(layout)
            {
                Text = Final,
                Font = UI.TrajanNormal,
                FontSize = 25
            };
            text.GameObject.transform.position += new Vector3(15, -15);
        }

        private void HurtHero(int dmg)
        {
            HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.other, dmg, 1);
        }

        private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            orig(self);

            if (self.CheckTouchingGround() && self.acceptingInput && !GameManager.instance.isPaused)
            {
                FramesOnFloor++;
            }
            text.Text = Final;

            ModHooks_HeroFixedUpdateHook();
        }

        private void ModHooks_HeroFixedUpdateHook()
        {   
            if (SaveData.Compass_Mode == true)
            {
                if (PlayerData.instance.equippedCharm_2 == true)
                    {
                        Final = FloorFrames;
                        Log(Final);
                    }
                else
                    {
                        Final = NoCompass;
                        Log(Final);
                    }
            }
            else
            {
                Final = FloorFrames;
            }
            if (PlayerData.instance.atBench == true)
            {
                FramesOnFloor = 0;
            }
            FrameInt = Convert.ToInt32(SaveData2.FramesLimit);
            if (PlayerData.instance.visitedDirtmouth == true)
            {
                if (SaveData2.Dirthmouth == false)
                {
                    SaveData2.FramesLimit = FramesOnFloor / SaveData.Divisor_of_frames_in_kings_pass_for_frame_limit;
                    FramesOnFloor = 0;

                    SaveData2.Dirthmouth = true;
                }
                if (FramesOnFloor > SaveData2.FramesLimit)
                {
                    HurtHero(1);
                    FramesOnFloor = 0;
                }
                if (PlayerData.instance.atBench == true)
                {
                    FramesOnFloor = 0;

                }
                if (SaveData2.HasDash == false)
                {
                    if (PlayerData.instance.hasDash == true)
                    {
                        SaveData2.FramesLimit = SaveData2.FramesLimit * SaveData.LowerNumber;
                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasDash = true;
                    }
                }

                if (SaveData2.HasWings == false)
                {
                    if (PlayerData.instance.hasDoubleJump == true)
                    {
                        SaveData2.FramesLimit = SaveData2.FramesLimit * SaveData.LowerNumber;
                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasWings = true;
                    }
                }
                if (SaveData2.HasSDash == false)
                {
                    if (PlayerData.instance.hasShadowDash == true)
                    {
                        SaveData2.FramesLimit = SaveData2.FramesLimit * SaveData.LowerNumber;

                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasSDash = true;
                    }
                }
                if (SaveData2.HasClaw == false)
                {
                    if (PlayerData.instance.hasWalljump == true)
                    {
                        SaveData2.FramesLimit = SaveData2.FramesLimit * SaveData.LowerNumber;
                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasClaw = true;
                    }
                }
                if (SaveData2.HasCDash == false)
                {
                    if (PlayerData.instance.hasSuperDash == true)
                    {
                        SaveData2.FramesLimit = SaveData2.FramesLimit * SaveData.LowerNumber;
                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasCDash = true;
                    }
                }
            }
        }

        public void OnLoadGlobal(SaveData s)
        {
            SaveData = s;
        }
        public SaveData OnSaveGlobal() => SaveData;
        public void OnLoadLocal(SaveData2 s)
        {
            SaveData2 = s;
        }
        public SaveData2 OnSaveLocal() => SaveData2;
    }
}
