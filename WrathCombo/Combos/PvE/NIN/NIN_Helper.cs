﻿using System.Collections.Frozen;
using System.Collections.Generic;
using Dalamud.Game.ClientState.JobGauge.Types;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using WrathCombo.Extensions;
using static WrathCombo.Combos.PvE.NIN.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
namespace WrathCombo.Combos.PvE;

internal partial class NIN
{
    #region Variables

    static NINGauge gauge = GetJobGauge<NINGauge>();
    
    public static FrozenSet<uint> MudraSigns = [Ten, Chi, Jin, TenCombo, ChiCombo, JinCombo];
    public static uint CurrentNinjutsu => OriginalHook(Ninjutsu);
    
    internal static bool InMudra = false;
    internal static bool STSimpleMode => IsEnabled(Preset.NIN_ST_SimpleMode);
    internal static bool AoESimpleMode => IsEnabled(Preset.NIN_AoE_SimpleMode);
    internal static bool NinjaWeave => CanWeave(.6f, 10);
    
    #region Mudra Logic
    internal static bool MudraPhase => OriginalHook(Ten) != Ten || OriginalHook(Chi) != Chi || OriginalHook(Jin) != Jin;
    internal static bool MudraReady => MudraCasting.CanCast();
    internal static uint MudraCharges => GetRemainingCharges(Ten);
    internal static bool MudraAlmostReady => MudraCharges == 1 && GetCooldownChargeRemainingTime(Ten) < 3;
    #endregion
    
    #region Ninjutsu Logic
    internal static bool CanUseFumaShuriken => LevelChecked(FumaShuriken) && MudraReady;
     
    internal static bool CanUseRaiton =>  LevelChecked(Raiton) && MudraReady && 
                                          (!HasKassatsu || !NIN_ST_AdvancedMode_Ninjitsus_Options[2] && !STSimpleMode) &&
                                           (TrickDebuff || // Buff Window
                                           !LevelChecked(Suiton) || //Dont Pool because of Suiton not learned yet
                                           GetCooldownChargeRemainingTime(Ten) < 3 || // Spend to avoid cap
                                           !NIN_ST_AdvancedMode_Raiton_Options[0] && !STSimpleMode || //Dont Pool because of Raiton Option
                                           NIN_ST_AdvancedMode_Raiton_Options[1] && !InMeleeRange() && 
                                           GetCooldownChargeRemainingTime(Ten) <= TrickCD - 10); //Uptime option
    
    internal static bool CanUseKaton =>  LevelChecked(Katon) && MudraReady &&
                                         (!HasKassatsu || !NIN_AoE_AdvancedMode_Ninjitsus_Options[2] && !STSimpleMode) &&
                                         (TrickDebuff || //Buff Window
                                          !LevelChecked(Huton) || //Dont Pool because of Huton not learned yet
                                          GetCooldownChargeRemainingTime(Ten) < 3 || // Spend to avoid cap
                                          !NIN_AoE_AdvancedMode_Katon_Options[0] && !AoESimpleMode || //Dont Pool because of Raiton Option
                                          NIN_AoE_AdvancedMode_Katon_Options[1] && !InMeleeRange() && 
                                          GetCooldownChargeRemainingTime(Ten) <= TrickCD - 10); //Uptime option
    
    internal static bool HasDoton => HasStatusEffect(Buffs.Doton);
    internal static float DotonRemaining => GetStatusEffectRemainingTime(Buffs.Doton);
    internal static bool CanUseDoton => LevelChecked(Doton) && MudraReady && 
                                        (!HasDoton || DotonRemaining <= 2) &&
                                        (TrickDebuff || //Buff Window
                                         GetCooldownChargeRemainingTime(Ten) < 3); // Use if you have Kassatsu before you get Hosho Ranryu
                                        
    
    internal static bool CanUseSuiton => LevelChecked(Suiton) && MudraReady && !HasStatusEffect(Buffs.ShadowWalker);
    
    internal static bool CanUseHuton => LevelChecked(Huton) && MudraReady && !HasStatusEffect(Buffs.ShadowWalker);
    
    internal static bool CanUseHyoshoRanryu => LevelChecked(HyoshoRanryu) && MudraReady && HasKassatsu && 
                                               (BuffWindow || IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && !STSimpleMode || KassatsuRemaining < 3);

