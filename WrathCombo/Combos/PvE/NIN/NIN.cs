using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Statuses;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;
using WrathCombo.Extensions;
using static WrathCombo.Combos.PvE.NIN.Config;

namespace WrathCombo.Combos.PvE;

internal partial class NIN : Melee
{
    #region Simple 
    internal class NIN_ST_SimpleMode : CustomCombo
    {
        protected internal MudraCasting MudraState = new();
        protected internal override Preset Preset => Preset.NIN_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        
        {
            if (actionID is not SpinningEdge)
                return actionID;
            
            NINGauge gauge = GetJobGauge<NINGauge>();
            
            if (OriginalHook(Ninjutsu) is Rabbit or Huton or Suiton or Doton or GokaMekkyaku or HyoshoRanryu)
                return OriginalHook(Ninjutsu);
            
            if (InMudra && MudraState.ContinueCurrentMudra(ref actionID))
                return actionID;
            
            if (IsNotEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus) || 
                ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat() ||
                ActionWatching.LastAction == OriginalHook(Ninjutsu))
                MudraState.CurrentMudra = MudraCasting.MudraState.None;
            
            if (HasStatusEffect(Buffs.TenChiJin))
                return STTenChiJin(actionID);
            
            #region Special Content
            
            if (OccultCrescent.ShouldUsePhantomActions() && !MudraPhase)
                return OccultCrescent.BestPhantomAction();
            
            if (Variant.CanRampart(Preset.NIN_Variant_Rampart) && !MudraPhase)
                return Variant.Rampart;
            
            if (Variant.CanCure(Preset.NIN_Variant_Cure, NIN_VariantCure) && !MudraPhase)
                return Variant.Cure;
            
            #endregion
            
            #region OGCDS
            
            if (InCombat() && HasBattleTarget())
            {
                if (CanKassatsu)
                    return Kassatsu;

                if (CanBunshin)
                    return Bunshin;

                if (CanTenChiJin)
                    return TenChiJin;
                
                if (CanTenriJindo)
                    return TenriJendo;
                
                if (CanAssassinate)
                    return OriginalHook(Assassinate);

                if (CanMeisui)
                    return NinkiWillOvercap ? OriginalHook(Bhavacakra) : OriginalHook(Meisui);

                if (CanBhavacakra && BhavacakraPooling)
                    return LevelChecked(Bhavacakra) ? OriginalHook(Bhavacakra) : OriginalHook(HellfrogMedium);
                
                if (CanMug && CombatEngageDuration().TotalSeconds > 5)
                    return OriginalHook(Mug);

                if (CanTrick && CombatEngageDuration().TotalSeconds > 5)
                    return OriginalHook(TrickAttack);
            }
            
            #endregion
            
            #region Ninjutsu
            if (CanUseHyoshoRanryu && MudraState.CastHyoshoRanryu(ref actionID) || 
                CanUseSuiton && TrickCD <= 18 && MudraState.CastSuiton(ref actionID) || 
                CanUseRaiton && MudraState.CastRaiton(ref actionID) ||
                CanUseFumaShuriken && !LevelChecked(Raiton) && MudraState.CastFumaShuriken(ref actionID))
                return actionID;
            #endregion
            
            #region Selfcare
            
            if (Role.CanSecondWind(40))
                return Role.SecondWind;

            if (ActionReady(ShadeShift) && (PlayerHealthPercentageHp() < 60 || RaidWideCasting()))
                return ShadeShift;

            if (Role.CanBloodBath(40))
                return Role.Bloodbath;
            
            #endregion
           
            #region GCDS
            
            if (CanThrowingDaggers)
                return OriginalHook(ThrowingDaggers);
            
            if (CanRaiju)
                return FleetingRaiju;

            if (CanPhantomKamaitachi)
                return PhantomKamaitachi;
            
