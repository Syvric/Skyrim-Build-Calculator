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
        }

        // --- CHARACTER CORE LOGIK ---

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
            // Sicherheitscheck: Wir haben LblEnchantment2 und CmbEnchantment2 hinzugefügt
            if (_isCalculating || !IsLoaded || TxtPlayerLevel == null || CmbRace == null ||
                CmbStandingStone == null || LblSoulGem == null || LblEnchantment2 == null || CmbEnchantment2 == null) return;

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

                // --- REGENERATION BERECHNEN ---

                // 1. Basis-Werte vom Findling (Standing Stone) holen
                double mRegenMult = stone?.MagickaRegenMult ?? 1.0;
                double hRegenMult = stone?.HealthRegenMult ?? 1.0;
                double sRegenMult = stone?.StaminaRegenMult ?? 1.0;

                // 2. MAGICKA: Recovery Perk (Restoration)
                // Wir nehmen .Max(), weil Recovery 2/2 die 1/2 ersetzt (nicht stapelt)
                double recoveryBonus = PerkDatabase?
                    .Where(p => p.IsActive && p.BaseName == "Recovery")
                    .Select(p => p.Multiplier - 1.0)
                    .DefaultIfEmpty(0.0)
                    .Max() ?? 0.0;
                mRegenMult += recoveryBonus;

                // 3. STAMINA: Wind Walker Perk (Light Armor)
                // Bedingung: Perk aktiv UND mindestens 4 Teile leichte Rüstung (Kopf, Brust, Hände, Füße)
                bool hasWindWalker = PerkDatabase?.Any(p => p.IsActive && p.Name.Contains("Wind Walker")) ?? false;
                int lightArmorCount = EquippedItems.Count(i => i.Category.ToLower().Contains("light armor"));

                if (hasWindWalker && lightArmorCount >= 4)
                {
                    sRegenMult += 0.5; // +50% Regeneration
                }

                // 4. UI AKTUALISIEREN
                // Formel: (Max-Stat * Basis-Regen-Rate * Multiplikator)
                // Basis-Raten Skyrim: Magicka 3%, Stamina 5%, Health 0.5% (außerhalb des Kampfes)
                TxtRegenMagicka.Text = $"(+{(totalMagicka * 0.03 * mRegenMult):0.0} /s)";
                TxtRegenHealth.Text = $"(+{(totalHealth * 0.005 * hRegenMult):0.0} /s)";
                TxtRegenStamina.Text = $"(+{(totalStamina * 0.05 * sRegenMult):0.0} /s)";

                // --- PERK VERFÜGBARKEIT ---
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

                // --- EXTRA EFFECT UI TOGGLE ---
                // Steuert, ob das zweite Verzauberungs-Feld angezeigt wird
                bool hasExtraEffect = PerkDatabase?.Any(p => p.IsActive && p.Name == "Extra Effect") ?? false;

                if (hasExtraEffect)
                {
                    LblEnchantment2.Visibility = Visibility.Visible;
                    CmbEnchantment2.Visibility = Visibility.Visible;
                }
                else
                {
                    LblEnchantment2.Visibility = Visibility.Collapsed;
                    CmbEnchantment2.Visibility = Visibility.Collapsed;
                    CmbEnchantment2.SelectedIndex = 0; // Zurück auf "None" setzen
                }
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

        // Die zentrale Logik, die alles steuert
        private void EquipItemLogic(string targetSlot)
        {
            if (CmbSelect.SelectedItem == null) return;

            var activePerks = PerkDatabase.Where(p => p.IsActive).ToList();
            int playerLevel = int.TryParse(TxtPlayerLevel.Text, out int lvl) ? lvl : 1;

            // --- NEU: Seelenstein-Multiplikator abrufen ---
            var currentGem = CmbSoulGem.SelectedItem as SoulGem;
            double gemMult = (currentGem == null || currentGem.Name == "None") ? 1.0 : currentGem.Multiplier;

            if (CmbSelect.SelectedItem is Weapon w)
            {
                bool isTwoHanded = (w.Category ?? "").ToLower().Contains("twohanded");

                if (isTwoHanded || targetSlot == "Main-Hand")
                {
                    RemoveItemBySlot("Off-Hand");
                    RemoveItemBySlot("Shield");
                }
                if (targetSlot == "Off-Hand") RemoveItemBySlot("Shield");

                // FIX: Hier wird jetzt 'gemMult' statt fest '1.0' verwendet
                var res = SkyrimCalculator.CalculateWeapon(w, playerLevel, activePerks, 1.0,
                          CmbEnchantment.SelectedItem as Enchantment,
                          CmbEnchantment2.SelectedItem as Enchantment,
                          gemMult);

                FinalizeEquip(w.Name, targetSlot, res.FinalDamage, res.SneakDamage, res.FinalEffectText, w.Category ?? "");
            }
            else if (CmbSelect.SelectedItem is Armor a)
            {
                if (a.Slot == "Shield")
                {
                    RemoveItemBySlot("Off-Hand");
                    var mainHand = EquippedItems.FirstOrDefault(i => i.Slot == "Main-Hand");
                    if (mainHand != null && (mainHand.Category?.ToLower().Contains("twohanded") ?? false))
                        RemoveItemBySlot("Main-Hand");
                }

                // FIX: Auch hier 'gemMult' für die Rüstung nutzen
                var res = SkyrimCalculator.CalculateArmor(a, playerLevel, activePerks,
                          CmbEnchantment.SelectedItem as Enchantment,
                          CmbEnchantment2.SelectedItem as Enchantment,
                          gemMult);

                FinalizeEquip(a.Name, a.Slot, res.FinalArmorRating, 0, res.FinalEffectText, a.Category ?? "");
            }

            UpdateTotalBuildStats();
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

            var activePerks = PerkDatabase.Where(p => p.IsActive).ToList();

            // --- Fists of Steel Vorbereitung ---
            bool hasFistsOfSteel = activePerks.Any(p => p.Name.Contains("Fists of Steel"));
            // Wir suchen, ob schwere Handschuhe ausgerüstet sind
            var heavyGauntlets = EquippedItems.FirstOrDefault(i => i.Slot == "Hands" && i.Category.ToLower().Contains("heavy armor"));

            foreach (var item in EquippedItems)
            {
                if (double.TryParse(item.Rating, out double value))
                {
                    if (item.Slot == "Main-Hand" || item.Slot == "Off-Hand")
                    {
                        double finalItemDamage = value;

                        // SPEZIALFALL: Fists of Steel
                        if (item.ItemName == "Fists" && hasFistsOfSteel && heavyGauntlets != null)
                        {
                            // Wir müssen den BASIS-Wert der Handschuhe aus der Datenbank holen (Skyrim-Regel!)
                            var baseArmorEntry = ArmorDatabase.FirstOrDefault(a => a.Name == heavyGauntlets.ItemName);
                            if (baseArmorEntry != null)
                            {
                                finalItemDamage += baseArmorEntry.ArmorRating;
                            }
                        }

                        totalDamage += finalItemDamage;

                        // Sneak Damage Berechnung für das Gesamtergebnis
                        if (double.TryParse(item.SneakRating, out double sValue))
                        {
                            // Wir passen den Schleichschaden proportional an, falls Fists of Steel den Schaden erhöht hat
                            double ratio = (value > 0) ? sValue / value : 1.0;
                            totalSneakDamage += (finalItemDamage * ratio);
                        }
                    }
                    else
                    {
                        // Alles was keine Waffe ist, ist Rüstung
                        totalArmor += value;
                    }
                }
            }

            // UI Aktualisierung
            TxtTotalDamage.Text = totalDamage.ToString("0");
            TxtTotalArmor.Text = totalArmor.ToString("0");
            TxtSneakDamage.Text = totalSneakDamage.ToString("0");
        }

        private void UpdateCalculations()
        {
            // --- 1. SCHRITT: STATS IMMER BERECHNEN ---
            CalculateAttributes();

            // --- 2. SCHRITT: DER TÜRSTEHER ---
            // CmbSoulGem wurde zur Sicherheitsabfrage hinzugefügt
            if (CmbSelect == null || CmbEnchantment == null || CmbEnchantment2 == null ||
                CmbSoulGem == null || CmbDifficulty == null || CmbSelect.SelectedItem == null) return;

            // --- 3. SCHRITT: ITEM-SPEZIFISCHE BERECHNUNG ---
            int level = int.TryParse(TxtPlayerLevel.Text, out int l) ? l : 1;
            var active = PerkDatabase.Where(p => p.IsActive).ToList();

            // Wir holen uns beide Verzauberungen aus den ComboBoxen
            var selectedEnch = CmbEnchantment.SelectedItem as Enchantment;
            var selectedEnch2 = CmbEnchantment2.SelectedItem as Enchantment;

            // --- SEELENSTEIN HOLEN ---
            var selectedGem = CmbSoulGem.SelectedItem as SoulGem;
            double gemMultiplier = (selectedGem == null || selectedGem.Name == "None") ? 1.0 : selectedGem.Multiplier;

            // --- DOPPEL-CHECK: Skyrim erlaubt nicht 2x denselben Effekt ---
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

                // Aufruf des Rechners mit beiden Verzauberungen UND dem gemMultiplier
                var calcResult = SkyrimCalculator.CalculateWeapon(w, level, active, dMult, selectedEnch, selectedEnch2, gemMultiplier);

                TxtWeaponCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                         ? w.Category
                                         : $"{w.Category} {calcResult.SmithingTierName}";

                TxtDamageDisplay.Text = Math.Round(calcResult.FinalDamage).ToString();
                TxtSneakDisplay.Text = Math.Round(calcResult.SneakDamage).ToString();
                TxtEffect.Text = calcResult.FinalEffectText;

                var stats = w.GetStatsForLevel(level);
                TxtReach.Text = w.Reach > 0 ? w.Reach.ToString() : "-";
                TxtSpeed.Text = w.Speed > 0 ? w.Speed.ToString() : "-";
                TxtStagger.Text = calcResult.FinalStagger > 0 ? calcResult.FinalStagger.ToString("0.00") : "-";
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

                // 1. Aufruf des Rechners (hast du schon)
                var calcResult = SkyrimCalculator.CalculateArmor(a, level, active, selectedEnch, selectedEnch2, gemMultiplier);

                // 2. Kategorie mit Schmiede-Grad setzen
                TxtArmorCategory.Text = string.IsNullOrEmpty(calcResult.SmithingTierName)
                                         ? a.Slot
                                         : $"{a.Slot} {calcResult.SmithingTierName}";

                // 3. Rüstungswert und Effekt-Text setzen
                TxtArmorDisplay.Text = Math.Round(calcResult.FinalArmorRating).ToString();
                TxtArmorEffect.Text = calcResult.FinalEffectText;

                // 4. HIER SIND DIE ÄNDERUNGEN: Gewicht und Wert
                var stats = a.GetStatsForLevel(level);

                // FIX: Jetzt wird das berechnete Gewicht genutzt (springt auf 0 bei Perks)
                TxtArmorWeight.Text = calcResult.FinalWeight > 0
                    ? calcResult.FinalWeight.ToString("0.#")
                    : "0 (Weightless)";

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

            // --- NEU: Sichtbarkeit der Buttons steuern ---
            if (CmbSelect.SelectedItem is Weapon w)
            {
                // Wir zeigen den Off-Hand Button NUR bei Einhandwaffen
                string cat = (w.Category ?? "").ToLower();
                bool isOneHanded = cat.Contains("one-handed") || cat.Contains("dagger") || cat.Contains("mace") || cat.Contains("war axe");

                BtnEquipOffHand.Visibility = isOneHanded ? Visibility.Visible : Visibility.Collapsed;

                // Panels umschalten
                WeaponPanel.Visibility = Visibility.Visible;
                ArmorPanel.Visibility = Visibility.Collapsed;

                enchantable = w.IsEnchantable;
            }
            else if (CmbSelect.SelectedItem is Armor a)
            {
                // Bei Rüstung/Schild brauchen wir keinen Off-Hand Button
                BtnEquipOffHand.Visibility = Visibility.Collapsed;

                // Panels umschalten
                WeaponPanel.Visibility = Visibility.Collapsed;
                ArmorPanel.Visibility = Visibility.Visible;

                enchantable = true;
            }

            // --- DEINE BESTEHENDE LOGIK (Verzauberungen) ---
            CmbEnchantment.IsEnabled = enchantable;
            CmbEnchantment2.IsEnabled = enchantable;

            if (!enchantable)
            {
                CmbEnchantment.SelectedIndex = 0;
                CmbEnchantment2.SelectedIndex = 0;
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