    internal static bool CanUseGokaMekkyaku => LevelChecked(GokaMekkyaku) && MudraReady && HasKassatsu && 
                                               (BuffWindow || IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && !STSimpleMode || KassatsuRemaining < 3);
    #endregion
    
    #region GCD Logic
    internal static bool TNArmorCrush => !MudraPhase && !OnTargetsFlank() && TargetNeedsPositionals() && Role.CanTrueNorth();
    internal static bool TNAeolianEdge => !MudraPhase && !OnTargetsRear() && TargetNeedsPositionals() && Role.CanTrueNorth();
    internal static bool CanPhantomKamaitachi => !MudraPhase && HasStatusEffect(Buffs.PhantomReady) &&
                                                 (TrickDebuff && ComboAction != GustSlash ||
                                                  !TrickDebuff);
    internal static bool CanThrowingDaggers => !MudraPhase && ActionReady(ThrowingDaggers) && HasTarget() && !InMeleeRange() &&
                                               !HasStatusEffect(Buffs.RaijuReady);
    internal static bool CanThrowingDaggersAoE => !MudraPhase && ActionReady(ThrowingDaggers) && HasTarget() && GetTargetDistance() >= 4.5 &&
                                                  !HasStatusEffect(Buffs.RaijuReady);
    internal static bool CanRaiju => !MudraPhase && HasStatusEffect(Buffs.RaijuReady);
    #endregion
    
    #region OGCD Logic
    // Buffs
    internal static bool BuffWindow => TrickDebuff || MugDebuff && TrickCD >= 30;
    internal static float TrickCD => GetCooldownRemainingTime(OriginalHook(TrickAttack));
    internal static float MugCD => GetCooldownRemainingTime(OriginalHook(Mug));
    
    internal static bool CanTrick => ActionReady(OriginalHook(TrickAttack)) && NinjaWeave && HasStatusEffect(Buffs.ShadowWalker) && 
                                     (!MudraPhase || HasKassatsu) &&
                                     (MugDebuff || MugCD >= 50 || IsNotEnabled(Preset.NIN_ST_AdvancedMode_Mug) && !STSimpleMode);
    
    internal static bool CanMug => ActionReady(OriginalHook(Mug)) && CanDelayedWeave(1.25f, .6f, 10) && 
                                   (!MudraPhase || HasKassatsu) &&
                                   (TrickCD <= 6 || IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && !STSimpleMode) &&
                                   (LevelChecked(Dokumori) && GetTargetDistance() <= 8 || InMeleeRange());
    
    internal static bool TrickDebuff => HasStatusEffect(Debuffs.TrickAttack, CurrentTarget) || 
                                        HasStatusEffect(Debuffs.KunaisBane, CurrentTarget) || 
                                        JustUsed(OriginalHook(TrickAttack));
    internal static bool MugDebuff => HasStatusEffect(Debuffs.Mug, CurrentTarget) || 
                                      HasStatusEffect(Debuffs.Dokumori, CurrentTarget) ||
                                      JustUsed(OriginalHook(Mug));
   
    // Ninki Usage
    internal static bool NinkiWillOvercap => gauge.Ninki > 50;
    internal static float BunshinCD => GetCooldownRemainingTime(Bunshin);
    internal static bool CanBunshin => LevelChecked(Bunshin) && IsOffCooldown(Bunshin) && gauge.Ninki >= 50 && NinjaWeave && !MudraPhase;
    
    internal static bool CanBhavacakra => NinjaWeave && !MudraPhase && gauge.Ninki >= 50 &&
                                          (BunshinCD < 20 && gauge.Ninki >= NinkiPool() || //Pooling for Bunshin
                                           BunshinCD > 20 ||
                                           MugCD < 5 && gauge.Ninki >= 50 ||
                                           IsNotEnabled(Preset.NIN_ST_AdvancedMode_Bunshin) && !STSimpleMode); //Bunshin not enabled to pool for
    
    internal static bool BhavacakraPooling => gauge.Ninki >= NinkiPool() || 
                                              TrickDebuff && gauge.Ninki >= 50 ||
                                              MugCD < 5 && gauge.Ninki >= 50;
    
    internal static bool CanHellfrogMedium => NinjaWeave && !MudraPhase && 
                                              LevelChecked(HellfrogMedium) && gauge.Ninki >= 50;
    
