using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using WrathCombo.Combos.PvE.Content;
using WrathCombo.Extensions;
using static WrathCombo.Combos.PvE.GNB.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Combos.PvE;

internal partial class GNB : Tank
{
    private static byte Ammo => GetJobGauge<GNBGauge>().Ammo;
    private static int MaxCartridges => TraitLevelChecked(Traits.CartridgeChargeII) ? 3 : TraitLevelChecked(Traits.CartridgeCharge) ? 2 : 0;
    private static byte AmmoComboStep => GetJobGauge<GNBGauge>().AmmoComboStep;
    private static float HPP => PlayerHealthPercentageHp();
    private static float NMcd => GetCooldownRemainingTime(NoMercy);
    private static float BFcd => GetCooldownRemainingTime(Bloodfest);
    private static float GFcd => GetCooldownRemainingTime(GnashingFang);
    private static float DDcd => GetCooldownRemainingTime(DoubleDown);
    private static bool HasNM => NMcd is >= 39.5f and <= 60;
    private static bool HasReign => HasStatusEffect(Buffs.ReadyToReign);
    private static bool CanBS => LevelChecked(BurstStrike) && Ammo > 0;
    private static bool CanGF => LevelChecked(GnashingFang) && GFcd < 0.6f && !HasStatusEffect(Buffs.ReadyToBlast) && AmmoComboStep == 0 && Ammo > 0;
    private static bool CanDD => LevelChecked(DoubleDown) && DDcd < 0.6f && Ammo > 0;
    private static bool CanBF => LevelChecked(Bloodfest) && BFcd < 0.6f;
    private static bool CanZone => LevelChecked(DangerZone) && GetCooldownRemainingTime(OriginalHook(DangerZone)) < 0.6f;
    private static bool CanSB => LevelChecked(SonicBreak) && HasStatusEffect(Buffs.ReadyToBreak);
    private static bool CanBow => LevelChecked(BowShock) && GetCooldownRemainingTime(BowShock) < 0.6f;
    private static bool CanContinue => LevelChecked(Continuation);
    private static bool CanReign => LevelChecked(ReignOfBeasts) && AmmoComboStep == 0 && HasReign;
    private static bool CanLateWeave => CanDelayedWeave(weaveStart: 0.9f);
    private static bool InOdd => BFcd is < 90 and > 20;
    private static bool MitUsed => JustUsed(OriginalHook(HeartOfStone), 4f) || JustUsed(OriginalHook(Nebula), 5f) || JustUsed(Camouflage, 5f) || JustUsed(Role.Rampart, 5f) || JustUsed(Aurora, 5f) || JustUsed(Superbolide, 9f);
    private static float GCDLength => ActionManager.GetAdjustedRecastTime(ActionType.Action, KeenEdge) / 1000f;
    private static bool SlowGNB => GCDLength >= 2.4800f;
    private static bool MidGNB => GCDLength is <= 2.4799f and >= 2.4500f;
    private static bool FastGNB => GCDLength is <= 2.4499f;
    private static bool In5y => GetTargetDistance() <= 5f;
    private static bool NotReadyYet(uint action) => IsOnCooldown(action) || !LevelChecked(action);


