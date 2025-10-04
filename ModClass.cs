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
        public double skill_mult = 0.8;
        public bool Compass_Mode = false;
        public int Startframes = 150;
    }
    public class Floor_is_lava : Mod, IGlobalSettings<SaveData>
    {
        public double FramesOnFloor = 0;
        public bool ToggleButtonInsideMenu => true;

        public static SaveData SaveData { get; set; } = new();
        new public string GetName() => "Floor is Lava";
        public override string GetVersion() => "1.1.0.0";

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
        public string FloorFrames()
        {
            return $"{FramesOnFloor}/{FramesLimit()}";
        }
        public string NoCompass = "";
        public string Final()
        {
            if (!SaveData.Compass_Mode)
            {
                return FloorFrames();
            }
            else
            {
                if (PlayerData.instance.equippedCharm_2)
                {
                    return FloorFrames();
                }
                else
                {
                    return NoCompass;
                }
            }
        }
        public int FramesLimit()
        {
            int result;
            int cnt = 0;
            if (PlayerData.instance.hasDash)
            {
                cnt += 2;
            }
            if (PlayerData.instance.hasSuperDash)
            {
                cnt++;
            }
            if (PlayerData.instance.hasWalljump)
            {
                cnt+=2;
            }
            if (PlayerData.instance.hasDoubleJump)
            {
                cnt+=2;
            }
            if (PlayerData.instance.hasShadowDash)
            {
                cnt++;
            }
            if (PlayerData.instance.hasAcidArmour)
            {
                cnt++;
            }
            result = (int)(Math.Pow(SaveData.skill_mult, cnt) * SaveData.Startframes);
            return result;
        }

        LayoutRoot layout;
        TextObject text;

        private void HeroController_SceneInit(On.HeroController.orig_SceneInit orig, HeroController self)
        {
            orig(self);

            Log("Creating FTC");
            layout = new LayoutRoot(false, "Floor is Lava");
            text = new(layout)
            {
                Text = Final(),
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
            text.Text = Final();

            ModHooks_HeroFixedUpdateHook();
        }

        private void ModHooks_HeroFixedUpdateHook()
        {   
            if (FramesOnFloor > FramesLimit())
            {
                HurtHero(1);    
                FramesOnFloor = 0;
            }
        }

        public void OnLoadGlobal(SaveData s)
        {
            SaveData = s;
        }
        public SaveData OnSaveGlobal() => SaveData;
    }
}
