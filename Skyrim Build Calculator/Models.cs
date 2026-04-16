using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Skyrim_Build_Architect
{
    // --- BASIS KLASSEN ---

    public class Weapon
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public double Damage { get; set; }
        public double Weight { get; set; }
        public double Value { get; set; }
        public double Reach { get; set; }
        public double Speed { get; set; }
        public double Stagger { get; set; }
        public string Effect { get; set; } = "";
        public bool IsEnchantable { get; set; } = true;

        public int MaxCharges { get; set; }
        public string MagicSchool { get; set; } = "";
        public string Element { get; set; } = "";

        public List<LeveledStat> LevelVariants { get; set; } = new();

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
        public List<LeveledStat> LevelVariants { get; set; } = new();

        public (double rating, double val, string eff) GetStatsForLevel(int l)
        {
            var f = LevelVariants.OrderByDescending(v => v.MinLevel).FirstOrDefault(v => l >= v.MinLevel);
            return f != null ? (f.ArmorRating, f.Value, f.Effect) : (ArmorRating, Value, Effect);
        }
    }

    // --- UI MODELLE ---

    public class EquippedItem : INotifyPropertyChanged
    {
        private string _rating = "";
        private string _enchantment = "";
        private string _sneakRating = "0";

        public string ItemName { get; set; } = "";
        public string Slot { get; set; } = "";
        public string Category { get; set; } = "";
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
        public string Enchantment
        {
            get => _enchantment;
            set { if (_enchantment != value) { _enchantment = value; OnPropertyChanged(nameof(Enchantment)); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class Perk : INotifyPropertyChanged
    {
        private bool _isActive;
        private bool _isAvailable;
        private string _availabilityHint = "";

        public string Name { get; set; } = "";
        public string BaseName { get; set; } = "";
        public string Description { get; set; } = "";
        public int RequiredLevel { get; set; }
        public string Category { get; set; } = "";
        public string SubCategory { get; set; } = "";
        public string SkillGroup { get; set; } = "";
        public double Multiplier { get; set; } = 1.0;

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // --- HILFSKLASSEN ---

    public class Enchantment
    {
        public string Name { get; set; } = "";
        public double BaseMagnitude { get; set; }
        public double AddedValue { get; set; }
        public string Description { get; set; } = "";
        public string SkillGroup { get; set; } = "";
        public List<string> CompatibleSlots { get; set; } = new();
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
        public override string ToString() => Name;
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

    public class LeveledStat
    {
        public int MinLevel { get; set; }
        public double Damage { get; set; }
        public double ArmorRating { get; set; }
        public double Value { get; set; }
        public string Effect { get; set; } = "";
    }

    // --- SPEICHER STRUKTUREN ---
    // Hier existiert jetzt alles garantiert nur ein einziges Mal!

    public class BuildSaveData
    {
        public string BuildName { get; set; } = "";
        public string Level { get; set; } = "1";
        public string SelectedRace { get; set; } = "";
        public string SelectedDifficulty { get; set; } = "";
        public string SelectedStandingStone { get; set; } = "";
        public string SelectedFleshSpell { get; set; } = "";

        public string InvestMagicka { get; set; } = "0";
        public string InvestHealth { get; set; } = "0";
        public string InvestStamina { get; set; } = "0";

        public List<string> ActivePerkNames { get; set; } = new();
        public List<EquippedItemSaveData> EquippedItems { get; set; } = new();
    }

    public class EquippedItemSaveData
    {
        public string Slot { get; set; } = "";
        public string ItemName { get; set; } = "";
        public string Category { get; set; } = "";
        public string Rating { get; set; } = "";
        public string Enchantment { get; set; } = "";
    }

    public class SavedBuild : BuildSaveData { }
}