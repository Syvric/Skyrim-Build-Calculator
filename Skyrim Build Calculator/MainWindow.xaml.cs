using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Skyrim_Build_Architect
{
    public partial class MainWindow : Window, INotifyPropertyChanged
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
        public Dictionary<string, EquippedItem?> CurrentBuild = new Dictionary<string, EquippedItem?>()
        {
            { "Weapon", null }, { "Shield", null }, { "Head", null },
            { "Chest", null }, { "Hands", null }, { "Feet", null },
            { "Ring", null }, { "Necklace", null }
        };

        // HIER EINGEFÜGT: Die neue Liste für flexibles Equipment (Dual-Wield Support)
        public ObservableCollection<EquippedItem> EquippedItems { get; set; } = new ObservableCollection<EquippedItem>();

        private bool isWeaponMode = true;

        // --- RUCKSACK LOGIK & BINDING ---
        private EquippedItem? _equippedBackpack;
        public EquippedItem? EquippedBackpack
        {
            get => _equippedBackpack;
            set
            {
                _equippedBackpack = value;

                // Wir rufen jetzt die NEU BENANNTE Methode auf:
                NotifyPropertyChanged("MaxCarryWeight");

                UpdateTotalBuildStats();
                CalculateAttributes();
            }
        }

        public double MaxCarryWeight
        {
            get
            {
                // Check, ob das Feld im XAML TxtInvestStamina oder TxtStaminaInvest heißt!
                int investStamina = int.TryParse(TxtInvestStamina.Text, out int s) ? s : 0;
                double baseCarry = 300 + (investStamina * 5);

                return baseCarry + CalculateBackpackBonus("capacity");
            }
        }

        // --- NEU: SCHWIERIGKEITSGRAD-LOGIK ---
        private Difficulty _selectedDifficulty = new Difficulty { Name = "Adept", DamageDealtMultiplier = 1.0, DamageTakenMultiplier = 1.0 };
        public Difficulty SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                _selectedDifficulty = value;

                // 1. UI über die Änderung informieren
                NotifyPropertyChanged(nameof(SelectedDifficulty));
                NotifyPropertyChanged(nameof(TotalWeaponDamage));
                NotifyPropertyChanged(nameof(PreviewDamage)); // Für das gelbe Fenster
                NotifyPropertyChanged(nameof(IncomingDamageDisplay));

                // 2. Synchronisation: Beide Logik-Pfade triggern
                // UpdateCalculations berechnet die gelbe Preview-Box
                UpdateCalculations();

                // UpdateTotalBuildStats berechnet die unteren Gesamt-Werte
                UpdateTotalBuildStats();
            }
        }

        // Hilfs-Properties für das Binding, damit das nameof() Ziel findet
        public string PreviewDamage => TxtDamageDisplay?.Text ?? "0";
        public string TotalWeaponDamage => TxtTotalDamage?.Text ?? "0";

        // Hilfs-Property für das gelbe Label
        public string IncomingDamageDisplay => $"{(_selectedDifficulty?.DamageTakenMultiplier ?? 1.0) * 100}%";
        public string TotalSneakDamage => TxtSneakDamage?.Text ?? "0";

        public string DisplayCategory
        {

            get
            {
                // Wir holen uns die aktuelle Perk-Liste für den Check
                var activePerks = PerkDatabase?.Where(p => p.IsActive).ToList() ?? new List<Perk>();

                if (CmbSelect.SelectedItem is Weapon w)
                {
                    // Logik: Ist ein Effekt da, aber der Perk fehlt?
                    bool needsArcane = !string.IsNullOrEmpty(w.Effect) && w.Effect != "None" &&
                                       !activePerks.Any(p => p.Name == "Arcane Blacksmith");

                    return needsArcane ? $"{w.Category} (Needs Arcane Blacksmith)" : w.Category;
                }

                if (CmbSelect.SelectedItem is Armor a)
                {
                    bool needsArcane = !string.IsNullOrEmpty(a.Effect) && a.Effect != "None" &&
                                       !activePerks.Any(p => p.Name == "Arcane Blacksmith");

                    return needsArcane ? $"{a.Category} (Needs Arcane Blacksmith)" : a.Category;
                }

                return "Unarmed";
            }
        }

        private double CalculateBackpackBonus(string keyword)
        {
            var bp = EquippedItems.FirstOrDefault(i => i.Slot == "Backpack");
            if (bp == null) return 0;

            string text = (bp.Enchantment ?? "") + " " + (bp.OriginalObject as Armor)?.Effect;
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                var match = System.Text.RegularExpressions.Regex.Match(text, @"(?:increased by\s+)?(\d+)");
                if (match.Success)
                {
                    return double.Parse(match.Groups[1].Value);
                }
            }
            return 0;
        }

        public MainWindow()
        {
            InitializeComponent();

            // 1. ZUERST ALLE DATEN LADEN
            // Ohne das hier sind die Listen leer und wir können nichts auswählen!
            LoadData();
            LoadRaceData();
            LoadStandingStones();

            // 2. DEN STANDARD-SCHWIERIGKEITSGRAD ÜBER DIE PROPERTY SETZEN
            // Das ersetzt das alte 'CmbDifficulty.SelectedIndex = 2'
            if (DifficultyDatabase != null && DifficultyDatabase.Count > 0)
            {
                // Wir suchen "Adept", falls nicht gefunden, nehmen wir den ersten Eintrag
                SelectedDifficulty = DifficultyDatabase.FirstOrDefault(d => d.Name == "Adept") ?? DifficultyDatabase[0];
            }

            // 3. UI-BINDUNGEN UND LISTEN INITIALISIEREN
            LstBuildItems.ItemsSource = EquippedItems;

            // Verzauberungen initialisieren
            CmbEnchantment2.ItemsSource = EnchantmentDatabase;
            CmbEnchantment2.SelectedIndex = 0;

            // Perk-Liste mit Gruppierung vorbereiten
            var view = (CollectionView)CollectionViewSource.GetDefaultView(PerkDatabase);
            if (view != null)
            {
                view.GroupDescriptions.Clear();
                view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                view.GroupDescriptions.Add(new PropertyGroupDescription("SubCategory"));
                LstPerks.ItemsSource = view;
            }

            // Datenbanken an die ComboBoxen binden
            CmbSoulGem.ItemsSource = SoulGemDatabase;
            CmbDifficulty.ItemsSource = DifficultyDatabase;
            CmbRace.ItemsSource = RaceDatabase;
            CmbStandingStone.ItemsSource = StandingStoneDatabase;

            // Weitere Standard-Auswahl setzen
            CmbRace.SelectedIndex = 0;
            CmbStandingStone.SelectedIndex = 0;

            // 4. INITIAL-BERECHNUNGEN (Sobald das Fenster bereit ist)
            this.Loaded += (s, e) =>
            {
                CalculateAttributes();
                UpdateTotalBuildStats();
                BtnShowWeapons_Click(this, new RoutedEventArgs());
                if (view != null) view.Refresh();
            };

            // Einmaliger Aufruf zur Sicherheit
            UpdateTotalBuildStats();
        }

        // --- CHARACTER CORE LOGIK ---

        private void CmbRace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Attribute anpassen (Magicka, Passive Effekte, etc.)
            CalculateAttributes();

            // 2. Taschenrechner für die Vorschau anwerfen (Fixt den Khajiit-Schaden!)
            UpdateCalculations();

            // 3. Gesamte ausgerüstete Stats aktualisieren
            UpdateTotalBuildStats();
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
            // 1. Erweiterter Sicherheitscheck
            if (_isCalculating || !IsLoaded || TxtPlayerLevel == null || CmbRace == null ||
                CmbStandingStone == null || LblSoulGem == null || LblEnchantment2 == null ||
                CmbEnchantment2 == null || LblAvailablePerkPoints == null) return;

            try
            {
                _isCalculating = true;

                // 2. Basis-Werte einlesen
                int.TryParse(TxtPlayerLevel.Text, out int level);
                if (level < 1) level = 1;

                int.TryParse(TxtInvestMagicka.Text, out int investMagicka);
                int.TryParse(TxtInvestHealth.Text, out int investHealth);
                int.TryParse(TxtInvestStamina.Text, out int investStamina);

                var race = CmbRace.SelectedItem as Race;
                var stone = CmbStandingStone.SelectedItem as StandingStone;
                var activePerks = PerkDatabase?.Where(p => p.IsActive).ToList() ?? new List<Perk>();

                // 3. UI Texte für Rasse und Stein
                TxtRacePassive.Text = race?.PassiveEffect ?? "-";
                TxtRacePower.Text = race?.Power ?? "-";
                TxtStoneDescription.Text = stone?.Description ?? "No stone selected.";

                // ============================================================
                // 4. DIE PUNKTETRENNUNG (SKYRIM-STYLE)
                // ============================================================
                int totalEarnedPoints = level - 1;
                SolidColorBrush goldBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1B80D"));

                int statsSpent = investMagicka + investHealth + investStamina;
                LblAvailablePoints.Text = $"Attribute Points: {statsSpent} / {totalEarnedPoints}";
                LblAvailablePoints.Foreground = (statsSpent > totalEarnedPoints) ? Brushes.Red : goldBrush;

                int perksSpent = activePerks.Count;
                LblAvailablePerkPoints.Text = $"Perk Points: {perksSpent} / {totalEarnedPoints}";
                LblAvailablePerkPoints.Foreground = (perksSpent > totalEarnedPoints) ? Brushes.Red : goldBrush;

                // ============================================================
                // 5. ATTRIBUT-WERTE BERECHNEN (INKL. SMART BACKPACK)
                // ============================================================
                double bpMagicka = 0;
                double bpStamina = 0;
                double bpCarryWeight = 0;

                var equippedBackpack = EquippedItems?.FirstOrDefault(i => i.Slot == "Backpack");
                if (equippedBackpack != null)
                {
                    // Wir schauen in den Effekt-Text (Enchantment oder Original-Effekt)
                    string effectText = equippedBackpack.Enchantment ?? "";
                    if (string.IsNullOrEmpty(effectText) && equippedBackpack.OriginalObject is Armor a)
                        effectText = a.Effect ?? "";

                    // Regex-Suche nach Zahlen im Text
                    if (effectText.Contains("capacity", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(effectText, @"\d+");
                        if (match.Success) bpCarryWeight = double.Parse(match.Value);
                    }

                    if (effectText.Contains("Magicka", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(effectText, @"\d+");
                        if (match.Success) bpMagicka = double.Parse(match.Value);
                    }

                    if (effectText.Contains("Stamina", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(effectText, @"\d+");
                        if (match.Success) bpStamina = double.Parse(match.Value);
                    }
                }

                // Berechnung der Haupt-Attribute inklusive Rucksack-Boni
                double totalMagicka = 100 + (race?.BonusMagicka ?? 0) + (investMagicka * 10) + (stone?.BonusMagicka ?? 0) + bpMagicka;
                double totalHealth = 100 + (investHealth * 10);
                double totalStamina = 100 + (investStamina * 10) + bpStamina;

                // --- REGENERATION BERECHNEN ---
                double mRegenMult = stone?.MagickaRegenMult ?? 1.0;
                double hRegenMult = stone?.HealthRegenMult ?? 1.0;
                double sRegenMult = stone?.StaminaRegenMult ?? 1.0;

                double recoveryBonus = activePerks
                    .Where(p => p.BaseName == "Recovery")
                    .Select(p => p.Multiplier - 1.0)
                    .DefaultIfEmpty(0.0)
                    .Max();
                mRegenMult += recoveryBonus;

                bool hasWindWalker = activePerks.Any(p => p.Name.Contains("Wind Walker"));
                int lightArmorCount = EquippedItems?.Count(i => (i.Category ?? "").ToLower().Contains("light armor")) ?? 0;

                if (hasWindWalker && lightArmorCount >= 4)
                {
                    sRegenMult += 0.5;
                }

                // ============================================================
                // 6. GLOBALE STATS BERECHNEN
                // ============================================================

                // 1. Traglast (Skyrim Basis 300 + 5 pro Stamina-Punkt + Rucksack-Bonus)
                double maxCarryWeight = 300 + (investStamina * 5) + bpCarryWeight;
                if (activePerks.Any(p => p.Name.Contains("Extra Pockets"))) maxCarryWeight += 100;
                if (stone?.Name == "The Steed Stone") maxCarryWeight += 100;

                // UI-Update für die maximale Traglast
                TxtTotalCarryWeight.Text = maxCarryWeight.ToString();

                // 2. Magieresistenz
                double totalMagicResist = 0;
                if (race?.Name.Contains("Breton") == true) totalMagicResist += 25;
                if (stone?.Name == "The Lord Stone") totalMagicResist += 25;

                var mrPerk = activePerks.Where(p => p.BaseName == "Magic Resistance")
                                        .OrderByDescending(p => p.RequiredLevel).FirstOrDefault();
                if (mrPerk != null)
                {
                    if (mrPerk.Name.Contains("1/3")) totalMagicResist += 10;
                    else if (mrPerk.Name.Contains("2/3")) totalMagicResist += 20;
                    else if (mrPerk.Name.Contains("3/3")) totalMagicResist += 30;
                }

                // 3. Sneak Bonus
                double stealthBonus = activePerks
                    .Where(p => p.BaseName == "Stealth")
                    .Select(p => p.Multiplier - 1.0)
                    .DefaultIfEmpty(0.0)
                    .Max();

                // 4. Spell Absorb (Atronach)
                double spellAbsorb = 0;
                if (stone != null && (stone.Name ?? "").ToLower().Contains("atronach")) spellAbsorb += 50;
                if (activePerks.Any(p => (p.BaseName ?? "").ToLower().Contains("atronach"))) spellAbsorb += 30;

                // ============================================================
                // 7. UI AKTUALISIEREN
                // ============================================================
                TxtTotalMagicka.Text = totalMagicka.ToString("0");
                TxtTotalHealth.Text = totalHealth.ToString("0");
                TxtTotalStamina.Text = totalStamina.ToString("0");

                TxtRegenMagicka.Text = $"(+{(totalMagicka * 0.03 * mRegenMult):0.0} /s)";
                TxtRegenHealth.Text = $"(+{(totalHealth * 0.005 * hRegenMult):0.0} /s)";
                TxtRegenStamina.Text = $"(+{(totalStamina * 0.05 * sRegenMult):0.0} /s)";

                TxtMagicResist.Text = totalMagicResist.ToString() + "%";
                TxtStealthDisplay.Text = "+" + (stealthBonus * 100).ToString("0") + "%";
                if (TxtSpellAbsorb != null) TxtSpellAbsorb.Text = spellAbsorb.ToString() + "%";

                // --- Perk Verfügbarkeit ---
                if (PerkDatabase != null)
                {
                    foreach (var perk in PerkDatabase)
                    {
                        bool shouldBeAvailable = level >= perk.RequiredLevel;
                        if (perk.IsAvailable != shouldBeAvailable) perk.IsAvailable = shouldBeAvailable;
                    }

                    if (TxtWarriorCount != null) TxtWarriorCount.Text = $"Warrior: {PerkDatabase.Count(p => p.IsActive && p.Category == "WARRIOR")}";
                    if (TxtThiefCount != null) TxtThiefCount.Text = $"Thief: {PerkDatabase.Count(p => p.IsActive && p.Category == "THIEF")}";
                    if (TxtMageCount != null) TxtMageCount.Text = $"Mage: {PerkDatabase.Count(p => p.IsActive && p.Category == "MAGE")}";
                }

                // --- Extra Effect UI Toggle ---
                bool hasExtraEffect = PerkDatabase?.Any(p => p.IsActive && p.Name == "Extra Effect") ?? false;
                LblEnchantment2.Visibility = hasExtraEffect ? Visibility.Visible : Visibility.Collapsed;
                CmbEnchantment2.Visibility = hasExtraEffect ? Visibility.Visible : Visibility.Collapsed;
                if (!hasExtraEffect) CmbEnchantment2.SelectedIndex = 0;
            }
            finally
            {
                _isCalculating = false;
            }
        }

        // ==========================================
        // 1. ZENTRALE BERECHNUNG (Single Source of Truth)
        // ==========================================
        public double CalculateTotalUnarmedDamage()
        {
            double unarmedDamage = 4; // Vanilla Standard

            // --- DER FIX: Den echten Namen aus dem Rassen-Objekt holen! ---
            string currentRace = (CmbRace.SelectedItem as Race)?.Name ?? "";

            if (currentRace == "Khajiit")
            {
                unarmedDamage = 22;
            }
            else if (currentRace == "Argonian")
            {
                unarmedDamage = 10;
            }

            if (EquippedItems == null) return unarmedDamage;

            var equippedHands = EquippedItems.FirstOrDefault(i => i.Slot == "Hands");
            var equippedRing = EquippedItems.FirstOrDefault(i => i.Slot == "Ring");

            if (equippedHands != null)
            {
                string effectText = "";
                if (equippedHands.OriginalObject is Armor a && !string.IsNullOrEmpty(a.Effect))
                    effectText += a.Effect + " ";

                if (!string.IsNullOrEmpty(equippedHands.Enchantment))
                    effectText += equippedHands.Enchantment + " ";

                // Der perfekte Regex, der im Log geglänzt hat
                if (effectText.Contains("Unarmed", StringComparison.OrdinalIgnoreCase))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(effectText, @"\d+");
                    if (match.Success)
                    {
                        unarmedDamage += double.Parse(match.Value);
                    }
                }

                // Perk: Fists of Steel
                bool hasFistsOfSteel = PerkDatabase.Any(p => p.Name == "Fists of Steel" && p.IsActive);
                if (hasFistsOfSteel && equippedHands.Category == "Heavy Armor" && equippedHands.OriginalObject is Armor armor)
                {
                    unarmedDamage += armor.ArmorRating;
                }
            }

            if (equippedRing != null && equippedRing.ItemName.Contains("Ring of the Beast"))
            {
                unarmedDamage += 20;
            }

            return unarmedDamage;
        }

        // ==========================================
        // 2. UI UPDATE METHODE
        // ==========================================
        private void UpdateWeaponOrUnarmedDamage()
        {
            if (EquippedItems == null) return;

            bool isWeaponEquipped = EquippedItems.Any(i =>
                i.Category == "Weapon" || i.Category.Contains("Sword") || i.Category.Contains("Axe"));

            // --- FALL 1: EINE WAFFE IST AUSGERÜSTET ---
            if (isWeaponEquipped)
            {
                LblWeaponDamageTitle.Text = "Total Weapon Damage:";
                return;
            }

            // --- FALL 2: WAFFENLOS ---
            LblWeaponDamageTitle.Text = "Unarmed Damage:";

            // Wert ECHTE 40 berechnen lassen
            double totalUnarmed = CalculateTotalUnarmedDamage();

            // Unten im Total-Stats UI eintragen
            if (TxtTotalDamage != null) TxtTotalDamage.Text = totalUnarmed.ToString();

            // --- DUMMY ITEM UPDATE ---
            var mainHandItem = EquippedItems.FirstOrDefault(i => i.Slot == "Main-Hand");

            if (mainHandItem != null)
            {
                mainHandItem.ItemName = "Unarmed (Fists)";
                mainHandItem.Rating = totalUnarmed.ToString(); // HIER KRIEGT DAS ITEM DIE 40!
                mainHandItem.Enchantment = "";
            }
            else
            {
                EquippedItems.Insert(0, new EquippedItem
                {
                    Slot = "Main-Hand",
                    ItemName = "Unarmed (Fists)",
                    Category = "Unarmed",
                    Rating = totalUnarmed.ToString(), // ODER HIER!
                    Enchantment = ""
                });
            }

            // ZWINGENDES UI-UPDATE (Äquivalent zu OnPropertyChanged in deinem aktuellen Setup)
            if (LstBuildItems != null)
            {
                LstBuildItems.Items.Refresh();
            }
        }


        // Event für den Haupt-Equip-Button
        private void BtnEquip_Click(object sender, RoutedEventArgs e)
        {
            if (CmbSelect.SelectedItem == null) return;

            EquippedItem? newItem = null;

            // ==========================================
            // 1. FALL: WAFFE (Inklusive Stäbe)
            // ==========================================
            if (CmbSelect.SelectedItem is Weapon w)
            {
                if (w.Name == "None") return;

                // Slot bestimmen (Zweihänder belegen beide Hände)
                string slot = (w.Category == "Two-Handed" || w.Category == "Bow" || w.Category == "Crossbow") ? "Both Hands" : "Main-Hand";

                newItem = new EquippedItem
                {
                    Slot = slot,
                    ItemName = w.Name,
                    Category = w.Category,
                    // Holt Schaden und Stab-Ladungen direkt aus der UI-Anzeige
                    Rating = TxtDamageDisplay.Text,
                    Enchantment = TxtEffect.Text,
                    OriginalObject = w
                };
            }
            // ==========================================
            // 2. FALL: RÜSTUNG
            // ==========================================
            else if (CmbSelect.SelectedItem is Armor a)
            {
                if (a.Name == "None") return;

                newItem = new EquippedItem
                {
                    Slot = a.Slot,
                    ItemName = a.Name,
                    Category = a.Category,
                    Rating = TxtArmorDisplay.Text,
                    Enchantment = TxtArmorEffect.Text,
                    OriginalObject = a
                };
            }

            // ==========================================
            // 3. GEGENSTAND AUSRÜSTEN & SLOT-REINIGUNG
            // ==========================================
            if (newItem != null)
            {
                // FIX für ObservableCollection: Wir suchen alle Items, die im gleichen Slot liegen oder blockiert werden
                var toRemove = EquippedItems.Where(i =>
                    i.Slot == newItem.Slot ||
                    (newItem.Slot == "Both Hands" && (i.Slot == "Main-Hand" || i.Slot == "Off-Hand")) ||
                    ((newItem.Slot == "Main-Hand" || newItem.Slot == "Off-Hand") && i.Slot == "Both Hands")
                ).ToList();

                // Jedes gefundene Item einzeln aus der Liste entfernen
                foreach (var r in toRemove)
                {
                    EquippedItems.Remove(r);
                }

                // Neues Item hinzufügen
                EquippedItems.Add(newItem);

                // Alles neu berechnen
                UpdateTotalBuildStats();

                // UI-Benachrichtigung (optional, falls PropertyChanged implementiert ist)
                NotifyPropertyChanged("TotalWeaponDamage");
                NotifyPropertyChanged("TotalArmorRating");
            }
        }

        private void BtnEquipOffHand_Click(object sender, RoutedEventArgs e)
        {
            // Sicherheits-Check: Nur weitermachen, wenn eine Waffe ausgewählt ist
            if (!(CmbSelect.SelectedItem is Weapon w)) return;
            if (w.Name == "None") return;

            string cat = (w.Category ?? "").ToLower();

            // 1. Validierung: Zweihand-Waffen blockieren
            if (cat.Contains("two-handed") || cat.Contains("bow") || cat.Contains("crossbow"))
            {
                MessageBox.Show($"{w.Category} weapons require both hands and cannot be equipped in the Off-Hand!", "Equipment Error");
                return;
            }

            // 2. Neues EquippedItem erstellen
            var newItem = new EquippedItem
            {
                Slot = "Off-Hand",
                ItemName = w.Name ?? "",      // Sicherstellen, dass Name nicht null ist
                Category = w.Category ?? "",  // Sicherstellen, dass Category nicht null ist

                // UI-Texte können manchmal als null gewertet werden, daher ?? ""
                Rating = TxtDamageDisplay.Text ?? "0",
                Enchantment = TxtEffect.Text ?? "None",

                OriginalObject = w
            };

            // 3. Slot-Bereinigung für ObservableCollection
            // Wir suchen alle Items, die den Slot belegen oder blockieren (Off-Hand oder Zweihänder)
            var toRemove = EquippedItems.Where(i => i.Slot == "Off-Hand" || i.Slot == "Both Hands").ToList();

            foreach (var itemToRemove in toRemove)
            {
                EquippedItems.Remove(itemToRemove);
            }

            // 4. Hinzufügen und Statistiken aktualisieren
            EquippedItems.Add(newItem);

            UpdateTotalBuildStats();

            // Optional: UI über Änderung informieren (falls für Bindings nötig)
            NotifyPropertyChanged("TotalWeaponDamage");
        }

        private void CmbFleshSpell_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Immer wenn der User einen neuen Zauber im Dropdown wählt, 
            // wird die Rüstung sofort neu berechnet!
            UpdateTotalBuildStats();
        }

        // Die zentrale Logik, die alles steuert
        private void EquipItemLogic(string slotName)
        {
            if (CmbSelect.SelectedItem == null) return;

            EquippedItem? newItem = null;

            // Fall 1: Es ist eine WAFFE
            if (isWeaponMode && CmbSelect.SelectedItem is Weapon w)
            {
                newItem = new EquippedItem
                {
                    Slot = slotName,
                    ItemName = w.Name,
                    Category = w.Category,
                    Rating = TxtDamageDisplay.Text,

                    // --- DIESE ZEILE HINZUFÜGEN ---
                    Enchantment = TxtEffect.Text,

                    OriginalObject = w
                };
            }
            // Fall 2: Es ist eine RÜSTUNG
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a)
            {
                newItem = new EquippedItem
                {
                    Slot = a.Slot,
                    ItemName = a.Name,
                    Category = a.Category,
                    Rating = TxtArmorDisplay.Text,

                    // --- DIESE ZEILE HINZUFÜGEN ---
                    Enchantment = TxtArmorEffect.Text,

                    OriginalObject = a
                };
            }

            if (newItem != null)
{
    // 1. Alten Gegenstand im selben Slot suchen
    var existing = EquippedItems.FirstOrDefault(i => i.Slot == newItem.Slot);
    
    if (existing != null) 
    {
        // --- DER FIX FÜR DAS ABLEGEN ---
        // Wenn das alte Item ein Rucksack war, setzen wir die Logik auf null
        if (existing.Slot == "Backpack")
        {
            this.EquippedBackpack = null;
        }
        
        EquippedItems.Remove(existing);
    }

    // 2. Neu hinzufügen
    EquippedItems.Add(newItem);

    // --- DER FIX FÜR DAS NEU ANLEGEN ---
    // Wenn das neue Item ein Rucksack ist, aktivieren wir die Logik
    if (newItem.Slot == "Backpack")
    {
        this.EquippedBackpack = newItem;
    }

    // ZWINGENDES UPDATE ALLER ZAHLEN:
    UpdateTotalBuildStats();

    // ZWINGENDES UPDATE FÜR DAS PREVIEW UND DUMMY-ITEM:
    UpdateWeaponOrUnarmedDamage();
}
        }

        private void FinalizeEquip(string name, string slot, double val, double sneakVal, string effect, string cat)
        {
            RemoveItemBySlot(slot);

            EquippedItems.Add(new EquippedItem
            {
                ItemName = name,
                Slot = slot,
                Rating = val.ToString("0"),
                SneakRating = sneakVal.ToString("0"), // Speichert jetzt den Schleichschaden
                Enchantment = effect,
                Category = cat
            });
        }

        private void RemoveItemBySlot(string slotName)
        {
            var existing = EquippedItems.FirstOrDefault(i => i.Slot == slotName);
            if (existing != null)
            {
                EquippedItems.Remove(existing);
            }
        }

        private void LstBuildItems_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Item direkt aus der Liste entfernen
            if (LstBuildItems.SelectedItem is EquippedItem selectedItem)
            {
                EquippedItems.Remove(selectedItem);
                UpdateTotalBuildStats();
                UpdateWeaponOrUnarmedDamage(); // <--- GENAU HIER!
            }
        }

        private void UpdateTotalBuildStats()
        {
            if (_isCalculating) return;

            double totalDamage = 0;
            double totalArmor = 0;
            double totalSneakDamage = 0;
            double currentWeight = 0;

            int playerLevel = int.TryParse(TxtPlayerLevel.Text, out int l) ? l : 1;
            var activePerks = PerkDatabase?.Where(p => p.IsActive).ToList() ?? new List<Perk>();

            // 1. Schwierigkeits-Multiplikator holen
            double diffMult = SelectedDifficulty?.DamageDealtMultiplier ?? 1.0;

            // Gewichts-Perks prüfen
            bool hasConditioning = activePerks.Any(p => p.Name.Contains("Conditioning"));
            bool hasUnhindered = activePerks.Any(p => p.Name.Contains("Unhindered"));

            // ============================================================
            // 2. HAUPTSCHLEIFE ÜBER AUSRÜSTUNG
            // ============================================================
            foreach (var item in EquippedItems)
            {
                // --- RÜSTUNG BERECHNEN ---
                if (item.OriginalObject is Armor armor)
                {
                    // Rüstung skaliert nicht mit Schwierigkeit (eingehender Schaden wird separat angezeigt)
                    var res = SkyrimCalculator.CalculateArmor(armor, playerLevel, activePerks, null, null, 1.0);

                    item.Rating = Math.Round(res.FinalArmorRating).ToString();
                    totalArmor += res.FinalArmorRating;

                    // Gewichtsberechnung (Perk-Check)
                    bool isHeavy = (armor.Category ?? "").ToLower().Contains("heavy");
                    bool isLight = (armor.Category ?? "").ToLower().Contains("light");

                    if ((isHeavy && hasConditioning) || (isLight && hasUnhindered))
                        currentWeight += 0; // Wiegt nichts durch Perk
                    else
                        currentWeight += armor.Weight;
                }

                // --- WAFFEN & STÄBE BERECHNEN ---
                else if (item.OriginalObject is Weapon weapon && weapon.Name != "None")
                {
                    // Wir übergeben diffMult direkt. Der Calculator erkennt intern:
                    // - Bei Stäben: Augmented Perks + diffMult
                    // - Bei Waffen: Skill Perks + Smithing + diffMult
                    var res = SkyrimCalculator.CalculateWeapon(weapon, playerLevel, activePerks, diffMult, null, null, 1.0);

                    item.Rating = Math.Round(res.FinalDamage).ToString();
                    totalDamage += res.FinalDamage;
                    totalSneakDamage += res.SneakDamage;
                    currentWeight += weapon.Weight;
                }

                // --- FÄUSTE / UNBEWAFFNET BERECHNEN ---
                else if (item.ItemName == "None" || item.ItemName == "Fists" || item.ItemName.Contains("Unarmed"))
                {
                    double fistDmg = CalculateTotalUnarmedDamage() * diffMult;
                    item.Rating = Math.Round(fistDmg).ToString();
                    totalDamage += fistDmg;
                    totalSneakDamage += (fistDmg * 2.0); // Standard-Sneak für Fäuste
                }
            }

            // ============================================================
            // 3. EXTERNE BONI (Steine & Zauber)
            // ============================================================

            // Lord Stone Bonus (+50 Armor)
            var stone = CmbStandingStone?.SelectedItem as StandingStone;
            if (stone?.Name != null && stone.Name.Contains("Lord")) totalArmor += 50;

            // Mage Armor Perk Logik (Flesh Spells)
            bool wearsPhysicalArmor = EquippedItems.Any(i =>
                (i.Category ?? "").ToLower().Contains("light armor") ||
                (i.Category ?? "").ToLower().Contains("heavy armor"));

            if (CmbFleshSpell?.SelectedItem is ComboBoxItem selectedSpell)
            {
                string spellName = selectedSpell.Content?.ToString() ?? "";
                double baseSpellArmor = 0;

                if (spellName.Contains("Oakflesh")) baseSpellArmor = 40;
                else if (spellName.Contains("Stoneflesh")) baseSpellArmor = 60;
                else if (spellName.Contains("Ironflesh")) baseSpellArmor = 80;
                else if (spellName.Contains("Ebonyflesh")) baseSpellArmor = 100;

                if (baseSpellArmor > 0)
                {
                    double mageArmorMult = 1.0;
                    if (!wearsPhysicalArmor) // Perk wirkt nur ohne Rüstung
                    {
                        var maPerk = activePerks.Where(p => p.BaseName == "Mage Armor")
                                                .OrderByDescending(p => p.RequiredLevel)
                                                .FirstOrDefault();
                        if (maPerk != null)
                        {
                            if (maPerk.Name.Contains("1/3")) mageArmorMult = 2.0;
                            else if (maPerk.Name.Contains("2/3")) mageArmorMult = 2.5;
                            else if (maPerk.Name.Contains("3/3")) mageArmorMult = 3.0;
                        }
                    }
                    totalArmor += (baseSpellArmor * mageArmorMult);
                }
            }

            // ============================================================
            // 4. UI AKTUALISIERUNG
            // ============================================================

            // Hauptwerte setzen
            if (TxtTotalDamage != null) TxtTotalDamage.Text = totalDamage.ToString("0");
            if (TxtTotalArmor != null) TxtTotalArmor.Text = totalArmor.ToString("0");
            if (TxtSneakDamage != null) TxtSneakDamage.Text = totalSneakDamage.ToString("0");

            // Tooltip zur Info über Schwierigkeits-Einfluss
            if (diffMult != 1.0 && TxtTotalDamage != null)
                TxtTotalDamage.ToolTip = $"Damage modified by difficulty ({SelectedDifficulty?.Name}): {diffMult * 100}%";

            // Eingehender Schaden Anzeige (Vorschau auf die Gefahr)
            if (TxtIncomingDamage != null)
            {
                double incoming = SelectedDifficulty?.DamageTakenMultiplier ?? 1.0;
                TxtIncomingDamage.Text = (incoming * 100).ToString("0") + "%";
                TxtIncomingDamage.Foreground = incoming > 1.0 ? Brushes.Red : (incoming < 1.0 ? Brushes.LightGreen : new SolidColorBrush(Color.FromRgb(225, 184, 13)));
            }

            // Gewicht & Traglast-Warnung
            if (TxtCurrentWeight != null)
            {
                TxtCurrentWeight.Text = currentWeight.ToString("0.#");
                double maxCarry = double.TryParse(TxtTotalCarryWeight?.Text, out double m) ? m : 300;
                TxtCurrentWeight.Foreground = (currentWeight > maxCarry) ? Brushes.Red : new SolidColorBrush(Color.FromRgb(225, 184, 13));
            }

            // Liste und Perks validieren
            if (LstBuildItems != null) LstBuildItems.Items.Refresh();
            ValidateArmorPerks();
        }

        private void UpdateCalculations()
        {
            CalculateAttributes();

            if (CmbSelect == null || CmbEnchantment == null || CmbEnchantment2 == null ||
                CmbSoulGem == null || CmbDifficulty == null || CmbSelect.SelectedItem == null) return;

            int level = int.TryParse(TxtPlayerLevel.Text, out int l) ? l : 1;
            var active = PerkDatabase.Where(p => p.IsActive).ToList();

            var selectedEnch = CmbEnchantment.SelectedItem as Enchantment;
            var selectedEnch2 = CmbEnchantment2.SelectedItem as Enchantment;
            var selectedGem = CmbSoulGem.SelectedItem as SoulGem;
            double gemMultiplier = (selectedGem == null || selectedGem.Name == "None") ? 1.0 : selectedGem.Multiplier;

            // Doppelte Verzauberung verhindern
            if (selectedEnch != null && selectedEnch2 != null &&
                selectedEnch.Name != "None" && selectedEnch.Name == selectedEnch2.Name)
            {
                _isCalculating = true;
                CmbEnchantment2.SelectedIndex = 0;
                _isCalculating = false;
                selectedEnch2 = CmbEnchantment2.SelectedItem as Enchantment;
            }

            double dMult = SelectedDifficulty?.DamageDealtMultiplier ?? 1.0;

            // ==========================================
            // WAFFEN-BERECHNUNG (Inklusive Stäbe)
            // ==========================================
            if (isWeaponMode && CmbSelect.SelectedItem is Weapon w)
            {
                if (w.Name == "None") return;

                bool isStaff = w.Category.ToLower().Contains("staff");

                double ammoDamage = 0;
                if (CmbAmmo != null && CmbAmmo.Visibility == Visibility.Visible && CmbAmmo.SelectedItem is Weapon ammo)
                {
                    ammoDamage = ammo.Damage;
                }

                // Neues Waffen-Objekt für den Calculator bauen (inkl. Stab-Daten)
                Weapon calcWeapon = new Weapon
                {
                    Name = w.Name,
                    Category = w.Category,
                    Damage = w.Damage + ammoDamage,
                    Speed = w.Speed,
                    Reach = w.Reach,
                    Stagger = w.Stagger,
                    IsEnchantable = w.IsEnchantable,
                    MaxCharges = w.MaxCharges,
                    Element = w.Element,
                    MagicSchool = w.MagicSchool,
                    Effect = w.Effect, // <--- DIESE ZEILE HAT GEFEHLT!
                    LevelVariants = w.LevelVariants // Auch die Varianten mitgeben, falls vorhanden
                };

                var calcResult = SkyrimCalculator.CalculateWeapon(calcWeapon, level, active, dMult, selectedEnch, selectedEnch2, gemMultiplier);

                // --- Kategorie & Arcane Blacksmith Warnung ---
                TxtWeaponCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                         ? w.Category
                                         : $"{w.Category} {calcResult.SmithingTierName}";

                if (calcResult.SmithingTierName.Contains("Arcane"))
                {
                    TxtWeaponCategory.Text = w.Category + "\n(Needs Arcane Blacksmith)";
                    TxtWeaponCategory.Foreground = Brushes.Red;
                }
                else
                {
                    TxtWeaponCategory.Foreground = new SolidColorBrush(Color.FromRgb(225, 184, 13));
                }

                // --- Schaden-Anzeige ---
                TxtDamageDisplay.Text = Math.Round(calcResult.FinalDamage).ToString();
                TxtSneakDisplay.Text = Math.Round(calcResult.SneakDamage).ToString();

                // --- SPEZIELLE STAB-LOGIK FÜR DIE UI-FELDER ---
                if (isStaff)
                {
                    TxtReach.Text = "N/A";
                    TxtSpeed.Text = "N/A";
                    TxtStagger.Text = "N/A";

                    // Bei Stäben zeigen wir die Ladungen im Effekt-Feld mit an
                    TxtEffect.Text = $"Max Charges: {w.MaxCharges}\n{calcResult.FinalEffectText}";
                }
                else
                {
                    TxtReach.Text = w.Reach > 0 ? w.Reach.ToString("0.0") : "-";
                    TxtSpeed.Text = w.Speed > 0 ? w.Speed.ToString("0.0") : "-";
                    TxtStagger.Text = calcResult.FinalStagger > 0 ? calcResult.FinalStagger.ToString("0.00") : "-";
                    TxtEffect.Text = calcResult.FinalEffectText;
                }

                var stats = w.GetStatsForLevel(level);
                TxtValue.Text = Math.Round(stats.val).ToString();
            }
            // ==========================================
            // RÜSTUNGS-BERECHNUNG (Bleibt wie gehabt)
            // ==========================================
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a)
            {
                if (a.Name == "None") return;

                var calcResult = SkyrimCalculator.CalculateArmor(a, level, active, selectedEnch, selectedEnch2, gemMultiplier);

                TxtArmorCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                         ? a.Slot
                                         : $"{a.Slot} {calcResult.SmithingTierName}";
                TxtArmorDisplay.Text = Math.Round(calcResult.FinalArmorRating).ToString();

                // Effekt-Logik für Rüstung (gekürzt zur Übersicht)
                string displayEffect = calcResult.FinalEffectText;
                if (displayEffect == "None" || string.IsNullOrWhiteSpace(displayEffect)) displayEffect = a.Effect;

                TxtArmorEffect.Text = displayEffect;
                var stats = a.GetStatsForLevel(level);
                TxtArmorWeight.Text = calcResult.FinalWeight > 0 ? calcResult.FinalWeight.ToString("0.#") : "0 (Weightless)";
                TxtArmorValue.Text = Math.Round(stats.val).ToString();
            }

            UpdateSpeedDisplay();
            ValidateArmorPerks();
        }

        private void ApplyItemFilter()
        {
            if (TxtSearch == null || CmbSelect == null) return;
            string search = TxtSearch.Text.ToLower();

            if (isWeaponMode)
            {
                // Hier filtern wir die Waffen UND werfen die Munition raus
                var filtered = WeaponDatabase.Where(w =>
                    w.Name.ToLower().Contains(search) &&
                    !w.Category.ToLower().Contains("ammunition") // <- Der Riegel für die Pfeile
                ).ToList();

                filtered.Insert(0, new Weapon { Name = "None", Category = "-", Damage = 0 });
                CmbSelect.ItemsSource = filtered;
            }
            else
            {
                // Filter für Rüstungen
                var filtered = ArmorDatabase.Where(a => a.Name.ToLower().Contains(search)).ToList();
                filtered.Insert(0, new Armor { Name = "None", Slot = "-", ArmorRating = 0 });
                CmbSelect.ItemsSource = filtered;
            }

            CmbSelect.SelectedIndex = 0;
        }

        // NEUE METHODE: Lädt nur Munition
        private void ApplyAmmoFilter(bool isCrossbow)
        {
            if (CmbAmmo == null || WeaponDatabase == null) return;

            string searchTag = isCrossbow ? "bolt" : "arrow";

            // Wir filtern die Datenbank nach dem entsprechenden Tag im Namen oder der Kategorie
            var ammoList = WeaponDatabase.Where(w =>
                w.Category.ToLower().Contains("ammunition") &&
                (w.Name.ToLower().Contains(searchTag) || w.Category.ToLower().Contains(searchTag))
            ).OrderBy(w => w.Damage).ToList();

            // Standard-Eintrag hinzufügen
            ammoList.Insert(0, new Weapon { Name = isCrossbow ? "No Bolts" : "No Arrows", Damage = 0 });

            CmbAmmo.ItemsSource = ammoList;
            CmbAmmo.SelectedIndex = 0;
        }

        private void Perk_Click(object sender, RoutedEventArgs e)
        {
            // Wir prüfen, ob der Absender eine CheckBox ist UND ob sie Text enthält
            if (sender is CheckBox chk && chk.Content != null)
            {
                string perkName = chk.Content.ToString() ?? "";

                // Wir suchen den Perk in der Datenbank (und stellen sicher, dass p.Name nicht null ist)
                var perk = PerkDatabase.FirstOrDefault(p =>
                    p.Name != null && perkName != "" && p.Name.Contains(perkName));

                if (perk != null)
                {
                    perk.IsActive = chk.IsChecked ?? false;
                }
            }

            // Alles neu berechnen
            UpdateCalculations();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Keine eigene Logik mehr hier drin, wir rufen nur den Master-Filter auf
            ApplyItemFilter();
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

        private void ApplyEnchantmentFilter()
        {
            if (CmbSelect == null || CmbEnchantment == null || CmbEnchantment2 == null) return;

            // Wir holen uns das aktuell gewählte Item
            var selectedItem = CmbSelect.SelectedItem;
            string currentSlot = "";

            if (selectedItem is Weapon) currentSlot = "Weapon";
            else if (selectedItem is Armor a) currentSlot = a.Slot;

            // Die Filter-Logik:
            // Wir zeigen nur Verzauberungen an, die entweder "None" heißen 
            // oder deren 'CompatibleSlots' den aktuellen Slot enthalten.
            var filteredList = EnchantmentDatabase.Where(e =>
                e.Name == "None" ||
                e.CompatibleSlots.Contains(currentSlot)
            ).ToList();

            // Wir weisen die gefilterte Liste BEIDEN Boxen zu
            CmbEnchantment.ItemsSource = filteredList;
            CmbEnchantment2.ItemsSource = filteredList;

            // Falls die aktuelle Auswahl durch den Filter ungültig wurde, auf "None" zurücksetzen
            if (CmbEnchantment.SelectedIndex == -1) CmbEnchantment.SelectedIndex = 0;
            if (CmbEnchantment2.SelectedIndex == -1) CmbEnchantment2.SelectedIndex = 0;
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

        private void CmbSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // --- FALL 1: NICHTS AUSGEWÄHLT (Oder "None") -> UNBEWAFFNET ANZEIGEN ---
            if (CmbSelect.SelectedItem == null || CmbSelect.SelectedItem.ToString() == "None" ||
                (CmbSelect.SelectedItem is Weapon wNone && wNone.Name == "None") ||
                (CmbSelect.SelectedItem is Armor aNone && aNone.Name == "None"))
            {
                WeaponPanel.Visibility = Visibility.Visible;
                ArmorPanel.Visibility = Visibility.Collapsed;

                TxtWeaponCategory.Text = "Unarmed";
                TxtDamageDisplay.Text = CalculateTotalUnarmedDamage().ToString();
                TxtSneakDisplay.Text = (CalculateTotalUnarmedDamage() * 2).ToString();
                TxtReach.Text = "-"; TxtSpeed.Text = "-"; TxtStagger.Text = "-"; TxtValue.Text = "0"; TxtEffect.Text = "None";

                BtnEquip.Content = "Equip Main-Hand";
                BtnEquip.Visibility = Visibility.Visible;
                BtnEquipOffHand.Visibility = Visibility.Collapsed;

                LblAmmo.Visibility = Visibility.Collapsed;
                CmbAmmo.Visibility = Visibility.Collapsed;

                CmbEnchantment.SelectedIndex = 0; CmbEnchantment2.SelectedIndex = 0;
                CmbEnchantment.IsEnabled = false; CmbEnchantment2.IsEnabled = false;

                UpdateCalculations();
                return;
            }

            bool enchantable = true;

            // --- FALL 2: WAFFE (Inklusive Stäbe) ---
            if (CmbSelect.SelectedItem is Weapon w)
            {
                string cat = (w.Category ?? "").ToLower();
                string name = (w.Name ?? "").ToLower();

                // 1. Fernkampf-Logik
                bool isBow = cat.Contains("bow") || name.Contains("bow");
                bool isCrossbow = cat.Contains("crossbow") || name.Contains("crossbow");
                bool isRanged = isBow || isCrossbow;

                LblAmmo.Visibility = isRanged ? Visibility.Visible : Visibility.Collapsed;
                CmbAmmo.Visibility = isRanged ? Visibility.Visible : Visibility.Collapsed;
                if (isRanged) ApplyAmmoFilter(isCrossbow);

                // 2. Button-Steuerung (Snippet-Logik integriert)
                bool isOneHanded = cat.Contains("one-handed") || cat.Contains("dagger") || cat.Contains("staff");
                bool isTwoHanded = cat.Contains("two-handed") || isRanged;

                if (isOneHanded)
                {
                    BtnEquip.Content = "Equip Main-Hand";
                    BtnEquipOffHand.Visibility = Visibility.Visible; // Jetzt auch für Stäbe sichtbar!
                }
                else if (isTwoHanded)
                {
                    BtnEquip.Content = "Equip Both Hands";
                    BtnEquipOffHand.Visibility = Visibility.Collapsed;
                }

                WeaponPanel.Visibility = Visibility.Visible;
                ArmorPanel.Visibility = Visibility.Collapsed;
                enchantable = w.IsEnchantable;
            }
            // --- FALL 3: RÜSTUNG (Inklusive Schilde) ---
            else if (CmbSelect.SelectedItem is Armor a)
            {
                string aCat = (a.Category ?? "").ToLower();
                LblAmmo.Visibility = Visibility.Collapsed;
                CmbAmmo.Visibility = Visibility.Collapsed;

                // Schilde erlauben Off-Hand
                BtnEquipOffHand.Visibility = aCat.Contains("shield") ? Visibility.Visible : Visibility.Collapsed;

                BtnEquip.Content = "Equip Armor";
                WeaponPanel.Visibility = Visibility.Collapsed;
                ArmorPanel.Visibility = Visibility.Visible;

                enchantable = string.IsNullOrEmpty(a.Effect) || a.Effect == "None";
            }

            // --- VERZAUBERUNGS-LOGIK ---
            CmbEnchantment.IsEnabled = enchantable;
            CmbEnchantment2.IsEnabled = enchantable;
            if (!enchantable)
            {
                CmbEnchantment.SelectedIndex = 0;
                CmbEnchantment2.SelectedIndex = 0;
            }

            UpdateEnchantmentList();
            UpdateCalculations();
            NotifyPropertyChanged(nameof(DisplayCategory));
        }

        // Neues Event für die Ammo-Box: Wenn der Pfeil gewechselt wird, sofort neu rechnen
        private void CmbAmmo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCalculations();
        }

        private void UpdateSpeedDisplay()
        {
            // Sicherheitscheck: Ist überhaupt eine Waffe ausgewählt?
            if (CmbSelect.SelectedItem is Weapon w)
            {
                double finalSpeed = w.Speed;
                string cat = (w.Category ?? "").ToLower();

                // 1. Ist es eine Einhandwaffe? (Zweihänder oder Bögen kriegen keinen Buff)
                bool isOneHanded = cat.Contains("one-handed") || cat.Contains("dagger") || cat.Contains("mace") || cat.Contains("war axe");

                // 2. Trägt der Spieler in BEIDEN Händen eine Einhand-Waffe?
                bool hasMainHandWeapon = EquippedItems.Any(i => i.Slot == "Main-Hand" && !i.Category.ToLower().Contains("shield") && !i.Category.ToLower().Contains("two-handed") && !i.Category.ToLower().Contains("archery"));
                bool hasOffHandWeapon = EquippedItems.Any(i => i.Slot == "Off-Hand" && !i.Category.ToLower().Contains("shield"));

                if (isOneHanded && hasMainHandWeapon && hasOffHandWeapon)
                {
                    // 3. Welcher Perk ist aktiv?
                    var flurryPerk = PerkDatabase?.FirstOrDefault(p => p.IsActive && p.BaseName == "Dual Flurry");
                    if (flurryPerk != null)
                    {
                        if (flurryPerk.Name.Contains("1/2")) finalSpeed *= 1.20; // +20% Speed
                        else if (flurryPerk.Name.Contains("2/2")) finalSpeed *= 1.35; // +35% Speed

                        // Optisches Feedback: Türkis einfärben, damit der User den Buff erkennt!
                        if (TxtSpeed != null) TxtSpeed.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20FFD4"));
                    }
                    else
                    {
                        if (TxtSpeed != null) TxtSpeed.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1B80D"));
                    }
                }
                else
                {
                    if (TxtSpeed != null) TxtSpeed.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1B80D"));
                }

                // Wert ins UI schreiben (mit 2 Nachkommastellen, z.B. 1.30 -> 1.76)
                if (TxtSpeed != null) TxtSpeed.Text = finalSpeed.ToString("0.00");
            }
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

            string currentSlot = "";
            if (isWeaponMode && CmbSelect.SelectedItem is Weapon) currentSlot = "Weapon";
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a) currentSlot = a.Slot;

            // Filter: Nur Effekte für den richtigen Slot (oder "None")
            var filtered = EnchantmentDatabase
                .Where(e => e.Name == "None" || e.CompatibleSlots.Contains(currentSlot))
                .ToList();

            // WICHTIG: Beiden Dropdowns die gefilterte Liste geben
            CmbEnchantment.ItemsSource = filtered;
            CmbEnchantment2.ItemsSource = filtered;
        }

        private void CmbStandingStone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCalculations();
            UpdateTotalBuildStats();
        }

        private void Perk_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_isCalculating) return;

            // Zuerst die Basis-Attribute (Regeneration, HP, etc.) berechnen
            CalculateAttributes();

            // Dann die Item-Vorschau (falls ein Item gewählt ist)
            UpdateCalculations();

            // Dann die gesamte Build-Statistik (für Rüstungswerte etc.)
            UpdateTotalBuildStats();
        }

        private void BtnSaveBuild_Click(object sender, RoutedEventArgs e)
        {
            var saveData = new BuildSaveData
            {
                BuildName = "Mein Skyrim Build",
                Level = TxtPlayerLevel.Text,
                SelectedRace = (CmbRace.SelectedItem as Race)?.Name ?? "None",
                SelectedDifficulty = SelectedDifficulty?.Name ?? "Adept",
                SelectedStandingStone = (CmbStandingStone.SelectedItem as StandingStone)?.Name ?? "None",
                SelectedFleshSpell = (CmbFleshSpell.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "None",

                InvestMagicka = TxtInvestMagicka.Text,
                InvestHealth = TxtInvestHealth.Text,
                InvestStamina = TxtInvestStamina.Text,

                ActivePerkNames = PerkDatabase?.Where(p => p.IsActive).Select(p => p.Name).ToList() ?? new List<string>(),

                EquippedItems = EquippedItems.Select(i => new EquippedItemSaveData
                {
                    Slot = i.Slot ?? "",
                    ItemName = i.ItemName ?? "",
                    Category = i.Category ?? "",
                    Rating = i.Rating ?? "0",
                    Enchantment = i.Enchantment ?? ""
                }).ToList()
            };

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Skyrim Build (*.json)|*.json",
                FileName = "MySkyrimBuild.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                    string json = System.Text.Json.JsonSerializer.Serialize(saveData, options);
                    System.IO.File.WriteAllText(dialog.FileName, json);
                    MessageBox.Show("Build erfolgreich in den Archiven von Winterfeste gespeichert! 📜✨", "Speichern erfolgreich");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler");
                }
            }
        }

        private void BtnLoadBuild_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Skyrim Build (*.json)|*.json" };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string json = System.IO.File.ReadAllText(dialog.FileName);

                    // Wir laden es in unsere neue BuildSaveData Struktur
                    var loadData = System.Text.Json.JsonSerializer.Deserialize<BuildSaveData>(json);

                    if (loadData == null) return;

                    // 1. RECHEN-STOPP (Damit UI-Updates nicht querfeuern)
                    _isCalculating = true;

                    // --- SCHWIERIGKEIT LADEN ---
                    if (!string.IsNullOrEmpty(loadData.SelectedDifficulty))
                    {
                        SelectedDifficulty = DifficultyDatabase.FirstOrDefault(d => d.Name == loadData.SelectedDifficulty)
                     ?? DifficultyDatabase.FirstOrDefault(d => d.Name == "Adept")
                     ?? (DifficultyDatabase.Count > 0 ? DifficultyDatabase[0] : new Difficulty { Name = "Adept", DamageDealtMultiplier = 1.0, DamageTakenMultiplier = 1.0 });
                    }

                    // 2. BASIS-DATEN SETZEN (Da es jetzt Strings sind, entfällt .ToString())
                    TxtPlayerLevel.Text = loadData.Level ?? "1";
                    TxtInvestMagicka.Text = loadData.InvestMagicka ?? "0";
                    TxtInvestHealth.Text = loadData.InvestHealth ?? "0";
                    TxtInvestStamina.Text = loadData.InvestStamina ?? "0";

                    // 3. RASSE & STEIN
                    CmbRace.SelectedItem = RaceDatabase.FirstOrDefault(r => r.Name == loadData.SelectedRace);
                    CmbStandingStone.SelectedItem = StandingStoneDatabase.FirstOrDefault(s => s.Name == loadData.SelectedStandingStone);

                    // 4. FLESH SPELL
                    foreach (ComboBoxItem item in CmbFleshSpell.Items)
                    {
                        if (item.Content.ToString() == loadData.SelectedFleshSpell)
                        {
                            CmbFleshSpell.SelectedItem = item;
                            break;
                        }
                    }

                    // 5. PERKS WIEDERHERSTELLEN
                    foreach (var perk in PerkDatabase)
                    {
                        perk.IsActive = loadData.ActivePerkNames != null && loadData.ActivePerkNames.Contains(perk.Name);
                    }

                    // 6. AUSRÜSTUNG WIEDERHERSTELLEN (WICHTIG!)
                    EquippedItems.Clear(); // Alte Liste leeren
                    if (loadData.EquippedItems != null)
                    {
                        foreach (var savedItem in loadData.EquippedItems)
                        {
                            object? original = null;
                            string cat = (savedItem.Category ?? "").ToLower();

                            // Prüfen, wo wir das Item suchen müssen (Rüstungs-Datenbank oder Waffen-Datenbank)
                            if (cat.Contains("armor") || cat.Contains("shield") || cat.Contains("clothing") || cat.Contains("jewelry") || cat.Contains("amulet") || cat.Contains("ring"))
                            {
                                original = ArmorDatabase.FirstOrDefault(a => a.Name == savedItem.ItemName);
                            }
                            else
                            {
                                // Wenn es keine Rüstung ist, muss es eine Waffe (oder ein Stab) sein
                                original = WeaponDatabase.FirstOrDefault(w => w.Name == savedItem.ItemName);
                            }

                            // Das UI-Element neu aufbauen und der Liste hinzufügen
                            EquippedItems.Add(new EquippedItem
                            {
                                Slot = savedItem.Slot ?? "",
                                ItemName = savedItem.ItemName ?? "",
                                Category = savedItem.Category ?? "",
                                Rating = savedItem.Rating ?? "",
                                Enchantment = savedItem.Enchantment ?? "",
                                OriginalObject = original // <--- Das Re-Linking für den Calculator!
                            });
                        }
                    }

                    // 7. FINALE BERECHNUNG
                    _isCalculating = false;

                    // Jetzt triggern wir die Updates, die alles (inklusive Items) neu berechnen
                    UpdateCalculations();
                    UpdateTotalBuildStats();

                    string displayBuildName = string.IsNullOrEmpty(loadData.BuildName) ? "Unbekannt" : loadData.BuildName;
                    MessageBox.Show($"Build '{displayBuildName}' wurde erfolgreich geladen! 📜✨", "Winterfeste Archive");
                }
                catch (Exception ex)
                {
                    _isCalculating = false;
                    MessageBox.Show("Der Erzmagier meldet einen Fehler beim Laden: " + ex.Message, "Fehler");
                }
            }
        }

        private void BtnExportText_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sb = new System.Text.StringBuilder();

                // --- HEADER ---
                sb.AppendLine("===============================================");
                sb.AppendLine("           SKYRIM BUILD ARCHITECT              ");
                sb.AppendLine("===============================================");
                sb.AppendLine($"BUILD EXPORT - LEVEL {TxtPlayerLevel.Text}");
                sb.AppendLine($"Race: {(CmbRace.SelectedItem as Race)?.Name ?? "None"}");
                sb.AppendLine($"Difficulty: {SelectedDifficulty?.Name ?? "Adept"}");
                sb.AppendLine("-----------------------------------------------");

                // --- ATTRIBUTE & STATS ---
                sb.AppendLine("--- CHARACTER STATS ---");
                sb.AppendLine($"Magicka: {TxtTotalMagicka.Text} {TxtRegenMagicka.Text}");
                sb.AppendLine($"Health:  {TxtTotalHealth.Text} {TxtRegenHealth.Text}");
                sb.AppendLine($"Stamina: {TxtTotalStamina.Text} {TxtRegenStamina.Text}");
                sb.AppendLine();
                sb.AppendLine($"Total Damage: {TxtTotalDamage.Text}");
                sb.AppendLine($"Armor Rating: {TxtTotalArmor.Text}");
                sb.AppendLine("-----------------------------------------------");

                // --- AUSRÜSTUNG (Angepasst an neue Felder) ---
                sb.AppendLine("--- EQUIPPED ITEMS ---");
                if (EquippedItems.Count == 0)
                {
                    sb.AppendLine("- No items equipped");
                }
                else
                {
                    foreach (var item in EquippedItems)
                    {
                        // Hauptzeile: Slot, Name und das Rating (Schaden/Rüstung/Charges)
                        sb.AppendLine($"{item.Slot}: {item.ItemName} ({item.Rating})");

                        // Falls ein Effekt/Enchantment da ist (und nicht "None"), rücken wir ihn ein
                        if (!string.IsNullOrEmpty(item.Enchantment) && item.Enchantment != "None")
                        {
                            // Wir säubern den String ein wenig für den Export
                            string cleanEffect = item.Enchantment.Replace("\r", "").Replace("\n", " ");
                            sb.AppendLine($"   -> {cleanEffect}");
                        }
                    }
                }
                sb.AppendLine();

                // --- PERKS ---
                sb.AppendLine("--- ACTIVE PERKS ---");
                var activePerks = PerkDatabase?.Where(p => p.IsActive).ToList() ?? new List<Perk>();
                if (activePerks.Count == 0)
                {
                    sb.AppendLine("- No perks selected");
                }
                else
                {
                    foreach (var p in activePerks.OrderBy(p => p.Category))
                    {
                        sb.AppendLine($"- [{p.Category}] {p.Name}");
                    }
                }
                sb.AppendLine("===============================================");

                // --- AKTIONEN: Zwischenablage & Datei ---

                // 1. In die Zwischenablage kopieren (wie im Snippet gewünscht)
                Clipboard.SetText(sb.ToString());

                // 2. Speicher-Dialog für die Datei
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Textdatei (*.txt)|*.txt",
                    FileName = $"Skyrim_Build_Lvl{TxtPlayerLevel.Text}.txt",
                    Title = "Build als Textdatei speichern"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString());
                    MessageBox.Show("Build wurde in die Zwischenablage kopiert UND als Datei gespeichert! 📜⚔️", "Export erfolgreich");
                }
                else
                {
                    // Falls der User den Dateidialog abbricht, sagen wir ihm, dass es trotzdem im Clipboard ist
                    MessageBox.Show("Export als Datei abgebrochen, aber der Build liegt in deiner Zwischenablage! 📋", "Info");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Exportieren: " + ex.Message, "Fehler");
            }
        }

        private string ShowInputDialog(string title, string prompt, string defaultText)
        {
            // Create a temporary window for input
            Window inputWindow = new Window
            {
                Title = title,
                Width = 350,
                Height = 170,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(18, 18, 18)), // Dark theme
                Foreground = new SolidColorBrush(Color.FromRgb(225, 184, 13)) // Gold text
            };

            StackPanel stack = new StackPanel { Margin = new Thickness(20) };
            TextBlock label = new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10), FontWeight = FontWeights.Bold };
            TextBox inputField = new TextBox { Text = defaultText, Padding = new Thickness(5), Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)), Foreground = Brushes.White, BorderBrush = Brushes.Gray };
            Button confirmButton = new Button { Content = "OK", IsDefault = true, Margin = new Thickness(0, 15, 0, 0), Padding = new Thickness(10, 5, 10, 5), Background = new SolidColorBrush(Color.FromRgb(225, 184, 13)), FontWeight = FontWeights.Bold };

            confirmButton.Click += (s, e) => { inputWindow.DialogResult = true; inputWindow.Close(); };

            stack.Children.Add(label);
            stack.Children.Add(inputField);
            stack.Children.Add(confirmButton);
            inputWindow.Content = stack;

            if (inputWindow.ShowDialog() == true)
            {
                return inputField.Text;
            }
            return "";
        }

        private void ValidateArmorPerks()
        {
            if (PerkDatabase == null || EquippedItems == null) return;

            // Wir nutzen .Trim(), um sicherzugehen, dass keine Leerzeichen das Level-Parsing stören
            int playerLevel = int.TryParse(TxtPlayerLevel.Text.Trim(), out int l) ? l : 1;

            var head = EquippedItems.FirstOrDefault(i => i.Slot.ToLower().Contains("head") || i.Slot.ToLower().Contains("helmet"));
            var chest = EquippedItems.FirstOrDefault(i => i.Slot.ToLower().Contains("chest") || i.Slot.ToLower().Contains("armor"));
            var hands = EquippedItems.FirstOrDefault(i => i.Slot.ToLower().Contains("hands") || i.Slot.ToLower().Contains("gauntlets"));
            var feet = EquippedItems.FirstOrDefault(i => i.Slot.ToLower().Contains("feet") || i.Slot.ToLower().Contains("boots"));

            bool isAllHeavy = false;
            bool isAllLight = false;
            bool isMatchingSet = false;

            if (head != null && chest != null && hands != null && feet != null)
            {
                isAllHeavy = (head.Category ?? "").ToLower().Contains("heavy") &&
                             (chest.Category ?? "").ToLower().Contains("heavy") &&
                             (hands.Category ?? "").ToLower().Contains("heavy") &&
                             (feet.Category ?? "").ToLower().Contains("heavy");

                isAllLight = (head.Category ?? "").ToLower().Contains("light") &&
                             (chest.Category ?? "").ToLower().Contains("light") &&
                             (hands.Category ?? "").ToLower().Contains("light") &&
                             (feet.Category ?? "").ToLower().Contains("light");

                if (isAllHeavy || isAllLight)
                {
                    string mH = (head.ItemName ?? "").Split(' ')[0];
                    string mC = (chest.ItemName ?? "").Split(' ')[0];
                    string mHa = (hands.ItemName ?? "").Split(' ')[0];
                    string mF = (feet.ItemName ?? "").Split(' ')[0];
                    isMatchingSet = (mH == mC && mC == mHa && mHa == mF);
                }
            }

            void UpdatePerkUI(string baseName, string subCat, bool armorCondition, string armorReqText)
            {
                var perk = PerkDatabase.FirstOrDefault(p =>
                    p.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase) &&
                    p.SubCategory.Equals(subCat, StringComparison.OrdinalIgnoreCase));

                if (perk == null) return;

                // Wir prüfen nur noch, ob die Rüstung passt. Das Level-Handling machen wir im Text.
                if (!armorCondition)
                {
                    perk.IsAvailable = false;
                    // Hier steht jetzt NUR noch die Bedingung, kein "Locked" mehr
                    perk.AvailabilityHint = armorReqText;
                }
                else
                {
                    perk.IsAvailable = true;
                    perk.AvailabilityHint = "Requirement met.";
                }

                // Falls die Rüstung nicht passt, muss der Haken raus
                if (!perk.IsAvailable) perk.IsActive = false;
            }

            // --- Aufrufe mit deinen Wunsch-Texten ---
            UpdatePerkUI("Well Fitted", "HEAVY ARMOR", isAllHeavy, "Requires 4 pieces of Heavy Armor: head, chest, hands, feet.");
            UpdatePerkUI("Matching Set", "HEAVY ARMOR", isAllHeavy && isMatchingSet, "Requires a matched set of Heavy Armor (all 4 pieces same material).");

            UpdatePerkUI("Custom Fit", "LIGHT ARMOR", isAllLight, "Requires 4 pieces of Light Armor: head, chest, hands, feet.");
            UpdatePerkUI("Matching Set", "LIGHT ARMOR", isAllLight && isMatchingSet, "Requires a matched set of Light Armor (all 4 pieces same material).");
            UpdatePerkUI("Wind Walker", "LIGHT ARMOR", isAllLight, "Requires 4 pieces of Light Armor: head, chest, hands, feet.");
        }

        private void TxtPerkSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 1. Sicherheitscheck: Haben wir überhaupt Daten?
            if (PerkDatabase == null || LstPerks == null) return;

            // 2. Den Suchbegriff holen und klein machen (für den Vergleich)
            string filter = TxtPerkSearch.Text.ToLower().Trim();

            // 3. Die "Sicht" auf die Perk-Liste holen (das CollectionView)
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(PerkDatabase);

            if (view != null)
            {
                // 4. Den Filter definieren
                view.Filter = (obj) =>
                {
                    // Wenn das Suchfeld leer ist, zeige alles an
                    if (string.IsNullOrWhiteSpace(filter)) return true;

                    if (obj is Perk p)
                    {
                        // Wir prüfen, ob der Name oder die Unterkategorie das Suchwort enthält
                        bool nameMatches = p.Name != null && p.Name.ToLower().Contains(filter);
                        bool categoryMatches = p.SubCategory != null && p.SubCategory.ToLower().Contains(filter);

                        return nameMatches || categoryMatches;
                    }
                    return false;
                };

                // 5. Die Liste zwingen, sich mit dem neuen Filter neu zu zeichnen
                view.Refresh();
            }
        }

        // Das hier gehört ganz unten in die Klasse MainWindow
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }


}