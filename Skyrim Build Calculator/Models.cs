using System.Collections.Generic;
using System.Linq;

namespace Skyrim_Build_Architect
{
    // --- DIE DATEN-MODELLE ---
    public class Weapon
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Slot => "Weapon";
        public double Damage { get; set; }
        public double Value { get; set; }
        public double Reach { get; set; }
        public double Speed { get; set; }
        public double Stagger { get; set; }
        public string Effect { get; set; } = "";
        public bool IsEnchantable { get; set; } = true;
        public List<LeveledStat> LevelVariants { get; set; } = new List<LeveledStat>();

        public (double dmg, double val, string eff) GetStatsForLevel(int playerLevel)
        {
            if (LevelVariants == null || LevelVariants.Count == 0) return (Damage, Value, Effect);
            var match = LevelVariants.Where(v => playerLevel >= v.MinLevel).OrderByDescending(v => v.MinLevel).FirstOrDefault();
            return match != null ? (match.Damage, match.Value, match.Effect) : (Damage, Value, Effect);
        }
    }

    public class Armor
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Slot { get; set; } = "";
        public double ArmorRating { get; set; }
        public double Weight { get; set; }
        public double Value { get; set; }
        public string Effect { get; set; } = "None";
        public List<LeveledStat> LevelVariants { get; set; } = new List<LeveledStat>();

        public (double rating, double val, string eff) GetStatsForLevel(int l)
        {
            var f = LevelVariants.OrderByDescending(v => v.MinLevel).FirstOrDefault(v => l >= v.MinLevel);
            return f != null ? (f.ArmorRating, f.Value, f.Effect) : (ArmorRating, Value, Effect);
        }
    }

    public class Perk : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isActive;
        private bool _isAvailable = true;

        public string Name { get; set; } = "";
        public string BaseName { get; set; } = "";
        public string Description { get; set; } = "";
        public int RequiredLevel { get; set; }
        public string Category { get; set; } = "";    // z.B. WARRIOR
        public string SubCategory { get; set; } = ""; // z.B. One-Handed
        public string SkillGroup { get; set; } = "";
        public double Multiplier { get; set; } = 1.0;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public class WeaponCalculationResult
        {
            public double FinalDamage { get; set; }
            public double SneakDamage { get; set; }
            public string FinalEffectText { get; set; } = "";
            public string SmithingTierName { get; set; } = "";
            public double FinalStagger { get; set; } // NEU: Damit der Wert zur UI kommt
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    OnPropertyChanged(nameof(IsAvailable));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class LeveledStat
    {
        public int MinLevel { get; set; }
        public double Damage { get; set; }
        public double ArmorRating { get; set; }
        public double Value { get; set; }
        public string Effect { get; set; } = "";
    }

    public class Enchantment
    {
        public string Name { get; set; } = "";
        public double AddedValue { get; set; }
        public string SkillGroup { get; set; } = "Enchanting";
        public string Description { get; set; } = "";

        // Das muss EXAKT so heißen, damit die Fehler in MainWindow verschwinden:
        public List<string> CompatibleSlots { get; set; } = new List<string>();
    }

    public class SoulGem
    {
        public string Name { get; set; } = "";
        public double Multiplier { get; set; }
    }

    public class Difficulty
    {
        public string Name { get; set; } = "";
        public double DamageDealtMultiplier { get; set; }
        public double DamageTakenMultiplier { get; set; }
    }

    public class Race
    {
        public string Name { get; set; } = "";
        public string PassiveEffect { get; set; } = "";
        public string Power { get; set; } = "";
        public int BonusMagicka { get; set; } = 0;
    }

    public class EquippedItemDisplay
    {
        public string Slot { get; set; } = "";
        public string ItemName { get; set; } = "";
        public string Enchantment { get; set; } = "";
        public string Rating { get; set; } = "";
    }

    public class StandingStone
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int BonusMagicka { get; set; } = 0;
        public int BonusArmor { get; set; } = 0;
        public int BonusMagicResist { get; set; } = 0;
        public double MagickaRegenMult { get; set; } = 1.0;
        public double HealthRegenMult { get; set; } = 1.0;
        public double StaminaRegenMult { get; set; } = 1.0;
        public double SpellAbsorption { get; set; } = 0;
    }

    public class SavedBuild
    {
        public string BuildName { get; set; } = "";
        public string SelectedRace { get; set; } = "";
        public int Level { get; set; }
        public int InvestMagicka { get; set; }
        public int InvestHealth { get; set; }
        public int InvestStamina { get; set; }
        public List<string> ActivePerkNames { get; set; } = new List<string>();
    }
}