    internal static bool HellfrogMediumPooling => gauge.Ninki >= NinkiPool() || 
                                                  TrickDebuff && gauge.Ninki >= 50 ||
                                                  MugCD < 5 && gauge.Ninki >= 50;
    internal static int NinkiPool()
    {
        if (HasStatusEffect(Buffs.Bunshin))
            return ComboAction == GustSlash ? 75: 95;
        return ComboAction == GustSlash ? 90 : 100;
    }
    
    // Other OGCDs
    internal static bool HasKassatsu => HasStatusEffect(Buffs.Kassatsu);
    internal static float KassatsuRemaining => GetStatusEffectRemainingTime(Buffs.Kassatsu);
    internal static bool CanKassatsu => !MudraPhase && ActionReady(Kassatsu) && NinjaWeave &&  
                                        (TrickCD < 10 && HasStatusEffect(Buffs.ShadowWalker) ||
                                         BuffWindow || 
                                         IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && !STSimpleMode);
    
    internal static bool CanKassatsuAoE => !MudraPhase && ActionReady(Kassatsu) && NinjaWeave &&  
                                        (TrickCD < 10 && HasStatusEffect(Buffs.ShadowWalker) ||
                                         BuffWindow || 
                                         IsNotEnabled(Preset.NIN_AoE_AdvancedMode_TrickAttack) && !AoESimpleMode);
    
    internal static bool CanMeisui => !MudraPhase && ActionReady(Meisui) && NinjaWeave && HasStatusEffect(Buffs.ShadowWalker) && 
                                      (BuffWindow || IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && !STSimpleMode);
    internal static bool CanMeisuiAoE => !MudraPhase && ActionReady(Meisui) && NinjaWeave && HasStatusEffect(Buffs.ShadowWalker) && 
                                      (BuffWindow || IsNotEnabled(Preset.NIN_AoE_AdvancedMode_TrickAttack) && !AoESimpleMode);

    internal static bool CanAssassinate => !MudraPhase && ActionReady(OriginalHook(Assassinate)) && NinjaWeave && 
                                           (BuffWindow || IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && !STSimpleMode);
    internal static bool CanAssassinateAoE => !MudraPhase && ActionReady(OriginalHook(Assassinate)) && NinjaWeave && 
                                           (BuffWindow || IsNotEnabled(Preset.NIN_AoE_AdvancedMode_TrickAttack) && !AoESimpleMode);

    internal static bool CanTenChiJin => !MudraPhase && !MudraAlmostReady && IsOffCooldown(TenChiJin) && LevelChecked(TenChiJin) && NinjaWeave &&
                                         (BuffWindow || IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && !STSimpleMode);
    internal static bool CanTenChiJinAoE => !MudraPhase && !MudraAlmostReady && IsOffCooldown(TenChiJin) && LevelChecked(TenChiJin) && NinjaWeave &&
                                            (BuffWindow || IsNotEnabled(Preset.NIN_AoE_AdvancedMode_TrickAttack) && !AoESimpleMode);

    internal static bool CanTenriJindo => NinjaWeave && HasStatusEffect(Buffs.TenriJendoReady);
    #endregion
    
    #endregion

    #region TCJ Methods
    internal static uint STTenChiJin(uint actionId)
    {
        if (OriginalHook(Ten) == TCJFumaShurikenTen)
            return OriginalHook(Ten);
        if (OriginalHook(Chi) == TCJRaiton)
            return OriginalHook(Chi);
        return OriginalHook(Jin) == TCJSuiton ? OriginalHook(Jin) : actionId;
    }
    internal static uint AoETenChiJinDoton(uint actionId)
    {
        if (OriginalHook(Jin) == TCJFumaShurikenJin)
            return OriginalHook(Jin);
        if (OriginalHook(Ten) == TCJKaton)
            return OriginalHook(Ten);
        return OriginalHook(Chi) == TCJDoton ? OriginalHook(Chi) : actionId;
    }
    internal static uint AoETenChiJinSuiton(uint actionId)
    {
        if (OriginalHook(Chi) == TCJFumaShurikenChi)
            return OriginalHook(Chi);
        if (OriginalHook(Ten) == TCJKaton)
            return OriginalHook(Ten);
        return OriginalHook(Jin) == TCJSuiton ? OriginalHook(Jin) : actionId;
    }
    #endregion
    
