using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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
        public List<string> AllowedSlots { get; set; } = new List<string>();
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

namespace Skyrim_Build_Architect
{
    public partial class MainWindow : Window
    {
        // --- SPERRE ---
        private bool _isCalculating = false;

        // --- DATENBANKEN ---
        public List<Weapon> WeaponDatabase { get; set; } = new List<Weapon>();
        public List<Armor> ArmorDatabase { get; set; } = new List<Armor>();
        public List<Enchantment> EnchantmentDatabase { get; set; } = new List<Enchantment>();
        public List<SoulGem> SoulGemDatabase { get; set; } = new List<SoulGem>();
        public List<Perk> PerkDatabase { get; set; } = new List<Perk>();
        public List<Difficulty> DifficultyDatabase { get; set; } = new List<Difficulty>();
        public List<Race> RaceDatabase { get; set; } = new List<Race>();
        public List<StandingStone> StandingStoneDatabase { get; set; } = new List<StandingStone>();

        // --- AKTUELLES EQUIPMENT ---
        public Dictionary<string, EquippedItemDisplay?> CurrentBuild = new Dictionary<string, EquippedItemDisplay?>()
        {
            { "Weapon", null }, { "Shield", null }, { "Head", null },
            { "Chest", null }, { "Hands", null }, { "Feet", null },
            { "Ring", null }, { "Necklace", null }
        };

        private bool isWeaponMode = true;

        public MainWindow()
        {
            InitializeComponent();

            LoadData();
            LoadRaceData();
            LoadStandingStones();

            var view = (CollectionView)CollectionViewSource.GetDefaultView(PerkDatabase);
            if (view != null)
            {
                view.GroupDescriptions.Clear();
                view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                view.GroupDescriptions.Add(new PropertyGroupDescription("SubCategory"));
                LstPerks.ItemsSource = view;
            }

            CmbSoulGem.ItemsSource = SoulGemDatabase;
            CmbDifficulty.ItemsSource = DifficultyDatabase;
            CmbRace.ItemsSource = RaceDatabase;
            CmbStandingStone.ItemsSource = StandingStoneDatabase;

            if (CmbDifficulty.Items.Count > 0) CmbDifficulty.SelectedIndex = 2; // Adept
            CmbRace.SelectedIndex = 0;
            CmbStandingStone.SelectedIndex = 0;

            this.Loaded += (s, e) =>
            {
                CalculateAttributes();
                UpdateTotalBuildStats();
                BtnShowWeapons_Click(this, new RoutedEventArgs());
                if (view != null) view.Refresh();
            };
        }

        // --- CHARACTER CORE LOGIK ---

        private void LoadRaceData()
        {
            RaceDatabase.Clear();
            RaceDatabase.Add(new Race { Name = "None", PassiveEffect = "No passive effect selected.", Power = "None", BonusMagicka = 0 });
            RaceDatabase.Add(new Race { Name = "Altmer (High Elf)", PassiveEffect = "+50 Base Magicka", Power = "Highborn", BonusMagicka = 50 });
            RaceDatabase.Add(new Race { Name = "Argonian", PassiveEffect = "Waterbreathing, 50% Disease Resist", Power = "Histskin" });
            RaceDatabase.Add(new Race { Name = "Bosmer (Wood Elf)", PassiveEffect = "50% Poison/Disease Resist", Power = "Command Animal" });
            RaceDatabase.Add(new Race { Name = "Breton", PassiveEffect = "25% Magic Resistance", Power = "Dragonskin" });
            RaceDatabase.Add(new Race { Name = "Dunmer (Dark Elf)", PassiveEffect = "50% Fire Resistance", Power = "Ancestor's Wrath" });
            RaceDatabase.Add(new Race { Name = "Imperial", PassiveEffect = "Find more Gold", Power = "Voice of the Emperor" });
            RaceDatabase.Add(new Race { Name = "Khajiit", PassiveEffect = "Claw Damage, Night Eye", Power = "Night Eye" });
            RaceDatabase.Add(new Race { Name = "Nord", PassiveEffect = "50% Frost Resistance", Power = "Battle Cry" });
            RaceDatabase.Add(new Race { Name = "Orc", PassiveEffect = "None", Power = "Berserker Rage" });
            RaceDatabase.Add(new Race { Name = "Redguard", PassiveEffect = "50% Poison Resistance", Power = "Adrenaline Rush" });
        }

