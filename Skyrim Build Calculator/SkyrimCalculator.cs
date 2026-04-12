using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyrim_Build_Architect
{
    // Dieser kleine Helfer speichert die fertigen Ergebnisse, 
    // um sie als Paket an das Hauptfenster zurückzuschicken.
    public class WeaponCalculationResult
    {
        public double FinalDamage { get; set; }
        public double SneakDamage { get; set; }
        public string SmithingTierName { get; set; } = "";
        public string FinalEffectText { get; set; } = "None";
    }

    // Das ist unser neuer Taschenrechner (ohne UI, nur pure Mathematik!)
    public static class SkyrimCalculator
    {
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

            // 1. Schmiede-Bonus berechnen
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

            // 2. Waffen-Perks (Armsman etc.)
            double wMult = 1.0;
            var matchingPerks = activePerks.Where(p => {
                string pGroup = (p.SkillGroup ?? "").ToLower();
                if (pGroup.Contains("onehanded") && (wName.Contains("dagger") || (wName.Contains("sword") && !wName.Contains("greatsword")) || wName.Contains("mace") || wName.Contains("war axe"))) return true;
                if (pGroup.Contains("twohanded") && (wName.Contains("greatsword") || wName.Contains("battleaxe") || wName.Contains("warhammer"))) return true;
                if (pGroup.Contains("archery") && (wName.Contains("bow") || wName.Contains("crossbow"))) return true;
                return false;
            }).ToList();

            if (matchingPerks.Any()) wMult = matchingPerks.Max(p => p.Multiplier);

            // 3. Finaler Schaden (Die realistische Formel)
            result.FinalDamage = (stats.dmg + smithingBonus) * wMult * difficultyMult;

            // 4. Sneak-Multiplikator
            double sMult = activePerks.Where(p => p.SkillGroup != null && p.SkillGroup.Contains("Sneak")).Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();
            result.SneakDamage = result.FinalDamage * sMult;

            // 5. Verzauberungen
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
    }
}