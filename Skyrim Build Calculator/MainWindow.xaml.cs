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
    // Die Daten-Modelle
    public class Weapon
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";

        // Neu hinzugefügt für die UESP-Verzauberungsfilter
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

        // Neu hinzugefügt für die UESP-Verzauberungsfilter
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

        // Diese Eigenschaft feuert jetzt ein Signal an die UI, wenn sie geändert wird
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

        // Damit die Perks sofort grau werden, wenn das Level nicht reicht
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

        // Neu hinzugefügt für die UESP-Verzauberungsfilter
        // Hier werden die erlaubten Slots gespeichert (z.B. "Weapon", "Head", "Chest", etc.)
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
        public double DamageTakenMultiplier { get; set; } // NEU hinzugefügt
    }
    public class Race
    {
        public string Name { get; set; } = "";
        public string PassiveEffect { get; set; } = "";
        public string Power { get; set; } = "";
        public int BonusMagicka { get; set; } = 0; // Für die Hochelfen!
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
        public double MagickaRegenMult { get; set; } = 1.0; // 1.0 = 100% (normal)
        public double HealthRegenMult { get; set; } = 1.0;
        public double StaminaRegenMult { get; set; } = 1.0;
        public double SpellAbsorption { get; set; } = 0;
    }

    // Alles nur hier drüber einfügen an Public Classes (Wichtig)
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
        // Das ist die Sperre, die den Absturz verhindert
        private bool _isCalculating = false;

        // Deine Datenbanken
        public List<Weapon> WeaponDatabase { get; set; } = new List<Weapon>();
        public List<Armor> ArmorDatabase { get; set; } = new List<Armor>();
        public List<Enchantment> EnchantmentDatabase { get; set; } = new List<Enchantment>();
        public List<SoulGem> SoulGemDatabase { get; set; } = new List<SoulGem>();
        public List<Perk> PerkDatabase { get; set; } = new List<Perk>();
        public List<Difficulty> DifficultyDatabase { get; set; } = new List<Difficulty>();
        public List<Race> RaceDatabase { get; set; } = new List<Race>();
        public List<StandingStone> StandingStoneDatabase { get; set; } = new List<StandingStone>();

        // Dein aktuelles Equipment
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

            // 1. Daten laden
            LoadData();
            LoadRaceData();
            LoadStandingStones();

            // 2. Perk-Gruppierung für die Expander (2 Ebenen: Haupt- und Unterkategorie)
            var view = (CollectionView)CollectionViewSource.GetDefaultView(PerkDatabase);
            if (view != null)
            {
                view.GroupDescriptions.Clear();
                // Ebene 1: WARRIOR, THIEF, etc.
                view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                // Ebene 2: One-Handed, Sneak, etc.
                view.GroupDescriptions.Add(new PropertyGroupDescription("SubCategory"));

                LstPerks.ItemsSource = view;
            }

            // 3. UI-Bindung (Datenquellen zuweisen)
            CmbSoulGem.ItemsSource = SoulGemDatabase;
            CmbDifficulty.ItemsSource = DifficultyDatabase;
            CmbRace.ItemsSource = RaceDatabase;
            CmbStandingStone.ItemsSource = StandingStoneDatabase;

            // Standardwerte setzen
            if (CmbDifficulty.Items.Count > 0) CmbDifficulty.SelectedIndex = 2; // Adept
            CmbRace.SelectedIndex = 0;
            CmbStandingStone.SelectedIndex = 0;

            // 4. DER FIX: Erst rechnen, wenn das Fenster wirklich fertig geladen ist
            this.Loaded += (s, e) =>
            {
                // Berechnet Level, Punkte und setzt das "IsAvailable" für Perks
                CalculateAttributes();

                // Berechnet Rüstung und Schaden
                UpdateTotalBuildStats();

                // Start-Modus (Waffen-Panel anzeigen)
                BtnShowWeapons_Click(this, new RoutedEventArgs());

                // Optional: Falls die Liste im Perk Architect noch nicht richtig sortiert ist
                if (view != null) view.Refresh();
            };
        }

        // --- CHARACTER CORE LOGIK ---

        private void LoadRaceData()
        {
            RaceDatabase.Clear();

            RaceDatabase.Add(new Race
            {
                Name = "None",
                PassiveEffect = "No passive effect selected.",
                Power = "None",
                BonusMagicka = 0
            });

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
                // 1. Verhindert, dass das Programm abstürzt, wenn man alles löscht
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = "0";
                    tb.SelectAll();
                }
                // 2. Entfernt führende Nullen (macht aus "01" eine "1")
                else if (int.TryParse(tb.Text, out int value))
                {
                    string cleanValue = value.ToString();

                    // Nur ändern, wenn es wirklich anders ist (verhindert Endlosschleifen)
                    if (tb.Text != cleanValue)
                    {
                        tb.Text = cleanValue;
                        // Setzt den Cursor ans Ende, damit man weitertippen kann
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
            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // Verhindert das Leerzeichen
            }
        }

        private void CalculateAttributes()
        {
            // Falls wir schon gerade rechnen, abbrechen (verhindert den Absturz)
            if (_isCalculating || !IsLoaded || TxtPlayerLevel == null || CmbRace == null || CmbStandingStone == null || LblSoulGem == null) return;

            try
            {
                _isCalculating = true; // Sperre aktivieren

                // 1. Eingaben lesen
                int.TryParse(TxtPlayerLevel.Text, out int level);
                if (level < 1) level = 1;

                int.TryParse(TxtInvestMagicka.Text, out int investMagicka);
                int.TryParse(TxtInvestHealth.Text, out int investHealth);
                int.TryParse(TxtInvestStamina.Text, out int investStamina);

                // 2. Race & Stone Boni
                var race = CmbRace.SelectedItem as Race;
                var stone = CmbStandingStone.SelectedItem as StandingStone;

                TxtRacePassive.Text = race?.PassiveEffect ?? "-";
                TxtRacePower.Text = race?.Power ?? "-";
                TxtStoneDescription.Text = stone?.Description ?? "No stone selected.";

                // 3. Punkte-Check
                int perksSpent = PerkDatabase?.Count(p => p.IsActive) ?? 0;
                int totalAvailablePoints = level - 1;
                int pointsSpent = investMagicka + investHealth + investStamina + perksSpent;

                LblAvailablePoints.Text = $"Stat Points: {pointsSpent} / {totalAvailablePoints}";
                LblAvailablePoints.Foreground = (pointsSpent > totalAvailablePoints)
                    ? Brushes.Red
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1B80D"));

                // 4. Basis-Attribute
                double totalMagicka = 100 + (race?.BonusMagicka ?? 0) + (investMagicka * 10) + (stone?.BonusMagicka ?? 0);
                double totalHealth = 100 + (investHealth * 10);
                double totalStamina = 100 + (investStamina * 10);

                TxtTotalMagicka.Text = totalMagicka.ToString("0");
                TxtTotalHealth.Text = totalHealth.ToString("0");
                TxtTotalStamina.Text = totalStamina.ToString("0");

                // 5. Regeneration
                double mRegenMult = stone?.MagickaRegenMult ?? 1.0;
                double hRegenMult = stone?.HealthRegenMult ?? 1.0;
                double sRegenMult = stone?.StaminaRegenMult ?? 1.0;

                TxtRegenMagicka.Text = $"(+{(totalMagicka * 0.03 * mRegenMult):0.0} /s)";
                TxtRegenHealth.Text = $"(+{(totalHealth * 0.005 * hRegenMult):0.0} /s)";
                TxtRegenStamina.Text = $"(+{(totalStamina * 0.05 * sRegenMult):0.0} /s)";

                // 6. Perk-Validierung
                if (PerkDatabase != null)
                {
                    foreach (var perk in PerkDatabase)
                    {
                        bool shouldBeAvailable = level >= perk.RequiredLevel;

                        // Nur ändern, wenn nötig (spart Rechenzeit)
                        if (perk.IsAvailable != shouldBeAvailable)
                        {
                            perk.IsAvailable = shouldBeAvailable;
                        }

                        // WICHTIG: Wenn nicht verfügbar, Häkchen entfernen
                        if (!perk.IsAvailable && perk.IsActive)
                        {
                            perk.IsActive = false;
                        }
                    }

                    // Zähler-Update
                    if (TxtWarriorCount != null) TxtWarriorCount.Text = $"Warrior: {PerkDatabase.Count(p => p.IsActive && p.Category == "WARRIOR")}";
                    if (TxtThiefCount != null) TxtThiefCount.Text = $"Thief: {PerkDatabase.Count(p => p.IsActive && p.Category == "THIEF")}";
                    if (TxtMageCount != null) TxtMageCount.Text = $"Mage: {PerkDatabase.Count(p => p.IsActive && p.Category == "MAGE")}";

                    // Refresh nur triggern, wenn die Liste existiert
                    //CollectionViewSource.GetDefaultView(PerkDatabase)?.Refresh();
                }
            }
            finally
            {
                _isCalculating = false; // Sperre wieder aufheben
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
                // Wir nehmen den Effekt direkt aus der ComboBox, falls vorhanden
                newItem.Enchantment = CmbEnchantment.SelectedItem != null ? TxtEffect.Text : "None";
                newItem.Rating = TxtDamageDisplay.Text;
                CurrentBuild["Weapon"] = newItem;
            }
            else if (!isWeaponMode && selected is Armor a)
            {
                // Wir prüfen, ob der Slot (z.B. "Head", "Chest") in unserem Dictionary existiert
                if (CurrentBuild.ContainsKey(a.Slot))
                {
                    newItem.Slot = a.Slot;
                    newItem.ItemName = a.Name;
                    newItem.Enchantment = CmbEnchantment.SelectedItem != null ? TxtArmorEffect.Text : "None";
                    newItem.Rating = TxtArmorDisplay.Text;
                    CurrentBuild[a.Slot] = newItem;
                }
            }

            // Ganz wichtig: Die Statistik-Methode aufrufen
            UpdateTotalBuildStats();
        }
        private void LstBuildItems_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 1. Prüfen, ob wirklich ein Item ausgewählt wurde
            if (LstBuildItems.SelectedItem is EquippedItemDisplay selectedItem)
            {
                // 2. Das Item aus dem Dictionary entfernen (auf null setzen)
                // Wir nutzen den 'Slot' Namen des ausgewählten Items als Schlüssel
                if (CurrentBuild.ContainsKey(selectedItem.Slot))
                {
                    CurrentBuild[selectedItem.Slot] = null;

                    // 3. Die Anzeige und die Summen sofort aktualisieren
                    UpdateTotalBuildStats();
                }
            }
        }

        private void UpdateTotalBuildStats()
        {
            double totalArmor = 0;
            double totalDamage = 0;
            double totalSneak = 0;

            // Die displayList wieder hinzufügen, damit der Fehler verschwindet
            var displayList = new List<EquippedItemDisplay>();

            // Sicherstellen, dass PerkDatabase nicht null ist (behebt gelbe Warnungen)
            var activePerks = PerkDatabase?.Where(p => p.IsActive).ToList() ?? new List<Perk>();

            string[] slotOrder = { "Weapon", "Head", "Necklace", "Chest", "Hands", "Ring", "Feet", "Shield" };

            foreach (var slot in slotOrder)
            {
                if (CurrentBuild.ContainsKey(slot) && CurrentBuild[slot] != null)
                {
                    var item = CurrentBuild[slot]!;
                    displayList.Add(item); // Hier wird displayList gefüllt

                    if (double.TryParse(item.Rating, out double val))
                    {
                        if (slot == "Weapon")
                        {
                            totalDamage += val;

                            // Sneak-Logik
                            double mult = 2.0;
                            string itemName = item.ItemName?.ToLower() ?? "";

                            if (itemName.Contains("dagger"))
                            {
                                // Dolche: 15x mit Assassin's Blade, 6x mit Backstab, 3x Basiswert (ohne Perks)
                                if (activePerks.Any(p => p.Name == "Assassin's Blade")) mult = 15.0;
                                else if (activePerks.Any(p => p.Name == "Backstab")) mult = 6.0;
                                else mult = 3.0; // <--- HIER war der Fehler! Vorher stand hier nichts, also fiel er auf 2.0 zurück.
                            }
                            else if (itemName.Contains("bow") || itemName.Contains("crossbow"))
                            {
                                // Bögen: 3x mit Deadly Aim, 2x Basiswert
                                if (activePerks.Any(p => p.Name == "Deadly Aim")) mult = 3.0;
                                else mult = 2.0;
                            }
                            else
                            {
                                // Alle anderen Nahkampfwaffen: 6x mit Backstab, 2x Basiswert
                                if (activePerks.Any(p => p.Name == "Backstab")) mult = 6.0;
                                else mult = 2.0;
                            }

                            totalSneak += (val * mult);
                        }
                        else
                        {
                            totalArmor += val;
                        }
                    }
                }
            }

            // UI-Updates mit Sicherheitsprüfung
            if (TxtTotalDamage != null) TxtTotalDamage.Text = totalDamage.ToString("0");
            if (TxtTotalArmor != null) TxtTotalArmor.Text = totalArmor.ToString("0");
            if (TxtSneakDamage != null) TxtSneakDamage.Text = totalSneak.ToString("0");

            if (LstBuildItems != null)
            {
                LstBuildItems.ItemsSource = null; // Kurz leeren, um Refresh zu erzwingen
                LstBuildItems.ItemsSource = displayList; // Die Liste anzeigen, die wir oben im Code befüllt haben
            }
        }

        private void UpdateCalculations()
        {
            if (CmbSelect == null || CmbEnchantment == null || CmbSoulGem == null || CmbDifficulty == null || CmbSelect.SelectedItem == null) return;

            CalculateAttributes();

            int level = int.TryParse(TxtPlayerLevel.Text, out int l) ? l : 1;
            var active = PerkDatabase.Where(p => p.IsActive).ToList();
            var selectedEnch = CmbEnchantment.SelectedItem as Enchantment;
            var selectedSoul = CmbSoulGem.SelectedItem as SoulGem;

            // Schwierigkeits-Multiplikator holen
            var diff = CmbDifficulty.SelectedItem as Difficulty;
            double dMult = diff?.DamageDealtMultiplier ?? 1.0;

            // Enchanting-Multiplier aus Perks
            double eMult = active.Where(p => p.SkillGroup == "Enchanting").Select(p => p.Multiplier).DefaultIfEmpty(1.0).Max();

            if (isWeaponMode && CmbSelect.SelectedItem is Weapon w)
            {
                // --- Prüfung auf "None" (bleibt gleich) ---
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

                // 1. Basis-Daten vorbereiten
                var stats = w.GetStatsForLevel(level);
                double wMult = 1.0;
                string wName = w.Name.ToLower();
                string wCategory = (w.Category ?? "").ToLower();

                // --- NEU: Dynamisches Schmiede-System basierend auf Level ---
                double smithingBonus = 0;
                string tierName = "";

                // Prüft, ob ein aktiver Smithing-Perk zur Waffe passt
                bool hasSmithingPerk = active.Any(p =>
                    p.SkillGroup == "Smithing" &&
                    (wName.Contains(p.BaseName.ToLower()) || wCategory.Contains(p.BaseName.ToLower()))
                );

                if (hasSmithingPerk)
                {
                    // Bonuswerte nach deiner Tabelle (Spalte "mit Perk")
                    if (level >= 41) { smithingBonus = 15; tierName = "(Legendary)"; }
                    else if (level >= 31) { smithingBonus = 8; tierName = "(Epic)"; }
                    else if (level >= 21) { smithingBonus = 6; tierName = "(Exquisite)"; }
                    else if (level >= 11) { smithingBonus = 4; tierName = "(Superior)"; }
                    else { smithingBonus = 2; tierName = "(Fine)"; }

                    TxtWeaponCategory.Text = $"{w.Category} {tierName}";
                }
                else
                {
                    TxtWeaponCategory.Text = w.Category;
                }

                // 2. Passende Waffen-Perks finden (Armsman etc.)
                var matchingPerks = active.Where(p => {
                    string pGroup = (p.SkillGroup ?? "").ToLower();
                    if (pGroup.Contains("onehanded") && (wName.Contains("dagger") || (wName.Contains("sword") && !wName.Contains("greatsword")) || wName.Contains("mace") || wName.Contains("war axe"))) return true;
                    if (pGroup.Contains("twohanded") && (wName.Contains("greatsword") || wName.Contains("battleaxe") || wName.Contains("warhammer"))) return true;
                    if (pGroup.Contains("archery") && (wName.Contains("bow") || wName.Contains("crossbow"))) return true;
                    return false;
                }).ToList();

                if (matchingPerks.Any()) wMult = matchingPerks.Max(p => p.Multiplier);

                // --- NEU: Die realistische Formel: (Basis + SchmiedeBonus) * Perks * Schwierigkeit ---
                double finalDmg = (stats.dmg + smithingBonus) * wMult * dMult;

                // 4. Verzauberungs-Effekt berechnen (bleibt gleich)
                if (selectedEnch != null && selectedEnch.Name != "None")
                {
                    double mag = selectedEnch.AddedValue * eMult;
                    string enchText = string.Format(selectedEnch.Description, Math.Round(mag));
                    TxtEffect.Text = (string.IsNullOrEmpty(stats.eff) || stats.eff == "None") ? enchText : stats.eff + " + " + enchText;
                }
                else
                {
                    TxtEffect.Text = !string.IsNullOrEmpty(stats.eff) ? stats.eff : "None";
                }

                // 5. UI Updates
                TxtDamageDisplay.Text = Math.Round(finalDmg).ToString();

                double sMult = active.Where(p => p.SkillGroup != null && p.SkillGroup.Contains("Sneak")).Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();
                TxtSneakDisplay.Text = Math.Round(finalDmg * sMult).ToString();

                TxtReach.Text = w.Reach > 0 ? w.Reach.ToString() : "-";
                TxtSpeed.Text = w.Speed > 0 ? w.Speed.ToString() : "-";
                TxtStagger.Text = w.Stagger > 0 ? w.Stagger.ToString() : "-";
                TxtValue.Text = Math.Round(stats.val).ToString();
            }
          
           {
            double totalArmor = 0;
            double totalDamage = 0;
            double totalSneak = 0;
            var sortedDisplayList = new List<EquippedItemDisplay>();

            // 1. Perks für Schleich-Multiplikatoren holen
            var activePerks = PerkDatabase.Where(p => p.IsActive).ToList();

            string[] slotOrder = { "Weapon", "Head", "Necklace", "Chest", "Hands", "Ring", "Feet", "Shield" };

            // 2. Ausgerüstete Items durchgehen
            foreach (var slotName in slotOrder)
            {
                if (CurrentBuild.ContainsKey(slotName) && CurrentBuild[slotName] != null)
                {
                    var item = CurrentBuild[slotName];
                    sortedDisplayList.Add(item!);

                    if (double.TryParse(item!.Rating, out double val))
                    {
                        if (slotName == "Weapon")
                        {
                            totalDamage += val;

                            // --- Präzise Sneak-Dmg Logik ---
                            double sMult = 3.0; // Standard
                            string wName = item.ItemName.ToLower();

                            if (wName.Contains("dagger"))
                            {
                                // Assassin's Blade (15x)
                                sMult = activePerks.Where(p => p.SkillGroup == "SneakDagger")
                                                   .Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();
                            }
                            else if (wName.Contains("bow") || wName.Contains("crossbow"))
                            {
                                // Deadly Aim (3x)
                                sMult = activePerks.Where(p => p.SkillGroup == "SneakBow")
                                                   .Select(p => p.Multiplier).DefaultIfEmpty(2.0).Max();
                            }
                            else
                            {
                                // Backstab (6x)
                                sMult = activePerks.Where(p => p.SkillGroup == "Sneak1H")
                                                   .Select(p => p.Multiplier).DefaultIfEmpty(3.0).Max();
                            }
                            totalSneak += (val * sMult);
                        }
                        else
                        {
                            totalArmor += val;
                        }
                    }
                }
            }

            // --- NEU: Findling-Bonus (z.B. +50 vom Fürstenstein) addieren ---
            double stoneArmor = (CmbStandingStone.SelectedItem as StandingStone)?.BonusArmor ?? 0;

            // --- UI Updates ---
            // Hier wird der Stein-Bonus auf die Summe der Rüstungsteile draufgerechnet
            TxtTotalArmor.Text = Math.Round(totalArmor + stoneArmor).ToString();
            TxtTotalDamage.Text = Math.Round(totalDamage).ToString();
            TxtSneakDamage.Text = totalSneak.ToString("0");

                // Die Liste in der UI aktualisieren
                LstBuildItems.ItemsSource = sortedDisplayList;
        }
        }
        private void ApplyItemFilter()
        {
            if (TxtSearch == null || CmbSelect == null) return;
            string search = TxtSearch.Text.ToLower();

            if (isWeaponMode)
            {
                var filtered = WeaponDatabase.Where(w => w.Name.ToLower().Contains(search)).ToList();
                // Ein leeres Waffen-Objekt am Anfang einfügen
                filtered.Insert(0, new Weapon { Name = "None", Category = "-", Damage = 0 });
                CmbSelect.ItemsSource = filtered;
            }
            else
            {
                var filtered = ArmorDatabase.Where(a => a.Name.ToLower().Contains(search)).ToList();
                // Ein leeres Rüstungs-Objekt am Anfang einfügen
                filtered.Insert(0, new Armor { Name = "None", Category = "-", ArmorRating = 0 });
                CmbSelect.ItemsSource = filtered;
            }

            // Wählt automatisch das erste Item aus (was jetzt immer "None" ist)
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
            // Wir holen uns das, was du im Dropdown angeklickt hast
            var selected = CmbEnchantment.SelectedItem;
            string effectText = "None";

            if (selected != null && selected.ToString() != "None")
            {
                // Da du {Binding Name} im XAML hast, nutzen wir "dynamic", 
                // um flexibel auf den Namen oder die Beschreibung zuzugreifen.
                dynamic enc = selected;

                // Wenn deine Verzauberungs-Klasse eine "Description" hat, nutze sie. 
                // Ansonsten nehmen wir den normalen Namen.
                try { effectText = enc.Description ?? enc.Name; }
                catch { effectText = enc.Name; }
            }

            // NEU: Trage den Text ins richtige UI-Feld ein
            if (isWeaponMode)
            {
                if (TxtEffect != null) TxtEffect.Text = effectText;
            }
            else
            {
                // Hier fehlte die Zuweisung für die Rüstung!
                if (TxtArmorEffect != null) TxtArmorEffect.Text = effectText;
            }

            // Nach dem Ändern des Textes neu durchrechnen lassen
            UpdateCalculations();
        }
        private void CmbSoulGem_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateCalculations();

        private void BtnShowWeapons_Click(object sender, RoutedEventArgs e)
        {
            isWeaponMode = true;
            WeaponPanel.Visibility = Visibility.Visible;
            ArmorPanel.Visibility = Visibility.Collapsed;

            // NEU: Seelenstein-Auswahl verstecken
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
                CmbSoulGem.SelectedIndex = 0; // Springt beim Wechsel immer auf "None"
            }

            ApplyItemFilter();
        }

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

                // Fülle den gelben Rüstungskasten mit den Standardwerten
                if (TxtArmorCategory != null) TxtArmorCategory.Text = a.Slot;
                if (TxtArmorWeight != null) TxtArmorWeight.Text = a.Weight.ToString();
                if (TxtArmorValue != null) TxtArmorValue.Text = a.Value.ToString();

                // HIER ist die neue Zeile für den Rüstungswert (Armor Rating)
                if (TxtArmorDisplay != null) TxtArmorDisplay.Text = a.ArmorRating.ToString();
            }

            // Hier wird die Liste basierend auf dem Slot (UESP-Regeln) gefiltert
            UpdateEnchantmentList();

            UpdateCalculations();
        }

        private void CmbDifficulty_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
            // Sicherheitscheck
            if (CmbDifficulty == null || TxtIncomingDamage == null) return;

               var selectedDiff = CmbDifficulty.SelectedItem;
            if (selectedDiff == null) return;

            // HIER IST DER FIX: Wir holen uns gezielt die Eigenschaft "Name" aus dem Objekt!
            string diffName = "";
            try
            {
                diffName = ((dynamic)selectedDiff).Name;
            }
            catch
            {
                // Falls etwas schiefgeht (oder es sich um einen reinen String handelt)
                diffName = selectedDiff.ToString() ?? "";
            }

            // 1. Basis-Multiplikator bestimmen
            double incomingMultiplier = 1.0; 

            if (diffName.Contains("Novice") || diffName.Contains("Novize")) incomingMultiplier = 0.5;
            else if (diffName.Contains("Apprentice") || diffName.Contains("Lehrling")) incomingMultiplier = 0.75;
            else if (diffName.Contains("Adept")) incomingMultiplier = 1.0;
            else if (diffName.Contains("Expert") || diffName.Contains("Experte")) incomingMultiplier = 1.5;
            else if (diffName.Contains("Master") || diffName.Contains("Meister")) incomingMultiplier = 2.0;
            else if (diffName.Contains("Legendary") || diffName.Contains("Legendär")) incomingMultiplier = 3.0;

            // 2. Das Label SOFORT updaten
            TxtIncomingDamage.Text = (incomingMultiplier * 100).ToString("0") + "%";

            // 3. Farbliche Hervorhebung
            if (incomingMultiplier > 1.0)
            {
                // Schwerer = Rot
                TxtIncomingDamage.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4444")); 
            }
            else if (incomingMultiplier < 1.0)
            {
                // Leichter = Grün
                TxtIncomingDamage.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#44FF44")); 
            }
            else
            {
                // Normal (100%) = Gold
                TxtIncomingDamage.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E1B80D")); 
            }

            // 4. Den Rest deiner Berechnungen ausführen
            UpdateCalculations(); 
        }

        private void UpdateEnchantmentList()
        {
            if (CmbSelect.SelectedItem == null) return;

            List<Enchantment> filteredList = new List<Enchantment>();

            // "None" (Keine Verzauberung) immer hinzufügen
            var noneEnch = EnchantmentDatabase.FirstOrDefault(e => e.Name == "None");
            if (noneEnch != null) filteredList.Add(noneEnch);

            if (isWeaponMode && CmbSelect.SelectedItem is Weapon w)
            {
                // Filtert alle Verzauberungen, die "Weapon" in ihrer AllowedSlots-Liste haben
                var weaponsEnchs = EnchantmentDatabase.Where(e => e.AllowedSlots != null && e.AllowedSlots.Contains("Weapon"));
                filteredList.AddRange(weaponsEnchs);
            }
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a)
            {
                // Filtert alle Verzauberungen, die den spezifischen Slot (z.B. "Head") erlauben
                var armorEnchs = EnchantmentDatabase.Where(e => e.AllowedSlots != null && e.AllowedSlots.Contains(a.Slot));
                filteredList.AddRange(armorEnchs);
            }

            // Die ComboBox mit der gefilterten Liste füllen
            CmbEnchantment.ItemsSource = filteredList;

            // Falls die Liste Einträge hat, den ersten ("None") auswählen
            if (filteredList.Count > 0) CmbEnchantment.SelectedIndex = 0;
        }

        private void LoadStandingStones()
        {
            StandingStoneDatabase.Clear();
            StandingStoneDatabase.Add(new StandingStone { Name = "None", Description = "No stone selected." });

            // --- Guardian Stones ---
            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Mage Stone",
                Description = "Magic skills advance 20% faster."
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Thief Stone",
                Description = "Thief skills advance 20% faster."
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Warrior Stone",
                Description = "Warrior skills advance 20% faster."
            });

            // --- Other Stones ---
            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Apprentice Stone",
                Description = "+100% Magicka Regen, -50% Magic Resistance",
                MagickaRegenMult = 2.0,
                BonusMagicResist = -50
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Atronach Stone",
                Description = "+50 Magicka, 50% Spell Absorption, -50% Magicka Regen",
                BonusMagicka = 50,
                SpellAbsorption = 50,
                MagickaRegenMult = 0.5
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Lady Stone",
                Description = "+25% Health & Stamina Regen",
                HealthRegenMult = 1.25,
                StaminaRegenMult = 1.25
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Lord Stone",
                Description = "+50 Armor, +25% Magic Resistance",
                BonusArmor = 50,
                BonusMagicResist = 25
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Lover Stone",
                Description = "All skills advance 15% faster."
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Ritual Stone",
                Description = "Reanimate all nearby corpses to fight for you once a day."
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Serpent Stone",
                Description = "Paralyze target for 5s and do 25 poison damage once a day."
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Shadow Stone",
                Description = "Invisibility for 60s once a day."
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Steed Stone",
                Description = "+100 Carry Weight, Equipped armor weighs nothing."
            });

            StandingStoneDatabase.Add(new StandingStone
            {
                Name = "The Tower Stone",
                Description = "Unlock any Expert level lock or lower once a day."
            });
        }

        private void CmbStandingStone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCalculations();
            UpdateTotalBuildStats();
        }

        private void Perk_CheckChanged(object sender, RoutedEventArgs e)
        {
            // Falls wir gerade im Hintergrund etwas berechnen, nicht dazwischenfunken!
            if (_isCalculating) return;

            // Ansonsten: Alles neu berechnen, da ein Perk ein- oder ausgeschaltet wurde
            UpdateCalculations();
        }

        // Erstellt eine Text-Datein
        private void BtnSaveBuild_Click(object sender, RoutedEventArgs e)
        {
            var build = new SavedBuild
            {
                BuildName = "My Build",
                // Das ?? "None" behebt die erste Warnung (CS8601)
                SelectedRace = (CmbRace.SelectedItem as Race)?.Name ?? "None",

                // Sicherer Cast der Textboxen
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
            // Die Variable raceName behebt die zweite Warnung (CS8602)
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

    } // Ende Klasse
} // Ende Namespace