    #region Mudra
    internal class MudraCasting
    {
        #region Mudra State Stuff
        
        public MudraState CurrentMudra = MudraState.None;
        
        public enum MudraState
        {
            None,
            CastingFumaShuriken,
            CastingKaton,
            CastingRaiton,
            CastingHuton,
            CastingDoton,
            CastingSuiton,
            CastingGokaMekkyaku,
            CastingHyoshoRanryu
        }
        
        public bool ContinueCurrentMudra(ref uint actionID)
        {
            if (ActionWatching.TimeSinceLastAction.TotalSeconds >= 2 && CurrentNinjutsu == Ninjutsu)
            {
                InMudra = false;
                ActionWatching.LastAction = 0;
                CurrentMudra = MudraState.None;
                return false;
            }

            if (ActionWatching.LastAction == FumaShuriken ||
                ActionWatching.LastAction == Katon ||
                ActionWatching.LastAction == Raiton ||
                ActionWatching.LastAction == Hyoton ||
                ActionWatching.LastAction == Huton ||
                ActionWatching.LastAction == Doton ||
                ActionWatching.LastAction == Suiton ||
                ActionWatching.LastAction == GokaMekkyaku ||
                ActionWatching.LastAction == HyoshoRanryu)
            {
                CurrentMudra = MudraState.None;
                InMudra = false;
            }

            return CurrentMudra switch
            {
                MudraState.None => false,
                MudraState.CastingFumaShuriken => CastFumaShuriken(ref actionID),
                MudraState.CastingKaton => CastKaton(ref actionID),
                MudraState.CastingRaiton => CastRaiton(ref actionID),
                MudraState.CastingHuton => CastHuton(ref actionID),
                MudraState.CastingDoton => CastDoton(ref actionID),
                MudraState.CastingSuiton => CastSuiton(ref actionID),
                MudraState.CastingGokaMekkyaku => CastGokaMekkyaku(ref actionID),
                MudraState.CastingHyoshoRanryu => CastHyoshoRanryu(ref actionID),
                _ => false
            };
        }
        #endregion
        
        #region Mudra Cast Check
        public static bool CanCast()
        {
            if (InMudra) return true;
            
            if (GetCooldown(GustSlash).CooldownTotal == 0.5) return true;

            if (GetRemainingCharges(Ten) == 0 &&
                !HasStatusEffect(Buffs.Mudra) &&
                !HasStatusEffect(Buffs.Kassatsu))
                return false;
            return true;
        }
        #endregion
        
        #region Fuma Shuriken
        public bool CastFumaShuriken(ref uint actionID) // Ten
        {
            if (CurrentMudra is MudraState.None or MudraState.CastingFumaShuriken)
            {
                // Reset State
                if (!CanCast() || ActionWatching.LastAction == FumaShuriken) 
                {
                    CurrentMudra = MudraState.None;
                    return false;
                }
                // Finish the Mudra
                if (ActionWatching.LastAction is Ten or TenCombo) 
                {
                    actionID = OriginalHook(Ninjutsu);
                    return true;
                }
                // Start the Mudra
                CurrentMudra = MudraState.CastingFumaShuriken;
                actionID = OriginalHook(Ten);
                return true;
            }
            CurrentMudra = MudraState.None;
            return false;
        }
        #endregion
        
        #region Raiton
        public bool CastRaiton(ref uint actionID)  // Ten Chi
        {
            if (Raiton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingRaiton)
            {
                // Finish the Mudra
                switch (ActionWatching.LastAction)
                {
                    case Ten or TenCombo or Jin or JinCombo:
                        actionID = OriginalHook(Chi);
                        return true;
                    case Chi or ChiCombo: //Chi == Bailout Fuma
                        actionID = OriginalHook(Ninjutsu);
                        return true;
                }
                // Start the Mudra
                CurrentMudra = MudraState.CastingRaiton;
                actionID = OriginalHook(Ten);
                return true;
            }
            CurrentMudra = MudraState.None;
            return false;
        }
        #endregion
        
