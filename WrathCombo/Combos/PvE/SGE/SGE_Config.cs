﻿using Dalamud.Interface.Colors;
using ImGuiNET;
using WrathCombo.CustomComboNS.Functions;
using static WrathCombo.Extensions.UIntExtensions;
using static WrathCombo.Window.Functions.SliderIncrements;
using static WrathCombo.Window.Functions.UserConfig;
namespace WrathCombo.Combos.PvE;

internal partial class SGE
{
    public static class Config
    {
        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                #region DPS

                case CustomComboPreset.SGE_ST_DPS_Opener:
                    DrawHorizontalRadioButton(SGE_SelectedOpener,
                        "Toxikon Opener", "Uses Toxikon opener", 0);

                    DrawHorizontalRadioButton(SGE_SelectedOpener,
                        "Pneuma Opener", "Uses Pneuma opener", 1);

                    ImGui.NewLine();
                    DrawBossOnlyChoice(SGE_Balance_Content);
                    break;

                case CustomComboPreset.SGE_ST_DPS:
                    DrawAdditionalBoolChoice(SGE_ST_DPS_Adv,
                        $"Apply all selected options to {Dosis2.ActionName()}", $"{Dosis.ActionName()} & {Dosis3.ActionName()} will behave normally.");
                    break;

                case CustomComboPreset.SGE_ST_DPS_EDosis:
                    DrawSliderInt(0, 50, SGE_ST_DPS_EDosisOption,
                        "Stop using at Enemy HP %. Set to Zero to disable this check.");

                    ImGui.Indent();
                    ImGui.TextColored(ImGuiColors.DalamudYellow, "Select what kind of enemies the HP check should be applied to:");

                    DrawHorizontalRadioButton(SGE_ST_DPS_EDosisSubOption,
                        "Non-Bosses", "Only applies the HP check above to non-bosses.\nAllows you to only stop DoTing early when it's not a boss.", 0);

                    DrawHorizontalRadioButton(SGE_ST_DPS_EDosisSubOption,
                        "All Enemies", "Applies the HP check above to all enemies.", 1);

                    DrawRoundedSliderFloat(0, 5, SGE_ST_DPS_EDosisRefresh,
                        "Seconds remaining before reapplying the DoT. Set to Zero to disable this check.", digits: 1);
                    ImGui.Unindent();
                    break;

                case CustomComboPreset.SGE_ST_DPS_Lucid:
                    DrawSliderInt(4000, 9500, SGE_ST_DPS_Lucid,
                        "MP Threshold", 150, Hundreds);
                    break;

                case CustomComboPreset.SGE_ST_DPS_Rhizo:
                    DrawSliderInt(1, 3, SGE_ST_DPS_Rhizo,
                        "Addersgall Threshold");
                    break;

                case CustomComboPreset.SGE_ST_DPS_Phlegma:
                    if (!SGE_ST_DPS_Phlegma_Burst)
                    {
                        DrawSliderInt(0, 1, SGE_ST_DPS_Phlegma,
                            "Number of charges to hold onto\nBurst will use all charges regardless of choice.");
                    }

                    DrawAdditionalBoolChoice(SGE_ST_DPS_Phlegma_Burst,
                        "Burst option", "Save Phlegma charges for burst.");
                    break;

                case CustomComboPreset.SGE_ST_DPS_AddersgallProtect:
                    DrawSliderInt(1, 3, SGE_ST_DPS_AddersgallProtect,
                        "Addersgall Threshold");
                    break;