    private static uint GetBozjaAction()
    {
        bool CanUse(uint action) => HasActionEquipped(action) && IsOffCooldown(action);
        bool IsEnabledAndUsable(Preset preset, uint action) => IsEnabled(preset) && CanUse(action);

        if (!InCombat() && IsEnabledAndUsable(Preset.GNB_Bozja_LostStealth, Bozja.LostStealth))
            return Bozja.LostStealth;

        if (CanWeave())
        {
            foreach (var (preset, action) in new[]
            { 
                (Preset.GNB_Bozja_LostFocus, Bozja.LostFocus),
                (Preset.GNB_Bozja_LostFontOfPower, Bozja.LostFontOfPower),
                (Preset.GNB_Bozja_LostSlash, Bozja.LostSlash),
                (Preset.GNB_Bozja_LostFairTrade, Bozja.LostFairTrade),
                (Preset.GNB_Bozja_LostAssassination, Bozja.LostAssassination), 
            })
                if (IsEnabledAndUsable(preset, action))
                    return action;

            foreach (var (preset, action, powerPreset) in new[]
            { 
                (Preset.GNB_Bozja_BannerOfNobleEnds, Bozja.BannerOfNobleEnds, Preset.GNB_Bozja_PowerEnds),
                (Preset.GNB_Bozja_BannerOfHonoredSacrifice, Bozja.BannerOfHonoredSacrifice, Preset.GNB_Bozja_PowerSacrifice) 
            })
                if (IsEnabledAndUsable(preset, action) && (!IsEnabled(powerPreset) || JustUsed(Bozja.LostFontOfPower, 5f)))
                    return action;

            if (IsEnabledAndUsable(Preset.GNB_Bozja_BannerOfHonedAcuity, Bozja.BannerOfHonedAcuity) && !HasStatusEffect(Bozja.Buffs.BannerOfTranscendentFinesse))
                return Bozja.BannerOfHonedAcuity;
        }

        foreach (var (preset, action, condition) in new[]
        { 
            (Preset.GNB_Bozja_LostDeath, Bozja.LostDeath, true),
            (Preset.GNB_Bozja_LostCure, Bozja.LostCure, PlayerHealthPercentageHp() <= GNB_Bozja_LostCure_Health),
            (Preset.GNB_Bozja_LostArise, Bozja.LostArise, GetTargetHPPercent() == 0 && !HasStatusEffect(RoleActions.Magic.Buffs.Raise)),
            (Preset.GNB_Bozja_LostReraise, Bozja.LostReraise, PlayerHealthPercentageHp() <= GNB_Bozja_LostReraise_Health),
            (Preset.GNB_Bozja_LostProtect, Bozja.LostProtect, !HasStatusEffect(Bozja.Buffs.LostProtect)),
            (Preset.GNB_Bozja_LostShell, Bozja.LostShell, !HasStatusEffect(Bozja.Buffs.LostShell)),
            (Preset.GNB_Bozja_LostBravery, Bozja.LostBravery, !HasStatusEffect(Bozja.Buffs.LostBravery)),
            (Preset.GNB_Bozja_LostBubble, Bozja.LostBubble, !HasStatusEffect(Bozja.Buffs.LostBubble)),
            (Preset.GNB_Bozja_LostParalyze3, Bozja.LostParalyze3, !JustUsed(Bozja.LostParalyze3, 60f))
        })
            if (IsEnabledAndUsable(preset, action) && condition)
                return action;

        if (IsEnabled(Preset.GNB_Bozja_LostSpellforge) && CanUse(Bozja.LostSpellforge) &&
            (!HasStatusEffect(Bozja.Buffs.LostSpellforge) || !HasStatusEffect(Bozja.Buffs.LostSteelsting)))
            return Bozja.LostSpellforge;

        if (IsEnabled(Preset.GNB_Bozja_LostSteelsting) && CanUse(Bozja.LostSteelsting) &&
            (!HasStatusEffect(Bozja.Buffs.LostSpellforge) || !HasStatusEffect(Bozja.Buffs.LostSteelsting)))
            return Bozja.LostSteelsting;

        return 0;
    }
    private static bool ShouldUseOther => Bozja.IsInBozja && GetBozjaAction() != 0;
    private static bool CanUse(uint action) => LevelChecked(action) && GetCooldownRemainingTime(action) <= 0.4f;
    private static bool CanUseOGCD(uint action) => CanUse(action) && CanWeave();

    #region Rotation

    private static bool ShouldUseNoMercy(Preset preset, bool ammoUnlocked, int hpConfig = 0, int bossConfig = 0, int burstOptions = 0)
    {
        if (!IsEnabled(preset) || GetTargetHPPercent() <= hpConfig)
            return false;

        //max ammo = 3
        //2 min - we want to enter NM with 2 carts ideally since we can only fit 8 GCDs (GF>DD>SB>GF>GF>RoB/BS>RoB/BS>RoB/BS)
        //1 min - we dont care, just pop it if we have 2+ carts (or 1 with special conditions)
        var three =
            (InOdd && (Ammo >= 2 || (ComboAction is BrutalShell && Ammo == 1))) ||
            (!InOdd && Ammo != 3);

        //max ammo = 2 or 0
        //just pop it if there's any ammo to use, and if not unlocked yet then just send it on CD
        var two = ammoUnlocked ? Ammo > 0 : CanUse(NoMercy);

        var condition =
                CanUse(NoMercy) && //can execute No Mercy
                InCombat() && //in combat
                HasBattleTarget() && //target available
                (TraitLevelChecked(Traits.CartridgeChargeII) ? three : two) && //ammo check
                burstOptions switch
                {
                    0 => DoubleDown.LevelChecked() ? (DDcd <= 2.5f && GFcd <= 0.5f) : (!GnashingFang.LevelChecked() || GFcd <= 0.5f), //align with both - GF then DD
                    1 => !GnashingFang.LevelChecked() || GFcd <= 0.5f, //or align with GF regardless of DD
                    2 => !DoubleDown.LevelChecked() || DDcd <= 0.5f, //or align with DD regardless of GF
                    3 => true, //or align with nothing and just use as soon as it's ready
                    _ => false
                };

        //because GNB is really a blue DPS, there are different ways of using because of SkS
        return (bossConfig == 0 || bossConfig == 1 && InBossEncounter()) &&
            (SlowGNB && condition && CanWeave()) || //slow - always use on CD
            (MidGNB && condition && (InOdd ? CanWeave() : CanLateWeave)) || //mid - on CD for 1m, late-weave for 2m
            (FastGNB && condition && CanLateWeave); //fast - always late-weave
    }