        #region Suiton
        public bool CastSuiton(ref uint actionID)  //Ten Chi Jin
        {
            if (Suiton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingSuiton)
            {
                //Finish the Mudra
                switch (ActionWatching.LastAction)
                {
                    case Ten or TenCombo:
                        actionID = OriginalHook(Chi);
                        return true;
                    case Chi or ChiCombo: //Chi == Bailout Hyoten
                        actionID = OriginalHook(Jin);
                        return true;
                    case Jin or JinCombo: //Jin == Bailout Fuma
                        actionID = OriginalHook(Ninjutsu);
                        return true;
                }
                // Start the Mudra
                CurrentMudra = MudraState.CastingSuiton;
                actionID = OriginalHook(Ten);
                return true;
            }
            CurrentMudra = MudraState.None;
            return false;
        }
        #endregion

        #region Hyosho Ranryu 
        public bool CastHyoshoRanryu(ref uint actionID) // Ten Jin
        {
            if (HyoshoRanryu.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingHyoshoRanryu)
            {
                //Finish the Mudra
                switch (ActionWatching.LastAction)
                {
                    case Ten or TenCombo or Chi or ChiCombo:
                        actionID = JinCombo;
                        return true;
                    case Jin or JinCombo: //Jin == Bailout to Fuma
                        actionID = OriginalHook(Ninjutsu);
                        return true;
                }
                // Start the Mudra
                CurrentMudra = MudraState.CastingHyoshoRanryu;
                actionID = OriginalHook(Ten);
                return true;
            }
            CurrentMudra = MudraState.None;
            return false;
        }
        #endregion
        
        #region Katon
        public bool CastKaton(ref uint actionID) // Jin Ten
        {
            if (Katon.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingKaton)
            {
                //Finish the Mudra
                switch (ActionWatching.LastAction)
                {
                    case Jin or JinCombo or Chi or ChiCombo:
                        actionID = OriginalHook(Ten);
                        return true;
                    case Ten or TenCombo: //Ten == Bailout to Fuma
                        actionID = OriginalHook(Ninjutsu);
                        return true;
                }
                // Start the Mudra
                CurrentMudra = MudraState.CastingKaton;
                actionID = OriginalHook(Jin);
                return true;
            }
            CurrentMudra = MudraState.None;
            return false;
        }
        #endregion
        
        #region Doton
        public bool CastDoton(ref uint actionID) // Jin Ten Chi
        {
            if (Doton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingDoton)
            {
                //Finish the Mudra
                switch (ActionWatching.LastAction)
                {
                    case Jin or JinCombo: 
                        actionID = OriginalHook(Ten);
                        return true;
                    case Ten or TenCombo: // Ten == Bailout to Raiton
                        actionID = OriginalHook(Chi);
                        return true;
                    case Chi or ChiCombo: //Chi == Bailout Fuma
                        actionID = OriginalHook(Ninjutsu);
                        return true;
                }
                // Start the Mudra
                CurrentMudra = MudraState.CastingDoton;
                actionID = OriginalHook(Jin);
                return true;
            }
            CurrentMudra = MudraState.None;
            return false;
        }
        #endregion
        
        #region Huton
        public bool CastHuton(ref uint actionID) // Jin Chi Ten
        {
            if (Huton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingHuton)
            {
                //Finish the Mudra
                switch (ActionWatching.LastAction)
                {
                    case Jin or JinCombo:
                        actionID = OriginalHook(Chi);
                        return true;
                    case Chi or ChiCombo: //Chi == Bailout katon
                        actionID = OriginalHook(Ten);
                        return true;
                    case Ten or TenCombo: // Ten == Bailout to Fuma
                        actionID = OriginalHook(Ninjutsu);
                        return true;
                }
                // Start the Mudra
                CurrentMudra = MudraState.CastingHuton;
                actionID = OriginalHook(Jin);
                return true;
            }
            CurrentMudra = MudraState.None;
            return false;
        }
        #endregion
        
        #region Goka Mekkyaku
        public bool CastGokaMekkyaku(ref uint actionID) // Jin Ten
        {
            if (GokaMekkyaku.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingGokaMekkyaku)
            {
                //Finish the Mudra
                switch (ActionWatching.LastAction)
                {
                    case Jin or JinCombo or Chi or ChiCombo:
                        actionID = OriginalHook(Ten);
                        return true;
                    case Ten or TenCombo: // Ten == Bailout to Fuma
                        actionID = OriginalHook(Ninjutsu);
                        return true;
                }
                // Start the Mudra
                CurrentMudra = MudraState.CastingGokaMekkyaku;
                actionID = OriginalHook(Jin);
                return true;
            }
            CurrentMudra = MudraState.None;
            return false;
        }
        #endregion
    }
    #endregion
    