                case CustomComboPreset.SGE_ST_DPS_Movement:
                    DrawHorizontalMultiChoice(SGE_ST_DPS_Movement,
                        Toxikon.ActionName(), $"Use {Toxikon.ActionName()} when Addersting charges are available.", 3, 0);
                    DrawPriorityInput(SGE_ST_DPS_Movement_Priority,
                        3, 0, $"{Toxikon.ActionName()} Priority: ");
                    DrawHorizontalMultiChoice(SGE_ST_DPS_Movement,
                        Dyskrasia.ActionName(), $"Use {Dyskrasia.ActionName()} when in range of a selected enemy target.", 3, 1);
                    DrawPriorityInput(SGE_ST_DPS_Movement_Priority,
                        3, 1, $"{Dyskrasia.ActionName()} Priority: ");
                    DrawHorizontalMultiChoice(SGE_ST_DPS_Movement,
                        Eukrasia.ActionName(), $"Use {Eukrasia.ActionName()}.", 3, 2);
                    DrawPriorityInput(SGE_ST_DPS_Movement_Priority,
                        3, 2, $"{Eukrasia.ActionName()} Priority: ");
                    break;


                case CustomComboPreset.SGE_AoE_DPS_Lucid:
                    DrawSliderInt(4000, 9500, SGE_AoE_DPS_Lucid,
                        "MP Threshold", 150, Hundreds);
                    break;

                case CustomComboPreset.SGE_AoE_DPS_Pneuma:
                    DrawHorizontalRadioButton(SGE_AoE_DPS_Pneuma_SubOption,
                        "All content", $"Uses {Pneuma.ActionName()} regardless of content.", 0);

                    DrawHorizontalRadioButton(SGE_AoE_DPS_Pneuma_SubOption,
                        "Bosses Only", $"Only uses {Pneuma.ActionName()} when the targeted enemy is a boss.", 1);
                    break;

                case CustomComboPreset.SGE_AoE_DPS_Rhizo:
                    DrawSliderInt(1, 3, SGE_AoE_DPS_Rhizo,
                        "Addersgall Threshold");
                    break;

                case CustomComboPreset.SGE_AoE_DPS_AddersgallProtect:
                    DrawSliderInt(1, 3, SGE_AoE_DPS_AddersgallProtect,
                        "Addersgall Threshold");
                    break;

                #endregion

                #region Heal

                case CustomComboPreset.SGE_ST_Heal:
                    DrawAdditionalBoolChoice(SGE_ST_Heal_Adv,
                        "Advanced Options", "", isConditionalChoice: true);

                    if (SGE_ST_Heal_Adv)
                    {
                        ImGui.Indent();
                        DrawAdditionalBoolChoice(SGE_ST_Heal_IncludeShields,
                            "Include Shields in HP Percent Sliders", "");
                        ImGui.Unindent();
                    }
                    break;

                case CustomComboPreset.SGE_ST_Heal_Esuna:
                    DrawSliderInt(0, 100, SGE_ST_Heal_Esuna,
                        "Stop using when below HP %. Set to Zero to disable this check");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Lucid:
                    DrawSliderInt(4000, 9500, SGE_ST_Heal_LucidOption,
                        "MP Threshold", 150, Hundreds);
                    break;