    private static bool ShouldUseBloodfest(Preset preset, int subOptions = 0) 
        => IsEnabled(preset) && //relative preset is enabled
        CanUseOGCD(Bloodfest) && //is unlocked and off cooldown
        HasBattleTarget() && //target available
        subOptions switch
        {
            0 => Ammo > 0, //use only if we have no ammo
            1 => true, //or use on CD regardless of ammo (not recommended)
            _ => false
        };

    private static bool ShouldUseZone(Preset preset, int subOptions = 0)
        => IsEnabled(preset) && //relative preset is enabled
        CanUseOGCD(LevelChecked(BlastingZone) ? BlastingZone : DangerZone) && //is unlocked and off cooldown
        HasBattleTarget() && //target available
        subOptions switch
        {
            0 => NMcd is < 57.5f and > 15f, //use optimally - one use buffed, one use unbuffed
            1 => true, //or use on CD regardless of anything (not recommended)
            _ => false
        };

    private static bool ShouldUseBowShock(Preset preset, int subOptions = 0)
        => IsEnabled(preset) && 
        CanUseOGCD(BowShock) &&
        subOptions switch
        {
            0 => NMcd is < 57.5f and >= 40, //use optimally - we usually never want this without NM but also not as soon as NM is active
            1 => true, //use on CD regardless of anything (not recommended)
            _ => false
        };

    private static bool ShouldUseContinuation(Preset preset)
        //primarily used ASAP - for GF/SC/WT and BS/FC fillers
        => IsEnabled(preset) && //relative preset is enabled
        HasBattleTarget() && //target available 
        In5y && //target in range
        HasStatusEffect(Buffs.ReadyToRip) || HasStatusEffect(Buffs.ReadyToTear) || HasStatusEffect(Buffs.ReadyToGouge) || //GF combo fillers
        (HasStatusEffect(Buffs.ReadyToBlast) && ((SlowGNB ? NMcd is > 1.5f || CanDelayedWeave(0.6f, 0) : (FastGNB || MidGNB)) || !TraitLevelChecked(Traits.CartridgeChargeII))) //slight optimization for HV - if we can fit a Hypervelocity use into our NM buff, we should
        ; 

    private static bool ShouldUseGnashingFang(Preset preset, int subOptions = 0)
        => IsEnabled(preset) && //relative preset is enabled
        HasBattleTarget() && //target available 
        CanUse(GnashingFang) && //can execute Gnashing fang
        subOptions switch
        {
            0 => NMcd is > 15 and < 35 || JustUsed(NoMercy, 5f), //same as Zone but aiming for GF->DD->etc. when buffed by NM
            1 => NMcd is > 15 and < 35 || (IsOnCooldown(DoubleDown) || !LevelChecked(DoubleDown)), //same as Zone but aiming for DD->GF->etc. when buffed by NM
            2 => true, //or use on CD regardless of anything (not recommended)
            _ => false
        };

    private static bool ShouldUseDoubleDown(Preset preset, int subOptions = 0)
        => IsEnabled(preset) && //relative preset is enabled
        HasBattleTarget() && //target available 
        In5y && //target in range
        CanUse(DoubleDown) && //can execute Double Down
        subOptions switch
        {
            0 => IsOnCooldown(GnashingFang), //use optimally since we usually never want this without NM but also aiming for GF->DD->etc. when buffed by NM
            1 => JustUsed(NoMercy, 5f), //use optimally since we usually never want this without NM but also aiming for DD->GF->etc. when buffed by NM
            2 => true, //or use on CD regardless of anything (not recommended)
            _ => false
        };

