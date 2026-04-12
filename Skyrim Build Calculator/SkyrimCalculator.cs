using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyrim_Build_Architect
{
    // --- ERGEBNIS-PAKETE ---

    public class WeaponCalculationResult
    {
        public double FinalDamage { get; set; }
        public double SneakDamage { get; set; }
        public string SmithingTierName { get; set; } = "";
        public string FinalEffectText { get; set; } = "None";
    }

    public class TotalBuildStatsResult
    {
        public double TotalArmor { get; set; }
        public double TotalDamage { get; set; }
        public double TotalSneak { get; set; }
    }

    public class ArmorCalculationResult
    {
        public double FinalArmorRating { get; set; }
        public string SmithingTierName { get; set; } = "";
        public string FinalEffectText { get; set; } = "None";
    }

    // --- DER TASCHENRECHNER ---

    public static class SkyrimCalculator
    {
        // 1. WAFFEN-BERECHNUNG
        public static WeaponCalculationResult CalculateWeapon(
            Weapon w,
            int level,
            List<Perk> activePerks,
            double difficultyMult,
            Enchantment? selectedEnch)
        {
            var result = new WeaponCalculationResult();
            var stats = w.GetStatsForLevel(level);

            string wName = w.Name.ToLower();
            string wCategory = (w.Category ?? "").ToLower();

            // Schmiede-Bonus
            double smithingBonus = 0;
            bool hasSmithingPerk = activePerks.Any(p =>
                p.SkillGroup == "Smithing" &&
                (wName.Contains(p.BaseName.ToLower()) || wCategory.Contains(p.BaseName.ToLower()))
            );

            if (hasSmithingPerk)
            {
                if (level >= 41) { smithingBonus = 15; result.SmithingTierName = "(Legendary)"; }
                else if (level >= 31) { smithingBonus = 8; result.SmithingTierName = "(Epic)"; }
                else if (level >= 21) { smithingBonus = 6; result.SmithingTierName = "(Exquisite)"; }
                else if (level >= 11) { smithingBonus = 4; result.SmithingTierName = "(Superior)"; }
                else { smithingBonus = 2; result.SmithingTierName = "(Fine)"; }
            }

            // Waffen-Perks
            double wMult = 1.0;
            var matchingPerks = activePerks.Where(p => {
                string pGroup = (p.SkillGroup ?? "").ToLower();
                if (pGroup.Contains("onehanded") && (wName.Contains("dagger") || (wName.Contains("sword") && !wName.Contains("greatsword")) || wName.Contains("mace") || wName.Contains("war axe"))) return true;
                if (pGroup.Contains("twohanded") && (wName.Contains("greatsword") || wName.Contains("battleaxe") || wName.Contains("warhammer"))) return true;
                if (pGroup.Contains("archery") && (wName.Contains("bow") || wName.Contains("crossbow"))) return true;
                return false;
            }).ToList();

            if (matchingPerks.Any()) wMult = matchingPerks.Max(p => p.Multiplier);

            // Finaler Schaden
            result.FinalDamage = (stats.dmg + smithingBonus) * wMult * difficultyMult;

            // Sneak-Multiplikator
            double sMult = activePerks.Where(p => p.SkillGroup != null && p.SkillGroup.Contains("Sneak")).Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();
            result.SneakDamage = result.FinalDamage * sMult;

            // Verzauberungen
            double eMult = activePerks.Where(p => p.SkillGroup == "Enchanting").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();

            if (selectedEnch != null && selectedEnch.Name != "None")
            {
                double mag = selectedEnch.AddedValue * eMult;
                string enchText = string.Format(selectedEnch.Description, Math.Round(mag));
                result.FinalEffectText = (string.IsNullOrEmpty(stats.eff) || stats.eff == "None") ? enchText : stats.eff + " + " + enchText;
            }
            else
            {
                result.FinalEffectText = !string.IsNullOrEmpty(stats.eff) ? stats.eff : "None";
            }

            return result;
        }

        // 2. RÜSTUNGS-BERECHNUNG
        public static ArmorCalculationResult CalculateArmor(
            Armor a,
            int level,
            List<Perk> activePerks,
            Enchantment? selectedEnch)
        {
            var result = new ArmorCalculationResult();
            var stats = a.GetStatsForLevel(level);

            string aName = a.Name.ToLower();
            string aCategory = (a.Category ?? "").ToLower();

            // Schmiede-Bonus
            double smithingBonus = 0;
            bool hasSmithingPerk = activePerks.Any(p =>
                p.SkillGroup == "Smithing" &&
                (aName.Contains(p.BaseName.ToLower()) || aCategory.Contains(p.BaseName.ToLower()))
            );

            if (hasSmithingPerk)
            {
                if (level >= 41) { smithingBonus = 15; result.SmithingTierName = "(Legendary)"; }
                else if (level >= 31) { smithingBonus = 8; result.SmithingTierName = "(Epic)"; }
                else if (level >= 21) { smithingBonus = 6; result.SmithingTierName = "(Exquisite)"; }
                else if (level >= 11) { smithingBonus = 4; result.SmithingTierName = "(Superior)"; }
                else { smithingBonus = 2; result.SmithingTierName = "(Fine)"; }
            }

            // Rüstungs-Perks
            double aMult = 1.0;
            var matchingPerks = activePerks.Where(p => {
                string pGroup = (p.SkillGroup ?? "").ToLower();
                if (pGroup.Contains("heavyarmor") && aCategory.Contains("heavy")) return true;
                if (pGroup.Contains("lightarmor") && aCategory.Contains("light")) return true;
                return false;
            }).ToList();

            if (matchingPerks.Any()) aMult = matchingPerks.Max(p => p.Multiplier);

            // Finaler Rüstungswert
            result.FinalArmorRating = (stats.rating + smithingBonus) * aMult;

            // Verzauberungen
            double eMult = activePerks.Where(p => p.SkillGroup == "Enchanting").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();

            if (selectedEnch != null && selectedEnch.Name != "None")
            {
                double mag = selectedEnch.AddedValue * eMult;
                string enchText = string.Format(selectedEnch.Description, Math.Round(mag));
                result.FinalEffectText = (string.IsNullOrEmpty(stats.eff) || stats.eff == "None") ? enchText : stats.eff + " + " + enchText;
            }
            else
            {
                result.FinalEffectText = !string.IsNullOrEmpty(stats.eff) ? stats.eff : "None";
            }

            return result;
        }

        // 3. GESAMTZIEHEN (TOTAL STATS)
        public static TotalBuildStatsResult CalculateTotalStats(
            IEnumerable<EquippedItemDisplay> equippedItems,
            List<Perk> activePerks,
            StandingStone? selectedStone)
        {
            var result = new TotalBuildStatsResult();

            foreach (var item in equippedItems)
            {
                if (double.TryParse(item.Rating, out double val))
                {
                    if (item.Slot == "Weapon")
                    {
                        result.TotalDamage += val;

                        // Sneak-Multiplikator für die Gesamtanzeige
                        double sMult = 3.0; // Standard
                        string wName = item.ItemName.ToLower();

                        if (wName.Contains("dagger"))
                            sMult = activePerks.Where(p => p.SkillGroup == "SneakDagger").Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();
                        else if (wName.Contains("bow") || wName.Contains("crossbow"))
                            sMult = activePerks.Where(p => p.SkillGroup == "SneakBow").Select(p => p.Multiplier).DefaultIfEmpty(2.0).Max();
                        else
                            sMult = activePerks.Where(p => p.SkillGroup == "Sneak1H").Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();

                        result.TotalSneak += (val * sMult);
                    }
                    else
                    {
                        result.TotalArmor += val;
                    }
                }
            }

            // Findling-Bonus auf die Rüstung addieren (z.B. Fürstenstein)
            double stoneArmor = selectedStone?.BonusArmor ?? 0;
            result.TotalArmor += stoneArmor;

            return result;
        }
    }
}