                case CustomComboPreset.SGE_ST_Heal_Soteria:
                    DrawSliderInt(0, 100, SGE_ST_Heal_Soteria,
                        "Start using when below HP %. Set to 100 to disable this check.");
                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 0, $"{Soteria.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Zoe:
                    DrawSliderInt(0, 100, SGE_ST_Heal_Zoe,
                        "Start using when below HP %. Set to 100 to disable this check.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 1, $"{Zoe.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Pepsis:
                    DrawSliderInt(0, 100, SGE_ST_Heal_Pepsis,
                        "Start using when below HP %. Set to 100 to disable this check.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 2, $"{Pepsis.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Taurochole:
                    DrawSliderInt(0, 100, SGE_ST_Heal_Taurochole,
                        "Start using when below HP %. Set to 100 to disable this check.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 3, $"{Taurochole.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Haima:
                    DrawSliderInt(0, 100, SGE_ST_Heal_Haima,
                        "Start using when below HP %. Set to 100 to disable this check.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 4, $"{Haima.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Krasis:
                    DrawSliderInt(0, 100, SGE_ST_Heal_Krasis,
                        "Start using when below HP %. Set to 100 to disable this check.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 5, $"{Krasis.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Druochole:
                    DrawSliderInt(0, 100, SGE_ST_Heal_Druochole,
                        "Start using when below HP %. Set to 100 to disable this check.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 6, $"{Druochole.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_EDiagnosis:
                    DrawSliderInt(0, 100, SGE_ST_Heal_EDiagnosisHP,
                        "Start using when below HP %. Set to 100 to disable this check.");

                    DrawHorizontalMultiChoice(SGE_ST_Heal_EDiagnosisOpts,
                        "Sage Shield Check", "Enable to not override an existing Sage's shield.", 2, 0);

                    DrawHorizontalMultiChoice(SGE_ST_Heal_EDiagnosisOpts,
                        "Scholar Shield Check", "Enable to not override an existing Scholar's shield.", 2, 1);

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 7, $"{EukrasianDiagnosis.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Kerachole:
                    DrawSliderInt(0, 100, SGE_ST_Heal_KeracholeHP,
                        "Start using when below HP %. Set to 100 to disable this check");

                    DrawAdditionalBoolChoice(SGE_ST_Heal_KeracholeBossOption,
                        "Not on Bosses", "Will not use on ST in Boss encounters.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 8, $"{Kerachole.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Physis:
                    DrawSliderInt(0, 100, SGE_ST_Heal_PhysisHP,
                        "Start using when below HP %. Set to 100 to disable this check");

                    DrawAdditionalBoolChoice(SGE_ST_Heal_PhysisBossOption,
                        "Not on Bosses", "Will not use on ST in Boss encounters.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 9, $"{Physis.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Panhaima:
                    DrawSliderInt(0, 100, SGE_ST_Heal_PanhaimaHP,
                        "Start using when below HP %. Set to 100 to disable this check");

                    DrawAdditionalBoolChoice(SGE_ST_Heal_PanhaimaBossOption,
                        "Not on Bosses", "Will not use on ST in Boss encounters.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 10, $"{Panhaima.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_ST_Heal_Holos:
                    DrawSliderInt(0, 100, SGE_ST_Heal_HolosHP,
                        "Start using when below HP %. Set to 100 to disable this check");

                    DrawAdditionalBoolChoice(SGE_ST_Heal_HolosBossOption,
                        "Not on Bosses", "Will not use on ST in Boss encounters.");

                    DrawPriorityInput(SGE_ST_Heals_Priority,
                        12, 11, $"{Holos.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Lucid:
                    DrawSliderInt(4000, 9500, SGE_AoE_Heal_LucidOption,
                        "MP Threshold", 150, Hundreds);
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Kerachole:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_KeracholeOption,
                        "Start using when below party average HP %. Set to 100 to disable this check");

                    DrawAdditionalBoolChoice(SGE_AoE_Heal_KeracholeTrait,
                        "Check for Enhanced Kerachole Trait (Heal over Time)", $"Enabling this will prevent {Kerachole.ActionName()} from being used when the Heal over Time trait is unavailable.");

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 0, $"{Kerachole.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Ixochole:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_IxocholeOption
                        , "Start using when below party average HP %. Set to 100 to disable this check");

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 1, $"{Ixochole.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Physis:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_PhysisOption,
                        "Start using when below party average HP %. Set to 100 to disable this check");

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 2, $"{Physis.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Holos:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_HolosOption,
                        "Start using when below party average HP %. Set to 100 to disable this check");

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 3, $"{Holos.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Panhaima:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_PanhaimaOption,
                        "Start using when below party average HP %. Set to 100 to disable this check");

                    DrawHorizontalMultiChoice(SGE_ST_Heal_PanhaimaOpts,
                        "Any Panhaima check", "Enable to not override an existing Panhaima.", 1, 0);

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 4, $"{Panhaima.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Pepsis:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_PepsisOption,
                        "Start using when below party average HP %. Set to 100 to disable this check");

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 5, $"{Pepsis.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Philosophia:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_PhilosophiaOption,
                        "Start using when below party average HP %. Set to 100 to disable this check");

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 6, $"{Philosophia.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_Zoe:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_ZoeOption,
                        "Start using when below party average HP %. Set to 100 to disable this check");

                    DrawAdditionalBoolChoice(SGE_AoE_Heal_ZoePneuma,
                        "Pneuma Option", "Chain to Pneuma After.");

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 7, $"{Pneuma.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_AoE_Heal_EPrognosis:
                    DrawSliderInt(0, 100, SGE_AoE_Heal_EPrognosisOption,
                        "Shield Check: Percentage of Party Members without shields to check for.");

                    DrawPriorityInput(SGE_AoE_Heals_Priority,
                        9, 8, $"{EukrasianPrognosis.ActionName()} Priority: ");
                    break;

                case CustomComboPreset.SGE_Eukrasia:
                    DrawRadioButton(SGE_Eukrasia_Mode,
                        $"{EukrasianDosis.ActionName()}", "", 0);

                    DrawRadioButton(SGE_Eukrasia_Mode,
                        $"{EukrasianDiagnosis.ActionName()}", "", 1);

                    DrawRadioButton(SGE_Eukrasia_Mode,
                        $"{EukrasianPrognosis.ActionName()}", "", 2);

                    DrawRadioButton(SGE_Eukrasia_Mode,
                        $"{EukrasianDyskrasia.ActionName()}", "", 3);
                    break;

                case CustomComboPreset.SGE_Hidden_Holos:
                    DrawSliderInt(0, 100, SGE_Hidden_HolosOption,
                        "Start using when below party average HP % to not waste the heal portion if desired. Set to 100 to disable this check");
                    break;

                #endregion
            }
        }

        #region Variables

        #region DPS

        public static UserBool
            SGE_ST_DPS_Adv = new("SGE_ST_DPS_Adv"),
            SGE_ST_DPS_Phlegma_Burst = new("SGE_ST_DPS_Phlegma_Burst");

        public static UserBoolArray
            SGE_ST_DPS_Movement = new("SGE_ST_DPS_Movement");

        public static UserInt
            SGE_Eukrasia_Mode = new("SGE_Eukrasia_Mode", 1),
            SGE_SelectedOpener = new("SGE_SelectedOpener", 0),
            SGE_ST_DPS_Lucid = new("SGE_ST_DPS_Lucid", 6500),
            SGE_ST_DPS_Rhizo = new("SGE_ST_DPS_Rhizo", 1),
            SGE_ST_DPS_Phlegma = new("SGE_ST_DPS_Phlegma", 0),
            SGE_ST_DPS_EDosisSubOption = new("SGE_ST_DPS_EDosisSubOption", 0),
            SGE_ST_DPS_EDosisOption = new("SGE_ST_DPS_EDosisOption", 10),
            SGE_ST_DPS_AddersgallProtect = new("SGE_ST_DPS_AddersgallProtect", 3),
            SGE_AoE_DPS_Lucid = new("SGE_AoE_Phlegma_Lucid", 6500),
            SGE_AoE_DPS_Rhizo = new("SGE_AoE_DPS_Rhizo", 1),
            SGE_AoE_DPS_AddersgallProtect = new("SGE_AoE_DPS_AddersgallProtect", 3),
            SGE_AoE_DPS_Pneuma_SubOption = new("SGE_AoE_DPS_Pneuma_SubOption", 1),
            SGE_Balance_Content = new("SGE_Balance_Content", 1);

        public static UserFloat
            SGE_ST_DPS_EDosisRefresh = new("SGE_ST_DPS_EDosisRefresh", 5.0f);

        public static UserIntArray
            SGE_ST_DPS_Movement_Priority = new("SGE_ST_Movement_Priority");

        #endregion

        #region Healing

        public static UserBool
            SGE_ST_Heal_Adv = new("SGE_ST_Heal_Adv"),
            SGE_ST_Heal_IncludeShields = new("SGE_ST_Heal_IncludeShields"),
            SGE_ST_Heal_KeracholeBossOption = new("SGE_ST_Heal_KeracholeBossOption"),
            SGE_ST_Heal_PanhaimaBossOption = new("SGE_ST_Heal_PanhaimaBossOption"),
            SGE_ST_Heal_PhysisBossOption = new("SGE_ST_Heal_PhysisBossOption"),
            SGE_ST_Heal_HolosBossOption = new("SGE_ST_Heal_HolosBossOption"),
            SGE_AoE_Heal_KeracholeTrait = new("SGE_AoE_Heal_KeracholeTrait"),
            SGE_AoE_Heal_ZoePneuma = new("SGE_AoE_Heal_ZoePneuma");

        public static UserInt
            SGE_ST_Heal_LucidOption = new("SGE_ST_Heal_LucidOption", 6500),
            SGE_ST_Heal_Zoe = new("SGE_ST_Heal_Zoe", 50),
            SGE_ST_Heal_Haima = new("SGE_ST_Heal_Haima", 50),
            SGE_ST_Heal_Krasis = new("SGE_ST_Heal_Krasis", 40),
            SGE_ST_Heal_Pepsis = new("SGE_ST_Heal_Pepsis", 70),
            SGE_ST_Heal_Soteria = new("SGE_ST_Heal_Soteria", 70),
            SGE_ST_Heal_EDiagnosisHP = new("SGE_ST_Heal_EDiagnosisHP", 70),
            SGE_ST_Heal_Druochole = new("SGE_ST_Heal_Druochole", 70),
            SGE_ST_Heal_Taurochole = new("SGE_ST_Heal_Taurochole", 60),
            SGE_ST_Heal_KeracholeHP = new("SGE_ST_Heal_KeracholeHP", 70),
            SGE_ST_Heal_PhysisHP = new("SGE_ST_Heal_PhysisHP", 70),
            SGE_ST_Heal_PanhaimaHP = new("SGE_ST_Heal_PanhaimaHP", 70),
            SGE_ST_Heal_HolosHP = new("SGE_ST_Heal_HolosHP", 70),
            SGE_ST_Heal_Esuna = new("SGE_ST_Heal_Esuna", 50),
            SGE_AoE_Heal_LucidOption = new("SGE_AoE_Heal_LucidOption", 6500),
            SGE_AoE_Heal_ZoeOption = new("SGE_AoE_Heal_PneumaOption", 50),
            SGE_AoE_Heal_PhysisOption = new("SGE_AoE_Heal_PhysisOption", 60),
            SGE_AoE_Heal_PhilosophiaOption = new("SGE_AoE_Heal_PhilosophiaOption", 40),
            SGE_AoE_Heal_PepsisOption = new("SGE_AoE_Heal_PepsisOption", 70),
            SGE_AoE_Heal_PanhaimaOption = new("SGE_AoE_Heal_PanhaimaOption", 50),
            SGE_AoE_Heal_KeracholeOption = new("SGE_AoE_Heal_KeracholeOption", 70),
            SGE_AoE_Heal_IxocholeOption = new("SGE_AoE_Heal_IxocholeOption", 70),
            SGE_AoE_Heal_HolosOption = new("SGE_AoE_Heal_HolosOption", 60),
            SGE_AoE_Heal_EPrognosisOption = new("SGE_AoE_Heal_EPrognosisOption", 70),
            SGE_Hidden_HolosOption = new("SGE_Hidden_HolosOption", 70);

        public static UserIntArray
            SGE_ST_Heals_Priority = new("SGE_ST_Heals_Priority"),
            SGE_AoE_Heals_Priority = new("SGE_AoE_Heals_Priority");

        public static UserBoolArray
            SGE_ST_Heal_EDiagnosisOpts = new("SGE_ST_Heal_EDiagnosisOpts"),
            SGE_ST_Heal_PanhaimaOpts = new("SGE_ST_Heal_PanhaimaOpts");

        #endregion

        #endregion
    }
}
