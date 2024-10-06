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
        public int Divisor_of_frames_in_kings_pass_for_frame_limit = 8;
        public int Divisor_of_frames_from_limit_which_are_taken_from_the_limit_per_skill = 8;
        



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
        public int FramesLimit = 10000000;
        
    }
    public class Floor_is_lava : Mod, IGlobalSettings<SaveData>, ILocalSettings<SaveData2>
    {
        public int FramesOnFloor = 0;



        public int FrameOnFloor = 0;

        public bool ToggleButtonInsideMenu => true;

        public static SaveData SaveData { get; set; } = new();
        public static SaveData2 SaveData2 { get; set; } = new();
        new public string GetName() => "Floor is Lava";
        public override string GetVersion() => "1.0.5.0";

        

        

        public int Minus_frames_On_floor_per_skill = 250;


        public override void Initialize()
        {
             
            On.HeroController.Update += HeroController_Update;
            ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
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

            On.HeroController.SceneInit += HeroController_SceneInit;
            On.HeroController.Update -= HeroController_Update;
            text.Destroy();
            layout.Destroy();
        }

        
        public string FloorFrames => $"{FramesOnFloor} of {SaveData2.FramesLimit}";

        LayoutRoot layout;
        TextObject text;

        

        private void HeroController_SceneInit(On.HeroController.orig_SceneInit orig, HeroController self)
        {
            orig(self);

            Log("Creating FTC");
            layout = new LayoutRoot(false, "Floor is Lava");

            text = new(layout) 
            {
                
                Text = FloorFrames,
                Font = UI.TrajanNormal,
                FontSize = 25
            };
            text.GameObject.transform.position += new Vector3(15, -15);
        }



        
        private void HurtHero(int dmg)
        {
            HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.other, dmg, 1);
        }

        
        
        private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);

            if (self.CheckTouchingGround() && self.acceptingInput && !GameManager.instance.isPaused)
            {
                
                
                    FramesOnFloor = FramesOnFloor + 1;
                
            }
            text.Text = FloorFrames;  
        }

        
        private void ModHooks_HeroUpdateHook()
        {
            
            
            if (PlayerData.instance.visitedDirtmouth == true)
            {
                if (SaveData2.Dirthmouth == false)
                {
                    SaveData2.FramesLimit = FramesOnFloor / SaveData.Divisor_of_frames_in_kings_pass_for_frame_limit;
                    Minus_frames_On_floor_per_skill = SaveData2.FramesLimit / SaveData.Divisor_of_frames_from_limit_which_are_taken_from_the_limit_per_skill;
                    SaveData2.Dirthmouth = true;
                }
            }

            if (PlayerData.instance.visitedCrossroads == true)
            {
                
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
                        SaveData2.  FramesLimit = SaveData2.FramesLimit - Minus_frames_On_floor_per_skill;
                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasDash = true;
                    }
                }
                if (SaveData2.HasWings == false)
                {
                    if (PlayerData.instance.hasDoubleJump == true)
                    {
                        SaveData2.FramesLimit = SaveData2.FramesLimit - Minus_frames_On_floor_per_skill;
                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasWings = true;
                    }
                }
                if (SaveData2.HasSDash == false)
                {
                    if (PlayerData.instance.hasShadowDash == true)
                    {
                        SaveData2.FramesLimit =   SaveData2.FramesLimit - Minus_frames_On_floor_per_skill;
                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasSDash = true;
                    }
                }
                if (SaveData2.HasClaw == false)
                {
                    if (PlayerData.instance.hasWalljump == true)
                    {
                        SaveData2.FramesLimit = SaveData2.FramesLimit - Minus_frames_On_floor_per_skill;
                        Log($"Limit set to {SaveData2.FramesLimit}");
                        SaveData2.HasClaw = true;
                    }
                }
                if (SaveData2.HasCDash == false)
                {
                    if (PlayerData.instance.hasSuperDash == true)
                    {
                        SaveData2.FramesLimit = SaveData2.FramesLimit - Minus_frames_On_floor_per_skill;
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