    #region Opener
    internal static NINOpenerMaxLevel4thGCDKunai Opener1 = new();
    internal static NINOpenerMaxLevel3rdGCDDokumori Opener2 = new();
    internal static NINOpenerMaxLevel3rdGCDKunai Opener3 = new();
    
    internal static WrathOpener Opener()
    {
        if (IsEnabled(Preset.NIN_ST_AdvancedMode))
        {
            if (NIN_Adv_Opener_Selection == 0 && Opener1.LevelChecked)
                return Opener1;

            if (NIN_Adv_Opener_Selection == 1 && Opener2.LevelChecked)
                return Opener2;

            if (NIN_Adv_Opener_Selection == 2 && Opener3.LevelChecked)
                return Opener3;
        }

        if (Opener1.LevelChecked)
            return Opener1;

        return WrathOpener.Dummy;
    }
     internal class NINOpenerMaxLevel4thGCDKunai : WrathOpener
    {
        //4th GCD Kunai
        public override List<uint> OpenerActions { get; set; } =
        [
            Ten,
            ChiCombo,
            JinCombo,
            Suiton,
            Kassatsu,
            SpinningEdge,
            GustSlash,
            Dokumori,
            Bunshin,
            PhantomKamaitachi,
            ArmorCrush,
            KunaisBane,
            ChiCombo,
            JinCombo,
            HyoshoRanryu,
            DreamWithinADream,
            Ten,
            ChiCombo,
            Raiton,
            TenChiJin,
            TCJFumaShurikenTen,
            TCJRaiton,
            TCJSuiton,
            Meisui,
            FleetingRaiju,
            ZeshoMeppo,
            TenriJendo,
            FleetingRaiju,
            Bhavacakra,
            Ten,
            ChiCombo,
            Raiton,
            FleetingRaiju,
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            12
        ];

        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        internal override UserData? ContentCheckConfig => NIN_Balance_Content;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(Ten) < 1) return false;
            if (IsOnCooldown(Mug)) return false;
            if (IsOnCooldown(TenChiJin)) return false;
            if (IsOnCooldown(PhantomKamaitachi)) return false;
            if (IsOnCooldown(Bunshin)) return false;
            if (IsOnCooldown(DreamWithinADream)) return false;
            if (IsOnCooldown(Kassatsu)) return false;
            if (IsOnCooldown(TrickAttack)) return false;

