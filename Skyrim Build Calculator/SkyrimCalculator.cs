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
    Enchantment? ench1,
    Enchantment? ench2 = null) // Unterstützt jetzt zwei Verzauberungen
        {
            var result = new WeaponCalculationResult();
            var stats = w.GetStatsForLevel(level);

            string wName = w.Name.ToLower();
            string wCategory = (w.Category ?? "").ToLower();

            // 1. Schmiede-Bonus
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

            // 2. Waffen-Perks (Einhand, Zweihand, Schießen)
            double wMult = 1.0;
            var matchingPerks = activePerks.Where(p => {
                string pGroup = (p.SkillGroup ?? "").ToLower();
                if (pGroup.Contains("onehanded") && (wName.Contains("dagger") || (wName.Contains("sword") && !wName.Contains("greatsword")) || wName.Contains("mace") || wName.Contains("war axe"))) return true;
                if (pGroup.Contains("twohanded") && (wName.Contains("greatsword") || wName.Contains("battleaxe") || wName.Contains("warhammer"))) return true;
                if (pGroup.Contains("archery") && (wName.Contains("bow") || wName.Contains("crossbow"))) return true;
                return false;
            }).ToList();

            if (matchingPerks.Any()) wMult = matchingPerks.Max(p => p.Multiplier);

            // 3. Finaler Schaden & Sneak Damage
            result.FinalDamage = (stats.dmg + smithingBonus) * wMult * difficultyMult;
            double roundedDamage = Math.Round(result.FinalDamage);

            double sMult = 3.0;
            if (wName.Contains("dagger"))
                sMult = activePerks.Where(p => p.SkillGroup == "SneakDagger").Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();
            else if (wName.Contains("bow") || wName.Contains("crossbow"))
                sMult = activePerks.Where(p => p.SkillGroup == "SneakBow").Select(p => p.Multiplier).DefaultIfEmpty(2.0).Max();
            else
                sMult = activePerks.Where(p => p.SkillGroup == "Sneak1H").Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();

            result.SneakDamage = roundedDamage * sMult;

            // 4. Verzauberungen berechnen
            List<string> effectTexts = new List<string>();

            // Bestehenden Waffen-Effekt hinzufügen (falls vorhanden)
            if (!string.IsNullOrEmpty(stats.eff) && stats.eff != "None")
                effectTexts.Add(stats.eff);

            // Interne Helfer-Funktion für die Berechnung einzelner Enchantments
            string ProcessEnch(Enchantment? ench)
            {
                if (ench == null || ench.Name == "None") return "";

                string eName = ench.Name.ToLower();
                double eMult = activePerks.Where(p => p.SkillGroup == "Enchanting").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();

                // Elementar-Perks anwenden
                if (activePerks.Any(p => p.Name.Contains("Fire Enchanter")) && (eName.Contains("fire") || eName.Contains("burn"))) eMult *= 1.25;
                if (activePerks.Any(p => p.Name.Contains("Frost Enchanter")) && eName.Contains("frost")) eMult *= 1.25;
                if (activePerks.Any(p => p.Name.Contains("Storm Enchanter")) && (eName.Contains("shock") || eName.Contains("lightning"))) eMult *= 1.25;

                double mag = ench.AddedValue * eMult;
                return string.Format(ench.Description, Math.Round(mag));
            }

            // Beide Slots berechnen
            string res1 = ProcessEnch(ench1);
            if (!string.IsNullOrEmpty(res1)) effectTexts.Add(res1);

            string res2 = ProcessEnch(ench2);
            if (!string.IsNullOrEmpty(res2)) effectTexts.Add(res2);

            // Texte zusammenfügen (z.B. "Feuerschaden + Frostschaden")
            result.FinalEffectText = effectTexts.Count > 0 ? string.Join(" + ", effectTexts) : "None";

            return result;
        }

        // 2. RÜSTUNGS-BERECHNUNG
        public static ArmorCalculationResult CalculateArmor(
    Armor a,
    int level,
    List<Perk> activePerks,
    Enchantment? ench1,
    Enchantment? ench2 = null) // Unterstützt jetzt zwei Slots
        {
            var result = new ArmorCalculationResult();
            var stats = a.GetStatsForLevel(level);

            string aName = a.Name.ToLower();
            string aSlot = (a.Slot ?? "").ToLower();

            // 1. Schmiede-Bonus
            double smithingBonus = 0;
            bool hasSmithingPerk = activePerks.Any(p =>
                p.SkillGroup == "Smithing" &&
                (aName.Contains(p.BaseName.ToLower()) || aSlot.Contains(p.BaseName.ToLower()))
            );

            if (hasSmithingPerk)
            {
                if (level >= 41) { smithingBonus = 15; result.SmithingTierName = "(Legendary)"; }
                else if (level >= 31) { smithingBonus = 8; result.SmithingTierName = "(Epic)"; }
                else if (level >= 21) { smithingBonus = 6; result.SmithingTierName = "(Exquisite)"; }
                else if (level >= 11) { smithingBonus = 4; result.SmithingTierName = "(Superior)"; }
                else { smithingBonus = 2; result.SmithingTierName = "(Fine)"; }
            }

            // 2. Rüstungs-Perks (Heavy/Light Armor)
            double aMult = 1.0;
            string category = a.Category?.ToLower() ?? "";
            if (category == "heavy")
                aMult = activePerks.Where(p => p.SkillGroup == "HeavyArmor").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();
            else if (category == "light")
                aMult = activePerks.Where(p => p.SkillGroup == "LightArmor").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();

            // HIER WAR DER FEHLER: stats.rating statt stats.dmg benutzen!
            result.FinalArmorRating = (stats.rating + smithingBonus) * aMult;

            // 3. Verzauberungen berechnen
            List<string> effectTexts = new List<string>();

            // Bestehenden Rüstungs-Effekt (z.B. von Quest-Items) hinzufügen
            if (!string.IsNullOrEmpty(stats.eff) && stats.eff != "None")
                effectTexts.Add(stats.eff);

            // Helfer-Funktion für Rüstungs-Enchantments
            string ProcessArmorEnch(Enchantment? ench)
            {
                if (ench == null || ench.Name == "None") return "";

                string eName = ench.Name.ToLower();
                string eDesc = ench.Description.ToLower();

                // Basis-Enchanting Perk (1.2 bis 2.0)
                double eMult = activePerks.Where(p => p.SkillGroup == "Enchanting").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();

                // Corpus Enchanter Perk (+25% auf Stats)
                if (activePerks.Any(p => p.Name.Contains("Corpus Enchanter")))
                {
                    if (eName.Contains("health") || eName.Contains("magicka") || eName.Contains("stamina"))
                        eMult *= 1.25;
                }

                // Insightful Enchanter Perk (+25% auf Skills/Rüstungs-Effekte)
                if (activePerks.Any(p => p.Name.Contains("Insightful Enchanter")))
                {
                    eMult *= 1.25;
                }

                double mag = ench.AddedValue * eMult;
                return string.Format(ench.Description, Math.Round(mag));
            }

            // Beide Slots verarbeiten
            string res1 = ProcessArmorEnch(ench1);
            if (!string.IsNullOrEmpty(res1)) effectTexts.Add(res1);

            string res2 = ProcessArmorEnch(ench2);
            if (!string.IsNullOrEmpty(res2)) effectTexts.Add(res2);

            // Effekte mit " + " verbinden
            result.FinalEffectText = effectTexts.Count > 0 ? string.Join(" + ", effectTexts) : "None";

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