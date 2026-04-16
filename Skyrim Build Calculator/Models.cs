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
        public double Weight { get; set; }
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

    // WICHTIG: Nur diese EINE Version von EquippedItem behalten!
    public class EquippedItem : System.ComponentModel.INotifyPropertyChanged
    {
        private string _rating = "";
        private string _sneakRating = "0";

        public string ItemName { get; set; } = "";
        public string Slot { get; set; } = "";
        public string Category { get; set; } = "";
        public string Enchantment { get; set; } = "";

        public object? OriginalObject { get; set; }

        public string Rating
        {
            get => _rating;
            set { if (_rating != value) { _rating = value; OnPropertyChanged(nameof(Rating)); } }
        }

        public string SneakRating
        {
            get => _sneakRating;
            set { if (_sneakRating != value) { _sneakRating = value; OnPropertyChanged(nameof(SneakRating)); } }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class Perk : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isActive;
        private bool _isAvailable = false;
        private string _availabilityHint = "Checking conditions...";

        // --- Diese Felder müssen vorhanden sein ---
        public string Name { get; set; } = "";
        public string BaseName { get; set; } = "";
        public string Description { get; set; } = "";
        public int RequiredLevel { get; set; }
        public string Category { get; set; } = "";
        public string SubCategory { get; set; } = "";
        public string SkillGroup { get; set; } = "";  // <--- War im letzten Schritt weg
        public double Multiplier { get; set; } = 1.0; // <--- War im letzten Schritt weg

        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set { if (_isAvailable != value) { _isAvailable = value; OnPropertyChanged(nameof(IsAvailable)); } }
        }

        public string AvailabilityHint
        {
            get => _availabilityHint;
            set { if (_availabilityHint != value) { _availabilityHint = value; OnPropertyChanged(nameof(AvailabilityHint)); } }
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
        public double BaseMagnitude { get; set; } // <--- Diese Zeile fehlt ihm gerade!
        public string SkillGroup { get; set; } = "Enchanting";
        public string Description { get; set; } = "";
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
        public string BuildName { get; set; } = "Dovahkiin";
        public string SelectedRace { get; set; } = "None";
        public int Level { get; set; } = 1;
        public int InvestMagicka { get; set; }
        public int InvestHealth { get; set; }
        public int InvestStamina { get; set; }
        public string SelectedStandingStone { get; set; } = "None";
        public string SelectedFleshSpell { get; set; } = "None";
        public List<string> ActivePerkNames { get; set; } = new List<string>();
        public List<EquippedItemSaveData> EquippedItems { get; set; } = new List<EquippedItemSaveData>();
    }

    public class EquippedItemSaveData
    {
        public string ItemName { get; set; } = "";
        public string Slot { get; set; } = "";
        public string Enchantment1 { get; set; } = "None";
        public string Enchantment2 { get; set; } = "None";
    }
}