            return true;
        }
    }

    internal class NINOpenerMaxLevel3rdGCDDokumori : WrathOpener
    {
        //3rd GCD Dokumori
        public override List<uint> OpenerActions { get; set; } =
        [
            Ten,
            ChiCombo,
            JinCombo,
            Suiton,
            Kassatsu,
            SpinningEdge,
            GustSlash,
            ArmorCrush,
            Dokumori,
            Bunshin,
            PhantomKamaitachi,
            KunaisBane,
            ChiCombo,
            JinCombo,
            HyoshoRanryu,
            DreamWithinADream,
            Ten,
            ChiCombo,
            Raiton,
            TenChiJin,
            TCJFumaShurikenTen,
            TCJRaiton,
            TCJSuiton,
            Meisui,
            FleetingRaiju,
            ZeshoMeppo,
            TenriJendo,
            FleetingRaiju,
            Ten,
            ChiCombo,
            Raiton,
            FleetingRaiju,
            Bhavacakra,
            SpinningEdge
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            12
        ];

        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        internal override UserData? ContentCheckConfig => NIN_Balance_Content;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(Ten) < 1) return false;
            if (IsOnCooldown(Mug)) return false;
            if (IsOnCooldown(TenChiJin)) return false;
            if (IsOnCooldown(PhantomKamaitachi)) return false;
            if (IsOnCooldown(Bunshin)) return false;
            if (IsOnCooldown(DreamWithinADream)) return false;
            if (IsOnCooldown(Kassatsu)) return false;
            if (IsOnCooldown(TrickAttack)) return false;

            return true;
        }
    }

    internal class NINOpenerMaxLevel3rdGCDKunai : WrathOpener
    {
        //3rd GCD Kunai
        public override List<uint> OpenerActions { get; set; } =
        [
            Ten,
            ChiCombo,
            JinCombo,
            Suiton,
            Kassatsu,
            SpinningEdge,
            GustSlash,
            Dokumori,
            Bunshin,
            PhantomKamaitachi,
            KunaisBane,
            ChiCombo,
            JinCombo,
            HyoshoRanryu,
            DreamWithinADream,
            Ten,
            ChiCombo,
            Raiton,
            TenChiJin,
            TCJFumaShurikenTen,
            TCJRaiton,
            TCJSuiton,
            Meisui,
            FleetingRaiju,
            ZeshoMeppo,
            TenriJendo,
            FleetingRaiju,
            ArmorCrush,
            Bhavacakra,
            Ten,
            ChiCombo,
            Raiton,
            FleetingRaiju,
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            11
        ];

        internal override UserData? ContentCheckConfig => NIN_Balance_Content;
        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(Ten) < 1) return false;
            if (IsOnCooldown(Mug)) return false;
            if (IsOnCooldown(TenChiJin)) return false;
            if (IsOnCooldown(PhantomKamaitachi)) return false;
            if (IsOnCooldown(Bunshin)) return false;
            if (IsOnCooldown(DreamWithinADream)) return false;
            if (IsOnCooldown(Kassatsu)) return false;
            if (IsOnCooldown(TrickAttack)) return false;

            return true;
        }
    }
    #endregion
    
    #region ID's

    public const byte ClassID = 29;
    public const byte JobID = 30;

    public const uint
        SpinningEdge = 2240,
        ShadeShift = 2241,
        GustSlash = 2242,
        Hide = 2245,
        Assassinate = 2246,
        ThrowingDaggers = 2247,
        Mug = 2248,
        DeathBlossom = 2254,
        AeolianEdge = 2255,
        TrickAttack = 2258,
        Kassatsu = 2264,
        ArmorCrush = 3563,
        DreamWithinADream = 3566,
        TenChiJin = 7403,
        Bhavacakra = 7402,
        HakkeMujinsatsu = 16488,
        Meisui = 16489,
        Bunshin = 16493,
        PhantomKamaitachi = 25774,
        ForkedRaiju = 25777,
        FleetingRaiju = 25778,
        HellfrogMedium = 7401,
        HollowNozuchi = 25776,
        TenriJendo = 36961,
        KunaisBane = 36958,
        ZeshoMeppo = 36960,
        Dokumori = 36957,

        //Mudras
        Ninjutsu = 2260,
        Rabbit = 2272,

        //-- initial state mudras (the ones with charges)
        Ten = 2259,
        Chi = 2261,
        Jin = 2263,

        //-- mudras used for combos (the ones used while you have the mudra buff)
        TenCombo = 18805,
        ChiCombo = 18806,
        JinCombo = 18807,

        //Ninjutsu
        FumaShuriken = 2265,
        Hyoton = 2268,
        Doton = 2270,
        Katon = 2266,
        Suiton = 2271,
        Raiton = 2267,
        Huton = 2269,
        GokaMekkyaku = 16491,
        HyoshoRanryu = 16492,

        //TCJ Jutsus (why they have another ID I will never know)
        TCJFumaShurikenTen = 18873,
        TCJFumaShurikenChi = 18874,
        TCJFumaShurikenJin = 18875,
        TCJKaton = 18876,
        TCJRaiton = 18877,
        TCJHyoton = 18878,
        TCJHuton = 18879,
        TCJDoton = 18880,
        TCJSuiton = 18881;

    public static class Buffs
    {
        public const ushort
            Mudra = 496,
            Kassatsu = 497,
            //Suiton = 507,
            Higi = 3850,
            TenriJendoReady = 3851,
            ShadowWalker = 3848,
            Hidden = 614,
            TenChiJin = 1186,
            AssassinateReady = 1955,
            RaijuReady = 2690,
            PhantomReady = 2723,
            Meisui = 2689,
            Doton = 501,
            Bunshin = 1954;
    }

    public static class Debuffs
    {
        public const ushort
            Dokumori = 3849,
            TrickAttack = 3254,
            KunaisBane = 3906,
            Mug = 638;
    }

    public static class Traits
    {
        public const uint
            EnhancedKasatsu = 250;
    }

    #endregion
}