    private static bool ShouldUseSonicBreak(Preset preset, int subOptions = 0)
        => IsEnabled(preset) && //relative preset is enabled
        HasBattleTarget() && //target available 
        LevelChecked(SonicBreak) && //is unlocked
        HasNM && //has No Mercy
        subOptions switch
        {
            0 => (IsOnCooldown(GnashingFang) || !LevelChecked(GnashingFang)) && (IsOnCooldown(DoubleDown) || !LevelChecked(DoubleDown)), //use after GF & DD
            1 => GetStatusEffectRemainingTime(Buffs.NoMercy) <= GCDLength + 0.1f, //or use as last GCD in NM
            2 => true, //or use as soon as possible regardless of anything (not recommended)
            _ => false
        };

    private static bool ShouldUseReignOfBeasts(Preset preset, int subOptions = 0)
        => IsEnabled(preset) && //relative preset is enabled
        HasBattleTarget() && //target available 
        In5y && //target in range
        LevelChecked(ReignOfBeasts) && //is unlocked
        HasReign && //has No Mercy
        subOptions switch
        {
            0 => AmmoComboStep == 0 && IsOnCooldown(GnashingFang) && IsOnCooldown(DoubleDown) && !HasStatusEffect(Buffs.ReadyToBreak), //use after GF, DD, SB, and rest of GF combo
            1 => AmmoComboStep == 0, //or use as soon as possible without breaking GF combo if possible
            2 => true, //or use as soon as possible regardless of anything (not recommended)
            _ => false
        };

    private static bool ShouldUseBSorFC(Preset preset, uint action, int subOptions = 0)
        => IsEnabled(preset) && //relative preset is enabled
        HasBattleTarget() && //target available 
        LevelChecked(action) && //is unlocked
        Ammo > 0 && //has Ammo
        (HasNM || //has No Mercy
        (TraitLevelChecked(Traits.CartridgeChargeII) && NMcd < 1 && Ammo == 3 && !InOdd)) && //or prep HV for NM
        subOptions switch
        {
            0 => NotReadyYet(GnashingFang) && NotReadyYet(DoubleDown) && !HasStatusEffect(Buffs.ReadyToBreak) && !HasReign && AmmoComboStep == 0, //use after GF, DD, SB, and rest of GF combo
            1 => NotReadyYet(GnashingFang) && NotReadyYet(DoubleDown), //or use after GF & DD to spend carts as soon as possible
            2 => true, //or use as soon as possible regardless of anything (not recommended)
            _ => false
        };

    private static bool ShouldUseLightningShot(Preset preset) 
        => IsEnabled(preset) && //relative preset is enabled
        LevelChecked(LightningShot) && //is unlocked
        !InMeleeRange() && //not in melee range
        HasBattleTarget() //target available
        ;

    private static uint STCombo(Preset preset)
    {
        if (IsEnabled(preset) && //relative preset is enabled
            ComboTimer > 0) //in combo
        {
            if (LevelChecked(BrutalShell) && //Brutal Shell unlocked
                ComboAction == KeenEdge) //last combo action was Keen Edge
                return BrutalShell;

            if (LevelChecked(SolidBarrel) && //Solid Barrel unlocked
                ComboAction == BrutalShell) //last combo action was Brutal Shell
            {
                return LevelChecked(BurstStrike) && //Burst Strike unlocked
                    Ammo == MaxCartridges && //max ammo
                    GNB_ST_Overcap_Choice == 0 //overcap option is enabled
                    ? BurstStrike //then use BS instead
                    : SolidBarrel; //otherwise end combo normally
            }
        }

        return KeenEdge; //combo starter
    }

    private static uint AOECombo(Preset preset)
    {
        if (IsEnabled(preset) && //relative preset is enabled
            ComboTimer > 0) //in combo
        {
            if (LevelChecked(DemonSlaughter) && //Demon Slaughter unlocked
                ComboAction == DemonSlice) //last combo action was Demon Slice
            {
                if (Ammo == MaxCartridges && //max ammo
                    GNB_AoE_Overcap_Choice == 0) //overcap option is enabled
                {
                    if (LevelChecked(FatedCircle)) //Fated Circle unlocked
                        return FatedCircle;

                    else //Fated Circle not unlocked
                    {
                        if (GNB_AoE_FatedCircle_BurstStrike == 0) //burst strike replacement option is enabled
                            return BurstStrike;
                    }
                }

                return DemonSlaughter;
            }
        }

        return DemonSlice; //combo starter
    }
    #endregion
}