            if (ComboTimer > 1f)
            {
                switch (ComboAction)
                {
                    case SpinningEdge when GustSlash.LevelChecked():
                        return OriginalHook(GustSlash);
                    
                    case GustSlash when GetTargetHPPercent() <= 10 && gauge.Kazematoi > 0: //Kazematoi Dump Below 10%
                        return TNAeolianEdge ? Role.TrueNorth : AeolianEdge;
                    
                    case GustSlash when ArmorCrush.LevelChecked():
                        return gauge.Kazematoi switch
                        {
                            0 => TNArmorCrush ? Role.TrueNorth : ArmorCrush,
                            >= 4 => TNAeolianEdge ? Role.TrueNorth : AeolianEdge,
                            _ => OnTargetsFlank() || !TargetNeedsPositionals() ? ArmorCrush: AeolianEdge
                        };
                    case GustSlash when !ArmorCrush.LevelChecked() && AeolianEdge.LevelChecked():
                        return TNAeolianEdge ? Role.TrueNorth : AeolianEdge;
                }
            }
            return OriginalHook(SpinningEdge);
            #endregion
        }
    }
    
    internal class NIN_AoE_SimpleMode : CustomCombo
    {
        protected internal MudraCasting MudraState = new();
        protected internal override Preset Preset => Preset.NIN_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        
        {
            if (actionID is not DeathBlossom)
                return actionID;
            
            if (OriginalHook(Ninjutsu) is Rabbit or Huton or Suiton or Doton or GokaMekkyaku or HyoshoRanryu)
                return OriginalHook(Ninjutsu);
            
            if (InMudra && MudraState.ContinueCurrentMudra(ref actionID))
                return actionID;
            
            if (IsNotEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus) || 
                ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat() ||
                ActionWatching.LastAction == OriginalHook(Ninjutsu))
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (HasStatusEffect(Buffs.TenChiJin))
                return DotonRemaining < 3
                    ? AoETenChiJinDoton(actionID) 
                    : AoETenChiJinSuiton(actionID);
            
            #region Special Content
            if (OccultCrescent.ShouldUsePhantomActions() && !MudraPhase)
                return OccultCrescent.BestPhantomAction();
            
            if (Variant.CanRampart(Preset.NIN_Variant_Rampart) && !MudraPhase)
                return Variant.Rampart;
            
            if (Variant.CanCure(Preset.NIN_Variant_Cure, NIN_VariantCure) && !MudraPhase)
                return Variant.Cure;
            #endregion
            
            #region OGCDS
            if (InCombat() && HasBattleTarget())
            {
                if (CanKassatsuAoE)
                    return Kassatsu;

                if (CanBunshin)
                    return Bunshin;

                if (CanTenChiJinAoE)
                    return TenChiJin;
                
                if (CanTenriJindo)
                    return TenriJendo;
                
                if (CanAssassinateAoE)
                    return OriginalHook(Assassinate);

                if (CanMeisuiAoE)
                    return NinkiWillOvercap ? OriginalHook(HellfrogMedium) : OriginalHook(Meisui);

                if (CanHellfrogMedium && HellfrogMediumPooling)
                    return OriginalHook(HellfrogMedium);
                
                if (CanMug && CombatEngageDuration().TotalSeconds > 5)
                    return OriginalHook(Mug);

                if (CanTrick && CombatEngageDuration().TotalSeconds > 5)
                    return OriginalHook(TrickAttack);
            }
            #endregion
            
            #region Ninjutsu
            if (CanUseGokaMekkyaku && MudraState.CastGokaMekkyaku(ref actionID) || 
                CanUseHuton && TrickCD <= 18 && MudraState.CastHuton(ref actionID) || 
                CanUseDoton && GetTargetHPPercent() >= 30 && MudraState.CastDoton(ref actionID) ||
                CanUseKaton && MudraState.CastKaton(ref actionID) ||
                CanUseFumaShuriken && !LevelChecked(Katon) && MudraState.CastFumaShuriken(ref actionID))
                return actionID;
            #endregion
            
            #region Selfcare
            if (Role.CanSecondWind(40))
                return Role.SecondWind;

            if (ActionReady(ShadeShift) && (PlayerHealthPercentageHp() < 60 || RaidWideCasting()))
                return ShadeShift;

            if (Role.CanBloodBath(40))
                return Role.Bloodbath;
            #endregion
           
            #region GCDS
            if (CanThrowingDaggersAoE)
                return OriginalHook(ThrowingDaggers);
            
            if (CanRaiju)
                return FleetingRaiju;

            if (CanPhantomKamaitachi)
                return PhantomKamaitachi;
            
            if (ComboTimer > 1f)
            {
                switch (ComboAction)
                {
                    case SpinningEdge when GustSlash.LevelChecked():
                        return OriginalHook(GustSlash);
                    case GustSlash when !ArmorCrush.LevelChecked() && AeolianEdge.LevelChecked():
                        return TNAeolianEdge ? Role.TrueNorth : AeolianEdge;
                    case DeathBlossom when LevelChecked(HakkeMujinsatsu):
                        return HakkeMujinsatsu;
                }
            }
            return LevelChecked(DeathBlossom)
                ? DeathBlossom
                : SpinningEdge;
            #endregion
        }
    }
    
    #endregion
    
    #region Advanced
    internal class NIN_ST_AdvancedMode : CustomCombo
    {
        protected internal MudraCasting MudraState = new();
        protected internal override Preset Preset => Preset.NIN_ST_AdvancedMode;
        protected override uint Invoke(uint actionID)
        
        {
            if (actionID is not SpinningEdge)
                return actionID;
            
            NINGauge gauge = GetJobGauge<NINGauge>();
            
            if (IsEnabled(Preset.NIN_ST_AdvancedMode_BalanceOpener) && 
                Opener().FullOpener(ref actionID))
            
            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus) &&
                OriginalHook(Ninjutsu) is Rabbit or Huton or Suiton or Doton or GokaMekkyaku or HyoshoRanryu)
                return OriginalHook(Ninjutsu);
            
            if (OriginalHook(Ninjutsu) != Ninjutsu && InMudra && MudraState.ContinueCurrentMudra(ref actionID))
                return actionID;
            
            if (IsNotEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus) || 
                ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat() ||
                ActionWatching.LastAction == OriginalHook(Ninjutsu))
                MudraState.CurrentMudra = MudraCasting.MudraState.None;
            
            if (NIN_ST_AdvancedMode_TenChiJin_Options[0] &&
                HasStatusEffect(Buffs.TenChiJin))
                return STTenChiJin(actionID);
            
            #region Special Content
            if (OccultCrescent.ShouldUsePhantomActions() && !MudraPhase)
                return OccultCrescent.BestPhantomAction();
            
            if (Variant.CanRampart(Preset.NIN_Variant_Rampart) && !MudraPhase)
                return Variant.Rampart;
            
            if (Variant.CanCure(Preset.NIN_Variant_Cure, NIN_VariantCure) && !MudraPhase)
                return Variant.Cure;
            #endregion
            
            #region OGCDS
            if (InCombat() && HasBattleTarget())
            {
                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Kassatsu) && CanKassatsu)
                    return Kassatsu;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bunshin) && CanBunshin)
                    return Bunshin;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_TenChiJin) && CanTenChiJin)
                    return TenChiJin;

                if (NIN_ST_AdvancedMode_TenChiJin_Options[0] && CanTenriJindo)
                    return TenriJendo;
                
                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Assassinate) && CanAssassinate)
                    return OriginalHook(Assassinate);

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Meisui) && CanMeisui)
                    return NinkiWillOvercap && IsEnabled(Preset.NIN_ST_AdvancedMode_Bhavacakra) 
                        ? OriginalHook(Bhavacakra) 
                        : OriginalHook(Meisui);

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bhavacakra) && CanBhavacakra && 
                    (BhavacakraPooling || !NIN_ST_AdvancedMode_Bhavacakra_Pooling))
                    return LevelChecked(Bhavacakra) ? OriginalHook(Bhavacakra) : OriginalHook(HellfrogMedium);
                
                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Mug) && CanMug && CombatEngageDuration().TotalSeconds > 5 &&
                    GetTargetHPPercent() > STMugThreshold)
                    return OriginalHook(Mug);

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && CanTrick && CombatEngageDuration().TotalSeconds > 5 &&
                    GetTargetHPPercent() > STTrickThreshold)
                    return OriginalHook(TrickAttack);
            }
            #endregion
            
            #region Ninjutsu
            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus))
            {
                if (NIN_ST_AdvancedMode_Ninjitsus_Options[2] && 
                    CanUseHyoshoRanryu && MudraState.CastHyoshoRanryu(ref actionID) || 
                    NIN_ST_AdvancedMode_Ninjitsus_Options[1] && 
                    CanUseSuiton && TrickCD <= NIN_ST_AdvancedMode_SuitonSetup && MudraState.CastSuiton(ref actionID) || 
                    NIN_ST_AdvancedMode_Ninjitsus_Options[0] && 
                    CanUseRaiton && MudraState.CastRaiton(ref actionID) ||
                    NIN_ST_AdvancedMode_Ninjitsus_Options[0] &&
                    CanUseFumaShuriken && !LevelChecked(Raiton) && MudraState.CastFumaShuriken(ref actionID))
                    return actionID;
            }
            #endregion
            
            #region Selfcare
            if (IsEnabled(Preset.NIN_ST_AdvancedMode_SecondWind) && Role.CanSecondWind(NIN_ST_AdvancedMode_SecondWindThreshold))
                return Role.SecondWind;
            
            if (IsEnabled(Preset.NIN_ST_AdvancedMode_ShadeShift) && ActionReady(ShadeShift) && 
                (PlayerHealthPercentageHp() < NIN_ST_AdvancedMode_ShadeShiftThreshold || 
                 NIN_ST_AdvancedMode_ShadeShiftRaidwide && RaidWideCasting()))
                return ShadeShift;
            
            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bloodbath) && Role.CanBloodBath(NIN_ST_AdvancedMode_BloodbathThreshold))
                return Role.Bloodbath;
            #endregion
           
            #region GCDS
            
            if (IsEnabled(Preset.NIN_ST_AdvancedMode_ThrowingDaggers) && CanThrowingDaggers && !MudraPhase)
                return OriginalHook(ThrowingDaggers);
            
            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Raiju) && CanRaiju)
                return NIN_ST_AdvancedMode_ForkedRaiju && !InMeleeRange() 
                    ? ForkedRaiju
                    : FleetingRaiju;

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_PhantomKamaitachi) && CanPhantomKamaitachi)
                return PhantomKamaitachi;
            
            if (ComboTimer > 1f)
            {
                switch (ComboAction)
                {
                    case SpinningEdge when GustSlash.LevelChecked():
                        return OriginalHook(GustSlash);
                    
                    case GustSlash when GetTargetHPPercent() <= NIN_ST_AdvancedMode_BurnKazematoi && gauge.Kazematoi > 0: //Kazematoi Dump Below 10%
                        return TNAeolianEdge && NIN_ST_AdvancedMode_TrueNorth ? Role.TrueNorth : AeolianEdge;
                    
                    case GustSlash when ArmorCrush.LevelChecked():
                        return gauge.Kazematoi switch
                        {
                            0 => TNArmorCrush && NIN_ST_AdvancedMode_TrueNorth ? Role.TrueNorth : ArmorCrush,
                            >= 4 => TNAeolianEdge && NIN_ST_AdvancedMode_TrueNorth ? Role.TrueNorth : AeolianEdge,
                            _ => OnTargetsFlank() || !TargetNeedsPositionals() ? ArmorCrush: AeolianEdge
                        };
                    case GustSlash when !ArmorCrush.LevelChecked() && AeolianEdge.LevelChecked():
                        return TNAeolianEdge ? Role.TrueNorth : AeolianEdge;
                }
            }
            return actionID;
            #endregion
        }
    }
    
    internal class NIN_AoE_AdvancedMode : CustomCombo
    {
        protected internal MudraCasting MudraState = new();
        protected internal override Preset Preset => Preset.NIN_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        
        {
            if (actionID is not DeathBlossom)
                return actionID;
            
            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus) &&
                OriginalHook(Ninjutsu) is Rabbit or Huton or Suiton or Doton or GokaMekkyaku or HyoshoRanryu)
                return OriginalHook(Ninjutsu);
            
            if (OriginalHook(Ninjutsu) != Ninjutsu && InMudra && MudraState.ContinueCurrentMudra(ref actionID))
                return actionID;
            
            if (IsNotEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus) || 
                ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat() ||
                ActionWatching.LastAction == OriginalHook(Ninjutsu))
                MudraState.CurrentMudra = MudraCasting.MudraState.None;
            
            if (NIN_AoE_AdvancedMode_TenChiJin_Options[0] &&
                HasStatusEffect(Buffs.TenChiJin))
                return DotonRemaining < 3
                ? AoETenChiJinDoton(actionID) 
                : AoETenChiJinSuiton(actionID);
            
            #region Special Content
            if (OccultCrescent.ShouldUsePhantomActions() && !MudraPhase)
                return OccultCrescent.BestPhantomAction();
            
            if (Variant.CanRampart(Preset.NIN_Variant_Rampart) && !MudraPhase)
                return Variant.Rampart;
            
            if (Variant.CanCure(Preset.NIN_Variant_Cure, NIN_VariantCure) && !MudraPhase)
                return Variant.Cure;
            #endregion
            
            #region OGCDS
            if (InCombat() && HasBattleTarget())
            {
                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Kassatsu) && CanKassatsuAoE)
                    return Kassatsu;

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Bunshin) && CanBunshin)
                    return Bunshin;

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_TenChiJin) && CanTenChiJinAoE)
                    return TenChiJin;

                if (NIN_AoE_AdvancedMode_TenChiJin_Options[0] && CanTenriJindo)
                    return TenriJendo;
                
                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Assassinate) && CanAssassinateAoE)
                    return OriginalHook(Assassinate);

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Meisui) && CanMeisuiAoE)
                    return NinkiWillOvercap && IsEnabled(Preset.NIN_AoE_AdvancedMode_HellfrogMedium) 
                        ? OriginalHook(HellfrogMedium) 
                        : OriginalHook(Meisui);

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_HellfrogMedium) && CanHellfrogMedium && 
                    (HellfrogMediumPooling || !NIN_AoE_AdvancedMode_HellfrogMedium_Pooling))
                    return OriginalHook(HellfrogMedium);
                
                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Mug) && CanMug && CombatEngageDuration().TotalSeconds > 5 &&
                    GetTargetHPPercent() > AoEMugThreshold)
                    return OriginalHook(Mug);

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_TrickAttack) && CanTrick && CombatEngageDuration().TotalSeconds > 5 &&
                    GetTargetHPPercent() > AoETrickThreshold)
                    return OriginalHook(TrickAttack);
            }
            #endregion
            
            #region Ninjutsu
            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus))
            {
                if (NIN_AoE_AdvancedMode_Ninjitsus_Options[3] && 
                    CanUseGokaMekkyaku && MudraState.CastGokaMekkyaku(ref actionID) || 
                    NIN_AoE_AdvancedMode_Ninjitsus_Options[2] && 
                    CanUseDoton && GetTargetHPPercent() >= NIN_AoE_AdvancedMode_Doton_Threshold && MudraState.CastDoton(ref actionID) || 
                    NIN_AoE_AdvancedMode_Ninjitsus_Options[1] && 
                    CanUseHuton && TrickCD <= NIN_ST_AdvancedMode_SuitonSetup && MudraState.CastHuton(ref actionID) || 
                    NIN_AoE_AdvancedMode_Ninjitsus_Options[0] && 
                    CanUseKaton && MudraState.CastKaton(ref actionID) ||
                    NIN_AoE_AdvancedMode_Ninjitsus_Options[0] &&
                    CanUseFumaShuriken && !LevelChecked(Raiton) && MudraState.CastFumaShuriken(ref actionID))
                    return actionID;
            }
            #endregion
            
            #region Selfcare
            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_SecondWind) && Role.CanSecondWind(NIN_AoE_AdvancedMode_SecondWindThreshold))
                return Role.SecondWind;
            
            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_ShadeShift) && ActionReady(ShadeShift) && 
                (PlayerHealthPercentageHp() < NIN_AoE_AdvancedMode_ShadeShiftThreshold || 
                 NIN_AoE_AdvancedMode_ShadeShiftRaidwide && RaidWideCasting()))
                return ShadeShift;
            
            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Bloodbath) && Role.CanBloodBath(NIN_AoE_AdvancedMode_BloodbathThreshold))
                return Role.Bloodbath;
            #endregion
           
            #region GCDS
            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_ThrowingDaggers) && CanThrowingDaggersAoE && !MudraPhase)
                return OriginalHook(ThrowingDaggers);

            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_PhantomKamaitachi) && CanPhantomKamaitachi)
                return PhantomKamaitachi;
            
            if (ComboTimer > 1f)
            {
                switch (ComboAction)
                {
                    case SpinningEdge when GustSlash.LevelChecked():
                        return OriginalHook(GustSlash);
                    case GustSlash when !ArmorCrush.LevelChecked() && AeolianEdge.LevelChecked():
                        return TNAeolianEdge ? Role.TrueNorth : AeolianEdge;
                    case DeathBlossom when LevelChecked(HakkeMujinsatsu):
                        return HakkeMujinsatsu;
                }
            }
            return LevelChecked(DeathBlossom) ? DeathBlossom : SpinningEdge;
            #endregion
        }
    }
    #endregion
    
    #region Standalone
    
    internal class NIN_ST_AeolianEdgeCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_ST_AeolianEdgeCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not AeolianEdge)
                return actionID;

            if (ComboTimer > 0)
            {
                if (ComboAction is SpinningEdge && LevelChecked(GustSlash))
                    return GustSlash;

                if (ComboAction is GustSlash && LevelChecked(AeolianEdge))
                    return AeolianEdge;
            }

            return SpinningEdge;
        }
    }

    internal class NIN_ArmorCrushCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_ArmorCrushCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not ArmorCrush)
                return actionID;
            if (ComboTimer > 0f)
            {
                if (ComboAction == SpinningEdge && GustSlash.LevelChecked())
                {
                    return GustSlash;
                }

                if (ComboAction == GustSlash && ArmorCrush.LevelChecked())
                {
                    return ArmorCrush;
                }
            }
            return SpinningEdge;
        }
    }

    internal class NIN_HideMug : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_HideMug;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Hide)
                return actionID;

            if (HasCondition(ConditionFlag.InCombat))
            {
                return OriginalHook(Mug);
            }

            if (HasStatusEffect(Buffs.Hidden))
            {
                return OriginalHook(TrickAttack);
            }

            return actionID;
        }
    }

    internal class NIN_KassatsuChiJin : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_KassatsuChiJin;

        protected override uint Invoke(uint actionID)
        {
            if (actionID == Chi && TraitLevelChecked(250) && HasStatusEffect(Buffs.Kassatsu))
            {
                return Jin;
            }
            return actionID;
        }
    }

    internal class NIN_KassatsuTrick : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_KassatsuTrick;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Kassatsu)
                return actionID;
            if (HasStatusEffect(Buffs.ShadowWalker) || HasStatusEffect(Buffs.Hidden))
            {
                return OriginalHook(TrickAttack);
            }
            return OriginalHook(Kassatsu);
        }
    }

    internal class NIN_TCJMeisui : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_TCJMeisui;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not TenChiJin)
                return actionID;

            if (HasStatusEffect(Buffs.ShadowWalker))
                return Meisui;

            if (HasStatusEffect(Buffs.TenChiJin) && IsEnabled(Preset.NIN_TCJ))
            {
                float tcjTimer = GetStatusEffectRemainingTime(Buffs.TenChiJin, anyOwner: true);

                if (tcjTimer > 5)
                    return OriginalHook(Ten);

                if (tcjTimer > 4)
                    return OriginalHook(Chi);

                if (tcjTimer > 3)
                    return OriginalHook(Jin);
            }
            return actionID;
        }
    }

    internal class NIN_Simple_Mudras : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_Simple_Mudras;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Ten or Chi or Jin) || !HasStatusEffect(Buffs.Mudra))
                return actionID;

            int mudrapath = NIN_SimpleMudra_Choice;

            if (mudrapath == 1)
            {
                if (Ten.LevelChecked() && actionID == Ten)
                {
                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) is Raiton)
                    {
                        return OriginalHook(JinCombo);
                    }

                    if (Chi.LevelChecked() && OriginalHook(Ninjutsu) is HyoshoRanryu)
                    {
                        return OriginalHook(ChiCombo);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (HasStatusEffect(Buffs.Kassatsu) && Traits.EnhancedKasatsu.TraitLevelChecked())
                            return JinCombo;

                        if (Chi.LevelChecked())
                            return OriginalHook(ChiCombo);

                        if (Jin.LevelChecked())
                            return OriginalHook(JinCombo);
                    }
                }

                if (Chi.LevelChecked() && actionID == Chi)
                {
                    if (OriginalHook(Ninjutsu) is Hyoton)
                    {
                        return OriginalHook(TenCombo);
                    }

                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(JinCombo);
                    }
                }

                if (Jin.LevelChecked() && actionID == Jin)
                {
                    if (OriginalHook(Ninjutsu) is GokaMekkyaku or Katon)
                    {
                        return OriginalHook(ChiCombo);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(TenCombo);
                    }
                }

                return OriginalHook(Ninjutsu);
            }

            if (mudrapath == 2)
            {
                if (Ten.LevelChecked() && actionID == Ten)
                {
                    if (Chi.LevelChecked() && OriginalHook(Ninjutsu) is Hyoton or HyoshoRanryu)
                    {
                        return OriginalHook(Chi);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (Jin.LevelChecked())
                            return OriginalHook(JinCombo);

                        if (Chi.LevelChecked())
                            return OriginalHook(ChiCombo);
                    }
                }

                if (Chi.LevelChecked() && actionID == Chi)
                {
                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) is Katon or GokaMekkyaku)
                    {
                        return OriginalHook(Jin);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(Ten);
                    }
                }

                if (Jin.LevelChecked() && actionID == Jin)
                {
                    if (OriginalHook(Ninjutsu) is Raiton)
                    {
                        return OriginalHook(Ten);
                    }

                    if (OriginalHook(Ninjutsu) == GokaMekkyaku)
                    {
                        return OriginalHook(Chi);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (HasStatusEffect(Buffs.Kassatsu) && Traits.EnhancedKasatsu.TraitLevelChecked())
                            return OriginalHook(Ten);
                        return OriginalHook(Chi);
                    }
                }

                return OriginalHook(Ninjutsu);
            }

            return actionID;
        }
    }
    
    #endregion
}
