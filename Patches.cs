using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace PhoenotopiaTweaks
{
    internal class Patches
    {
        public static void Test()
        {
            if (Input.GetKeyDown(KeyCode.Z))
                Util.Message("ZZZZZZZZZZZ");
        }

        [HarmonyPatch(typeof(OpeningMenuLogic), "_GoToState")]
        class OpeningMenuLogic_GoToState_Patch
        {
            public static void Prefix(OpeningMenuLogic __instance, ref OpeningMenuLogic.STATE new_state)
            {
                if (!Config.quickStart.Value)
                    return;

                //Main.logger.LogMessage("OpeningMenuLogic _GoToState " + new_state);
                if (new_state == OpeningMenuLogic.STATE.CAPE_COSMIC_LOGO_1)
                    new_state = OpeningMenuLogic.STATE.FILE_SELECT_1;
            }
        }

        [HarmonyPatch(typeof(HeartHudLogic), "Update")]
        class HeartHudLogic_ActionShake_Patch
        {
            public static bool Prefix(HeartHudLogic __instance)
            {
                //Test();
                return !Config.hudTweaks.Value;
            }
        }

        static bool selecting = false;

        [HarmonyPatch(typeof(ThingWheelLogic), "Update")]
        class ThingWheelLogic_Update_Patch
        {
            public static bool Prefix(ThingWheelLogic __instance)
            {
                if (!Config.quickSelect.Value)
                    return true;

                if (PT2.menu.is_active)
                    return false;

                __instance._active_time -= Time.deltaTime;
                bool flag1 = !PT2.director.is_directing && !PT2.gale_script.QueryStatus(GALE_QUERY_STATUS.IS_COOKING) && PT2.director.CanPlayerPause();
                bool flag2 = !PT2.director.control.IsControlStickDeadZone(0.4f, false) && flag1;
                if (PT2.game_paused && PT2.director._is_SELECT_PAUSE)
                    return false;

                if (PT2.gale_script.QueryStatus(GALE_QUERY_STATUS.IS_FISHING_FIGHTING))
                    flag2 = false;

                if (flag2)
                {
                    if (flag1 && PT2.director.control.RIGHT_STICK_CLICK)
                        __instance._intent_to_tool_hud_equip = true;

                    __instance._active_time = 0.5f;
                    __instance._DetectRightStickAndWriteToIndex(false);
                    if (PT2.director.control.IsControlStickDeadZone(0.9f, false))
                    {
                        selecting = true;
                        __instance._DetectRightStickAndWriteToIndex(true);
                        //Util.Message(" 0.9 right_stick_index " + __instance._right_stick_index);
                    }
                    __instance._Animate_Diamond(false, true);
                    __instance._Animate_Sprites(false, false);
                }
                else
                    __instance._Animate_Diamond(expand_at_edges: true);

                if (__instance._transform.localPosition.x != 0)
                    __instance._transform.localPosition = new Vector3(0f, 0f, 10f);

                if (__instance._active_time <= 0.2f)
                {
                    if (__instance._active_time <= 0)
                    {
                        if (__instance._transform.localScale.x == 0)
                            return false;
                        __instance._transform.localScale = Vector3.zero;
                    }
                    else
                        __instance._transform.localScale = __instance._active_time / 0.2f * __instance.GAMEPLAY_SCALE * Vector3.one;
                }
                else
                {
                    if (__instance._transform.localScale.x != __instance.GAMEPLAY_SCALE)
                        __instance._transform.localScale = __instance.GAMEPLAY_SCALE * Vector3.one;
                    //__instance._DetectRightStickAndWriteToIndex(true);
                    if (selecting && __instance._right_stick_index > -1 && PT2.director.control.IsControlStickDeadZone(0.1f, false))
                    {
                        //Util.Message("selecting " + __instance._right_stick_index);
                        EquipSelected(__instance);
                    }
                }
                return false;
            }

            private static void EquipSelected(ThingWheelLogic thingWheelLogic)
            {
                thingWheelLogic._intent_to_tool_hud_equip = false;
                thingWheelLogic.HandleEquipOnToolHudEvent(true);
                thingWheelLogic.JuiceTweenDiamond(true);
                selecting = false;
            }
        }


        [HarmonyPatch(typeof(GaleLogicOne), "_STATE_OcarinaPlaying")]
        class ThingWheelLogic_STATE_OcarinaPlaying_Patch
        {
            public static bool Prefix(GaleLogicOne __instance)
            {
                if (Config.stupidFlashes.Value)
                    return true;

                __instance._JavelinOrGunMovement();
                if (PT2.save_file.gales_using_item_ID != PT2.save_file.tool_hud_ID)
                {
                    __instance._MoveCancelations(true);
                    __instance._GoToState(GaleLogicOne.GALE_STATE.OCARINA_PUTAWAY);
                }
                else
                {
                    if (__instance._control.IsControlStickDeadZone(0.4f))
                        __instance._anim.SetInteger(__instance.anim_state, !__instance.DEBUG_PLAYING_FLUTE ? 104 : 114);
                    else if (Mathf.Abs(__instance._control.LEFT_RIGHT_AXIS) > Mathf.Abs(__instance._control.UP_DOWN_AXIS))
                    {
                        if (__instance._control.LEFT_RIGHT_AXIS < 0)
                            __instance._anim.SetInteger(__instance.anim_state, __instance._transform.localScale.x <= 0 ? (!__instance.DEBUG_PLAYING_FLUTE ? 101 : 111) : (!__instance.DEBUG_PLAYING_FLUTE ? 100 : 110));
                        else
                            __instance._anim.SetInteger(__instance.anim_state, __instance._transform.localScale.x <= 0 ? (!__instance.DEBUG_PLAYING_FLUTE ? 100 : 110) : (!__instance.DEBUG_PLAYING_FLUTE ? 101 : 111));
                    }
                    else if (__instance._control.UP_DOWN_AXIS < 0)
                        __instance._anim.SetInteger(__instance.anim_state, !__instance.DEBUG_PLAYING_FLUTE ? 103 : 113);
                    else
                        __instance._anim.SetInteger(__instance.anim_state, !__instance.DEBUG_PLAYING_FLUTE ? 102 : 112);
                    if (__instance._control.TOOL_HELD)
                    {
                        if (__instance._is_sprinting)
                            return false;

                        __instance._is_sprinting = true;
                        PT2.juicer.J_ScaleSineWobble(__instance._sprite_transform, 0.3f, 0.05f);
                        float pitch = 1f;
                        if (__instance._control.SPRINT_HELD)
                            pitch = 0.94388f;
                        if (__instance._control.CROUCH_HELD)
                            pitch = 1.05946f;
                        //Vector3 vector2 = GL.M_AngleToVector2(70f + 40f * UnityEngine.Random.value, 8f + 4f * UnityEngine.Random.value);
                        if (__instance._control.IsControlStickDeadZone(0.4f))
                        {
                            __instance._anim.SetInteger(__instance.anim_state, !__instance.DEBUG_PLAYING_FLUTE ? 104 : 114);
                            PT2.item_gen.EmitMusicalNote(__instance._transform.position, 'N', pitch: pitch, is_spheralis: !__instance.DEBUG_PLAYING_FLUTE);
                            __instance._PlayedOcarinaNote('N');
                        }
                        else if (Mathf.Abs(__instance._control.LEFT_RIGHT_AXIS) > Mathf.Abs(__instance._control.UP_DOWN_AXIS))
                        {
                            if (__instance._control.LEFT_RIGHT_AXIS < 0)
                            {
                                __instance._anim.SetInteger(__instance.anim_state, __instance._transform.localScale.x <= 0 ? (!__instance.DEBUG_PLAYING_FLUTE ? 101 : 111) : (!__instance.DEBUG_PLAYING_FLUTE ? 100 : 110));
                                PT2.item_gen.EmitMusicalNote(__instance._transform.position, 'L', pitch: pitch, is_spheralis: !__instance.DEBUG_PLAYING_FLUTE);
                                __instance._PlayedOcarinaNote('L');
                            }
                            else
                            {
                                __instance._anim.SetInteger(__instance.anim_state, __instance._transform.localScale.x <= 0 ? (!__instance.DEBUG_PLAYING_FLUTE ? 100 : 110) : (!__instance.DEBUG_PLAYING_FLUTE ? 101 : 111));
                                PT2.item_gen.EmitMusicalNote(__instance._transform.position, 'R', pitch: pitch, is_spheralis: !__instance.DEBUG_PLAYING_FLUTE);
                                __instance._PlayedOcarinaNote('R');
                            }
                        }
                        else if (__instance._control.UP_DOWN_AXIS < 0)
                        {
                            __instance._anim.SetInteger(__instance.anim_state, !__instance.DEBUG_PLAYING_FLUTE ? 103 : 113);
                            PT2.item_gen.EmitMusicalNote(__instance._transform.position, 'D', pitch: pitch, is_spheralis: !__instance.DEBUG_PLAYING_FLUTE);
                            __instance._PlayedOcarinaNote('D');
                        }
                        else
                        {
                            __instance._anim.SetInteger(__instance.anim_state, !__instance.DEBUG_PLAYING_FLUTE ? 102 : 112);
                            PT2.item_gen.EmitMusicalNote(__instance._transform.position, 'U', pitch: pitch, is_spheralis: !__instance.DEBUG_PLAYING_FLUTE);
                            __instance._PlayedOcarinaNote('U');
                        }
                        if (__instance.DEBUG_PLAYING_FLUTE)
                            return false;
                        //PT2.p_g.NEWTYPE_GraphicBurst(false, 42, __instance._transform.position, Color.white, 1f, 0.0f).SetMotion_CustomTweens(Vector3.zero, 0.85f, 3f, 0.0f, 0.5f * Vector3.one, 1.5f * Vector3.one, 0.0f, Vector3.zero, 1);
                        PT2.sound_g.PlayGlobalUncommonSfx("waker", 1f, src_index: 2);
                    }
                    else
                        __instance._is_sprinting = false;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveCircleLogic), "InformStringEvent")]
        class SaveCircleLogic_InformStringEvent_Patch
        {
            public static bool Prefix(SaveCircleLogic __instance, string message)
            {
                if (Config.stupidFlashes.Value)
                    return true;

                switch (message)
                {
                    case "save_anim":
                        PT2.sound_g.PlayCommonSfx(248, Vector3.zero, pan_level: 0f, pitch_level: 1.3f, initial_delay: 0.15f);
                        __instance.animator.SetInteger(GL.anim, 51);
                        __instance._hover_up_down = false;
                        break;
                    case "fx1":
                        PT2.sound_g.PlayGlobalCommonSfx(190, 1f, src_index: 2);
                        break;
                    case "fx2":
                        //PT2.screen_covers.HazeScreen("ffffff", 1f, 0.75f, 0f);
                        PT2.director.StartCommand(DB.GetLine("SAVE_GAME_TEXT"), null);
                        break;
                    case "spin_loop":
                        __instance.animator.SetInteger(GL.anim, 50);
                        __instance._hover_up_down = true;
                        break;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Hurtbox), "DoDamageToHurtbox")]
        class Hurtbox_DoDamageToHurtbox_Patch
        {
            public static bool Prefix(Hurtbox __instance, Hitbox.AttackStat attack_stats, Vector3 hitbox_position, ref Hurtbox.AttackResult __result)
            {
                if (Config.playerDamageMultiplier.Value == 1 && Config.enemyDamageMultiplier.Value == 1)
                    return true;

                if (attack_stats.activation_ID == __instance._last_attack_act_id && Time.time - __instance._last_attack_time < __instance.allowance_time_between_atks)
                {
                    __result = new Hurtbox.AttackResult() { atk_status = Hitbox.ATK_STATUS.NONE };
                    return false;
                }
                __instance._last_attack_time = Time.time;
                __instance._last_attack_act_id = attack_stats.activation_ID;
                int finalDamage = !attack_stats.HasAttackEffect(Hitbox.ATK_EFFECT.IGNORE_DEF) ? attack_stats.damage_amount - __instance.defense : attack_stats.damage_amount;
                if (finalDamage < 1)
                    finalDamage = !attack_stats.IsExclusivelyThisAtkEffect(Hitbox.ATK_EFFECT.WIND_PUSH) ? 1 : 0;

                finalDamage = ModifyDamage(finalDamage, __instance);
                Vector3 collision_point;
                if (attack_stats.collide_calc_mode == Hitbox.COLLIDE_PT_CALC_MODE.DEFAULT)
                {
                    collision_point = __instance._transform.position;
                    RaycastHit2D[] raycastHit2DArray = Physics2D.LinecastAll((Vector2)hitbox_position, (Vector2)__instance._transform.position, GL.mask_HURTBOX);
                    for (int index = 0; index < raycastHit2DArray.Length; ++index)
                    {
                        if (raycastHit2DArray[index].collider.GetComponent<Hurtbox>() == __instance)
                        {
                            collision_point = (Vector3)raycastHit2DArray[index].point;
                            break;
                        }
                    }
                }
                else
                    collision_point = attack_stats.collide_calc_mode == Hitbox.COLLIDE_PT_CALC_MODE.HITBOX || attack_stats.collide_calc_mode == Hitbox.COLLIDE_PT_CALC_MODE.HITBOX_OFFSET_B ? hitbox_position : __instance._transform.position;

                bool attackResult = __instance._hurtable.ReceiveAttackResult(new Hurtbox.AttackResult()
                {
                    final_damage_transferred = finalDamage,
                    atk_status = Hitbox.ATK_STATUS.HURT_RECEIVED,
                    atk_effect1 = attack_stats.atk_effect1,
                    atk_effect2 = attack_stats.atk_effect2,
                    knock_back = attack_stats.GetKnockback(hitbox_position, __instance.logic_holder.position) / __instance.poise,
                    dmg_to_limb = __instance.limb_number,
                    collided_layer_mask = GL.mask_HITBOX,
                    atk_class = attack_stats.atk_class,
                    damaged_party = __instance,
                    atk_immune_tag = attack_stats.atk_immune_tag,
                    side_effects = attack_stats.side_effects
                });
                __result = new Hurtbox.AttackResult
                {
                    atk_status = attack_stats.atk_class != Hitbox.ATK_CLASS.PROJECTILE ? Hitbox.ATK_STATUS.ATK_SUCCESS : Hitbox.ATK_STATUS.PROJ_CONTINUE,
                    damaged_party = __instance,
                    atk_effect1 = attack_stats.atk_effect1,
                    atk_effect2 = attack_stats.atk_effect2,
                    collided_layer_mask = GL.mask_HURTBOX,
                    final_damage_transferred = finalDamage
                };
                Hurtbox.IPEAS(collision_point, attackResult, __instance.hurtbox_material, attack_stats.hitbox_material, attack_stats.color);
                return false;
            }

            public static int ModifyDamage(int damage, Hurtbox hurtbox)
            {
                float finalDamage;
                if (hurtbox.GetComponentInParent<GaleLogicOne>())
                {
                    //Util.Message("ModifyDamage Gale " + damage);
                    finalDamage = damage * Config.playerDamageMultiplier.Value;
                    //Util.Message("ModifyDamage Gale mod " + finalDamage);
                }
                else
                {
                    //Util.Message("ModifyDamage " + damage);
                    finalDamage = damage * Config.enemyDamageMultiplier.Value;
                    //Util.Message("ModifyDamage mod " + finalDamage);
                }
                return (int)finalDamage;
            }
        }

        static bool camIconDisabled = false;

        [HarmonyPatch(typeof(CamHudLogic), "FixedUpdate")]
        class CamHudLogic_FixedUpdate_Patch
        {
            public static bool Prefix(CamHudLogic __instance)
            {
                if (Config.hudTweaks.Value)
                {
                    if (!camIconDisabled)
                    {
                        Transform icon = __instance.transform.Find("Graphic");
                        if (icon != null)
                        {
                            icon.gameObject.SetActive(false);
                            camIconDisabled = true;
                        }
                    }
                    return false;
                }
                if (camIconDisabled)
                {
                    Transform icon = __instance.transform.Find("Graphic");
                    if (icon != null)
                    {
                        icon.gameObject.SetActive(true);
                        camIconDisabled = false;
                    }
                }
                return true;
            }
        }

        static bool staminaBGdisabled = false;

        [HarmonyPatch(typeof(StaminaHudLogic), "FixedUpdate")]
        class StaminaHudLogic_FixedUpdate_Patch
        {
            public static void Postfix(StaminaHudLogic __instance)
            {
                if (Config.hudTweaks.Value)
                {
                    if (!staminaBGdisabled)
                    {
                        Transform t = __instance.transform.Find("StaminaBG");
                        if (t != null)
                        {
                            t.gameObject.SetActive(false);
                            staminaBGdisabled = true;
                        }
                        t = __instance.transform.Find("StaminaBarBlack");
                        if (t != null)
                        {
                            t.gameObject.SetActive(false);
                            staminaBGdisabled = true;
                        }
                        t = __instance.transform.Find("StaminaCutoffLine");
                        if (t != null)
                        {
                            t.gameObject.SetActive(false);
                            staminaBGdisabled = true;
                        }
                    }
                    return;
                }
                if (staminaBGdisabled)
                {
                    Transform t = __instance.transform.Find("StaminaBG");
                    if (t != null)
                    {
                        t.gameObject.SetActive(true);
                        staminaBGdisabled = false;
                    }
                    t = __instance.transform.Find("StaminaBarBlack");
                    if (t != null)
                    {
                        t.gameObject.SetActive(true);
                        staminaBGdisabled = false;
                    }
                    t = __instance.transform.Find("StaminaCutoffLine");
                    if (t != null)
                    {
                        t.gameObject.SetActive(true);
                        staminaBGdisabled = false;
                    }
                }
            }
        }


    }
}