        private void CmbRace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbRace.SelectedItem is Race selectedRace)
            {
                TxtRacePassive.Text = selectedRace.PassiveEffect;
                TxtRacePower.Text = selectedRace.Power;
                CalculateAttributes();
            }
        }

        private void TxtPlayerLevel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (string.IsNullOrWhiteSpace(tb.Text) || tb.Text == "0")
                {
                    tb.Text = "1";
                    tb.SelectAll();
                }
                else if (int.TryParse(tb.Text, out int value))
                {
                    string cleanValue = value.ToString();
                    if (tb.Text != cleanValue)
                    {
                        tb.Text = cleanValue;
                        tb.CaretIndex = tb.Text.Length;
                    }
                }
            }
            CalculateAttributes();
            UpdateCalculations();
        }

        private void Attribute_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = "0";
                    tb.SelectAll();
                }
                else if (int.TryParse(tb.Text, out int value))
                {
                    string cleanValue = value.ToString();
                    if (tb.Text != cleanValue)
                    {
                        tb.Text = cleanValue;
                        tb.CaretIndex = tb.Text.Length;
                    }
                }
            }
            CalculateAttributes();
        }

        private void OnlyNumbers_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NoSpaces_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space) e.Handled = true;
        }

        private void CalculateAttributes()
        {
            if (_isCalculating || !IsLoaded || TxtPlayerLevel == null || CmbRace == null || CmbStandingStone == null || LblSoulGem == null) return;

            try
            {
                _isCalculating = true;

                int.TryParse(TxtPlayerLevel.Text, out int level);
                if (level < 1) level = 1;

                int.TryParse(TxtInvestMagicka.Text, out int investMagicka);
                int.TryParse(TxtInvestHealth.Text, out int investHealth);
                int.TryParse(TxtInvestStamina.Text, out int investStamina);

                var race = CmbRace.SelectedItem as Race;
                var stone = CmbStandingStone.SelectedItem as StandingStone;

                TxtRacePassive.Text = race?.PassiveEffect ?? "-";
                TxtRacePower.Text = race?.Power ?? "-";
                TxtStoneDescription.Text = stone?.Description ?? "No stone selected.";

                int perksSpent = PerkDatabase?.Count(p => p.IsActive) ?? 0;
                int totalAvailablePoints = level - 1;
                int pointsSpent = investMagicka + investHealth + investStamina + perksSpent;

                LblAvailablePoints.Text = $"Stat Points: {pointsSpent} / {totalAvailablePoints}";
                LblAvailablePoints.Foreground = (pointsSpent > totalAvailablePoints)
                    ? Brushes.Red
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1B80D"));

                double totalMagicka = 100 + (race?.BonusMagicka ?? 0) + (investMagicka * 10) + (stone?.BonusMagicka ?? 0);
                double totalHealth = 100 + (investHealth * 10);
                double totalStamina = 100 + (investStamina * 10);

                TxtTotalMagicka.Text = totalMagicka.ToString("0");
                TxtTotalHealth.Text = totalHealth.ToString("0");
                TxtTotalStamina.Text = totalStamina.ToString("0");

                double mRegenMult = stone?.MagickaRegenMult ?? 1.0;
                double hRegenMult = stone?.HealthRegenMult ?? 1.0;
                double sRegenMult = stone?.StaminaRegenMult ?? 1.0;

                TxtRegenMagicka.Text = $"(+{(totalMagicka * 0.03 * mRegenMult):0.0} /s)";
                TxtRegenHealth.Text = $"(+{(totalHealth * 0.005 * hRegenMult):0.0} /s)";
                TxtRegenStamina.Text = $"(+{(totalStamina * 0.05 * sRegenMult):0.0} /s)";

                if (PerkDatabase != null)
                {
                    foreach (var perk in PerkDatabase)
                    {
                        bool shouldBeAvailable = level >= perk.RequiredLevel;
                        if (perk.IsAvailable != shouldBeAvailable) perk.IsAvailable = shouldBeAvailable;
                        if (!perk.IsAvailable && perk.IsActive) perk.IsActive = false;
                    }

                    if (TxtWarriorCount != null) TxtWarriorCount.Text = $"Warrior: {PerkDatabase.Count(p => p.IsActive && p.Category == "WARRIOR")}";
                    if (TxtThiefCount != null) TxtThiefCount.Text = $"Thief: {PerkDatabase.Count(p => p.IsActive && p.Category == "THIEF")}";
                    if (TxtMageCount != null) TxtMageCount.Text = $"Mage: {PerkDatabase.Count(p => p.IsActive && p.Category == "MAGE")}";
                }
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private void BtnEquip_Click(object sender, RoutedEventArgs e)
        {
            var selected = CmbSelect.SelectedItem;
            if (selected == null || selected.ToString() == "None") return;

            var newItem = new EquippedItemDisplay();

            if (isWeaponMode && selected is Weapon w)
            {
                newItem.Slot = "Weapon";
                newItem.ItemName = w.Name;
                newItem.Enchantment = CmbEnchantment.SelectedItem != null ? TxtEffect.Text : "None";
                newItem.Rating = TxtDamageDisplay.Text;
                CurrentBuild["Weapon"] = newItem;
            }
            else if (!isWeaponMode && selected is Armor a)
            {
                if (CurrentBuild.ContainsKey(a.Slot))
                {
                    newItem.Slot = a.Slot;
                    newItem.ItemName = a.Name;
                    newItem.Enchantment = CmbEnchantment.SelectedItem != null ? TxtArmorEffect.Text : "None";
                    newItem.Rating = TxtArmorDisplay.Text;
                    CurrentBuild[a.Slot] = newItem;
                }
            }

            UpdateTotalBuildStats();
        }

        private void LstBuildItems_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LstBuildItems.SelectedItem is EquippedItemDisplay selectedItem)
            {
                if (CurrentBuild.ContainsKey(selectedItem.Slot))
                {
                    CurrentBuild[selectedItem.Slot] = null;
                    UpdateTotalBuildStats();
                }
            }
        }

        // --- HIER IST DIE SAUBERE UPDATE TOTAL STATS METHODE ---
        private void UpdateTotalBuildStats()
        {
            var sortedDisplayList = new List<EquippedItemDisplay>();
            string[] slotOrder = { "Weapon", "Head", "Necklace", "Chest", "Hands", "Ring", "Feet", "Shield" };

            // 1. Ausgerüstete Items in die richtige Reihenfolge bringen
            foreach (var slotName in slotOrder)
            {
                if (CurrentBuild.ContainsKey(slotName) && CurrentBuild[slotName] != null)
                {
                    sortedDisplayList.Add(CurrentBuild[slotName]!);
                }
            }

            // 2. Den Taschenrechner rufen! (Keine Mathematik mehr hier!)
            var activePerks = PerkDatabase.Where(p => p.IsActive).ToList();
            var stone = CmbStandingStone.SelectedItem as StandingStone;

            // WICHTIG: Erfordert die Methode "CalculateTotalStats" in SkyrimCalculator.cs
            var stats = SkyrimCalculator.CalculateTotalStats(sortedDisplayList, activePerks, stone);

            // 3. UI updaten
            if (TxtTotalArmor != null) TxtTotalArmor.Text = Math.Round(stats.TotalArmor).ToString();
            if (TxtTotalDamage != null) TxtTotalDamage.Text = Math.Round(stats.TotalDamage).ToString();
            if (TxtSneakDamage != null) TxtSneakDamage.Text = stats.TotalSneak.ToString("0");

            if (LstBuildItems != null)
            {
                LstBuildItems.ItemsSource = null;
                LstBuildItems.ItemsSource = sortedDisplayList;
            }
        }

        // --- HIER IST DIE NEUE, KOMPLETTE UPDATE CALCULATIONS METHODE ---
        private void UpdateCalculations()
        {
            if (CmbSelect == null || CmbEnchantment == null || CmbDifficulty == null || CmbSelect.SelectedItem == null) return;

            CalculateAttributes();

            int level = int.TryParse(TxtPlayerLevel.Text, out int l) ? l : 1;
            var active = PerkDatabase.Where(p => p.IsActive).ToList();
            var selectedEnch = CmbEnchantment.SelectedItem as Enchantment;

            var diff = CmbDifficulty.SelectedItem as Difficulty;
            double dMult = diff?.DamageDealtMultiplier ?? 1.0;

            if (isWeaponMode && CmbSelect.SelectedItem is Weapon w)
            {
                if (w.Name == "None")
                {
                    TxtWeaponCategory.Text = "-";
                    TxtDamageDisplay.Text = "-";
                    TxtSneakDisplay.Text = "-";
                    TxtReach.Text = "-";
                    TxtSpeed.Text = "-";
                    TxtStagger.Text = "-";
                    TxtValue.Text = "-";
                    TxtEffect.Text = "None";
                    return;
                }

                // Waffe berechnen
                var calcResult = SkyrimCalculator.CalculateWeapon(w, level, active, dMult, selectedEnch);

                TxtWeaponCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                         ? w.Category
                                         : $"{w.Category} {calcResult.SmithingTierName}";

                TxtDamageDisplay.Text = Math.Round(calcResult.FinalDamage).ToString();
                TxtSneakDisplay.Text = Math.Round(calcResult.SneakDamage).ToString();
                TxtEffect.Text = calcResult.FinalEffectText;

                var stats = w.GetStatsForLevel(level);
                TxtReach.Text = w.Reach > 0 ? w.Reach.ToString() : "-";
                TxtSpeed.Text = w.Speed > 0 ? w.Speed.ToString() : "-";
                TxtStagger.Text = w.Stagger > 0 ? w.Stagger.ToString() : "-";
                TxtValue.Text = Math.Round(stats.val).ToString();
            }
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a)
            {
                if (a.Name == "None")
                {
                    TxtArmorCategory.Text = "-";
                    TxtArmorDisplay.Text = "-";
                    TxtArmorWeight.Text = "-";
                    TxtArmorValue.Text = "-";
                    TxtArmorEffect.Text = "None";
                    return;
                }

                // Rüstung berechnen
                var calcResult = SkyrimCalculator.CalculateArmor(a, level, active, selectedEnch);

                TxtArmorCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                         ? a.Slot
                                         : $"{a.Slot} {calcResult.SmithingTierName}";

                TxtArmorDisplay.Text = Math.Round(calcResult.FinalArmorRating).ToString();
                TxtArmorEffect.Text = calcResult.FinalEffectText;

                var stats = a.GetStatsForLevel(level);
                TxtArmorWeight.Text = a.Weight > 0 ? a.Weight.ToString() : "-";
                TxtArmorValue.Text = Math.Round(stats.val).ToString();
            }
        }

        private void ApplyItemFilter()
        {
            if (TxtSearch == null || CmbSelect == null) return;
            string search = TxtSearch.Text.ToLower();

            if (isWeaponMode)
            {
                var filtered = WeaponDatabase.Where(w => w.Name.ToLower().Contains(search)).ToList();
                filtered.Insert(0, new Weapon { Name = "None", Category = "-", Damage = 0 });
                CmbSelect.ItemsSource = filtered;
            }
            else
            {
                var filtered = ArmorDatabase.Where(a => a.Name.ToLower().Contains(search)).ToList();
                filtered.Insert(0, new Armor { Name = "None", Category = "-", ArmorRating = 0 });
                CmbSelect.ItemsSource = filtered;
            }

            CmbSelect.SelectedIndex = 0;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyItemFilter();

        private void TxtPerkSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string f = TxtPerkSearch.Text.ToLower();
            var v = CollectionViewSource.GetDefaultView(PerkDatabase);
            if (v != null) v.Filter = o => string.IsNullOrEmpty(f) ||
                ((Perk)o).Name.ToLower().Contains(f) || ((Perk)o).Category.ToLower().Contains(f);
        }

        private void CmbEnchantment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = CmbEnchantment.SelectedItem;
            string effectText = "None";

            if (selected != null && selected.ToString() != "None")
            {
                dynamic enc = selected;
                try { effectText = enc.Description ?? enc.Name; }
                catch { effectText = enc.Name; }
            }

            if (isWeaponMode)
            {
                if (TxtEffect != null) TxtEffect.Text = effectText;
            }
            else
            {
                if (TxtArmorEffect != null) TxtArmorEffect.Text = effectText;
            }

            UpdateCalculations();
        }

        private void CmbSoulGem_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateCalculations();

        private void BtnShowWeapons_Click(object sender, RoutedEventArgs e)
        {
            isWeaponMode = true;
            WeaponPanel.Visibility = Visibility.Visible;
            ArmorPanel.Visibility = Visibility.Collapsed;

            if (LblSoulGem != null) LblSoulGem.Visibility = Visibility.Collapsed;
            if (CmbSoulGem != null) CmbSoulGem.Visibility = Visibility.Collapsed;

            CmbEnchantment.SelectedIndex = 0;
            ApplyItemFilter();
        }

        private void BtnShowArmor_Click(object sender, RoutedEventArgs e)
        {
            isWeaponMode = false;
            WeaponPanel.Visibility = Visibility.Collapsed;
            ArmorPanel.Visibility = Visibility.Visible;

            if (LblSoulGem != null) LblSoulGem.Visibility = Visibility.Visible;
            if (CmbSoulGem != null)
            {
                CmbSoulGem.Visibility = Visibility.Visible;
                CmbSoulGem.SelectedIndex = 0;
            }

            ApplyItemFilter();
        }

        // --- HIER IST DIE AUFGERÄUMTE SELECTION CHANGED METHODE ---
        private void CmbSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbSelect.SelectedItem == null) return;

            if (isWeaponMode && CmbSelect.SelectedItem is Weapon w)
            {
                CmbEnchantment.IsEnabled = w.IsEnchantable;
                if (!w.IsEnchantable) CmbEnchantment.SelectedIndex = 0;
            }
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a)
            {
                CmbEnchantment.IsEnabled = true;
                // UI-Updates entfernt, passiert jetzt sauber in UpdateCalculations!
            }

            UpdateEnchantmentList();
            UpdateCalculations();
        }

        private void CmbDifficulty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDifficulty == null || TxtIncomingDamage == null) return;

            var selectedDiff = CmbDifficulty.SelectedItem;
            if (selectedDiff == null) return;

            string diffName = "";
            try { diffName = ((dynamic)selectedDiff).Name; }
            catch { diffName = selectedDiff.ToString() ?? ""; }

            double incomingMultiplier = 1.0;

            if (diffName.Contains("Novice") || diffName.Contains("Novize")) incomingMultiplier = 0.5;
            else if (diffName.Contains("Apprentice") || diffName.Contains("Lehrling")) incomingMultiplier = 0.75;
            else if (diffName.Contains("Adept")) incomingMultiplier = 1.0;
            else if (diffName.Contains("Expert") || diffName.Contains("Experte")) incomingMultiplier = 1.5;
            else if (diffName.Contains("Master") || diffName.Contains("Meister")) incomingMultiplier = 2.0;
            else if (diffName.Contains("Legendary") || diffName.Contains("Legendär")) incomingMultiplier = 3.0;

            TxtIncomingDamage.Text = (incomingMultiplier * 100).ToString("0") + "%";

            if (incomingMultiplier > 1.0)
                TxtIncomingDamage.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4444"));
            else if (incomingMultiplier < 1.0)
                TxtIncomingDamage.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#44FF44"));
            else
                TxtIncomingDamage.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E1B80D"));

            UpdateCalculations();
        }

        private void UpdateEnchantmentList()
        {
            if (CmbSelect.SelectedItem == null) return;

            List<Enchantment> filteredList = new List<Enchantment>();
            var noneEnch = EnchantmentDatabase.FirstOrDefault(e => e.Name == "None");
            if (noneEnch != null) filteredList.Add(noneEnch);

            if (isWeaponMode && CmbSelect.SelectedItem is Weapon w)
            {
                var weaponsEnchs = EnchantmentDatabase.Where(e => e.AllowedSlots != null && e.AllowedSlots.Contains("Weapon"));
                filteredList.AddRange(weaponsEnchs);
            }
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a)
            {
                var armorEnchs = EnchantmentDatabase.Where(e => e.AllowedSlots != null && e.AllowedSlots.Contains(a.Slot));
                filteredList.AddRange(armorEnchs);
            }

            CmbEnchantment.ItemsSource = filteredList;
            if (filteredList.Count > 0) CmbEnchantment.SelectedIndex = 0;
        }

        private void LoadStandingStones()
        {
            StandingStoneDatabase.Clear();
            StandingStoneDatabase.Add(new StandingStone { Name = "None", Description = "No stone selected." });

            StandingStoneDatabase.Add(new StandingStone { Name = "The Mage Stone", Description = "Magic skills advance 20% faster." });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Thief Stone", Description = "Thief skills advance 20% faster." });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Warrior Stone", Description = "Warrior skills advance 20% faster." });

            StandingStoneDatabase.Add(new StandingStone { Name = "The Apprentice Stone", Description = "+100% Magicka Regen, -50% Magic Resistance", MagickaRegenMult = 2.0, BonusMagicResist = -50 });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Atronach Stone", Description = "+50 Magicka, 50% Spell Absorption, -50% Magicka Regen", BonusMagicka = 50, SpellAbsorption = 50, MagickaRegenMult = 0.5 });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Lady Stone", Description = "+25% Health & Stamina Regen", HealthRegenMult = 1.25, StaminaRegenMult = 1.25 });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Lord Stone", Description = "+50 Armor, +25% Magic Resistance", BonusArmor = 50, BonusMagicResist = 25 });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Lover Stone", Description = "All skills advance 15% faster." });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Ritual Stone", Description = "Reanimate all nearby corpses to fight for you once a day." });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Serpent Stone", Description = "Paralyze target for 5s and do 25 poison damage once a day." });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Shadow Stone", Description = "Invisibility for 60s once a day." });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Steed Stone", Description = "+100 Carry Weight, Equipped armor weighs nothing." });
            StandingStoneDatabase.Add(new StandingStone { Name = "The Tower Stone", Description = "Unlock any Expert level lock or lower once a day." });
        }

        private void CmbStandingStone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCalculations();
            UpdateTotalBuildStats();
        }

        private void Perk_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_isCalculating) return;
            UpdateCalculations();
        }

        private void BtnSaveBuild_Click(object sender, RoutedEventArgs e)
        {
            var build = new SavedBuild
            {
                BuildName = "My Build",
                SelectedRace = (CmbRace.SelectedItem as Race)?.Name ?? "None",
                Level = int.TryParse(TxtPlayerLevel.Text, out int l) ? l : 1,
                InvestMagicka = int.TryParse(TxtInvestMagicka.Text, out int m) ? m : 0,
                InvestHealth = int.TryParse(TxtInvestHealth.Text, out int h) ? h : 0,
                InvestStamina = int.TryParse(TxtInvestStamina.Text, out int s) ? s : 0,
                ActivePerkNames = PerkDatabase.Where(p => p.IsActive).Select(p => p.Name).ToList()
            };

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Skyrim Build (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == true)
            {
                string jsonString = JsonSerializer.Serialize(build, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFileDialog.FileName, jsonString);
                MessageBox.Show("Build erfolgreich gespeichert!");
            }
        }

        private void BtnLoadBuild_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Skyrim Build (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                string jsonString = File.ReadAllText(openFileDialog.FileName);
                var build = JsonSerializer.Deserialize<SavedBuild>(jsonString);

                if (build != null)
                {
                    TxtPlayerLevel.Text = build.Level.ToString();
                    TxtInvestMagicka.Text = build.InvestMagicka.ToString();
                    TxtInvestHealth.Text = build.InvestHealth.ToString();
                    TxtInvestStamina.Text = build.InvestStamina.ToString();

                    var race = RaceDatabase.FirstOrDefault(r => r.Name == build.SelectedRace);
                    if (race != null) CmbRace.SelectedItem = race;

                    foreach (var perk in PerkDatabase)
                    {
                        perk.IsActive = build.ActivePerkNames.Contains(perk.Name);
                    }

                    UpdateCalculations();
                    MessageBox.Show("Build erfolgreich geladen!");
                }
            }
        }

        private void BtnExportText_Click(object sender, RoutedEventArgs e)
        {
            string raceName = (CmbRace.SelectedItem as Race)?.Name ?? "None";

            string summary = $"--- SKYRIM BUILD SUMMARY ---\n" +
                             $"Race: {raceName}\n" +
                             $"Level: {TxtPlayerLevel.Text}\n\n" +
                             $"STATS:\n" +
                             $"- Magicka: {TxtTotalMagicka.Text} {TxtRegenMagicka.Text}\n" +
                             $"- Health: {TxtTotalHealth.Text} {TxtRegenHealth.Text}\n" +
                             $"- Stamina: {TxtTotalStamina.Text} {TxtRegenStamina.Text}\n\n" +
                             $"ACTIVE PERKS:\n" +
                             string.Join("\n", PerkDatabase.Where(p => p.IsActive).Select(p => "- " + p.Name));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Textdatei (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, summary);
                MessageBox.Show("Export abgeschlossen!");
            }
        }
    }
}