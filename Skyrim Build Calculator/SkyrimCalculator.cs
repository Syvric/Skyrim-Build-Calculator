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
        public string FinalEffectText { get; set; } = "";
        public string SmithingTierName { get; set; } = "";
        public double FinalStagger { get; set; }
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
        public double FinalWeight { get; set; } // Neu
        public string SmithingTierName { get; set; } = "";
        public string FinalEffectText { get; set; } = "None";
    }

    // --- DER TASCHENRECHNER ---

    public static class SkyrimCalculator
    {
        // ==========================================
        // 1. WAFFEN-BERECHNUNG
        // ==========================================
        public static WeaponCalculationResult CalculateWeapon(
     Weapon w,
     int level,
     List<Perk> activePerks,
     double difficultyMult,
     Enchantment? ench1,
     Enchantment? ench2,
     double gemMult)
        {
            var result = new WeaponCalculationResult();
            var stats = w.GetStatsForLevel(level);

            string wName = w.Name.ToLower();
            string wCategory = (w.Category ?? "").ToLower();

            double currentDamage = stats.dmg;

            // 1. BOUND WEAPONS (Mystic Binding)
            if (wName.Contains("bound") && activePerks.Any(p => p.Name == "Mystic Binding"))
            {
                if (wName.Contains("dagger")) currentDamage = 10;
                else if (wName.Contains("sword")) currentDamage = 14;
                else if (wName.Contains("battleaxe")) currentDamage = 24;
                else if (wName.Contains("bow")) currentDamage = 24;
            }

            // 2. SCHMIEDE-BONUS
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

            // 3. WAFFEN-PERKS (Stacking Fix)
            double wMult = 1.0;
            var matchingPerks = activePerks.Where(p => {
                string pSub = (p.SubCategory ?? "").ToLower();
                if (pSub.Contains("one-handed") && (wName.Contains("dagger") || (wName.Contains("sword") && !wName.Contains("greatsword")) || wName.Contains("mace") || wName.Contains("war axe") || wName.Contains("pickaxe"))) return true;
                if (pSub.Contains("two-handed") && (wName.Contains("greatsword") || wName.Contains("battleaxe") || wName.Contains("warhammer"))) return true;
                if (pSub.Contains("archery") && (wName.Contains("bow") || wName.Contains("crossbow"))) return true;
                return false;
            }).ToList();

            foreach (var group in matchingPerks.GroupBy(p => p.BaseName))
            {
                wMult *= group.Max(p => p.Multiplier);
            }

            // 4. FINALER SCHADEN & SNEAK
            result.FinalDamage = (currentDamage + smithingBonus) * wMult * difficultyMult;
            double roundedDamage = Math.Round(result.FinalDamage);

            double sMult = 3.0;
            if (wName.Contains("dagger"))
                sMult = activePerks.Where(p => p.SkillGroup == "SneakDagger").Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();
            else if (wName.Contains("bow") || wName.Contains("crossbow"))
                sMult = activePerks.Where(p => p.SkillGroup == "SneakBow").Select(p => p.Multiplier).DefaultIfEmpty(2.0).Max();
            else
                sMult = activePerks.Where(p => p.SkillGroup == "Sneak1H").Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();

            result.SneakDamage = roundedDamage * sMult;

            // 5. STAGGER
            result.FinalStagger = w.Stagger;
            if (activePerks.Any(p => p.Name == "Power Shot") && wCategory.Contains("archery"))
                result.FinalStagger = Math.Max(result.FinalStagger, 0.25);

            // ==========================================
            // 6. SPEZIAL-TEXTE FÜR PERKS (NEU!)
            // ==========================================
            List<string> effectTexts = new List<string>();

            // Basis-Effekt der Waffe (falls vorhanden)
            if (!string.IsNullOrEmpty(stats.eff) && stats.eff != "None")
                effectTexts.Add(stats.eff);

            // Schwert-Perks (Bladesman / Deep Wounds)
            if (wName.Contains("sword") || wName.Contains("greatsword"))
            {
                var p = activePerks.FirstOrDefault(ap => ap.BaseName == "Bladesman" || ap.BaseName == "Deep Wounds");
                if (p != null)
                {
                    if (p.Name.Contains("1/3")) effectTexts.Add("10% Crit Chance");
                    else if (p.Name.Contains("2/3")) effectTexts.Add("15% Crit Chance");
                    else if (p.Name.Contains("3/3")) effectTexts.Add("20% Crit Chance");
                }
            }
            // Streitkolben-Perks (Bone Breaker / Skullcrusher)
            if (wName.Contains("mace") || wName.Contains("warhammer"))
            {
                var p = activePerks.FirstOrDefault(ap => ap.BaseName == "Bone Breaker" || ap.BaseName == "Skullcrusher");
                if (p != null)
                {
                    if (p.Name.Contains("1/3")) effectTexts.Add("Ignores 25% Armor");
                    else if (p.Name.Contains("2/3")) effectTexts.Add("Ignores 50% Armor");
                    else if (p.Name.Contains("3/3")) effectTexts.Add("Ignores 75% Armor");
                }
            }
            // Axt-Perks (Hack and Slash / Limbsplitter)
            if (wName.Contains("war axe") || wName.Contains("battleaxe"))
            {
                if (activePerks.Any(ap => ap.BaseName == "Hack and Slash" || ap.BaseName == "Limbsplitter"))
                    effectTexts.Add("Causes extra bleeding damage");
            }

            // 7. VERZAUBERUNGEN (Unverändert, aber integriert)
            string ProcessEnch(Enchantment? ench)
            {
                if (ench == null || ench.Name == "None") return "";
                string eName = ench.Name.ToLower();
                double eMult = activePerks.Where(p => p.SkillGroup == "Enchanting").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();

                bool isFire = eName.Contains("fire") || eName.Contains("burn") || eName.Contains("chaos");
                bool isFrost = eName.Contains("frost") || eName.Contains("chaos");
                bool isShock = eName.Contains("shock") || eName.Contains("lightning") || eName.Contains("chaos");

                if (isFire && activePerks.Any(p => p.Name.Contains("Fire Enchanter"))) eMult *= 1.25;
                if (isFrost && activePerks.Any(p => p.Name.Contains("Frost Enchanter"))) eMult *= 1.25;
                if (isShock && activePerks.Any(p => p.Name.Contains("Storm Enchanter"))) eMult *= 1.25;

                if (isFire) eMult *= activePerks.Where(p => p.BaseName == "Augmented Flames").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();
                if (isFrost) eMult *= activePerks.Where(p => p.BaseName == "Augmented Frost").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();
                if (isShock) eMult *= activePerks.Where(p => p.BaseName == "Augmented Shock").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();

                if (isFrost && wName.Contains("stalhrim")) eMult *= 1.25;

                double mag = ench.AddedValue * eMult * gemMult;
                return string.Format(ench.Description, Math.Round(mag));
            }

            string res1 = ProcessEnch(ench1); if (!string.IsNullOrEmpty(res1)) effectTexts.Add(res1);
            string res2 = ProcessEnch(ench2); if (!string.IsNullOrEmpty(res2)) effectTexts.Add(res2);

            result.FinalEffectText = effectTexts.Count > 0 ? string.Join(" + ", effectTexts) : "None";

            return result;
        }

        // ==========================================
        // 2. RÜSTUNGS-BERECHNUNG
        // ==========================================
        public static ArmorCalculationResult CalculateArmor(
    Armor a,
    int level,
    List<Perk> activePerks,
    Enchantment? ench1,
    Enchantment? ench2,
    double gemMult)
        {
            var result = new ArmorCalculationResult();
            var stats = a.GetStatsForLevel(level);

            string aSlot = (a.Slot ?? "").ToLower();
            string category = a.Category?.ToLower() ?? "";

            // 1. SCHMIEDE-BONUS (Bleibt gleich)
            double smithingBonus = 0;
            bool hasSmithingPerk = activePerks.Any(p =>
                p.SkillGroup == "Smithing" &&
                (a.Name.ToLower().Contains(p.BaseName.ToLower()) || aSlot.Contains(p.BaseName.ToLower()))
            );

            if (hasSmithingPerk)
            {
                if (level >= 41) { smithingBonus = 15; result.SmithingTierName = "(Legendary)"; }
                else if (level >= 31) { smithingBonus = 8; result.SmithingTierName = "(Epic)"; }
                else if (level >= 21) { smithingBonus = 6; result.SmithingTierName = "(Exquisite)"; }
                else if (level >= 11) { smithingBonus = 4; result.SmithingTierName = "(Superior)"; }
                else { smithingBonus = 2; result.SmithingTierName = "(Fine)"; }
            }

            // 2. RÜSTUNGS-PERKS (FIX: Shield Wall entfernt!)
            double aMult = 1.0;

            // NUR wenn es kein Schild ist, wirken die Rüstungs-Multiplikatoren (Juggernaut etc.)
            if (aSlot != "shield")
            {
                string targetGroup = category.Contains("heavy") ? "HeavyArmor" : (category.Contains("light") ? "LightArmor" : "");
                if (!string.IsNullOrEmpty(targetGroup))
                {
                    var armorPerks = activePerks.Where(p => p.SkillGroup == targetGroup).ToList();
                    foreach (var group in armorPerks.GroupBy(p => p.BaseName))
                        aMult *= group.Max(p => p.Multiplier);
                }
            }

            // Der finale Rüstungswert ist jetzt 1:1 wie im Skyrim-Inventar
            result.FinalArmorRating = (stats.rating + smithingBonus) * aMult;

            // 3. GEWICHTS-LOGIK (Bleibt so, das ist korrekt)
            result.FinalWeight = a.Weight;
            bool hasWeightlessPerk = (category.Contains("heavy") && activePerks.Any(p => p.Name.Contains("Conditioning"))) ||
                                     (category.Contains("light") && activePerks.Any(p => p.Name.Contains("Unhindered")));
            if (hasWeightlessPerk) result.FinalWeight = 0;

            // 4. VERZAUBERUNGEN (Bleibt gleich)
            // ... (dein restlicher Code für Enchantments)

            return result;
        }

        // ==========================================
        // 3. GESAMTZIEHEN (TOTAL STATS)
        // ==========================================
        public static TotalBuildStatsResult CalculateTotalStats(
            IEnumerable<EquippedItem> equippedItems,
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

                        double sMult = 3.0;
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

            double stoneArmor = selectedStone?.BonusArmor ?? 0;
            result.TotalArmor += stoneArmor;

            return result;
        }
    }
}