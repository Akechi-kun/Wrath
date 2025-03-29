using Dalamud.Game.ClientState.JobGauge.Types;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Linq;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;

namespace WrathCombo.Combos.PvE;

internal partial class PLD : TankJob
{
    private static PLDGauge Gauge => CustomComboFunctions.GetJobGauge<PLDGauge>();

    internal class PLD_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_ST_SimpleMode;
        internal static int RoyalAuthorityCount => ActionWatching.CombatActions.Count(x => x == OriginalHook(RageOfHalone));

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FastBlade)
                return actionID;

            #region Variables
            var FoFleft = GetBuffRemainingTime(Buffs.FightOrFlight);
            var FoFcd = GetCooldownRemainingTime(FightOrFlight);
            var Reqcd = GetCooldownRemainingTime(Requiescat);
            var CurrentMP = LocalPlayer.CurrentMp;
            var HasReq = HasEffect(Buffs.Requiescat);
            var HasDM = HasEffect(Buffs.DivineMight);
            var HasFoF = FoFcd >= 40;
            var HasMPforDM = CurrentMP >= GetResourceCost(HolySpirit);
            var HasMPforReq = CurrentMP >= GetResourceCost(HolySpirit) * 3.6;
            var inBurstWindow = JustUsed(FightOrFlight, 30f);
            var HasAC = HasEffect(Buffs.AtonementReady) || HasEffect(Buffs.SupplicationReady) || HasEffect(Buffs.SepulchreReady);
            var HoldDMorAC = ComboAction is RoyalAuthority ? FoFcd < 5 : ComboAction is FastBlade ? FoFcd < 2.5 : ComboAction is RiotBlade && ActionReady(FightOrFlight);

            var MitUsed = JustUsed(OriginalHook(Sheltron), 3f) ||
                             JustUsed(OriginalHook(Sentinel), 4f) ||
                             JustUsed(DivineVeil, 4f) ||
                             JustUsed(Role.Rampart, 4f) ||
                             JustUsed(HallowedGround, 9f);
            #endregion

            // Interrupt
            if (Role.CanInterject())
                return Role.Interject;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PLD_Variant_Cure, Config.PLD_VariantCure))
                return Variant.Cure;

            #region Mitigations

            if (Config.PLD_ST_MitsOptions != 1)
            {
                if (InCombat() && //Player is in combat
                    !MitUsed) //Player has not used a mitigation ability in the last 4-9 seconds
                {
                    //HallowedGround
                    if (ActionReady(HallowedGround) && //HallowedGround is ready
                        PlayerHealthPercentageHp() < 30) //Player's health is below 30%
                        return HallowedGround;

                    if (IsPlayerTargeted())
                    {
                        //Sentinel / Damnation
                        if (ActionReady(OriginalHook(Sentinel)) && //Sentinel is ready
                            PlayerHealthPercentageHp() < 60) //Player's health is below 60%
                            return OriginalHook(Sentinel);

                        //Rampart
                        if (Role.CanRampart(80)) //Player's health is below 80%
                            return Role.Rampart;

                        //Reprisal
                        if (Role.CanReprisal(90)) //Player's health is below 80%
                            return Role.Reprisal;
                    }

                    //Bulwark
                    if (ActionReady(Bulwark) && //Bulwark is ready
                        PlayerHealthPercentageHp() < 70) //Player's health is below 80%
                        return Bulwark;

                    //Sheltron
                    if (ActionReady(OriginalHook(Sheltron)) && //Sheltron
                        PlayerHealthPercentageHp() < 90) //Player's health is below 95%
                        return OriginalHook(Sheltron);
                }
            }
            #endregion

            if (HasBattleTarget())
            {
                // Goring Blade
                if (HasEffect(Buffs.GoringBladeReady) && InMeleeRange())
                    return GoringBlade;

                // Weavables
                if (CanWeave())
                {
                    if (InMeleeRange())
                    {
                        // Fight or Flight
                        if (ActionReady(FightOrFlight) &&
                            (LevelChecked(RoyalAuthority) ? RoyalAuthorityCount > 0 : ComboAction is FastBlade))
                            return FightOrFlight;

                        // Requiescat (or Imperator)
                        if (ActionReady(Requiescat) && HasFoF)
                            return OriginalHook(Requiescat);

                        // Variant Ultimatum
                        if (Variant.CanUltimatum(CustomComboPreset.PLD_Variant_Ultimatum))
                            return Variant.Ultimatum;

                        // Circle of Scorn & Spirits Within (or Expiacion)
                        if (FoFcd > 15)
                        {
                            if (ActionReady(CircleOfScorn))
                                return CircleOfScorn;

                            if (ActionReady(SpiritsWithin))
                                return OriginalHook(SpiritsWithin);
                        }

                        if (LevelChecked(Intervene) &&
                            HasFoF && 
                            HasCharges(Intervene) &&
                            TimeMoving.Ticks == 0 &&
                            !WasLastAction(Intervene))
                            return Intervene;
                    }

                    // Variant Spirit Dart
                    if (Variant.CanSpiritDart(CustomComboPreset.PLD_Variant_SpiritDart))
                        return Variant.SpiritDart;

                    // Blade of Honor
                    if (LevelChecked(BladeOfHonor) && HasEffect(Buffs.BladeOfHonor))
                        return BladeOfHonor;
                }

                // Requiescat Phase
                if (HasMPforDM)
                {
                    // ECommons Gauge doesnt have `ConfiteorComboStep` unlike CS, so here's a workaround
                    if ((LevelChecked(Confiteor) && HasEffect(Buffs.ConfiteorReady)) || // Confiteor
                        (LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor)) // Blades
                        return OriginalHook(Confiteor);

                    // Pre-Blades
                    if (HasReq)
                        return HolySpirit;
                }

                // Atonement Combo
                if (HasAC && InMeleeRange() && !HoldDMorAC)
                    return OriginalHook(Atonement);

                // Holy Spirit
                if (HasDM && HasMPforDM && !HoldDMorAC)
                    return HolySpirit;

                // Out of Range
                if (LevelChecked(ShieldLob) && !InMeleeRange())
                    return ShieldLob;
            }

            // Basic Combo
            if (ComboTimer > 0)
            {
                if (ComboAction is FastBlade && LevelChecked(RiotBlade))
                    return RiotBlade;

                if (ComboAction is RiotBlade && LevelChecked(RageOfHalone))
                    return OriginalHook(RageOfHalone);
            }

            return actionID;
        }
    }

    internal class PLD_AoE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not TotalEclipse)
                return actionID;

            #region Variables
            float FoFcd = GetCooldownRemainingTime(FightOrFlight);
            float Reqcd = GetCooldownRemainingTime(Requiescat);
            uint CurrentMP = LocalPlayer.CurrentMp;
            bool HasReq = HasEffect(Buffs.Requiescat);
            bool HasDM = HasEffect(Buffs.DivineMight);
            bool HasMPforDM = CurrentMP >= GetResourceCost(HolySpirit);
            bool HasMPforReq = CurrentMP >= GetResourceCost(HolySpirit) * 3.6;
            bool MitUsed = JustUsed(OriginalHook(Sheltron), 3f) ||
                             JustUsed(OriginalHook(Sentinel), 4f) ||
                             JustUsed(DivineVeil, 4f) ||
                             JustUsed(Role.Rampart, 4f) ||
                             JustUsed(HallowedGround, 9f);
            #endregion

            // Interrupt
            if (Role.CanInterject())
                return Role.Interject;

            // Stun
            if (TargetIsCasting())
                if (ActionReady(ShieldBash))
                    return ShieldBash;
                else if (Role.CanLowBlow())
                    return Role.LowBlow;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PLD_Variant_Cure, Config.PLD_VariantCure))
                return Variant.Cure;

            if (Config.PLD_AoE_MitsOptions != 1)
            {
                if (InCombat() && //Player is in combat
                    !MitUsed) //Player has not used a mitigation ability in the last 4-9 seconds
                {
                    //Hallowed Ground
                    if (ActionReady(HallowedGround) && //Hallowed Ground is ready
                        PlayerHealthPercentageHp() < 30) //Player's health is below 30%
                        return HallowedGround;

                    if (IsPlayerTargeted())
                    {
                        //Sentinel / Guardian
                        if (ActionReady(OriginalHook(Sentinel)) && //Sentinel is ready
                            PlayerHealthPercentageHp() < 60) //Player's health is below 60%
                            return OriginalHook(Sentinel);

                        //Rampart
                        if (Role.CanRampart(80))
                            return Role.Rampart;

                        //Reprisal
                        if (Role.CanReprisal(90, checkTargetForDebuff:false))
                            return Role.Reprisal;
                    }

                    //Bulwark
                    if (ActionReady(Bulwark) && //Bulwark is ready
                        PlayerHealthPercentageHp() < 70) //Player's health is below 80%
                        return Bulwark;

                    //Sheltron
                    if (ActionReady(OriginalHook(Sheltron)) && //Sheltron
                        PlayerHealthPercentageHp() < 90) //Player's health is below 95%
                        return OriginalHook(Sheltron);
                }
            }

            if (HasBattleTarget())
            {
                // Weavables
                if (CanWeave())
                {
                    if (InMeleeRange())
                    {
                        // Requiescat
                        if (ActionReady(Requiescat) && FoFcd > 50)
                            return OriginalHook(Requiescat);

                        // Fight or Flight
                        if (ActionReady(FightOrFlight) && ((Reqcd < 0.5f && HasMPforReq && CanWeave(1.5f)) || !LevelChecked(Requiescat)))
                            return FightOrFlight;

                        // Variant Ultimatum
                        if (Variant.CanUltimatum(CustomComboPreset.PLD_Variant_Ultimatum))
                            return Variant.Ultimatum;

                        // Circle of Scorn / Spirits Within
                        if (FoFcd > 15)
                        {
                            if (ActionReady(CircleOfScorn))
                                return CircleOfScorn;

                            if (ActionReady(SpiritsWithin))
                                return OriginalHook(SpiritsWithin);
                        }
                    }

                    // Variant Spirit Dart
                    if (Variant.CanSpiritDart(CustomComboPreset.PLD_Variant_SpiritDart))
                        return Variant.SpiritDart;

                    // Blade of Honor
                    if (LevelChecked(BladeOfHonor) && OriginalHook(Requiescat) == BladeOfHonor)
                        return OriginalHook(Requiescat);
                }

                // Confiteor & Blades
                if (HasMPforDM && (HasEffect(Buffs.ConfiteorReady) || (LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor)))
                    return OriginalHook(Confiteor);
            }

            // Holy Circle
            if (LevelChecked(HolyCircle) && HasMPforDM && (HasDM || HasReq))
                return HolyCircle;

            // Basic Combo
            if (ComboTimer > 0 && ComboAction is TotalEclipse && LevelChecked(Prominence))
                return Prominence;

            return actionID;
        }
    }

    internal class PLD_ST_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_ST_AdvancedMode;
        internal static int RoyalAuthorityCount => ActionWatching.CombatActions.Count(x => x == OriginalHook(RageOfHalone));

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FastBlade)
                return actionID;

            #region Variables
            var FoFleft = GetBuffRemainingTime(Buffs.FightOrFlight);
            var FoFcd = GetCooldownRemainingTime(FightOrFlight);
            var Reqcd = GetCooldownRemainingTime(Requiescat);
            var CurrentMP = LocalPlayer.CurrentMp;
            var HasReq = HasEffect(Buffs.Requiescat);
            var HasDM = HasEffect(Buffs.DivineMight);
            var HasFoF = FoFcd >= 40;
            var HasMPforDM = CurrentMP >= GetResourceCost(HolySpirit);
            var HasMPforReq = CurrentMP >= GetResourceCost(HolySpirit) * 3.6;
            var HasAC = HasEffect(Buffs.AtonementReady) || HasEffect(Buffs.SupplicationReady) || HasEffect(Buffs.SepulchreReady);
            var HoldDMorAC = ComboAction is RoyalAuthority ? FoFcd < 5 : ComboAction is FastBlade ? FoFcd < 2.5 : ComboAction is RiotBlade && ActionReady(FightOrFlight);
            bool isAboveMPReserve = IsNotEnabled(CustomComboPreset.PLD_ST_AdvancedMode_MP_Reserve) ||
                        (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_MP_Reserve) && CurrentMP >= GetResourceCost(HolySpirit) + Config.PLD_ST_MP_Reserve);

            var MitUsed = JustUsed(OriginalHook(Sheltron), 3f) ||
                             JustUsed(OriginalHook(Sentinel), 4f) ||
                             JustUsed(DivineVeil, 4f) ||
                             JustUsed(Role.Rampart, 4f) ||
                             JustUsed(HallowedGround, 9f);
            #endregion

            // Interrupt
            if (IsEnabled(CustomComboPreset.PLD_ST_Interrupt)
                && Role.CanInterject())
                return Role.Interject;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PLD_Variant_Cure, Config.PLD_VariantCure))
                return Variant.Cure;
            // Variant Ultimatum
            if (Variant.CanUltimatum(CustomComboPreset.PLD_Variant_Ultimatum) && CanWeave())
                return Variant.Ultimatum;
            // Variant Spirit Dart
            if (Variant.CanSpiritDart(CustomComboPreset.PLD_Variant_SpiritDart))
                return Variant.SpiritDart;



            if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_BalanceOpener) &&
                Opener().FullOpener(ref actionID))
                return actionID;

            if (HasBattleTarget())
            {
                if (InMeleeRange())
                {
                    if (CanWeave())
                    {
                        // Fight or Flight
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_FoF) &&
                            ActionReady(FightOrFlight) && CanWeave() &&
                            (LevelChecked(RoyalAuthority) ? RoyalAuthorityCount > 0 : ComboAction is FastBlade) &&
                            GetTargetHPPercent() >= Config.PLD_ST_FoF_Trigger)
                            return FightOrFlight;
                        // Requiescat
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Requiescat) &&
                            ActionReady(Requiescat) && HasMPforReq && HasFoF)
                            return OriginalHook(Requiescat);
                    }

                    // Early Goring Blade
                    if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_GoringBlade) &&
                        Config.PLD_ST_GoringBlade_SubOption == 0 &&
                        HasEffect(Buffs.GoringBladeReady) &&
                        InMeleeRange() && HasFoF)
                        return GoringBlade;

                    if (CanWeave())
                    {
                        // Circle of Scorn / Spirits Within (or Expiacion)
                        if (FoFcd > 15)
                        {
                            if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_CircleOfScorn) && 
                                ActionReady(CircleOfScorn))
                                return CircleOfScorn;
                            if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_SpiritsWithin) && 
                                ActionReady(SpiritsWithin))
                                return OriginalHook(SpiritsWithin);
                        }
                    }
                }

                if (CanWeave())
                {
                    // Blade of Honor
                    if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_BladeOfHonor) &&
                    LevelChecked(BladeOfHonor) && HasEffect(Buffs.BladeOfHonor) && CanWeave())
                        return BladeOfHonor;

                    // Intervene
                    if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Intervene) &&
                        LevelChecked(Intervene) &&
                        TimeMoving.Ticks == 0 &&
                        !WasLastAction(Intervene) &&
                        HasFoF && GetRemainingCharges(Intervene) > Config.PLD_Intervene_HoldCharges &&
                        ((Config.PLD_Intervene_MeleeOnly == 1 && InMeleeRange()) || (GetTargetDistance() == 0 && Config.PLD_Intervene_MeleeOnly == 2)))
                        return Intervene;

                    // Mitigation
                    if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Mitigation) && IsPlayerTargeted() && !MitUsed && InCombat())
                    {
                        // Hallowed Ground
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_HallowedGround) && ActionReady(HallowedGround) &&
                            PlayerHealthPercentageHp() < Config.PLD_ST_HallowedGround_Health && (Config.PLD_ST_HallowedGround_SubOption == 1 ||
                            (TargetIsBoss() && Config.PLD_ST_HallowedGround_SubOption == 2)))
                            return HallowedGround;

                        // Sentinel / Guardian
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Sentinel) && ActionReady(OriginalHook(Sentinel)) &&
                            PlayerHealthPercentageHp() < Config.PLD_ST_Sentinel_Health && (Config.PLD_ST_Sentinel_SubOption == 1 ||
                                (TargetIsBoss() && Config.PLD_ST_Sentinel_SubOption == 2)))
                            return OriginalHook(Sentinel);

                        // Rampart
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Rampart) &&

                            Role.CanRampart(Config.PLD_ST_Rampart_Health) && (Config.PLD_ST_Rampart_SubOption == 1 ||
                                (TargetIsBoss() && Config.PLD_ST_Rampart_SubOption == 2)))
                            return Role.Rampart;

                        // Sheltron
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Sheltron) && LevelChecked(Sheltron) &&
                            Gauge.OathGauge >= Config.PLD_ST_SheltronOption && PlayerHealthPercentageHp() < 95 &&
                            !HasEffect(Buffs.Sheltron) && !HasEffect(Buffs.HolySheltron))
                            return OriginalHook(Sheltron);

                    }
                }
                // Holy Spirit optimization
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_HolySpirit) &&
                    HasDM && HasMPforDM && 
                    HasEffect(Buffs.AtonementReady) &&
                    GetCooldownRemainingTime(Buffs.FightOrFlight) is <= 2.5f and > 0)
                    return HolySpirit;

                // Requiescat Phase
                if (HasMPforDM)
                {
                    // Confiteor & Blades
                    if ((IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Confiteor) && HasEffect(Buffs.ConfiteorReady)) ||
                        (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Blades) && LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor))
                        return OriginalHook(Confiteor);

                    // Pre-Blades
                    if ((IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Confiteor) || IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Blades)) && HasReq)
                        return HolySpirit;
                }

                // Late Goring Blade
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_GoringBlade) && 
                    Config.PLD_ST_GoringBlade_SubOption == 1 &&
                    HasEffect(Buffs.GoringBladeReady) && InMeleeRange() &&
                    HasFoF && !HasReq && Reqcd > 3)
                    return GoringBlade;

                // Atonement Combo
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Atonement) && 
                    HasAC && InMeleeRange() && !HoldDMorAC)
                    return OriginalHook(Atonement);

                // Holy Spirit
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_HolySpirit) && 
                    HasDM && HasMPforDM && !HoldDMorAC)
                    return HolySpirit;

                // Out of Range
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_ShieldLob) && !InMeleeRange())
                {
                    // Holy Spirit (Not Moving)
                    if (LevelChecked(HolySpirit) && 
                        HasMPforDM && isAboveMPReserve && 
                        TimeMoving.Ticks == 0 && 
                        Config.PLD_ShieldLob_SubOption == 2)
                        return HolySpirit;

                    // Shield Lob
                    if (LevelChecked(ShieldLob))
                        return ShieldLob;
                }
            }

            // Basic Combo
            if (ComboTimer > 0)
            {
                if (ComboAction is FastBlade && LevelChecked(RiotBlade))
                    return RiotBlade;

                if (ComboAction is RiotBlade && LevelChecked(RageOfHalone))
                    return OriginalHook(RageOfHalone);
            }

            return actionID;
        }
    }

    internal class PLD_AoE_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not TotalEclipse)
                return actionID;

            #region Variables
            float FoFcd = GetCooldownRemainingTime(FightOrFlight);
            float Reqcd = GetCooldownRemainingTime(Requiescat);
            uint CurrentMP = LocalPlayer.CurrentMp;
            bool HasReq = HasEffect(Buffs.Requiescat);
            bool HasDM = HasEffect(Buffs.DivineMight);
            bool HasMPforDM = CurrentMP >= GetResourceCost(HolySpirit);
            bool hasJustUsedMitigation = JustUsed(OriginalHook(Sheltron), 3f) || JustUsed(OriginalHook(Sentinel), 5f) ||
                                         JustUsed(Role.Rampart, 5f) || JustUsed(HallowedGround, 9f);
            bool HasMPforReq = (IsNotEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve) && CurrentMP >= GetResourceCost(HolySpirit) * 3.6) ||
                                   (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve) && CurrentMP >= (GetResourceCost(HolySpirit) * 3.6) + Config.PLD_AoE_MP_Reserve);
            bool isAboveMPReserve = IsNotEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve) ||
                                    (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve) && CurrentMP >= GetResourceCost(HolySpirit) + Config.PLD_AoE_MP_Reserve);
            #endregion

            // Interrupt
            if (IsEnabled(CustomComboPreset.PLD_AoE_Interrupt)
                && Role.CanInterject())
                return Role.Interject;

            // Stun
            if (IsEnabled(CustomComboPreset.PLD_AoE_Stun) && TargetIsCasting())
                if (ActionReady(ShieldBash))
                    return ShieldBash;
                else if (Role.CanLowBlow())
                    return Role.LowBlow;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PLD_Variant_Cure, Config.PLD_VariantCure))
                return Variant.Cure;

            if (HasBattleTarget())
            {
                // Weavables
                if (CanWeave())
                {
                    if (InMeleeRange())
                    {
                        // Requiescat
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Requiescat) && ActionReady(Requiescat) && FoFcd > 50)
                            return OriginalHook(Requiescat);

                        // Fight or Flight
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_FoF) && ActionReady(FightOrFlight) && GetTargetHPPercent() >= Config.PLD_AoE_FoF_Trigger &&
                            ((Reqcd < 0.5f && HasMPforReq && CanWeave(1.5)) || !LevelChecked(Requiescat)))
                            return FightOrFlight;

                        // Variant Ultimatum
                        if (Variant.CanUltimatum(CustomComboPreset.PLD_Variant_Ultimatum))
                            return Variant.Ultimatum;

                        // Circle of Scorn / Spirits Within
                        if (FoFcd > 15)
                        {
                            if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_CircleOfScorn) && ActionReady(CircleOfScorn))
                                return CircleOfScorn;

                            if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_SpiritsWithin) && ActionReady(SpiritsWithin))
                                return OriginalHook(SpiritsWithin);
                        }
                    }

                    // Variant Spirit Dart
                    if (Variant.CanSpiritDart(CustomComboPreset.PLD_Variant_SpiritDart))
                        return Variant.SpiritDart;

                    // Intervene
                    if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Intervene) && LevelChecked(Intervene) && TimeMoving.Ticks == 0 &&
                        FoFcd > 40 && GetRemainingCharges(Intervene) > Config.PLD_AoE_Intervene_HoldCharges && !WasLastAction(Intervene) &&
                        ((Config.PLD_AoE_Intervene_MeleeOnly == 1 && InMeleeRange()) || (GetTargetDistance() == 0 && Config.PLD_AoE_Intervene_MeleeOnly == 2)))
                        return Intervene;

                    // Blade of Honor
                    if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_BladeOfHonor) && LevelChecked(BladeOfHonor) && OriginalHook(Requiescat) == BladeOfHonor)
                        return OriginalHook(Requiescat);

                    // Mitigation
                    if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Mitigation) && IsPlayerTargeted() && !hasJustUsedMitigation && InCombat())
                    {
                        // Hallowed Ground
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_HallowedGround) && ActionReady(HallowedGround) &&
                            PlayerHealthPercentageHp() < Config.PLD_AoE_HallowedGround_Health && (Config.PLD_AoE_HallowedGround_SubOption == 1 ||
                                (TargetIsBoss() && Config.PLD_AoE_HallowedGround_SubOption == 2)))
                            return HallowedGround;

                        // Sentinel / Guardian
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Sentinel) && ActionReady(OriginalHook(Sentinel)) &&
                            PlayerHealthPercentageHp() < Config.PLD_AoE_Sentinel_Health && (Config.PLD_AoE_Sentinel_SubOption == 1 ||
                                (TargetIsBoss() && Config.PLD_AoE_Sentinel_SubOption == 2)))
                            return OriginalHook(Sentinel);

                        // Rampart
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Rampart) && 
                            Role.CanRampart(Config.PLD_AoE_Rampart_Health) && (Config.PLD_AoE_Rampart_SubOption == 1 ||
                                (TargetIsBoss() && Config.PLD_AoE_Rampart_SubOption == 2)))
                            return Role.Rampart;

                        // Sheltron
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Sheltron) && LevelChecked(Sheltron) &&
                            Gauge.OathGauge >= Config.PLD_AoE_SheltronOption && PlayerHealthPercentageHp() < 95 &&
                            !HasEffect(Buffs.Sheltron) && !HasEffect(Buffs.HolySheltron))
                            return OriginalHook(Sheltron);
                    }
                }

                // Confiteor & Blades
                if (HasMPforDM && ((IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Confiteor) && HasEffect(Buffs.ConfiteorReady)) ||
                                         (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Blades) && LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor)))
                    return OriginalHook(Confiteor);
            }

            // Holy Circle
            if (LevelChecked(HolyCircle) && HasMPforDM && ((IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_HolyCircle) && isAboveMPReserve && HasDM) ||
                    ((IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Confiteor) || IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Blades)) && HasReq)))
                return HolyCircle;

            // Basic Combo
            if (ComboTimer > 0 && ComboAction is TotalEclipse && LevelChecked(Prominence))
                return Prominence;

            return actionID;
        }
    }

    internal class PLD_Requiescat_Confiteor : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_Requiescat_Options;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Requiescat or Imperator))
                return actionID;

            // Fight or Flight
            if (Config.PLD_Requiescat_SubOption == 2 && //sub option enabled
                ActionReady(FightOrFlight) && //Fight or Flight is ready
                (ActionReady(Requiescat) || !LevelChecked(Requiescat))) //Requiescat is ready or not unlocked
                return FightOrFlight;
            // Goring Blade
            if (Config.PLD_Requiescat_SubOption == 2 && //sub option enabled
                HasEffect(Buffs.GoringBladeReady) && //Goring Blade is ready
                GetCooldownRemainingTime(FightOrFlight) >= 40) //Fight or Flight is active
                return GoringBlade;

            // Confiteor & Blades
            if ((LevelChecked(Confiteor) && HasEffect(Buffs.ConfiteorReady)) || //Confiteor
                (LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor)) //Blades
                return OriginalHook(Confiteor);

            // Pre-Blades
            if (!LevelChecked(BladeOfFaith) && //Blades are still locked
                HasEffect(Buffs.Requiescat)) //Requiescat is active
            {
                // AoE
                if (LevelChecked(HolyCircle) && NumberOfEnemiesInRange(HolyCircle, null) > 2) //TODO: does this actually work? null target should be player? untested
                    return HolyCircle;
                else
                    return HolySpirit;
            }

            return actionID;
        }
    }

    internal class PLD_CircleOfScorn : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_SpiritsWithin;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (SpiritsWithin or Expiacion))
                return actionID;

            if (IsOffCooldown(OriginalHook(SpiritsWithin)))
                return OriginalHook(SpiritsWithin);

            if (ActionReady(CircleOfScorn) && (Config.PLD_SpiritsWithin_SubOption == 1 || (Config.PLD_SpiritsWithin_SubOption == 2 && JustUsed(OriginalHook(SpiritsWithin), 5f))))
                return CircleOfScorn;

            return actionID;
        }
    }

    internal class PLD_ShieldLob_HolySpirit : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_ShieldLob_Feature;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not ShieldLob)
                return actionID;

            if (LevelChecked(HolySpirit) && GetResourceCost(HolySpirit) <= LocalPlayer.CurrentMp && (TimeMoving.Ticks == 0 || HasEffect(Buffs.DivineMight)))
                return HolySpirit;

            return actionID;
        }
    }

    #region One-Button Mitigation
    internal class PLD_Mit_OneButton : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_Mit_OneButton;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Bulwark)
                return actionID;

            if (IsEnabled(CustomComboPreset.PLD_Mit_HallowedGround_Max) &&
                ActionReady(HallowedGround) &&
                PlayerHealthPercentageHp() <= Config.PLD_Mit_HallowedGround_Max_Health &&
                ContentCheck.IsInConfiguredContent(
                    Config.PLD_Mit_HallowedGround_Max_Difficulty,
                    Config.PLD_Mit_HallowedGround_Max_DifficultyListSet
                ))
                return HallowedGround;

            foreach (int priority in Config.PLD_Mit_Priorities.Items.OrderBy(x => x))
            {
                int index = Config.PLD_Mit_Priorities.IndexOf(priority);
                if (CheckMitigationConfigMeetsRequirements(index, out uint action))
                    return action;
            }

            return actionID;
        }
    }
    #endregion

}
