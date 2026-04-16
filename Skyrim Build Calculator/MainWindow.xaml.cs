using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.ObjectModel;

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
        public Dictionary<string, EquippedItem?> CurrentBuild = new Dictionary<string, EquippedItem?>()
        {
            { "Weapon", null }, { "Shield", null }, { "Head", null },
            { "Chest", null }, { "Hands", null }, { "Feet", null },
            { "Ring", null }, { "Necklace", null }
        };

        // HIER EINGEFÜGT: Die neue Liste für flexibles Equipment (Dual-Wield Support)
        public ObservableCollection<EquippedItem> EquippedItems { get; set; } = new ObservableCollection<EquippedItem>();

        private bool isWeaponMode = true;

        public MainWindow()
        {
            InitializeComponent();

            // --- NEU: Die Brücke zwischen Code und UI ---
            // Damit die Liste der ausgerüsteten Items auch wirklich angezeigt wird:
            LstBuildItems.ItemsSource = EquippedItems;

            LoadData();
            LoadRaceData();
            LoadStandingStones();

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

            // Standard-Auswahl setzen
            if (CmbDifficulty.Items.Count > 0) CmbDifficulty.SelectedIndex = 2; // Adept
            CmbRace.SelectedIndex = 0;
            CmbStandingStone.SelectedIndex = 0;

            // Initial-Berechnungen durchführen, sobald das Fenster geladen ist
            this.Loaded += (s, e) =>
            {
                CalculateAttributes();
                UpdateTotalBuildStats();
                BtnShowWeapons_Click(this, new RoutedEventArgs());
                if (view != null) view.Refresh();
            };
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
            // 1. Erweiterter Sicherheitscheck (inkl. dem neuen Perk-Label)
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
                int totalEarnedPoints = level - 1; // Level 1 = 0 Punkte, Level 100 = 99 Punkte
                SolidColorBrush goldBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1B80D"));

                // --- Attribute (Magicka, Health, Stamina) ---
                int statsSpent = investMagicka + investHealth + investStamina;
                LblAvailablePoints.Text = $"Attribute Points: {statsSpent} / {totalEarnedPoints}";
                LblAvailablePoints.Foreground = (statsSpent > totalEarnedPoints) ? Brushes.Red : goldBrush;

                // --- Perks ---
                int perksSpent = activePerks.Count;
                LblAvailablePerkPoints.Text = $"Perk Points: {perksSpent} / {totalEarnedPoints}";
                LblAvailablePerkPoints.Foreground = (perksSpent > totalEarnedPoints) ? Brushes.Red : goldBrush;

                // ============================================================
                // 5. ATTRIBUT-WERTE BERECHNEN
                // ============================================================
                double totalMagicka = 100 + (race?.BonusMagicka ?? 0) + (investMagicka * 10) + (stone?.BonusMagicka ?? 0);
                double totalHealth = 100 + (investHealth * 10);
                double totalStamina = 100 + (investStamina * 10);

                // --- REGENERATION BERECHNEN ---
                double mRegenMult = stone?.MagickaRegenMult ?? 1.0;
                double hRegenMult = stone?.HealthRegenMult ?? 1.0;
                double sRegenMult = stone?.StaminaRegenMult ?? 1.0;

                // Recovery Perk (Restoration)
                double recoveryBonus = activePerks
                    .Where(p => p.BaseName == "Recovery")
                    .Select(p => p.Multiplier - 1.0)
                    .DefaultIfEmpty(0.0)
                    .Max();
                mRegenMult += recoveryBonus;

                // Wind Walker Perk (Light Armor)
                bool hasWindWalker = activePerks.Any(p => p.Name.Contains("Wind Walker"));
                int lightArmorCount = EquippedItems.Count(i => (i.Category ?? "").ToLower().Contains("light armor"));

                if (hasWindWalker && lightArmorCount >= 4)
                {
                    sRegenMult += 0.5;
                }

                // ============================================================
                // 6. GLOBALE STATS BERECHNEN
                // ============================================================

                // 1. Traglast (Skyrim Basis 300 + 5 pro Stamina-Punkt)
                double totalCarryWeight = 300 + (investStamina * 5);
                if (activePerks.Any(p => p.Name.Contains("Extra Pockets"))) totalCarryWeight += 100;
                if (stone?.Name == "The Steed Stone") totalCarryWeight += 100;

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

                TxtTotalCarryWeight.Text = totalCarryWeight.ToString();
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
                        if (!perk.IsAvailable && perk.IsActive) perk.IsActive = false;
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

        // Event für den Haupt-Equip-Button
        private void BtnEquip_Click(object sender, RoutedEventArgs e)
        {
            EquipItemLogic("Main-Hand");
        }

        private void BtnEquipOffHand_Click(object sender, RoutedEventArgs e)
        {
            EquipItemLogic("Off-Hand");
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
                    Slot = slotName, // z.B. "Main-Hand" oder "Off-Hand"
                    ItemName = w.Name,
                    Category = w.Category,
                    Rating = TxtDamageDisplay.Text,      // Aktueller Wert aus der UI
                    OriginalObject = w                   // DAS WICHTIGSTE: Die ganze Waffe mitschicken!
                };
            }
            // Fall 2: Es ist eine RÜSTUNG
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a)
            {
                newItem = new EquippedItem
                {
                    Slot = a.Slot,                       // Nutzt den Slot aus der Datenbank (Head, Chest, etc.)
                    ItemName = a.Name,
                    Category = a.Category,
                    Rating = TxtArmorDisplay.Text,      // Aktueller Wert aus der UI
                    OriginalObject = a                   // DAS WICHTIGSTE: Die ganze Rüstung mitschicken!
                };
            }

            if (newItem != null)
            {
                // Alten Gegenstand im selben Slot entfernen
                var existing = EquippedItems.FirstOrDefault(i => i.Slot == newItem.Slot);
                if (existing != null) EquippedItems.Remove(existing);

                // Neu hinzufügen
                EquippedItems.Add(newItem);

                // Stats sofort aktualisieren
                UpdateTotalBuildStats();
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
            var selectedRace = CmbRace?.SelectedItem as Race; // NEU: Rasse holen für Unarmed

            bool hasConditioning = activePerks.Any(p => p.Name.Contains("Conditioning"));
            bool hasUnhindered = activePerks.Any(p => p.Name.Contains("Unhindered"));

            // Schwere Handschuhe für Fists of Steel suchen
            var heavyGauntletsItem = EquippedItems.FirstOrDefault(i => i.Slot == "Hands" && (i.Category ?? "").ToLower().Contains("heavy armor"));
            Armor? heavyGauntlets = heavyGauntletsItem?.OriginalObject as Armor;

            // ============================================================
            // 1. SCHLEIFE ÜBER ALLE ITEMS (LIVE-NEUBERECHNUNG)
            // ============================================================
            foreach (var item in EquippedItems)
            {
                if (item.OriginalObject is Armor armor)
                {
                    var res = SkyrimCalculator.CalculateArmor(armor, playerLevel, activePerks, null, null, 1.0);
                    item.Rating = Math.Round(res.FinalArmorRating).ToString();
                    totalArmor += res.FinalArmorRating;

                    bool isHeavy = (armor.Category ?? "").ToLower().Contains("heavy");
                    bool isLight = (armor.Category ?? "").ToLower().Contains("light");

                    if ((isHeavy && hasConditioning) || (isLight && hasUnhindered))
                        currentWeight += 0;
                    else
                        currentWeight += armor.Weight;
                }
                else if (item.OriginalObject is Weapon weapon && weapon.Name != "None")
                {
                    // Waffe live neu berechnen
                    var diffMult = DifficultyDatabase.FirstOrDefault(d => d.Name == (CmbDifficulty.SelectedItem as ComboBoxItem)?.Content.ToString())?.DamageDealtMultiplier ?? 1.0;
                    var res = SkyrimCalculator.CalculateWeapon(weapon, playerLevel, activePerks, diffMult, null, null, 1.0);

                    item.Rating = Math.Round(res.FinalDamage).ToString();
                    totalDamage += res.FinalDamage;
                    totalSneakDamage += res.SneakDamage;
                    currentWeight += weapon.Weight;
                }
                else if (item.ItemName == "None" || item.ItemName == "Fists") // HIER IST DER FIX!
                {
                    // Wenn "None" ausgerüstet ist, nutze unseren intelligenten Unarmed-Calculator
                    double fistDmg = SkyrimCalculator.CalculateUnarmedDamage(selectedRace, activePerks, heavyGauntlets, 0);
                    item.Rating = fistDmg.ToString();
                    totalDamage += fistDmg;

                    // Faustkampf macht bei Schleichangriffen 2x Schaden
                    totalSneakDamage += (fistDmg * 2.0);
                }
            }

            // ==========================================
            // 2. BONI: STEINE & FLESH SPELLS
            // ==========================================
            var stone = CmbStandingStone?.SelectedItem as StandingStone;
            if (stone?.Name != null && stone.Name.Contains("Lord")) totalArmor += 50;

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
                    if (!wearsPhysicalArmor)
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

            // ==========================================
            // 3. UI AKTUALISIERUNG
            // ==========================================
            if (TxtTotalDamage != null) TxtTotalDamage.Text = totalDamage.ToString("0");
            if (TxtTotalArmor != null) TxtTotalArmor.Text = totalArmor.ToString("0");
            if (TxtSneakDamage != null) TxtSneakDamage.Text = totalSneakDamage.ToString("0");

            if (TxtCurrentWeight != null)
            {
                TxtCurrentWeight.Text = currentWeight.ToString("0.#");
                double maxCarry = double.TryParse(TxtTotalCarryWeight?.Text, out double m) ? m : 300;
                TxtCurrentWeight.Foreground = (currentWeight > maxCarry)
                    ? Brushes.Red
                    : new SolidColorBrush(Color.FromRgb(225, 184, 13));
            }

            // Zum Schluss: Liste aktualisieren, falls "None" jetzt "16" als Rating hat
            if (LstBuildItems != null)
            {
                LstBuildItems.Items.Refresh();
            }

            ValidateArmorPerks();
        }

        private void UpdateCalculations()
        {
            // --- 1. SCHRITT: GLOBALE STATS IMMER BERECHNEN ---
            CalculateAttributes();

            // --- 2. SCHRITT: DER TÜRSTEHER (Sicherheitscheck) ---
            if (CmbSelect == null || CmbEnchantment == null || CmbEnchantment2 == null ||
                CmbSoulGem == null || CmbDifficulty == null || CmbAmmo == null ||
                CmbSelect.SelectedItem == null) return;

            // --- 3. SCHRITT: BASIS-DATEN VORBEREITEN ---
            int level = int.TryParse(TxtPlayerLevel.Text, out int l) ? l : 1;
            var active = PerkDatabase.Where(p => p.IsActive).ToList();

            var selectedEnch = CmbEnchantment.SelectedItem as Enchantment;
            var selectedEnch2 = CmbEnchantment2.SelectedItem as Enchantment;

            var selectedGem = CmbSoulGem.SelectedItem as SoulGem;
            double gemMultiplier = (selectedGem == null || selectedGem.Name == "None") ? 1.0 : selectedGem.Multiplier;

            if (selectedEnch != null && selectedEnch2 != null &&
                selectedEnch.Name != "None" && selectedEnch.Name == selectedEnch2.Name)
            {
                _isCalculating = true;
                CmbEnchantment2.SelectedIndex = 0;
                _isCalculating = false;
                selectedEnch2 = CmbEnchantment2.SelectedItem as Enchantment;
            }

            var diff = CmbDifficulty.SelectedItem as Difficulty;
            double dMult = diff?.DamageDealtMultiplier ?? 1.0;

            // ==========================================
            // 4. WAFFEN-BERECHNUNG (Inkl. Munition & Faust)
            // ==========================================
            if (isWeaponMode && CmbSelect.SelectedItem is Weapon w)
            {
                if (w.Name == "None")
                {
                    // --- NEU: HIER KOMMT DIE FAUST-LOGIK REIN ---
                    var race = CmbRace.SelectedItem as Race;
                    // Wir suchen die aktuell ausgerüsteten Handschuhe in der Liste der bereits angelegten Items
                    var currentGauntlets = EquippedItems.FirstOrDefault(i => i.Slot == "Hands")?.OriginalObject as Armor;

                    // Falls du spezifische "Fortify Unarmed" Verzauberungen auf Ringen/Handschuhen hast, 
                    // müssten die hier addiert werden (aktuell 0)
                    double unarmedEnch = 0;

                    double finalFistDamage = SkyrimCalculator.CalculateUnarmedDamage(race, active, currentGauntlets, unarmedEnch);

                    // UI für Fäuste füllen
                    TxtWeaponCategory.Text = "Unarmed";
                    TxtDamageDisplay.Text = Math.Round(finalFistDamage).ToString();
                    TxtSneakDisplay.Text = Math.Round(finalFistDamage * 2.0).ToString(); // Standard 2x für Sneak-Fists

                    TxtReach.Text = "0.7"; // Fäuste haben eine kurze Reichweite
                    TxtSpeed.Text = "1.0";
                    TxtStagger.Text = "0.00";
                    TxtValue.Text = "-";
                    TxtEffect.Text = (currentGauntlets != null && currentGauntlets.Name.Contains("Brawler")) ? "Brawler's Bonus active" : "None";

                    // Wichtig: Hier kein return mehr, damit die Abschluss-Updates unten noch laufen!
                }
                else
                {
                    // --- Normale Waffenberechnung (Bogen, Schwert etc.) ---
                    double ammoDamage = 0;
                    if (CmbAmmo.Visibility == Visibility.Visible && CmbAmmo.SelectedItem is Weapon ammo)
                    {
                        ammoDamage = ammo.Damage;
                    }

                    Weapon calcWeapon = new Weapon
                    {
                        Name = w.Name,
                        Category = w.Category,
                        Damage = w.Damage + ammoDamage,
                        Speed = w.Speed,
                        Reach = w.Reach,
                        Stagger = w.Stagger,
                        IsEnchantable = w.IsEnchantable
                    };

                    var calcResult = SkyrimCalculator.CalculateWeapon(calcWeapon, level, active, dMult, selectedEnch, selectedEnch2, gemMultiplier);

                    // Suche diese Stelle:
                    TxtWeaponCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                             ? w.Category
                                             : $"{w.Category} {calcResult.SmithingTierName}";

                    if (calcResult.SmithingTierName.Contains("Arcane"))
                    {
                        // Wir machen einen Zeilenumbruch (\n), damit "Needs Arcane..." unter der Kategorie steht
                        TxtWeaponCategory.Text = w.Category + "\n(Needs Arcane Blacksmith)";
                        TxtWeaponCategory.Foreground = Brushes.Red;
                    }
                    else
                    {
                        TxtWeaponCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                                 ? w.Category
                                                 : $"{w.Category} {calcResult.SmithingTierName}";
                        TxtWeaponCategory.Foreground = new SolidColorBrush(Color.FromRgb(225, 184, 13));
                    }

                    TxtDamageDisplay.Text = Math.Round(calcResult.FinalDamage).ToString();
                    TxtSneakDisplay.Text = Math.Round(calcResult.SneakDamage).ToString();
                    TxtEffect.Text = calcResult.FinalEffectText;

                    var stats = w.GetStatsForLevel(level);
                    TxtReach.Text = w.Reach > 0 ? w.Reach.ToString() : "-";
                    TxtSpeed.Text = w.Speed > 0 ? w.Speed.ToString() : "-";
                    TxtStagger.Text = calcResult.FinalStagger > 0 ? calcResult.FinalStagger.ToString("0.00") : "-";
                    TxtValue.Text = Math.Round(stats.val).ToString();
                }
            }
            // ==========================================
            // 5. RÜSTUNGS-BERECHNUNG
            // ==========================================
            else if (!isWeaponMode && CmbSelect.SelectedItem is Armor a)
            {
                if (a.Name == "None")
                {
                    TxtArmorCategory.Text = "-"; TxtArmorDisplay.Text = "-";
                    TxtArmorWeight.Text = "-"; TxtArmorValue.Text = "-";
                    TxtArmorEffect.Text = "None";
                }
                else
                {
                    var calcResult = SkyrimCalculator.CalculateArmor(a, level, active, selectedEnch, selectedEnch2, gemMultiplier);

                    TxtArmorCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                             ? a.Slot
                                             : $"{a.Slot} {calcResult.SmithingTierName}";

                    TxtArmorDisplay.Text = Math.Round(calcResult.FinalArmorRating).ToString();
                    TxtArmorEffect.Text = calcResult.FinalEffectText;

                    var stats = a.GetStatsForLevel(level);
                    TxtArmorWeight.Text = calcResult.FinalWeight > 0
                        ? calcResult.FinalWeight.ToString("0.#")
                        : "0 (Weightless)";

                    TxtArmorValue.Text = Math.Round(stats.val).ToString();
                }
            }

            // --- 6. ABSCHLUSS-UPDATES ---
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
            if (CmbSelect.SelectedItem == null) return;

            bool enchantable = true;

            // --- FALL 1: WAFFE ---
            if (CmbSelect.SelectedItem is Weapon w)
            {
                string cat = (w.Category ?? "").ToLower();
                string name = (w.Name ?? "").ToLower();

                bool isBow = cat.Contains("bow") || name.Contains("bow");
                bool isCrossbow = cat.Contains("crossbow") || name.Contains("crossbow");
                bool isRanged = isBow || isCrossbow;

                LblAmmo.Visibility = isRanged ? Visibility.Visible : Visibility.Collapsed;
                CmbAmmo.Visibility = isRanged ? Visibility.Visible : Visibility.Collapsed;

                if (isRanged)
                {
                    // Wir übergeben jetzt, ob es eine Armbrust ist
                    ApplyAmmoFilter(isCrossbow);
                }

                // --- Bestehende Logik: Einhand & Off-Hand ---
                bool isOneHanded = cat.Contains("one-handed");
                BtnEquipOffHand.Visibility = isOneHanded ? Visibility.Visible : Visibility.Collapsed;

                // Panels umschalten
                WeaponPanel.Visibility = Visibility.Visible;
                ArmorPanel.Visibility = Visibility.Collapsed;

                // Ist die Waffe verzauberbar?
                enchantable = w.IsEnchantable;
            }
            // --- FALL 2: RÜSTUNG ---
            else if (CmbSelect.SelectedItem is Armor a)
            {
                // Fernkampf-UI immer ausblenden bei Rüstung
                LblAmmo.Visibility = Visibility.Collapsed;
                CmbAmmo.Visibility = Visibility.Collapsed;

                // Bei normaler Rüstung keinen Off-Hand Button, AUSSER es ist ein Schild!
                if (a.Category == "Shield")
                {
                    BtnEquipOffHand.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnEquipOffHand.Visibility = Visibility.Collapsed;
                }

                // Panels umschalten
                WeaponPanel.Visibility = Visibility.Collapsed;
                ArmorPanel.Visibility = Visibility.Visible;

                // Rüstung ist nur verzauberbar, wenn das Effect-Feld komplett leer ist.
                enchantable = string.IsNullOrEmpty(a.Effect);
            }

            // --- Verzauberungs-UI aktualisieren ---
            CmbEnchantment.IsEnabled = enchantable;
            CmbEnchantment2.IsEnabled = enchantable;

            if (!enchantable)
            {
                CmbEnchantment.SelectedIndex = 0;
                CmbEnchantment2.SelectedIndex = 0;
            }

            // Listen-Filter und Berechnungen triggern
            UpdateEnchantmentList();
            UpdateCalculations();
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
            var build = new SavedBuild
            {
                SelectedRace = (CmbRace.SelectedItem as Race)?.Name ?? "None",
                Level = int.TryParse(TxtPlayerLevel.Text, out int l) ? l : 1,
                InvestMagicka = int.TryParse(TxtInvestMagicka.Text, out int im) ? im : 0,
                InvestHealth = int.TryParse(TxtInvestHealth.Text, out int ih) ? ih : 0,
                InvestStamina = int.TryParse(TxtInvestStamina.Text, out int ist) ? ist : 0,
                SelectedStandingStone = (CmbStandingStone.SelectedItem as StandingStone)?.Name ?? "None",
                SelectedFleshSpell = (CmbFleshSpell.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "None",
                ActivePerkNames = PerkDatabase.Where(p => p.IsActive).Select(p => p.Name).ToList()
            };

            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "Skyrim Build (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                string json = System.Text.Json.JsonSerializer.Serialize(build, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(dialog.FileName, json);
                MessageBox.Show("Build erfolgreich in den Archiven von Winterfeste gespeichert!");
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
                    var loadData = System.Text.Json.JsonSerializer.Deserialize<SavedBuild>(json);

                    if (loadData == null) return;

                    // 1. RECHEN-STOPP: Wir schalten die Live-Berechnung kurz aus,
                    // damit das Programm nicht bei jedem einzelnen Feld flackert.
                    _isCalculating = true;

                    // 2. BASIS-DATEN SETZEN
                    TxtPlayerLevel.Text = loadData.Level.ToString();
                    TxtInvestMagicka.Text = loadData.InvestMagicka.ToString();
                    TxtInvestHealth.Text = loadData.InvestHealth.ToString();
                    TxtInvestStamina.Text = loadData.InvestStamina.ToString();

                    // 3. RASSE SUCHEN & AUSWÄHLEN
                    // Wir suchen in der RaceDatabase nach dem Namen aus der Datei
                    CmbRace.SelectedItem = RaceDatabase.FirstOrDefault(r => r.Name == loadData.SelectedRace);

                    // 4. STEIN SUCHEN & AUSWÄHLEN
                    CmbStandingStone.SelectedItem = StandingStoneDatabase.FirstOrDefault(s => s.Name == loadData.SelectedStandingStone);

                    // 5. FLESH SPELL (ZAUBER) SUCHEN
                    // Da dies einfache ComboBoxItems sind, suchen wir nach dem Content-Text
                    foreach (ComboBoxItem item in CmbFleshSpell.Items)
                    {
                        if (item.Content.ToString() == loadData.SelectedFleshSpell)
                        {
                            CmbFleshSpell.SelectedItem = item;
                            break;
                        }
                    }

                    // 6. PERKS WIEDERHERSTELLEN
                    // Erst alle Perks ausschalten, dann nur die aus der Datei aktivieren
                    foreach (var perk in PerkDatabase)
                    {
                        perk.IsActive = loadData.ActivePerkNames.Contains(perk.Name);
                    }

                    // 7. FINALE BERECHNUNG
                    // Jetzt schalten wir den Rechner wieder ein und rufen alles einmal auf
                    _isCalculating = false;

                    UpdateCalculations();
                    UpdateTotalBuildStats();

                    MessageBox.Show($"Build '{loadData.BuildName}' wurde erfolgreich geladen!", "Winterfeste Archive");
                }
                catch (Exception ex)
                {
                    _isCalculating = false;
                    MessageBox.Show("Fehler beim Laden: " + ex.Message);
                }
            }
        }

        private void BtnExportText_Click(object sender, RoutedEventArgs e)
        {
            // 1. Ask for the build name
            string buildName = ShowInputDialog("Build Name", "What should your build be named?", "Dovahkiin Build");

            // If the user cancels or enters nothing, we stop
            if (string.IsNullOrEmpty(buildName)) return;

            var race = CmbRace.SelectedItem as Race;
            var stone = CmbStandingStone.SelectedItem as StandingStone;
            var fleshSpell = (CmbFleshSpell.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "None";

            var sb = new System.Text.StringBuilder();

            // --- Header ---
            sb.AppendLine("===============================================");
            sb.AppendLine("          SKYRIM BUILD ARCHITECT               ");
            sb.AppendLine("===============================================");
            sb.AppendLine($"BUILD NAME: {buildName.ToUpper()}");
            sb.AppendLine($"Level: {TxtPlayerLevel.Text} | Race: {race?.Name ?? "None"}");
            sb.AppendLine($"Stone: {stone?.Name ?? "None"}");
            sb.AppendLine($"Active Spell: {fleshSpell}");
            sb.AppendLine();

            // --- Core Stats ---
            sb.AppendLine("--- CHARACTER ATTRIBUTES ---");
            sb.AppendLine($"Magicka: {TxtTotalMagicka.Text} {TxtRegenMagicka.Text}");
            sb.AppendLine($"Health:  {TxtTotalHealth.Text} {TxtRegenHealth.Text}");
            sb.AppendLine($"Stamina: {TxtTotalStamina.Text} {TxtRegenStamina.Text}");
            sb.AppendLine();

            // --- Advanced Stats ---
            sb.AppendLine("--- ADVANCED DEFENSES ---");
            sb.AppendLine($"Armor Rating:   {TxtTotalArmor.Text}");
            sb.AppendLine($"Carry Weight:   {TxtTotalCarryWeight.Text}");
            sb.AppendLine($"Magic Resist:   {TxtMagicResist.Text}");
            sb.AppendLine($"Spell Absorb:   {TxtSpellAbsorb.Text}");
            sb.AppendLine($"Sneak Bonus:    {TxtStealthDisplay.Text}");
            sb.AppendLine();

            // --- Equipment ---
            sb.AppendLine("--- EQUIPPED ITEMS ---");
            if (EquippedItems.Count == 0) sb.AppendLine("- No items equipped");
            foreach (var item in EquippedItems)
            {
                sb.AppendLine($"- [{item.Slot}] {item.ItemName} (Rating: {item.Rating})");
                if (!string.IsNullOrEmpty(item.Enchantment) && item.Enchantment != "None")
                    sb.AppendLine($"  * Enchantment: {item.Enchantment}");
            }
            sb.AppendLine();

            // --- Perks ---
            sb.AppendLine("--- INVESTED PERKS ---");
            var activePerks = PerkDatabase.Where(p => p.IsActive).ToList();
            if (activePerks.Count == 0) sb.AppendLine("- No perks selected");
            else
            {
                foreach (var p in activePerks.OrderBy(p => p.Category))
                {
                    sb.AppendLine($"- [{p.Category}] {p.Name}");
                }
            }
            sb.AppendLine("===============================================");

            // --- File Save Dialog ---
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text File (*.txt)|*.txt",
                FileName = $"{buildName.Replace(" ", "_")}.txt",
                Title = "Save Build as Text File"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString());
                    MessageBox.Show($"File '{saveDialog.SafeFileName}' was created successfully!", "Export Successful");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating file: " + ex.Message);
                }
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

    }

}