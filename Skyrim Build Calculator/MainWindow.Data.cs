using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyrim_Build_Architect
{
    public partial class MainWindow
    {
        // Hilfsmethode für Perks
        private void AddP(string name, string desc, string skill, string cat, string sub, int ranks, double mult, int startLvl, int lvlStep, int textStart = 20, int textStep = 20)
        {
            for (int i = 1; i <= ranks; i++)
            {
                PerkDatabase.Add(new Perk
                {
                    Name = $"{name} {i}/{ranks}",
                    BaseName = name,
                    Description = desc.Replace("{X}", (textStart + (i - 1) * textStep).ToString()),
                    SkillGroup = skill,
                    Category = cat,
                    SubCategory = sub,
                    RequiredLevel = i == 1 ? startLvl : startLvl + (i - 1) * lvlStep,
                    Multiplier = 1.0 + (i * mult)
                });
            }
        }

        public void LoadData()
        {
            // Erst alle Listen leeren
            PerkDatabase.Clear();
            WeaponDatabase.Clear();
            ArmorDatabase.Clear();
            EnchantmentDatabase.Clear();
            SoulGemDatabase.Clear();
            DifficultyDatabase.Clear(); // Ganz wichtig!

            // Dann befüllen
            LoadAllPerks();
            LoadDifficultyData(); // Muss HIER aufgerufen werden
            LoadArmorData();
            LoadWeaponData();
            LoadEnchantmentData();
            LoadSoulGemData();
        }

        private void LoadAllPerks()
        {

            // ==========================================
            // WARRIOR CATEGORY
            // ==========================================

            // ONE-HANDED
            string oh = "ONE-HANDED";
            AddP("Armsman", "One-Handed weapons do {X}% more damage.", "OneHanded", "WARRIOR", oh, 5, 0.2, 0, 20);
            AddP("Bladesman", "Attacks with swords have a chance of doing even more critical damage.", "SwordSpecial", "WARRIOR", oh, 3, 0.0, 30, 30);
            AddP("Bone Breaker", "Attacks with maces ignore up to 75% of armor.", "MaceSpecial", "WARRIOR", oh, 3, 0.0, 30, 30);
            AddP("Hack and Slash", "Attacks with war axes cause extra bleeding damage.", "AxeSpecial", "WARRIOR", oh, 3, 0.0, 30, 30);
            AddP("Dual Flurry", "Dual wielding attacks are 20% / 35% faster.", "DualSpeed", "WARRIOR", oh, 2, 0.15, 30, 20);

            PerkDatabase.Add(new Perk { Name = "Fighting Stance", BaseName = "Fighting Stance", Description = "Power attacks with one-handed weapons cost 25% less stamina.", RequiredLevel = 20, SkillGroup = "None", Category = "WARRIOR", SubCategory = oh });
            PerkDatabase.Add(new Perk { Name = "Critical Charge", BaseName = "Critical Charge", Description = "Can do a one-handed power attack while sprinting that does double critical damage.", RequiredLevel = 40, SkillGroup = "None", Category = "WARRIOR", SubCategory = oh });
            PerkDatabase.Add(new Perk { Name = "Savage Strike (+25%)", BaseName = "Savage Strike", Description = "Standing power attacks do 25% bonus damage with a chance to decapitate your enemies.", RequiredLevel = 50, SkillGroup = "PowerAttack", Category = "WARRIOR", SubCategory = oh, Multiplier = 1.25 });
            PerkDatabase.Add(new Perk { Name = "Dual Savagery (+50%)", BaseName = "Dual Savagery", Description = "Dual wielding power attacks do 50% bonus damage.", RequiredLevel = 70, SkillGroup = "DualDamage", Category = "WARRIOR", SubCategory = oh, Multiplier = 1.5 });
            PerkDatabase.Add(new Perk { Name = "Paralyzing Strike", BaseName = "Paralyzing Strike", Description = "Backwards power attack has a 25% chance to paralyze the target.", RequiredLevel = 100, SkillGroup = "None", Category = "WARRIOR", SubCategory = oh });

            // TWO-HANDED
            string th = "TWO-HANDED";
            AddP("Barbarian", "Two-handed weapons do {X}% more damage.", "TwoHanded", "WARRIOR", th, 5, 0.2, 0, 20);
            AddP("Deep Wounds", "Attacks with greatswords have a chance of doing even more critical damage.", "GSwordSpecial", "WARRIOR", th, 3, 0.0, 30, 30);
            AddP("Skullcrusher", "Attacks with war hammers ignore up to 75% of armor.", "HammerSpecial", "WARRIOR", th, 3, 0.0, 30, 30);
            AddP("Limbsplitter", "Attacks with battle axes cause extra bleeding damage.", "BAxeSpecial", "WARRIOR", th, 3, 0.0, 30, 30);

            PerkDatabase.Add(new Perk { Name = "Champion's Stance", BaseName = "Champion's Stance", Description = "Power attacks with two-handed weapons cost 25% less stamina.", RequiredLevel = 20, SkillGroup = "None", Category = "WARRIOR", SubCategory = th });
            PerkDatabase.Add(new Perk { Name = "Great Critical Charge", BaseName = "Great Critical Charge", Description = "Can do a two-handed power attack while sprinting that does double critical damage.", RequiredLevel = 40, SkillGroup = "None", Category = "WARRIOR", SubCategory = th });
            PerkDatabase.Add(new Perk { Name = "Devastating Blow (+25%)", BaseName = "Devastating Blow", Description = "Standing power attacks do 25% bonus damage with a chance to decapitate your enemies.", RequiredLevel = 50, SkillGroup = "PowerAttack", Category = "WARRIOR", SubCategory = th, Multiplier = 1.25 });
            PerkDatabase.Add(new Perk { Name = "Sweep", BaseName = "Sweep", Description = "Sideways power attacks with two-handed weapons hit all targets in front of you.", RequiredLevel = 70, SkillGroup = "None", Category = "WARRIOR", SubCategory = th });
            PerkDatabase.Add(new Perk { Name = "Warmaster", BaseName = "Warmaster", Description = "Backwards power attack has a 25% chance to paralyze the target.", RequiredLevel = 100, SkillGroup = "None", Category = "WARRIOR", SubCategory = th });

            // ARCHERY
            string ar = "ARCHERY";
            AddP("Overdraw", "Bows do {X}% more damage.", "Archery", "WARRIOR", ar, 5, 0.2, 0, 20);
            AddP("Critical Shot", "Chance of a critical hit that does extra damage.", "ArcheryCrit", "WARRIOR", ar, 3, 0.0, 30, 30);
            AddP("Steady Hand", "Zooming in with a bow slows time by 25% / 50%.", "None", "WARRIOR", ar, 2, 0.0, 40, 20);

            PerkDatabase.Add(new Perk { Name = "Eagle Eye", BaseName = "Eagle Eye", Description = "Pressing Block while aiming will zoom in your view.", RequiredLevel = 30, SkillGroup = "None", Category = "WARRIOR", SubCategory = ar });
            PerkDatabase.Add(new Perk { Name = "Power Shot", BaseName = "Power Shot", Description = "Arrows stagger all but the largest opponents 50% of the time.", RequiredLevel = 50, SkillGroup = "None", Category = "WARRIOR", SubCategory = ar });
            PerkDatabase.Add(new Perk { Name = "Hunter's Discipline", BaseName = "Hunter's Discipline", Description = "Recover twice as many arrows from dead bodies.", RequiredLevel = 50, SkillGroup = "None", Category = "WARRIOR", SubCategory = ar });
            PerkDatabase.Add(new Perk { Name = "Ranger", BaseName = "Ranger", Description = "Can move faster with a drawn bow.", RequiredLevel = 60, SkillGroup = "None", Category = "WARRIOR", SubCategory = ar });
            PerkDatabase.Add(new Perk { Name = "Quick Shot", BaseName = "Quick Shot", Description = "Can draw a bow 30% faster.", RequiredLevel = 70, SkillGroup = "None", Category = "WARRIOR", SubCategory = ar });
            PerkDatabase.Add(new Perk { Name = "Bullseye", BaseName = "Bullseye", Description = "15% chance of paralyzing the target for a few seconds.", RequiredLevel = 100, SkillGroup = "None", Category = "WARRIOR", SubCategory = ar });

            // BLOCK
            string bl = "BLOCK";
            // Shield Wall: 20, 25, 30, 35, 40% (Start 20, Step 5)
            AddP("Shield Wall", "Blocking is {X}% more effective.", "Block", "WARRIOR", bl, 5, 0.05, 0, 20, 20, 5);

            PerkDatabase.Add(new Perk { Name = "Quick Reflexes", BaseName = "Quick Reflexes", Description = "Time slows down if you are blocking during an enemy's power attack.", RequiredLevel = 30, SkillGroup = "None", Category = "WARRIOR", SubCategory = bl });
            PerkDatabase.Add(new Perk { Name = "Deflect Arrows", BaseName = "Deflect Arrows", Description = "Arrows hitting the shield do no damage.", RequiredLevel = 30, SkillGroup = "None", Category = "WARRIOR", SubCategory = bl });
            PerkDatabase.Add(new Perk { Name = "Power Bash", BaseName = "Power Bash", Description = "Able to do a power bash.", RequiredLevel = 30, SkillGroup = "None", Category = "WARRIOR", SubCategory = bl });
            PerkDatabase.Add(new Perk { Name = "Elemental Protection", BaseName = "Elemental Protection", Description = "Blocking with a shield reduces fire, frost, and shock damage by 50%.", RequiredLevel = 50, SkillGroup = "None", Category = "WARRIOR", SubCategory = bl });
            PerkDatabase.Add(new Perk { Name = "Deadly Bash", BaseName = "Deadly Bash", Description = "Bashing does five times more damage.", RequiredLevel = 50, SkillGroup = "None", Category = "WARRIOR", SubCategory = bl });
            PerkDatabase.Add(new Perk { Name = "Block Runner", BaseName = "Block Runner", Description = "Able to move faster with a shield or weapon raised.", RequiredLevel = 70, SkillGroup = "None", Category = "WARRIOR", SubCategory = bl });
            PerkDatabase.Add(new Perk { Name = "Disarming Bash", BaseName = "Disarming Bash", Description = "Chance to disarm when power bashing.", RequiredLevel = 70, SkillGroup = "None", Category = "WARRIOR", SubCategory = bl });
            PerkDatabase.Add(new Perk { Name = "Shield Charge", BaseName = "Shield Charge", Description = "Sprinting with a shield raised knocks down most targets.", RequiredLevel = 100, SkillGroup = "None", Category = "WARRIOR", SubCategory = bl });

            // HEAVY ARMOR
            string ha = "HEAVY ARMOR";
            AddP("Juggernaut", "Increases armor rating for Heavy Armor by {X}%.", "HeavyArmor", "WARRIOR", ha, 5, 0.2, 0, 20);

            PerkDatabase.Add(new Perk { Name = "Well Fitted (+25%)", BaseName = "Well Fitted", Description = "25% Armor bonus if wearing all Heavy Armor: head, chest, hands, feet.", RequiredLevel = 30, SkillGroup = "HeavyArmor", Category = "WARRIOR", SubCategory = ha, Multiplier = 1.25 });
            PerkDatabase.Add(new Perk { Name = "Fists of Steel", BaseName = "Fists of Steel", Description = "Unarmed attacks with Heavy Armor gauntlets do their armor rating in extra damage.", RequiredLevel = 30, SkillGroup = "None", Category = "WARRIOR", SubCategory = ha });
            PerkDatabase.Add(new Perk { Name = "Cushioned", BaseName = "Cushioned", Description = "Half damage from falling if wearing all Heavy Armor.", RequiredLevel = 50, SkillGroup = "None", Category = "WARRIOR", SubCategory = ha });
            PerkDatabase.Add(new Perk { Name = "Tower of Strength", BaseName = "Tower of Strength", Description = "50% less stagger when wearing only Heavy Armor.", RequiredLevel = 50, SkillGroup = "None", Category = "WARRIOR", SubCategory = ha });
            PerkDatabase.Add(new Perk { Name = "Matching Set (+25%)", BaseName = "Matching Set", Description = "Additional 25% Armor bonus if wearing a matched set of Heavy Armor.", RequiredLevel = 70, SkillGroup = "HeavyArmor", Category = "WARRIOR", SubCategory = ha, Multiplier = 1.25 });
            PerkDatabase.Add(new Perk { Name = "Conditioning (Weightless)", BaseName = "Conditioning", Description = "Heavy Armor weighs nothing and doesn't slow you down when worn.", RequiredLevel = 70, SkillGroup = "None", Category = "WARRIOR", SubCategory = ha });
            PerkDatabase.Add(new Perk { Name = "Reflect Blows (10%)", BaseName = "Reflect Blows", Description = "10% chance to reflect melee damage back to the enemy while wearing all Heavy Armor.", RequiredLevel = 100, SkillGroup = "None", Category = "WARRIOR", SubCategory = ha });

            // SMITHING

            string cCat = "WARRIOR"; // Hier von "CRAFTS" auf "WARRIOR" geändert
            string cSub = "SMITHING";

            PerkDatabase.Add(new Perk { Name = "Steel Smithing", BaseName = "Steel", Description = "Can create Steel armor and weapons at forges, and improve them twice as much.", RequiredLevel = 0, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Elven Smithing", BaseName = "Elven", Description = "Can create Elven armor and weapons at forges, and improve them twice as much.", RequiredLevel = 30, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Dwarven Smithing", BaseName = "Dwarven", Description = "Can create Dwarven armor and weapons at forges, and improve them twice as much.", RequiredLevel = 30, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Advanced Armors", BaseName = "Advanced", Description = "Can create Scaled and Plate armor at forges, and improve them twice as much.", RequiredLevel = 50, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Orcish Smithing", BaseName = "Orcish", Description = "Can create Orcish armor and weapons at forges, and improve them twice as much.", RequiredLevel = 50, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Arcane Blacksmith", BaseName = "Arcane", Description = "You can improve magical weapons and armor.", RequiredLevel = 60, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Glass Smithing", BaseName = "Glass", Description = "Can create Glass armor and weapons at forges, and improve them twice as much.", RequiredLevel = 70, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Ebony Smithing", BaseName = "Ebony", Description = "Can create Ebony armor and weapons at forges, and improve them twice as much.", RequiredLevel = 80, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Daedric Smithing", BaseName = "Daedric", Description = "Can create Daedric armor and weapons at forges, and improve them twice as much.", RequiredLevel = 90, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });
            PerkDatabase.Add(new Perk { Name = "Dragon Armor", BaseName = "Dragon", Description = "Can create Dragon armor and weapons at forges, and improve them twice as much.", RequiredLevel = 100, SkillGroup = "Smithing", Category = cCat, SubCategory = cSub });

            // ==========================================
            // THIEF CATEGORY
            // ==========================================

            // LIGHT ARMOR
            string la = "LIGHT ARMOR";
            AddP("Agile Defender", "Increases armor rating for Light Armor by {X}%.", "LightArmor", "THIEF", la, 5, 0.2, 0, 20);

            PerkDatabase.Add(new Perk { Name = "Custom Fit (+25%)", BaseName = "Custom Fit", Description = "25% Armor bonus if wearing all Light Armor: head, chest, hands, and feet.", RequiredLevel = 30, SkillGroup = "LightArmor", Category = "THIEF", SubCategory = la, Multiplier = 1.25 });
            PerkDatabase.Add(new Perk { Name = "Unhindered (Weightless)", BaseName = "Unhindered", Description = "Light Armor weighs nothing and doesn't slow you down when worn.", RequiredLevel = 50, SkillGroup = "None", Category = "THIEF", SubCategory = la });
            PerkDatabase.Add(new Perk { Name = "Wind Walker (+50% Stam Reg)", BaseName = "Wind Walker", Description = "Stamina regenerates 50% faster in all Light Armor.", RequiredLevel = 60, SkillGroup = "None", Category = "THIEF", SubCategory = la });
            PerkDatabase.Add(new Perk { Name = "Matching Set (+25%)", BaseName = "Matching Set", Description = "Additional 25% Armor bonus if wearing a matched set of Light Armor.", RequiredLevel = 70, SkillGroup = "LightArmor", Category = "THIEF", SubCategory = la, Multiplier = 1.25 });
            PerkDatabase.Add(new Perk { Name = "Deft Movement (10% Avoid)", BaseName = "Deft Movement", Description = "10% chance of avoiding all damage from a melee attack while wearing all Light Armor.", RequiredLevel = 100, SkillGroup = "None", Category = "THIEF", SubCategory = la });

            // SNEAK
            string sn = "SNEAK";
            // Stealth: 20, 25, 30, 35, 40% (Start 20, Step 5)
            AddP("Stealth", "You are {X}% harder to detect while sneaking.", "Sneak", "THIEF", sn, 5, 0.05, 0, 20, 20, 5);

            PerkDatabase.Add(new Perk { Name = "Backstab (1H x6)", BaseName = "Backstab", Description = "Sneak attacks with one-handed weapons now do 6x damage.", RequiredLevel = 30, SkillGroup = "Sneak1H", Category = "THIEF", SubCategory = sn, Multiplier = 6.0 });
            PerkDatabase.Add(new Perk { Name = "Muffled Movement", BaseName = "Muffled Movement", Description = "Noise from armor is reduced by 50%.", RequiredLevel = 30, SkillGroup = "None", Category = "THIEF", SubCategory = sn });
            PerkDatabase.Add(new Perk { Name = "Deadly Aim (Bow x3)", BaseName = "Deadly Aim", Description = "Sneak attacks with bows now do 3x damage.", RequiredLevel = 40, SkillGroup = "SneakBow", Category = "THIEF", SubCategory = sn, Multiplier = 3.0 });
            PerkDatabase.Add(new Perk { Name = "Light Foot (No Traps)", BaseName = "Light Foot", Description = "You won't trigger pressure plates.", RequiredLevel = 40, SkillGroup = "None", Category = "THIEF", SubCategory = sn });
            PerkDatabase.Add(new Perk { Name = "Assassin's Blade (Dagger x15)", BaseName = "Assassin's Blade", Description = "Sneak attacks with daggers now do 15x damage.", RequiredLevel = 50, SkillGroup = "SneakDagger", Category = "THIEF", SubCategory = sn, Multiplier = 15.0 });
            PerkDatabase.Add(new Perk { Name = "Silent Roll", BaseName = "Silent Roll", Description = "Sprinting while sneaking executes a silent forward roll.", RequiredLevel = 50, SkillGroup = "None", Category = "THIEF", SubCategory = sn });
            PerkDatabase.Add(new Perk { Name = "Silence", BaseName = "Silence", Description = "Walking and running does not affect detection.", RequiredLevel = 70, SkillGroup = "None", Category = "THIEF", SubCategory = sn });
            PerkDatabase.Add(new Perk { Name = "Shadow Warrior", BaseName = "Shadow Warrior", Description = "Crouching stops combat for a moment and forces distant opponents to seek a target.", RequiredLevel = 100, SkillGroup = "None", Category = "THIEF", SubCategory = sn });

            // LOCKPICKING
            string lk = "LOCKPICKING";
            PerkDatabase.Add(new Perk { Name = "Novice Lockpicking", BaseName = "Mastery", Description = "Novice locks are much easier to pick.", RequiredLevel = 0, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Apprentice Lockpicking", BaseName = "Mastery", Description = "Apprentice locks are much easier to pick.", RequiredLevel = 25, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Adept Lockpicking", BaseName = "Mastery", Description = "Adept locks are much easier to pick.", RequiredLevel = 50, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Expert Lockpicking", BaseName = "Mastery", Description = "Expert locks are much easier to pick.", RequiredLevel = 75, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Master Lockpicking", BaseName = "Mastery", Description = "Master locks are much easier to pick.", RequiredLevel = 100, Category = "THIEF", SubCategory = lk });

            PerkDatabase.Add(new Perk { Name = "Quick Hands", BaseName = "Quick Hands", Description = "Able to pick locks without being noticed.", RequiredLevel = 40, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Wax Key", BaseName = "Wax Key", Description = "Automatically gives you a copy of a picked lock's key if it has one.", RequiredLevel = 50, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Golden Touch", BaseName = "Golden Touch", Description = "Find more gold in chests and containers.", RequiredLevel = 60, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Treasure Hunter", BaseName = "Treasure Hunter", Description = "50% greater chance of finding special treasure.", RequiredLevel = 70, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Locksmith", BaseName = "Locksmith", Description = "Pick starts close to the lock opening position.", RequiredLevel = 80, Category = "THIEF", SubCategory = lk });
            PerkDatabase.Add(new Perk { Name = "Unbreakable", BaseName = "Unbreakable", Description = "Lockpicks never break.", RequiredLevel = 100, Category = "THIEF", SubCategory = lk });

            // PICKPOCKET
            string pk = "PICKPOCKET";
            AddP("Light Fingers", "Pickpocketing chance is increased by {X}%.", "None", "THIEF", pk, 5, 0.2, 0, 20, 20, 20);

            PerkDatabase.Add(new Perk { Name = "Night Thief", BaseName = "Night Thief", Description = "+25% chance to pickpocket if the target is asleep.", RequiredLevel = 30, Category = "THIEF", SubCategory = pk });
            PerkDatabase.Add(new Perk { Name = "Poisoned", BaseName = "Poisoned", Description = "Silently harm enemies by placing poisons in their pockets.", RequiredLevel = 40, Category = "THIEF", SubCategory = pk });
            PerkDatabase.Add(new Perk { Name = "Cutpurse", BaseName = "Cutpurse", Description = "Pickpocketing gold is 50% easier.", RequiredLevel = 40, Category = "THIEF", SubCategory = pk });
            PerkDatabase.Add(new Perk { Name = "Extra Pockets (+100 Carry)", BaseName = "Extra Pockets", Description = "Carrying capacity is increased by 100.", RequiredLevel = 50, Category = "THIEF", SubCategory = pk });
            PerkDatabase.Add(new Perk { Name = "Keymaster", BaseName = "Keymaster", Description = "Pickpocketing keys almost always works.", RequiredLevel = 60, Category = "THIEF", SubCategory = pk });
            PerkDatabase.Add(new Perk { Name = "Misdirection", BaseName = "Misdirection", Description = "Can pickpocket equipped weapons.", RequiredLevel = 70, Category = "THIEF", SubCategory = pk });
            PerkDatabase.Add(new Perk { Name = "Perfect Touch", BaseName = "Perfect Touch", Description = "Can pickpocket equipped items.", RequiredLevel = 100, Category = "THIEF", SubCategory = pk });

            // SPEECH
            string sp = "SPEECH";
            // Haggling: 10%, 15%, 20%, 25%, 30% (Start 10, Step 5)
            AddP("Haggling", "Buying and selling prices are {X}% better.", "None", "THIEF", sp, 5, 0.05, 0, 20, 10, 5);

            PerkDatabase.Add(new Perk { Name = "Bribery", BaseName = "Bribery", Description = "Can bribe guards to ignore crimes.", RequiredLevel = 30, Category = "THIEF", SubCategory = sp });
            PerkDatabase.Add(new Perk { Name = "Allure", BaseName = "Allure", Description = "10% better prices with the opposite sex.", RequiredLevel = 30, Category = "THIEF", SubCategory = sp });
            PerkDatabase.Add(new Perk { Name = "Merchant", BaseName = "Merchant", Description = "Can sell any type of item to any kind of merchant.", RequiredLevel = 50, Category = "THIEF", SubCategory = sp });
            PerkDatabase.Add(new Perk { Name = "Persuasion", BaseName = "Persuasion", Description = "Persuasion attempts are 30% easier.", RequiredLevel = 50, Category = "THIEF", SubCategory = sp });
            PerkDatabase.Add(new Perk { Name = "Investor", BaseName = "Investor", Description = "Can invest 500 gold with a shopkeeper.", RequiredLevel = 70, Category = "THIEF", SubCategory = sp });
            PerkDatabase.Add(new Perk { Name = "Intimidation", BaseName = "Intimidation", Description = "Intimidation is twice as likely to succeed.", RequiredLevel = 70, Category = "THIEF", SubCategory = sp });
            PerkDatabase.Add(new Perk { Name = "Fence", BaseName = "Fence", Description = "Can barter stolen goods with invested merchants.", RequiredLevel = 90, Category = "THIEF", SubCategory = sp });
            PerkDatabase.Add(new Perk { Name = "Master Trader (+1000 Gold)", BaseName = "Master Trader", Description = "Every merchant gains 1000 gold for bartering.", RequiredLevel = 100, Category = "THIEF", SubCategory = sp });

            // ALCHEMY
            string alc = "ALCHEMY";
            AddP("Alchemist", "Potions and poisons you make are {X}% stronger.", "Alchemy", "THIEF", alc, 5, 0.2, 0, 20, 20, 20);
            // Experimenter: reveals 2, 3, 4 effects
            AddP("Experimenter", "Eating an ingredient reveals {X} effects.", "None", "THIEF", alc, 3, 0.0, 50, 20, 2, 1);

            PerkDatabase.Add(new Perk { Name = "Physician (+25%)", BaseName = "Physician", Description = "Restore Health/Magicka/Stamina potions are 25% more powerful.", RequiredLevel = 20, Multiplier = 1.25, SkillGroup = "Alchemy", Category = "THIEF", SubCategory = alc });
            PerkDatabase.Add(new Perk { Name = "Benefactor (+25%)", BaseName = "Benefactor", Description = "Beneficial potions have 25% greater magnitude.", RequiredLevel = 30, Multiplier = 1.25, SkillGroup = "Alchemy", Category = "THIEF", SubCategory = alc });
            PerkDatabase.Add(new Perk { Name = "Poisoner (+25%)", BaseName = "Poisoner", Description = "Poisons you mix are 25% more powerful.", RequiredLevel = 30, Multiplier = 1.25, SkillGroup = "Alchemy", Category = "THIEF", SubCategory = alc });
            PerkDatabase.Add(new Perk { Name = "Concentrated Poison", BaseName = "Concentrated Poison", Description = "Poisons applied to weapons last for twice as many hits.", RequiredLevel = 60, Category = "THIEF", SubCategory = alc });
            PerkDatabase.Add(new Perk { Name = "Green Thumb", BaseName = "Green Thumb", Description = "Two ingredients are gathered from plants.", RequiredLevel = 70, Category = "THIEF", SubCategory = alc });
            PerkDatabase.Add(new Perk { Name = "Snakeblood", BaseName = "Snakeblood", Description = "50% resistance to all poisons.", RequiredLevel = 80, Category = "THIEF", SubCategory = alc });
            PerkDatabase.Add(new Perk { Name = "Purity", BaseName = "Purity", Description = "Removes negative effects from potions and positive from poisons.", RequiredLevel = 100, Category = "THIEF", SubCategory = alc });

            // ==========================================
            // MAGE CATEGORY
            // ==========================================

            // ILLUSION
            string il = "ILLUSION";
            PerkDatabase.Add(new Perk { Name = "Novice Illusion", BaseName = "Mastery", Description = "Cast Novice level Illusion spells for half Magicka.", RequiredLevel = 0, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Apprentice Illusion", BaseName = "Mastery", Description = "Cast Apprentice level Illusion spells for half Magicka.", RequiredLevel = 25, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Adept Illusion", BaseName = "Mastery", Description = "Cast Adept level Illusion spells for half Magicka.", RequiredLevel = 50, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Expert Illusion", BaseName = "Mastery", Description = "Cast Expert level Illusion spells for half Magicka.", RequiredLevel = 75, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Master Illusion", BaseName = "Mastery", Description = "Cast Master level Illusion spells for half Magicka.", RequiredLevel = 100, Category = "MAGE", SubCategory = il });

            PerkDatabase.Add(new Perk { Name = "Animage", BaseName = "Animage", Description = "Illusion spells now work on higher level animals.", RequiredLevel = 20, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Illusion Dual Casting", BaseName = "Dual Casting", Description = "Dual casting Illusion overcharges the effect.", RequiredLevel = 20, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Hypnotic Gaze", BaseName = "Hypnotic Gaze", Description = "Calm spells work on higher level opponents.", RequiredLevel = 30, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Kindred Mage", BaseName = "Kindred Mage", Description = "All Illusion spells now work on higher level people.", RequiredLevel = 40, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Quiet Casting", BaseName = "Quiet Casting", Description = "All spellcasting is silent to others.", RequiredLevel = 50, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Aspect of Terror", BaseName = "Aspect of Terror", Description = "Fear spells work on higher level opponents. (Fire +10 Dmg).", RequiredLevel = 50, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Rage", BaseName = "Rage", Description = "Frenzy spells work on higher level opponents.", RequiredLevel = 70, Category = "MAGE", SubCategory = il });
            PerkDatabase.Add(new Perk { Name = "Master of the Mind", BaseName = "Master of the Mind", Description = "Illusion spells work on undead, daedra and automata.", RequiredLevel = 90, Category = "MAGE", SubCategory = il });

            // DESTRUCTION
            string de = "DESTRUCTION";
            PerkDatabase.Add(new Perk { Name = "Novice Destruction", BaseName = "Mastery", Description = "Cast Novice level Destruction spells for half Magicka.", RequiredLevel = 0, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Apprentice Destruction", BaseName = "Mastery", Description = "Cast Apprentice level Destruction spells for half Magicka.", RequiredLevel = 25, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Adept Destruction", BaseName = "Mastery", Description = "Cast Adept level Destruction spells for half Magicka.", RequiredLevel = 50, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Expert Destruction", BaseName = "Mastery", Description = "Cast Expert level Destruction spells for half Magicka.", RequiredLevel = 75, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Master Destruction", BaseName = "Mastery", Description = "Cast Master level Destruction spells for half Magicka.", RequiredLevel = 100, Category = "MAGE", SubCategory = de });

            // Elemental Boosts (Augmented Flames, Frost, Shock)
            AddP("Augmented Flames", "Fire spells do {X}% more damage.", "ElementalBoost", "MAGE", de, 2, 0.25, 30, 30, 25, 25);
            AddP("Augmented Frost", "Frost spells do {X}% more damage.", "ElementalBoost", "MAGE", de, 2, 0.25, 30, 30, 25, 25);
            AddP("Augmented Shock", "Shock spells do {X}% more damage.", "ElementalBoost", "MAGE", de, 2, 0.25, 30, 30, 25, 25);

            PerkDatabase.Add(new Perk { Name = "Destruction Dual Casting", BaseName = "Dual Casting", Description = "Dual casting overcharges Destruction spells.", RequiredLevel = 20, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Impact", BaseName = "Impact", Description = "Most Destruction spells will stagger an opponent when dual cast.", RequiredLevel = 40, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Rune Master", BaseName = "Rune Master", Description = "Can place runes five times farther away.", RequiredLevel = 40, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Intense Flames", BaseName = "Intense Flames", Description = "Fire damage causes targets to flee if health is low.", RequiredLevel = 50, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Deep Freeze", BaseName = "Deep Freeze", Description = "Frost damage paralyzes targets if health is low.", RequiredLevel = 60, Category = "MAGE", SubCategory = de });
            PerkDatabase.Add(new Perk { Name = "Disintegrate", BaseName = "Disintegrate", Description = "Shock damage disintegrates targets if health is low.", RequiredLevel = 70, Category = "MAGE", SubCategory = de });

            // ALTERATION
            string alt = "ALTERATION";
            PerkDatabase.Add(new Perk { Name = "Novice Alteration", BaseName = "Mastery", Description = "Cast Novice level Alteration spells for half Magicka.", RequiredLevel = 0, Category = "MAGE", SubCategory = alt });
            PerkDatabase.Add(new Perk { Name = "Apprentice Alteration", BaseName = "Mastery", Description = "Cast Apprentice level Alteration spells for half Magicka.", RequiredLevel = 25, Category = "MAGE", SubCategory = alt });
            PerkDatabase.Add(new Perk { Name = "Adept Alteration", BaseName = "Mastery", Description = "Cast Adept level Alteration spells for half Magicka.", RequiredLevel = 50, Category = "MAGE", SubCategory = alt });
            PerkDatabase.Add(new Perk { Name = "Expert Alteration", BaseName = "Mastery", Description = "Cast Expert level Alteration spells for half Magicka.", RequiredLevel = 75, Category = "MAGE", SubCategory = alt });
            PerkDatabase.Add(new Perk { Name = "Master Alteration", BaseName = "Mastery", Description = "Cast Master level Alteration spells for half Magicka.", RequiredLevel = 100, Category = "MAGE", SubCategory = alt });

            // Magic Resistance & Mage Armor (1/3, 2/3, 3/3)
            AddP("Magic Resistance", "Blocks {X}% of a spell's effects.", "None", "MAGE", alt, 3, 0.0, 30, 20, 10, 10);
            // Mage Armor braucht manuelle Multiplikatoren (2x, 2.5x, 3x)
            PerkDatabase.Add(new Perk { Name = "Mage Armor (1/3)", BaseName = "Mage Armor", Description = "Stoneflesh is twice as strong if wearing no armor.", RequiredLevel = 30, Category = "MAGE", SubCategory = alt });
            PerkDatabase.Add(new Perk { Name = "Mage Armor (2/3)", BaseName = "Mage Armor", Description = "Stoneflesh is 2.5 times as strong if wearing no armor.", RequiredLevel = 50, Category = "MAGE", SubCategory = alt });
            PerkDatabase.Add(new Perk { Name = "Mage Armor (3/3)", BaseName = "Mage Armor", Description = "Stoneflesh is 3 times as strong if wearing no armor.", RequiredLevel = 70, Category = "MAGE", SubCategory = alt });

            PerkDatabase.Add(new Perk { Name = "Alteration Dual Casting", BaseName = "Dual Casting", Description = "Dual casting an Alteration spell overcharges the duration.", RequiredLevel = 20, Category = "MAGE", SubCategory = alt });
            PerkDatabase.Add(new Perk { Name = "Stability", BaseName = "Stability", Description = "Alteration spells have greater duration.", RequiredLevel = 70, Category = "MAGE", SubCategory = alt });
            PerkDatabase.Add(new Perk { Name = "Atronach", BaseName = "Atronach", Description = "Absorb 30% of the Magicka from any spell that hits you.", RequiredLevel = 100, Category = "MAGE", SubCategory = alt });

            // CONJURATION
            string cj = "CONJURATION";
            PerkDatabase.Add(new Perk { Name = "Novice Conjuration", BaseName = "Mastery", Description = "Cast Novice level Conjuration spells for half Magicka.", RequiredLevel = 0, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Apprentice Conjuration", BaseName = "Mastery", Description = "Cast Apprentice level Conjuration spells for half Magicka.", RequiredLevel = 25, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Adept Conjuration", BaseName = "Mastery", Description = "Cast Adept level Conjuration spells for half Magicka.", RequiredLevel = 50, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Expert Conjuration", BaseName = "Mastery", Description = "Cast Expert level Conjuration spells for half Magicka.", RequiredLevel = 75, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Master Conjuration", BaseName = "Mastery", Description = "Cast Master level Conjuration spells for half Magicka.", RequiredLevel = 100, Category = "MAGE", SubCategory = cj });

            PerkDatabase.Add(new Perk { Name = "Conjuration Dual Casting", BaseName = "Dual Casting", Description = "Dual casting a Conjuration spell overcharges the spell, allowing it to last longer.", RequiredLevel = 20, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Mystic Binding", BaseName = "Mystic Binding", Description = "Bound weapons do more damage.", RequiredLevel = 20, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Soul Stealer", BaseName = "Soul Stealer", Description = "Bound weapons cast Soul Trap on targets.", RequiredLevel = 30, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Oblivion Binding", BaseName = "Oblivion Binding", Description = "Bound weapons will banish summoned creatures and turn raised ones.", RequiredLevel = 50, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Necromancy", BaseName = "Necromancy", Description = "Reanimated undead have a greater duration.", RequiredLevel = 40, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Dark Souls", BaseName = "Dark Souls", Description = "Reanimated undead have 100 points more health.", RequiredLevel = 50, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Summoner (1/2)", BaseName = "Summoner", Description = "Can summon atronachs or reanimated undead twice as far away.", RequiredLevel = 30, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Summoner (2/2)", BaseName = "Summoner", Description = "Can summon atronachs or reanimated undead three times as far away.", RequiredLevel = 70, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Atromancy", BaseName = "Atromancy", Description = "Summoned Atronachs last twice as long.", RequiredLevel = 40, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Elemental Potency", BaseName = "Elemental Potency", Description = "Conjured Atronachs are 50% more potent.", RequiredLevel = 50, Category = "MAGE", SubCategory = cj });
            PerkDatabase.Add(new Perk { Name = "Twin Souls", BaseName = "Twin Souls", Description = "You can have two atronachs or reanimated zombies.", RequiredLevel = 100, Category = "MAGE", SubCategory = cj });

            // --- RESTORATION ---
            string re = "RESTORATION";

            PerkDatabase.Add(new Perk { Name = "Novice Restoration", BaseName = "Mastery", Description = "Cast Novice level Restoration spells for half Magicka.", RequiredLevel = 0, Category = "MAGE", SubCategory = re });
            PerkDatabase.Add(new Perk { Name = "Apprentice Restoration", BaseName = "Mastery", Description = "Cast Apprentice level Restoration spells for half Magicka.", RequiredLevel = 25, Category = "MAGE", SubCategory = re });
            PerkDatabase.Add(new Perk { Name = "Adept Restoration", BaseName = "Mastery", Description = "Cast Adept level Restoration spells for half Magicka.", RequiredLevel = 50, Category = "MAGE", SubCategory = re });
            PerkDatabase.Add(new Perk { Name = "Expert Restoration", BaseName = "Mastery", Description = "Cast Expert level Restoration spells for half Magicka.", RequiredLevel = 75, Category = "MAGE", SubCategory = re });
            PerkDatabase.Add(new Perk { Name = "Master Restoration", BaseName = "Mastery", Description = "Cast Master level Restoration spells for half Magicka.", RequiredLevel = 100, Category = "MAGE", SubCategory = re });

            PerkDatabase.Add(new Perk { Name = "Restoration Dual Casting", BaseName = "Dual Casting", Description = "Dual casting a Restoration spell overcharges the effect.", RequiredLevel = 20, Category = "MAGE", SubCategory = re });
            PerkDatabase.Add(new Perk { Name = "Regeneration", BaseName = "Regeneration", Description = "Healing spells cure 50% more.", RequiredLevel = 20, Category = "MAGE", SubCategory = re });
            PerkDatabase.Add(new Perk { Name = "Respite", BaseName = "Respite", Description = "Healing spells also restore Stamina.", RequiredLevel = 40, Category = "MAGE", SubCategory = re });
            PerkDatabase.Add(new Perk { Name = "Necromage", BaseName = "Necromage", Description = "All spells are more effective against undead.", RequiredLevel = 70, Category = "MAGE", SubCategory = re });
            PerkDatabase.Add(new Perk { Name = "Ward Absorb", BaseName = "Ward Absorb", Description = "Wards recharge your Magicka when hit with spells.", RequiredLevel = 60, Category = "MAGE", SubCategory = re });

            // Hier sind die gefixten Recovery-Perks mit Multiplier und SkillGroup
            PerkDatabase.Add(new Perk { Name = "Recovery (1/2)", BaseName = "Recovery", Description = "Magicka regenerates 25% faster.", RequiredLevel = 30, Category = "MAGE", SubCategory = re, SkillGroup = "Restoration", Multiplier = 1.25 });
            PerkDatabase.Add(new Perk { Name = "Recovery (2/2)", BaseName = "Recovery", Description = "Magicka regenerates 50% faster.", RequiredLevel = 60, Category = "MAGE", SubCategory = re, SkillGroup = "Restoration", Multiplier = 1.50 });

            PerkDatabase.Add(new Perk { Name = "Avoid Death", BaseName = "Avoid Death", Description = "Once a day, heals 250 points automatically if you fall below 10% health.", RequiredLevel = 90, Category = "MAGE", SubCategory = re });


            // ==========================================
            // 1. PERKS (Bleiben so, die sind perfekt)
            // ==========================================
            string en = "ENCHANTING";
            PerkDatabase.Add(new Perk { Name = "Enchanter 1/5", BaseName = "Enchanter", Description = "New enchantments are 20% stronger.", RequiredLevel = 0, Category = "MAGE", SubCategory = en, SkillGroup = "Enchanting", Multiplier = 1.2 });
            PerkDatabase.Add(new Perk { Name = "Enchanter 2/5", BaseName = "Enchanter", Description = "New enchantments are 40% stronger.", RequiredLevel = 20, Category = "MAGE", SubCategory = en, SkillGroup = "Enchanting", Multiplier = 1.4 });
            PerkDatabase.Add(new Perk { Name = "Enchanter 3/5", BaseName = "Enchanter", Description = "New enchantments are 60% stronger.", RequiredLevel = 40, Category = "MAGE", SubCategory = en, SkillGroup = "Enchanting", Multiplier = 1.6 });
            PerkDatabase.Add(new Perk { Name = "Enchanter 4/5", BaseName = "Enchanter", Description = "New enchantments are 80% stronger.", RequiredLevel = 60, Category = "MAGE", SubCategory = en, SkillGroup = "Enchanting", Multiplier = 1.8 });
            PerkDatabase.Add(new Perk { Name = "Enchanter 5/5", BaseName = "Enchanter", Description = "New enchantments are 100% stronger.", RequiredLevel = 80, Category = "MAGE", SubCategory = en, SkillGroup = "Enchanting", Multiplier = 2.0 });

            PerkDatabase.Add(new Perk { Name = "Fire Enchanter", BaseName = "Fire Enchanter", Description = "Fire enchantments on weapons and armor are 25% stronger.", RequiredLevel = 30, Category = "MAGE", SubCategory = en, SkillGroup = "EnchantingSpecial" });
            PerkDatabase.Add(new Perk { Name = "Frost Enchanter", BaseName = "Frost Enchanter", Description = "Frost enchantments on weapons and armor are 25% stronger.", RequiredLevel = 40, Category = "MAGE", SubCategory = en, SkillGroup = "EnchantingSpecial" });
            PerkDatabase.Add(new Perk { Name = "Storm Enchanter", BaseName = "Storm Enchanter", Description = "Shock enchantments on weapons and armor are 25% stronger.", RequiredLevel = 50, Category = "MAGE", SubCategory = en, SkillGroup = "EnchantingSpecial" });
            PerkDatabase.Add(new Perk { Name = "Insightful Enchanter", BaseName = "Insightful Enchanter", Description = "Skill enchantments on armor are 25% stronger.", RequiredLevel = 50, Category = "MAGE", SubCategory = en, SkillGroup = "EnchantingSpecial" });
            PerkDatabase.Add(new Perk { Name = "Corpus Enchanter", BaseName = "Corpus Enchanter", Description = "Health, magicka, and stamina enchantments on armor are 25% stronger.", RequiredLevel = 70, Category = "MAGE", SubCategory = en, SkillGroup = "EnchantingSpecial" });

            PerkDatabase.Add(new Perk { Name = "Extra Effect", BaseName = "Extra Effect", Description = "Can put two enchantments on the same item.", RequiredLevel = 100, Category = "MAGE", SubCategory = en });

            // ==========================================
            // 2. ENCHANTMENT HELPER LISTS
            // ==========================================
            var combatSlots = new List<string> { "Gloves", "Boots", "Ring", "Amulet" };
            var magicSlots = new List<string> { "Head", "Chest", "Ring", "Amulet" };
            var resistSlots = new List<string> { "Boots", "Shield", "Ring", "Amulet" };

            // ==========================================
            // 3. ENCHANTMENTS
            // ==========================================
            EnchantmentDatabase.Add(new Enchantment { Name = "None", Description = "", AddedValue = 0, CompatibleSlots = new List<string> { "Weapon", "Chest", "Boots", "Gloves", "Head", "Shield", "Ring", "Amulet" } });

            // Waffen
            EnchantmentDatabase.Add(new Enchantment { Name = "Fire Damage", Description = "Burns for {0} points.", AddedValue = 10, CompatibleSlots = new List<string> { "Weapon" } });
            EnchantmentDatabase.Add(new Enchantment { Name = "Frost Damage", Description = "Frost damage of {0} points.", AddedValue = 10, CompatibleSlots = new List<string> { "Weapon" } });
            EnchantmentDatabase.Add(new Enchantment { Name = "Absorb Health", Description = "Absorb {0} points of health.", AddedValue = 8, CompatibleSlots = new List<string> { "Weapon" } });

            // Rüstung & Schmuck
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Health", Description = "Increases Health by {0} points.", AddedValue = 20, CompatibleSlots = new List<string> { "Chest", "Shield", "Ring", "Amulet" } });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Magicka", Description = "Increases Magicka by {0} points.", AddedValue = 20, CompatibleSlots = new List<string> { "Head", "Chest", "Ring", "Amulet" } });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify One-Handed", Description = "One-handed does {0}% more damage.", AddedValue = 15, CompatibleSlots = combatSlots });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Destruction", Description = "Destruction costs {0}% less.", AddedValue = 12, CompatibleSlots = magicSlots });
            EnchantmentDatabase.Add(new Enchantment { Name = "Resist Fire", Description = "Fire resistance {0}%.", AddedValue = 15, CompatibleSlots = resistSlots });
        }

        private void LoadWeaponData()
        {


            // ==========================================
            // SWORDS (One-Handed)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Amber Sword (AE)", Category = "One-Handed", Damage = 15, Value = 1030, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Dark Sword (AE)", Category = "One-Handed", Damage = 12, Value = 900, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Sword", Category = "One-Handed", Damage = 14, Value = 1250, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Sword (DG)", Category = "One-Handed", Damage = 15, Value = 1500, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Sword", Category = "One-Handed", Damage = 10, Value = 135, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Sword", Category = "One-Handed", Damage = 13, Value = 720, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Scimitar (AE)", Category = "One-Handed", Damage = 15, Value = 1100, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Sword", Category = "One-Handed", Damage = 11, Value = 235, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Glass Sword", Category = "One-Handed", Damage = 12, Value = 410, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Golden Sword (AE)", Category = "One-Handed", Damage = 11, Value = 1000, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Iron Sword", Category = "One-Handed", Damage = 7, Value = 25, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Madness Sword (AE)", Category = "One-Handed", Damage = 16, Value = 1450, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Nord Hero Sword", Category = "One-Handed", Damage = 11, Value = 250, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic Sword (DB)", Category = "One-Handed", Damage = 11, Value = 290, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish Sword", Category = "One-Handed", Damage = 9, Value = 75, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Remnant Scimitar (AE)", Category = "One-Handed", Damage = 12, Value = 1200, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Skyforge Steel Sword", Category = "One-Handed", Damage = 11, Value = 70, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Stalhrim Sword (DB)", Category = "One-Handed", Damage = 13, Value = 985, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Steel Sword", Category = "One-Handed", Damage = 8, Value = 45, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Falmer Sword", Category = "One-Handed", Damage = 10, Value = 67, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Honed Falmer Sword", Category = "One-Handed", Damage = 12, Value = 135, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Forsworn Sword", Category = "One-Handed", Damage = 10, Value = 15, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Forsworn Axe", Category = "One-Handed", Damage = 11, Value = 20, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });

            // ==========================================
            // WAR AXES (One-Handed)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Amber War Axe (AE)", Category = "One-Handed", Damage = 16, Value = 2200, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Dark War Axe (AE)", Category = "One-Handed", Damage = 13, Value = 1000, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric War Axe", Category = "One-Handed", Damage = 15, Value = 2300, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone War Axe (DG)", Category = "One-Handed", Damage = 16, Value = 3000, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven War Axe", Category = "One-Handed", Damage = 11, Value = 230, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony War Axe", Category = "One-Handed", Damage = 14, Value = 1500, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Elven War Axe", Category = "One-Handed", Damage = 12, Value = 465, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Glass War Axe", Category = "One-Handed", Damage = 13, Value = 820, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Golden War Axe (AE)", Category = "One-Handed", Damage = 12, Value = 1200, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Iron War Axe", Category = "One-Handed", Damage = 8, Value = 30, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Irkngthand War Axe (AE)", Category = "One-Handed", Damage = 13, Value = 135, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Madness War Axe (AE)", Category = "One-Handed", Damage = 17, Value = 3100, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Nord Hero War Axe", Category = "One-Handed", Damage = 12, Value = 300, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic War Axe (DB)", Category = "One-Handed", Damage = 12, Value = 425, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish War Axe", Category = "One-Handed", Damage = 10, Value = 150, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Skyforge Steel War Axe", Category = "One-Handed", Damage = 12, Value = 150, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Stalhrim War Axe (DB)", Category = "One-Handed", Damage = 15, Value = 1750, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Steel War Axe", Category = "One-Handed", Damage = 9, Value = 55, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });

            // ==========================================
            // MACES (One-Handed)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Amber Mace (AE)", Category = "One-Handed", Damage = 17, Value = 2500, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Dark Mace (AE)", Category = "One-Handed", Damage = 14, Value = 1300, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Mace", Category = "One-Handed", Damage = 16, Value = 2750, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Mace (DG)", Category = "One-Handed", Damage = 17, Value = 3500, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Mace", Category = "One-Handed", Damage = 12, Value = 275, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Mace", Category = "One-Handed", Damage = 15, Value = 1750, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Mace", Category = "One-Handed", Damage = 13, Value = 550, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Glass Mace", Category = "One-Handed", Damage = 14, Value = 950, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Golden Mace (AE)", Category = "One-Handed", Damage = 13, Value = 1500, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Iron Mace", Category = "One-Handed", Damage = 9, Value = 35, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Madness Mace (AE)", Category = "One-Handed", Damage = 18, Value = 3600, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Nord Hero Mace (AE)", Category = "One-Handed", Damage = 13, Value = 350, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic Mace (DB)", Category = "One-Handed", Damage = 13, Value = 475, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish Mace", Category = "One-Handed", Damage = 11, Value = 190, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Stalhrim Mace (DB)", Category = "One-Handed", Damage = 16, Value = 2100, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });
            WeaponDatabase.Add(new Weapon { Name = "Steel Mace", Category = "One-Handed", Damage = 10, Value = 65, Reach = 1.0, Speed = 0.8, Stagger = 1.0 });

            // ==========================================
            // GREATSWORDS (Two-Handed)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Amber Greatsword (AE)", Category = "Two-Handed", Damage = 25, Value = 2100, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Dark Greatsword (AE)", Category = "Two-Handed", Damage = 20, Value = 1400, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Greatsword", Category = "Two-Handed", Damage = 24, Value = 2500, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Greatsword (DG)", Category = "Two-Handed", Damage = 25, Value = 2725, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Greatsword", Category = "Two-Handed", Damage = 19, Value = 370, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Greatsword", Category = "Two-Handed", Damage = 22, Value = 1850, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Greatsword", Category = "Two-Handed", Damage = 20, Value = 600, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Glass Greatsword", Category = "Two-Handed", Damage = 21, Value = 900, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Golden Greatsword (AE)", Category = "Two-Handed", Damage = 21, Value = 2100, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Iron Greatsword", Category = "Two-Handed", Damage = 15, Value = 50, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Madness Greatsword (AE)", Category = "Two-Handed", Damage = 26, Value = 3000, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Nord Hero Greatsword", Category = "Two-Handed", Damage = 20, Value = 450, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic Greatsword (DB)", Category = "Two-Handed", Damage = 20, Value = 585, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish Greatsword", Category = "Two-Handed", Damage = 18, Value = 250, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Skyforge Steel Greatsword", Category = "Two-Handed", Damage = 20, Value = 150, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Stalhrim Greatsword (DB)", Category = "Two-Handed", Damage = 23, Value = 2300, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Steel Greatsword", Category = "Two-Handed", Damage = 17, Value = 90, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Ancient Nord Greatsword", Category = "Two-Handed", Damage = 15, Value = 35, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Honed Ancient Nord Greatsword", Category = "Two-Handed", Damage = 21, Value = 150, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic Greatsword (DB)", Category = "Two-Handed", Damage = 20, Value = 585, Reach = 1.3, Speed = 0.7, Stagger = 1.1 });

            // ==========================================
            // BATTLEAXES (Two-Handed)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Amber Battleaxe (AE)", Category = "Two-Handed", Damage = 26, Value = 2300, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Dark Battleaxe (AE)", Category = "Two-Handed", Damage = 21, Value = 1600, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Battleaxe", Category = "Two-Handed", Damage = 25, Value = 2750, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Battleaxe (DG)", Category = "Two-Handed", Damage = 26, Value = 3000, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Battleaxe", Category = "Two-Handed", Damage = 20, Value = 400, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Battleaxe", Category = "Two-Handed", Damage = 23, Value = 1585, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Battleaxe", Category = "Two-Handed", Damage = 21, Value = 650, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Glass Battleaxe", Category = "Two-Handed", Damage = 22, Value = 980, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Golden Battleaxe (AE)", Category = "Two-Handed", Damage = 22, Value = 2500, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Iron Battleaxe", Category = "Two-Handed", Damage = 16, Value = 55, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Madness Battleaxe (AE)", Category = "Two-Handed", Damage = 27, Value = 3300, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Nord Hero Battleaxe", Category = "Two-Handed", Damage = 21, Value = 500, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic Battleaxe (DB)", Category = "Two-Handed", Damage = 21, Value = 550, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish Battleaxe", Category = "Two-Handed", Damage = 19, Value = 265, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Skyforge Steel Battleaxe", Category = "Two-Handed", Damage = 21, Value = 150, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Stalhrim Battleaxe (DB)", Category = "Two-Handed", Damage = 24, Value = 1950, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });
            WeaponDatabase.Add(new Weapon { Name = "Steel Battleaxe", Category = "Two-Handed", Damage = 18, Value = 100, Reach = 1.3, Speed = 0.7, Stagger = 1.15 });

            // ==========================================
            // WARHAMMERS (Two-Handed)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Amber Warhammer (AE)", Category = "Two-Handed", Damage = 28, Value = 2600, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Ancient Nord Warhammer (AE)", Category = "Two-Handed", Damage = 20, Value = 35, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Dark Warhammer (AE)", Category = "Two-Handed", Damage = 23, Value = 1800, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Warhammer", Category = "Two-Handed", Damage = 27, Value = 4000, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Warhammer (DG)", Category = "Two-Handed", Damage = 28, Value = 4275, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Warhammer", Category = "Two-Handed", Damage = 22, Value = 450, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Dawnguard Warhammer (DG)", Category = "Two-Handed", Damage = 22, Value = 500, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Warhammer", Category = "Two-Handed", Damage = 25, Value = 1725, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Warhammer", Category = "Two-Handed", Damage = 23, Value = 735, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Glass Warhammer", Category = "Two-Handed", Damage = 24, Value = 1100, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Golden Warhammer (AE)", Category = "Two-Handed", Damage = 24, Value = 2800, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Honed Ancient Nord Warhammer (AE)", Category = "Two-Handed", Damage = 23, Value = 50, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Iron Warhammer", Category = "Two-Handed", Damage = 18, Value = 60, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Madness Warhammer (AE)", Category = "Two-Handed", Damage = 29, Value = 4500, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Nord Hero Warhammer (AE)", Category = "Two-Handed", Damage = 23, Value = 550, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic Warhammer (DB)", Category = "Two-Handed", Damage = 23, Value = 625, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish Warhammer", Category = "Two-Handed", Damage = 21, Value = 300, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Stalhrim Warhammer (DB)", Category = "Two-Handed", Damage = 26, Value = 2100, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Steel Warhammer", Category = "Two-Handed", Damage = 20, Value = 110, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "The Longhammer", Category = "Two-Handed", Damage = 21, Value = 90, Reach = 1.3, Speed = 0.8, Stagger = 1.25 });
            WeaponDatabase.Add(new Weapon { Name = "Stalhrim Warhammer (DB)", Category = "Two-Handed", Damage = 26, Value = 2100, Reach = 1.3, Speed = 0.6, Stagger = 1.25 });

            // ==========================================
            // BOWS & CROSSBOWS (Archery)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Amber Bow (AE)", Category = "Archery", Damage = 20, Value = 1900, Reach = 0, Speed = 0.5, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Dark Bow (AE)", Category = "Archery", Damage = 13, Value = 900, Reach = 0, Speed = 0.5, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Bow", Category = "Archery", Damage = 19, Value = 2500, Reach = 0, Speed = 0.5, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Bow (DG)", Category = "Archery", Damage = 20, Value = 2725, Reach = 0, Speed = 0.75, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Bow", Category = "Archery", Damage = 12, Value = 270, Reach = 0, Speed = 0.75, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Bow", Category = "Archery", Damage = 17, Value = 1440, Reach = 0, Speed = 0.5625, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Bow", Category = "Archery", Damage = 13, Value = 470, Reach = 0, Speed = 0.6875, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Glass Bow", Category = "Archery", Damage = 15, Value = 820, Reach = 0, Speed = 0.625, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Golden Bow (AE)", Category = "Archery", Damage = 11, Value = 1000, Reach = 0, Speed = 0.5, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Hunting Bow", Category = "Archery", Damage = 7, Value = 50, Reach = 0, Speed = 0.9375, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Long Bow", Category = "Archery", Damage = 6, Value = 30, Reach = 0, Speed = 1.0, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Madness Bow (AE)", Category = "Archery", Damage = 21, Value = 2800, Reach = 0, Speed = 0.5, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Nord Hero Bow", Category = "Archery", Damage = 11, Value = 200, Reach = 0, Speed = 0.875, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic Bow (DB)", Category = "Archery", Damage = 13, Value = 450, Reach = 0, Speed = 0.6875, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish Bow", Category = "Archery", Damage = 10, Value = 150, Reach = 0, Speed = 0.8125, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Stalhrim Bow (DB)", Category = "Archery", Damage = 17, Value = 1800, Reach = 0, Speed = 0.5625, Stagger = 0 });

            // CROSSBOWS
            WeaponDatabase.Add(new Weapon { Name = "Crossbow (DG)", Category = "Archery", Damage = 19, Value = 120, Reach = 0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Crossbow (AE)", Category = "Archery", Damage = 28, Value = 2800, Reach = 0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Crossbow (AE)", Category = "Archery", Damage = 30, Value = 3200, Reach = 0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Crossbow (DG)", Category = "Archery", Damage = 22, Value = 350, Reach = 0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Crossbow (AE)", Category = "Archery", Damage = 25, Value = 1500, Reach = 0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Crossbow (AE)", Category = "Archery", Damage = 23, Value = 650, Reach = 0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Enhanced Crossbow (DG)", Category = "Archery", Damage = 19, Value = 200, Reach = 0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Enhanced Dwarven Crossbow (DG)", Category = "Archery", Damage = 22, Value = 450, Reach = 0, Speed = 1.0, Stagger = 0.75 });

            // ==========================================
            // AMMUNITION (Arrows & Bolts)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Iron Arrow", Category = "Ammunition", Damage = 8, Value = 1 });
            WeaponDatabase.Add(new Weapon { Name = "Steel Arrow", Category = "Ammunition", Damage = 10, Value = 2 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish Arrow", Category = "Ammunition", Damage = 12, Value = 3 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Arrow", Category = "Ammunition", Damage = 14, Value = 4 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Arrow", Category = "Ammunition", Damage = 16, Value = 5 });
            WeaponDatabase.Add(new Weapon { Name = "Glass Arrow", Category = "Ammunition", Damage = 18, Value = 6 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Arrow", Category = "Ammunition", Damage = 20, Value = 7 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Arrow", Category = "Ammunition", Damage = 24, Value = 8 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Arrow (DG)", Category = "Ammunition", Damage = 25, Value = 9 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Bolt (DG)", Category = "Ammunition", Damage = 14, Value = 4 });

            // ==========================================
            // DAGGERS & FACTION WEAPONS
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Amber Dagger (AE)", Category = "One-Handed", Damage = 11, Value = 410, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Daedric Dagger", Category = "One-Handed", Damage = 11, Value = 500, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Dragonbone Dagger (DG)", Category = "One-Handed", Damage = 12, Value = 600, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Dwarven Dagger", Category = "One-Handed", Damage = 7, Value = 55, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Ebony Dagger", Category = "One-Handed", Damage = 10, Value = 290, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Elven Dagger", Category = "One-Handed", Damage = 8, Value = 95, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Glass Dagger", Category = "One-Handed", Damage = 9, Value = 165, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Iron Dagger", Category = "One-Handed", Damage = 4, Value = 10, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Orcish Dagger", Category = "One-Handed", Damage = 6, Value = 30, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Steel Dagger", Category = "One-Handed", Damage = 5, Value = 15, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Nordic Dagger (DB)", Category = "One-Handed", Damage = 10, Value = 165, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Steel Dagger", Category = "One-Handed", Damage = 5, Value = 15, Reach = 0.7, Speed = 1.3, Stagger = 0 });

            // FACTION ITEMS
            WeaponDatabase.Add(new Weapon { Name = "Blades Sword", Category = "One-Handed", Damage = 11, Value = 300, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Dawnguard War Axe (DG)", Category = "One-Handed", Damage = 11, Value = 110, Reach = 1.0, Speed = 0.9, Stagger = 0.85 });
            WeaponDatabase.Add(new Weapon { Name = "Imperial Sword", Category = "One-Handed", Damage = 8, Value = 23, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Silver Sword", Category = "One-Handed", Damage = 8, Value = 100, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });
            WeaponDatabase.Add(new Weapon { Name = "Scimitar", Category = "One-Handed", Damage = 11, Value = 5, Reach = 1.0, Speed = 1.0, Stagger = 0.75 });

            // ==========================================
            // TOOLS (Special: Dagger Speed, Sword Perks)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Pickaxe", Category = "One-Handed", Damage = 5, Value = 5, Reach = 1.0, Speed = 1.3, Stagger = 0.75, IsEnchantable = true });
            WeaponDatabase.Add(new Weapon { Name = "Woodcutter's Axe", Category = "One-Handed", Damage = 5, Value = 5, Reach = 1.0, Speed = 1.3, Stagger = 0.75, IsEnchantable = true });
            
            // Ancient Nordic Pickaxe (Wichtig für Stalhrim!)
            WeaponDatabase.Add(new Weapon { Name = "Ancient Nordic Pickaxe (DB)", Category = "One-Handed", Damage = 5, Value = 40, Reach = 1.0, Speed = 1.3, Stagger = 0.75, IsEnchantable = true });

            // The "Dinner" Set (Ja, man kann mit Besteck kämpfen!)
            WeaponDatabase.Add(new Weapon { Name = "Knife (Weapon)", Category = "One-Handed", Damage = 1, Value = 1, Reach = 0.7, Speed = 1.3, Stagger = 0 });
            WeaponDatabase.Add(new Weapon { Name = "Fork (Weapon)", Category = "One-Handed", Damage = 1, Value = 1, Reach = 0.7, Speed = 1.3, Stagger = 0 });

            // Shiv (Die Gefängnis-Waffe aus Cidhna Mine)
            WeaponDatabase.Add(new Weapon { Name = "Shiv", Category = "One-Handed", Damage = 5, Value = 5, Reach = 0.7, Speed = 1.3, Stagger = 0 });

            // ==========================================
            // UNARMED (Fists)
            // ==========================================
            WeaponDatabase.Add(new Weapon { Name = "Fists", Category = "Unarmed", Damage = 4, Value = 0, Reach = 0.7, Speed = 1.0, Stagger = 0.5, IsEnchantable = false });

            // ==========================================
            // UNIQUE / ARTIFACTS (Most are IsEnchantable = false)
            // ==========================================

            // Daedric Artifacts
            WeaponDatabase.Add(new Weapon { Name = "Blade of Woe", Category = "One-Handed", Damage = 12, Value = 880, Reach = 0.7, Speed = 1.0, Stagger = 0, Effect = "Absorbs 10 Health.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Dawnbreaker", Category = "One-Handed", Damage = 12, Value = 740, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "Explosion vs Undead.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Mace of Molag Bal", Category = "One-Handed", Damage = 16, Value = 1257, Reach = 1.0, Speed = 0.8, Stagger = 1.0, Effect = "25 Stamina/Magicka damage. Soul Trap.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Mehrunes' Razor", Category = "One-Handed", Damage = 11, Value = 860, Reach = 0.7, Speed = 1.3, Stagger = 0, Effect = "1.98% Instant Kill chance.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Volendrung", Category = "Two-Handed", Damage = 25, Value = 1843, Reach = 1.3, Speed = 0.7, Stagger = 1.25, Effect = "Absorbs 50 Stamina.", IsEnchantable = false });

            // Quest Rewards & Set Items
            WeaponDatabase.Add(new Weapon { Name = "Wuuthrad", Category = "Two-Handed", Damage = 25, Value = 2000, Reach = 1.3, Speed = 0.7, Stagger = 1.15, Effect = "1.2x Damage against Elves.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Windshear", Category = "One-Handed", Damage = 11, Value = 40, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "Bash has 100% Stagger chance.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Keening", Category = "One-Handed", Damage = 8, Value = 20, Reach = 0.7, Speed = 1.3, Stagger = 0, Effect = "Absorbs Health/Magicka/Stamina", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Ghostblade", Category = "One-Handed", Damage = 8, Value = 300, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "3 extra points of damage, ignores armor.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Valdr's Lucky Dagger", Category = "One-Handed", Damage = 5, Value = 15, Reach = 0.7, Speed = 1.3, Stagger = 0, Effect = "25% Critical Hit chance", IsEnchantable = true });

            // Red Eagle & Ancient Sets
            WeaponDatabase.Add(new Weapon { Name = "Red Eagle's Fury", Category = "One-Handed", Damage = 8, Value = 45, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "Unique named sword.", IsEnchantable = true });
            WeaponDatabase.Add(new Weapon { Name = "Red Eagle's Bane", Category = "One-Handed", Damage = 11, Value = 1133, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "Burns Undead for 10 points and turns them.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Eduj", Category = "One-Handed", Damage = 11, Value = 250, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "10 Frost Damage.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Okin", Category = "One-Handed", Damage = 12, Value = 300, Reach = 1.0, Speed = 0.9, Stagger = 0.85, Effect = "10 Frost Damage.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Bolar's Oathblade", Category = "One-Handed", Damage = 11, Value = 292, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "25 Stamina Dmg, Fear up to Lvl 12.", IsEnchantable = false });

            // DLC Uniques (Dawnguard & Dragonborn)
            WeaponDatabase.Add(new Weapon { Name = "Harkon's Sword (DG)", Category = "One-Handed", Damage = 8, Value = 1472, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "Absorbs Health/Magicka/Stamina if Vampire.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Bloodskal Blade (DB)", Category = "Two-Handed", Damage = 21, Value = 500, Reach = 1.3, Speed = 0.7, Stagger = 1.1, Effect = "Energy blast on power attacks (30 dmg).", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Champion's Cudgel (DB)", Category = "Two-Handed", Damage = 24, Value = 1767, Reach = 1.3, Speed = 0.6, Stagger = 1.25, Effect = "50% chance for Elemental Damage.", IsEnchantable = false });

            // The Longhammer (Einzigartig: Hat die Speed eines Einhandschwerts!)
            WeaponDatabase.Add(new Weapon { Name = "The Longhammer", Category = "Two-Handed", Damage = 21, Value = 90, Reach = 1.3, Speed = 0.8, Stagger = 1.25, IsEnchantable = true });

            // Aegisbane (Eis-Hammer)
            WeaponDatabase.Add(new Weapon { Name = "Aegisbane", Category = "Two-Handed", Damage = 18, Value = 60, Reach = 1.3, Speed = 0.6, Stagger = 1.25, Effect = "5 Frost Damage to Health and Stamina.", IsEnchantable = false });

            // Trollsbane (Feuer-Hammer)
            WeaponDatabase.Add(new Weapon { Name = "Trollsbane", Category = "Two-Handed", Damage = 20, Value = 110, Reach = 1.3, Speed = 0.6, Stagger = 1.25, Effect = "15 Fire Damage to Trolls.", IsEnchantable = false });

            // Ghostly Weapons (Drain-Set aus Labyrinthion)
            WeaponDatabase.Add(new Weapon { Name = "Drainheart Sword", Category = "One-Handed", Damage = 11, Value = 73, Reach = 1.0, Speed = 1.0, Stagger = 0.75, Effect = "Absorbs 15 Stamina.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Drainblood Battleaxe", Category = "Two-Handed", Damage = 21, Value = 266, Reach = 1.3, Speed = 0.7, Stagger = 1.15, Effect = "Absorbs 15 Health.", IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Drainspell Bow", Category = "Archery", Damage = 14, Value = 415, Reach = 0, Speed = 0.625, Stagger = 0, Effect = "Absorbs 15 Magicka.", IsEnchantable = false });

            // Einzigartiges Werkzeug: Notched Pickaxe (Spitzhacke des Gipfelstürmers)
            WeaponDatabase.Add(new Weapon { Name = "Notched Pickaxe", Category = "One-Handed", Damage = 5, Value = 300, Reach = 1.0, Speed = 1.3, Stagger = 0.75, Effect = "5 Shock damage. Boosts Smithing.", IsEnchantable = false });

            // Poacher's Axe (Einzigartige Holzhackeraxt)
            WeaponDatabase.Add(new Weapon { Name = "Poacher's Axe", Category = "One-Handed", Damage = 5, Value = 15, Reach = 1.0, Speed = 1.3, Stagger = 0.75, Effect = "3 extra damage to animals.", IsEnchantable = false });

            // Zephyr (Der schnellste Bogen im Spiel)
            WeaponDatabase.Add(new Weapon { Name = "Zephyr (DG)", Category = "Archery", Damage = 12, Value = 670, Reach = 0, Speed = 1.0, Stagger = 0, Effect = "Fires 30% faster than a standard bow.", IsEnchantable = false });

            // 2. Auriel's Bow
            WeaponDatabase.Add(new Weapon { Name = "Auriel's Bow (DG)", Category = "Archery", Damage = 13, Value = 1440, Reach = 0, Speed = 1.0, Stagger = 0, Effect = "Sun Damage. Triple damage vs Undead.", IsEnchantable = false });

            // Headsman's Axe
            WeaponDatabase.Add(new Weapon { Name = "Headsman's Axe", Category = "Two-Handed", Damage = 17, Value = 15, Reach = 1.5, Speed = 0.7, Stagger = 1.15 });

            // Dragon Priest Dagger
            WeaponDatabase.Add(new Weapon { Name = "Dragon Priest Dagger", Category = "One-Handed", Damage = 6, Value = 9, Reach = 0.7, Speed = 1.3, Stagger = 0 });

            // Horksbane
            WeaponDatabase.Add(new Weapon { Name = "Horksbane", Category = "One-Handed", Damage = 10, Value = 115, Reach = 1.0, Speed = 0.8, Stagger = 1.0, Effect = "20 extra damage to Horkers.", IsEnchantable = false });
            // ==========================================
            // LEVELED ITEMS
            // ==========================================

            // Chillrend (Das Glas-Schwert der Diebesgilde)
            WeaponDatabase.Add(new Weapon
            {
                Name = "Chillrend",
                Category = "One-Handed",
                Reach = 1.0,
                Speed = 1.0,
                Stagger = 0.75,
                IsEnchantable = false,
                LevelVariants = new List<LeveledStat> {
        new LeveledStat { MinLevel = 1, Damage = 10, Value = 542, Effect = "5 Frost, 2s Paralyze" },
        new LeveledStat { MinLevel = 46, Damage = 15, Value = 1442, Effect = "30 Frost, 2s Paralyze" } }
            });

            // Nightingale Blade (Das Gegenstück zum Bogen)
            WeaponDatabase.Add(new Weapon
            {
                Name = "Nightingale Blade",
                Category = "One-Handed",
                Reach = 1.0,
                Speed = 1.0,
                Stagger = 0.75,
                IsEnchantable = false,
                LevelVariants = new List<LeveledStat> {
        new LeveledStat { MinLevel = 1, Damage = 10, Value = 380, Effect = "Absorbs 5 HP/Stamina" },
        new LeveledStat { MinLevel = 46, Damage = 14, Value = 1425, Effect = "Absorbs 25 HP/Stamina" } }
            });

            // Nightingale Bow (Bogen der Nachtigall)
            WeaponDatabase.Add(new Weapon
            {
                Name = "Nightingale Bow",
                Category = "Archery",
                Reach = 0,
                Speed = 0.5,
                Stagger = 0,
                IsEnchantable = false,
                LevelVariants = new List<LeveledStat> {
        new LeveledStat { MinLevel = 1, Damage = 12, Value = 470, Effect = "10 Frost/5 Shock" },
        new LeveledStat { MinLevel = 46, Damage = 19, Value = 3700, Effect = "30 Frost/15 Shock" } }
            });

            // Dragonbane (Das Anti-Drachen-Schwert)
            WeaponDatabase.Add(new Weapon
            {
                Name = "Dragonbane",
                Category = "One-Handed",
                Reach = 1.0,
                Speed = 1.0,
                Stagger = 0.75,
                IsEnchantable = false,
                LevelVariants = new List<LeveledStat> {
        new LeveledStat { MinLevel = 1, Damage = 10, Value = 412, Effect = "20 Lightning, 40 extra dmg vs Dragons" },
        new LeveledStat { MinLevel = 46, Damage = 14, Value = 1172, Effect = "40 Lightning, 80 extra dmg vs Dragons" } }
            });

            // Miraak's Sword (Höchste Reichweite!)
            WeaponDatabase.Add(new Weapon
            {
                Name = "Miraak's Sword (DB)",
                Category = "One-Handed",
                Reach = 1.2,
                Speed = 1.0,
                Stagger = 0.75,
                IsEnchantable = false,
                LevelVariants = new List<LeveledStat> {
        new LeveledStat { MinLevel = 1, Damage = 12, Value = 415, Effect = "Absorbs 15 Stamina" },
        new LeveledStat { MinLevel = 60, Damage = 16, Value = 1245, Effect = "Absorbs 25 Stamina" } }
            });

            // The Pale Blade (Frost/Furcht)
            WeaponDatabase.Add(new Weapon
            {
                Name = "The Pale Blade",
                Category = "One-Handed",
                Reach = 1.0,
                Speed = 1.0,
                Stagger = 0.75,
                IsEnchantable = false,
                LevelVariants = new List<LeveledStat> {
        new LeveledStat { MinLevel = 1, Damage = 8, Value = 129, Effect = "5 Frost, Fear up to Lvl 8" },
        new LeveledStat { MinLevel = 27, Damage = 13, Value = 753, Effect = "25 Frost, Fear up to Lvl 16" } }
            });

            // Gauldur Blackblade
            WeaponDatabase.Add(new Weapon
            {
                Name = "Gauldur Blackblade",
                Category = "One-Handed",
                Reach = 1.0,
                Speed = 1.0,
                Stagger = 0.75,
                IsEnchantable = false,
                LevelVariants = new List<LeveledStat> {
        new LeveledStat { MinLevel = 1, Damage = 8, Value = 265, Effect = "Absorbs 5 Health" },
        new LeveledStat { MinLevel = 36, Damage = 11, Value = 875, Effect = "Absorbs 25 Health" } }
            });

            // Gauldur Blackbow
            WeaponDatabase.Add(new Weapon
            {
                Name = "Gauldur Blackbow",
                Category = "Archery",
                Reach = 0,
                Speed = 0.625,
                Stagger = 0,
                IsEnchantable = false,
                LevelVariants = new List<LeveledStat> {
        new LeveledStat { MinLevel = 1, Damage = 8, Value = 270, Effect = "Absorbs 10 Magicka" },
        new LeveledStat { MinLevel = 36, Damage = 14, Value = 1000, Effect = "Absorbs 30 Magicka" } }
            });

            // SPELL WEAPONS (Bound)
            WeaponDatabase.Add(new Weapon { Name = "Bound Sword (Spell)", Category = "One-Handed", Damage = 9, Value = 0, Reach = 1.0, Speed = 1.0, Stagger = 0.75, IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Bound Dagger (Spell)", Category = "One-Handed", Damage = 6, Value = 0, Reach = 0.7, Speed = 1.3, Stagger = 0, IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Bound Battleaxe (Spell)", Category = "Two-Handed", Damage = 17, Value = 0, Reach = 1.3, Speed = 0.7, Stagger = 1.15, IsEnchantable = false });
            WeaponDatabase.Add(new Weapon { Name = "Bound Bow (Spell)", Category = "Archery", Damage = 18, Value = 0, Reach = 0, Speed = 0.875, Stagger = 0, IsEnchantable = false });

            // SORTIERUNG AM ENDE
            WeaponDatabase = WeaponDatabase.OrderBy(w => w.Name).ToList();

        }

        private void LoadArmorData()
        {
            ArmorDatabase.Clear();

            // ==========================================
            // HEAVY ARMOR - IRON
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Iron Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 25, Weight = 30.0, Value = 125 });
            ArmorDatabase.Add(new Armor { Name = "Iron Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 10, Weight = 6.0, Value = 25 });
            ArmorDatabase.Add(new Armor { Name = "Iron Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 10, Weight = 5.0, Value = 25 });
            ArmorDatabase.Add(new Armor { Name = "Iron Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 15, Weight = 5.0, Value = 60 });
            ArmorDatabase.Add(new Armor { Name = "Iron Shield", Slot = "Shield", Category = "Shield", ArmorRating = 20, Weight = 12.0, Value = 60 });

            ArmorDatabase.Add(new Armor { Name = "Banded Iron Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 28, Weight = 35.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Banded Iron Shield", Slot = "Shield", Category = "Shield", ArmorRating = 22, Weight = 12.0, Value = 100 });

            // ==========================================
            // HEAVY ARMOR - STEEL
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Steel Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 31, Weight = 35.0, Value = 275 });
            ArmorDatabase.Add(new Armor { Name = "Steel Plate Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 40, Weight = 38.0, Value = 625 });
            ArmorDatabase.Add(new Armor { Name = "Steel Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 12, Weight = 7.0, Value = 55 });
            ArmorDatabase.Add(new Armor { Name = "Steel Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 12, Weight = 4.0, Value = 55 });
            ArmorDatabase.Add(new Armor { Name = "Steel Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 17, Weight = 5.0, Value = 135 });
            ArmorDatabase.Add(new Armor { Name = "Steel Shield", Slot = "Shield", Category = "Shield", ArmorRating = 24, Weight = 12.0, Value = 150 });

            // ==========================================
            // HEAVY ARMOR - DWARVEN
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Dwarven Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 34, Weight = 45.0, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Dwarven Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 13, Weight = 10.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Dwarven Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 13, Weight = 8.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Dwarven Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 18, Weight = 12.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Dwarven Shield", Slot = "Shield", Category = "Shield", ArmorRating = 26, Weight = 12.0, Value = 225 });

            // ==========================================
            // HEAVY ARMOR - ORCISH
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Orcish Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 40, Weight = 45.0, Value = 1000 });
            ArmorDatabase.Add(new Armor { Name = "Orcish Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 15, Weight = 10.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Orcish Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 15, Weight = 7.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Orcish Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 20, Weight = 10.0, Value = 500 });
            ArmorDatabase.Add(new Armor { Name = "Orcish Shield", Slot = "Shield", Category = "Shield", ArmorRating = 30, Weight = 14.0, Value = 500 });

            // ==========================================
            // HEAVY ARMOR - EBONY
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Ebony Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 43, Weight = 38.0, Value = 1500 });
            ArmorDatabase.Add(new Armor { Name = "Ebony Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 16, Weight = 7.0, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Ebony Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 16, Weight = 7.0, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Ebony Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 21, Weight = 10.0, Value = 750 });
            ArmorDatabase.Add(new Armor { Name = "Ebony Shield", Slot = "Shield", Category = "Shield", ArmorRating = 32, Weight = 14.0, Value = 750 });

            // ==========================================
            // HEAVY ARMOR - DAEDRIC
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Daedric Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 49, Weight = 50.0, Value = 3200 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 18, Weight = 10.0, Value = 625 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 18, Weight = 6.0, Value = 625 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 15.0, Value = 1600 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Shield", Slot = "Shield", Category = "Shield", ArmorRating = 36, Weight = 15.0, Value = 1600 });


            // ==========================================
            // HEAVY ARMOR - DRAGONPLATE
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Dragonplate Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 46, Weight = 40.0, Value = 2125 });
            ArmorDatabase.Add(new Armor { Name = "Dragonplate Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 17, Weight = 8.0, Value = 425 });
            ArmorDatabase.Add(new Armor { Name = "Dragonplate Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 17, Weight = 8.0, Value = 425 });
            ArmorDatabase.Add(new Armor { Name = "Dragonplate Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 22, Weight = 10.0, Value = 1050 });
            ArmorDatabase.Add(new Armor { Name = "Dragonplate Shield", Slot = "Shield", Category = "Shield", ArmorRating = 34, Weight = 15.0, Value = 1050 });

            // ==========================================
            // HEAVY ARMOR - FRACTION (IMPERIAL / BLADES / WOLF)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Imperial Heavy Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 25, Weight = 35.0, Value = 100 });
            ArmorDatabase.Add(new Armor { Name = "Imperial Heavy Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 10, Weight = 8.0, Value = 20 });
            ArmorDatabase.Add(new Armor { Name = "Imperial Heavy Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 10, Weight = 6.0, Value = 20 });
            ArmorDatabase.Add(new Armor { Name = "Imperial Heavy Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 15, Weight = 5.0, Value = 50 });

            ArmorDatabase.Add(new Armor { Name = "Blades Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 38, Weight = 45.0, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Blades Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 13, Weight = 14.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Blades Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 13, Weight = 10.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Blades Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 18, Weight = 12.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Blades Shield", Slot = "Shield", Category = "Shield", ArmorRating = 26, Weight = 15.0, Value = 225 });

            ArmorDatabase.Add(new Armor { Name = "Wolf Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 30, Weight = 20.0, Value = 150 });
            ArmorDatabase.Add(new Armor { Name = "Wolf Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 11, Weight = 4.0, Value = 30 });
            ArmorDatabase.Add(new Armor { Name = "Wolf Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 11, Weight = 4.0, Value = 30 });
            ArmorDatabase.Add(new Armor { Name = "Wolf Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 16, Weight = 4.0, Value = 75 });

            // ==========================================
            // HEAVY ARMOR - ANCIENT NORD & FALMER
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Ancient Nord Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 28, Weight = 35.0, Value = 235 });
            ArmorDatabase.Add(new Armor { Name = "Ancient Nord Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 10, Weight = 5.0, Value = 40 });
            ArmorDatabase.Add(new Armor { Name = "Ancient Nord Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 10, Weight = 5.0, Value = 40 });
            ArmorDatabase.Add(new Armor { Name = "Ancient Nord Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 15, Weight = 8.0, Value = 100 });

            ArmorDatabase.Add(new Armor { Name = "Falmer Heavy Armor (DG)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 38, Weight = 45.0, Value = 1200 });
            ArmorDatabase.Add(new Armor { Name = "Falmer Heavy Boots (DG)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 13, Weight = 10.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Falmer Heavy Gauntlets (DG)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 13, Weight = 7.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Falmer Heavy Helmet (DG)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 18, Weight = 10.0, Value = 600 });

            ArmorDatabase.Add(new Armor { Name = "Shellbug Helmet (DG)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 22, Weight = 12.0, Value = 1000 });

            // ==========================================
            // HEAVY ARMOR - UNIQUE ARTIFACTS
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Masque of Clavicus Vile", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 7.0, Value = 1277, Effect = "Prices 20% better, +10 Speech, 5% Magicka Regen." });
            ArmorDatabase.Add(new Armor { Name = "Ebony Mail", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 45, Weight = 28.0, Value = 5000, Effect = "Muffle. Poison Cloak (5 pts/s)." });
            ArmorDatabase.Add(new Armor { Name = "Helm of Yngol", Slot = "Head", Category = "Heavy Armor", ArmorRating = 21, Weight = 8.0, Value = 455, Effect = "+30% Frost Resistance." });
            ArmorDatabase.Add(new Armor { Name = "Konahrik", Slot = "Head", Category = "Heavy Armor", ArmorRating = 24, Weight = 7.0, Value = 3200, Effect = "Low health chance to heal & damage enemies." });

            ArmorDatabase.Add(new Armor { Name = "Spellbreaker", Slot = "Shield", Category = "Shield", ArmorRating = 38, Weight = 12.0, Value = 275, Effect = "Blocking creates Ward (50 pts)." });
            ArmorDatabase.Add(new Armor { Name = "Aetherial Shield (DG)", Slot = "Shield", Category = "Shield", ArmorRating = 26, Weight = 12.0, Value = 1250, Effect = "Bash turns enemy Ethereal (15s)." });
            ArmorDatabase.Add(new Armor { Name = "Auriel's Shield (DG)", Slot = "Shield", Category = "Shield", ArmorRating = 32, Weight = 14.0, Value = 750, Effect = "Stores block energy for Power Bash." });
            ArmorDatabase.Add(new Armor { Name = "Shield of Ysgramor", Slot = "Shield", Category = "Shield", ArmorRating = 30, Weight = 12.0, Value = 1750, Effect = "+20% Magic Resist, +20 Health." });
            ArmorDatabase.Add(new Armor { Name = "Targe of the Blooded", Slot = "Shield", Category = "Shield", ArmorRating = 20, Weight = 11.0, Value = 300, Effect = "Bash deals 3 bleeding dmg for 5s." });

            // Drachenpriestermasken (Schwer)
            ArmorDatabase.Add(new Armor { Name = "Rahgot", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 9.0, Value = 455, Effect = "+70 Stamina." });
            ArmorDatabase.Add(new Armor { Name = "Hevnoraak", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 9.0, Value = 455, Effect = "Immunity to Disease and Poison." });
            ArmorDatabase.Add(new Armor { Name = "Dukaan (DB)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 9.0, Value = 455, Effect = "50% Frost Resist, 25% Frost spell dmg." });
            ArmorDatabase.Add(new Armor { Name = "Zahkriisos (DB)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 9.0, Value = 455, Effect = "50% Shock Resist, 25% Shock spell dmg." });
            ArmorDatabase.Add(new Armor { Name = "Otar", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 9.0, Value = 455, Effect = "+30% Fire, Frost, and Shock Resistance." });
            ArmorDatabase.Add(new Armor { Name = "Vokun", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 9.0, Value = 455, Effect = "-20% Conjuration, Illusion, Alteration Cost." });
            ArmorDatabase.Add(new Armor { Name = "Ahzidal (DB)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 9.0, Value = 455, Effect = "50% Fire Resist, 25% Fire spell dmg." });

            ArmorDatabase.Add(new Armor { Name = "Visage of Mzund (DB)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 24, Weight = 15.0, Value = 1300, Effect = "+60 Stamina, Breath of Nchuak attack." });
            ArmorDatabase.Add(new Armor { Name = "Lord's Mail (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 45, Weight = 35.0, Value = 2000, Effect = "Absorb Health, Resist Poison/Magic." });
            ArmorDatabase.Add(new Armor { Name = "Fists of Randagulf (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 15, Weight = 7.0, Value = 500, Effect = "+20% Block/Melee Damage." });

            // Quest Uniques
            ArmorDatabase.Add(new Armor { Name = "Jagged Crown", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 9.0, Value = 5000, Effect = "The crown of high kings (No enchantment, but unique)." });
            ArmorDatabase.Add(new Armor { Name = "Imperial Helmet (Closed)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 15, Weight = 5.0, Value = 50 });
            ArmorDatabase.Add(new Armor { Name = "Imperial Officer's Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 16, Weight = 5.0, Value = 65 });

            // ==========================================
            // HEAVY ARMOR - DLC (NORDIC / STALHRIM / DAWNGUARD)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Nordic Carved Armor (DB)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 43, Weight = 35.0, Value = 1100 });
            ArmorDatabase.Add(new Armor { Name = "Nordic Carved Boots (DB)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 16, Weight = 7.0, Value = 225 });
            ArmorDatabase.Add(new Armor { Name = "Nordic Carved Gauntlets (DB)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 16, Weight = 6.0, Value = 225 });
            ArmorDatabase.Add(new Armor { Name = "Nordic Carved Helmet (DB)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 21, Weight = 8.0, Value = 550 });
            ArmorDatabase.Add(new Armor { Name = "Nordic Carved Shield (DB)", Slot = "Shield", Category = "Shield", ArmorRating = 32, Weight = 12.0, Value = 550 });

            ArmorDatabase.Add(new Armor { Name = "Stalhrim Armor (DB)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 46, Weight = 35.0, Value = 1900 });
            ArmorDatabase.Add(new Armor { Name = "Stalhrim Boots (DB)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 17, Weight = 7.0, Value = 375 });
            ArmorDatabase.Add(new Armor { Name = "Stalhrim Gauntlets (DB)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 17, Weight = 7.0, Value = 375 });
            ArmorDatabase.Add(new Armor { Name = "Stalhrim Helmet (DB)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 22, Weight = 10.0, Value = 950 });
            ArmorDatabase.Add(new Armor { Name = "Stalhrim Shield (DB)", Slot = "Shield", Category = "Shield", ArmorRating = 34, Weight = 13.0, Value = 950 });

            ArmorDatabase.Add(new Armor { Name = "Dawnguard Heavy Armor (DG)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 38, Weight = 45.0, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Dawnguard Heavy Boots (DG)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 13, Weight = 9.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Dawnguard Heavy Gauntlets (DG)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 13, Weight = 6.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Dawnguard Full Helmet (DG)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 18, Weight = 12.0, Value = 200 });

            // ==========================================
            // HEAVY ARMOR - BONEMOLD (DB DLC)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Bonemold Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 28, Weight = 28.0, Value = 165 });
            ArmorDatabase.Add(new Armor { Name = "Bonemold Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 11, Weight = 7.0, Value = 35 });
            ArmorDatabase.Add(new Armor { Name = "Bonemold Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 11, Weight = 5.0, Value = 35 });
            ArmorDatabase.Add(new Armor { Name = "Bonemold Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 16, Weight = 6.0, Value = 80 });
            ArmorDatabase.Add(new Armor { Name = "Bonemold Shield", Slot = "Shield", Category = "Shield", ArmorRating = 22, Weight = 8.0, Value = 85 });

            ArmorDatabase.Add(new Armor { Name = "Improved Bonemold Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 31, Weight = 28.0, Value = 240 });

            // ==========================================
            // HEAVY ARMOR - AHZIDAL'S RELICS (DB)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Ahzidal's Armor of Retribution", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 40, Weight = 45.0, Value = 1000, Effect = "Chance to paralyze enemies who strike you." });
            ArmorDatabase.Add(new Armor { Name = "Ahzidal's Boots of Waterwalking", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 15, Weight = 10.0, Value = 200, Effect = "Waterwalking. +10 Enchanting if 4 items worn." });
            ArmorDatabase.Add(new Armor { Name = "Ahzidal's Gauntlets of Warding", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 15, Weight = 7.0, Value = 200, Effect = "Wards are 25% more effective, but cost 50% more." });
            ArmorDatabase.Add(new Armor { Name = "Ahzidal's Helm of Vision", Slot = "Head", Category = "Heavy Armor", ArmorRating = 20, Weight = 10.0, Value = 500, Effect = "Conjuration/Rune spells cost more but have more range." });

            // ==========================================
            // HEAVY ARMOR - CHITIN HEAVY (DB DLC)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Chitin Heavy Armor", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 37, Weight = 40.0, Value = 650 });
            ArmorDatabase.Add(new Armor { Name = "Chitin Heavy Boots", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 14, Weight = 8.0, Value = 140 });
            ArmorDatabase.Add(new Armor { Name = "Chitin Heavy Gauntlets", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 14, Weight = 6.0, Value = 140 });
            ArmorDatabase.Add(new Armor { Name = "Chitin Heavy Helmet", Slot = "Head", Category = "Heavy Armor", ArmorRating = 19, Weight = 8.0, Value = 325 });

            // ==========================================
            // HEAVY ARMOR - ANNIVERSARY EDITION (MADNESS / SAINTS)
            // ==========================================

            // Madness Armor
            ArmorDatabase.Add(new Armor { Name = "Madness Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 49, Weight = 45.0, Value = 3500 });
            ArmorDatabase.Add(new Armor { Name = "Madness Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 18, Weight = 9.0, Value = 650 });
            ArmorDatabase.Add(new Armor { Name = "Madness Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 18, Weight = 6.0, Value = 650 });
            ArmorDatabase.Add(new Armor { Name = "Madness Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 10.0, Value = 1750 });
            ArmorDatabase.Add(new Armor { Name = "Madness Shield (AE)", Slot = "Shield", Category = "Shield", ArmorRating = 36, Weight = 12.0, Value = 1750 });

            // Golden Armor (Saints)
            ArmorDatabase.Add(new Armor { Name = "Golden Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 37, Weight = 42.0, Value = 700 });
            ArmorDatabase.Add(new Armor { Name = "Golden Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 14, Weight = 9.0, Value = 150 });
            ArmorDatabase.Add(new Armor { Name = "Golden Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 14, Weight = 7.0, Value = 150 });
            ArmorDatabase.Add(new Armor { Name = "Golden Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 19, Weight = 8.0, Value = 350 });
            ArmorDatabase.Add(new Armor { Name = "Golden Shield (AE)", Slot = "Shield", Category = "Shield", ArmorRating = 28, Weight = 10.0, Value = 350 });

            // ==========================================
            // HEAVY ARMOR - CIVIL WAR CHAMPIONS (AE)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Imperial Dragon Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 46, Weight = 40.0, Value = 2125, Effect = "Resist Fire/Poison 50%." });
            ArmorDatabase.Add(new Armor { Name = "Imperial Dragon Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 17, Weight = 8.0, Value = 425, Effect = "Resist Frost 50%." });
            ArmorDatabase.Add(new Armor { Name = "Imperial Dragon Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 17, Weight = 8.0, Value = 425, Effect = "Block 40% better." });
            ArmorDatabase.Add(new Armor { Name = "Imperial Dragon Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 22, Weight = 10.0, Value = 1050, Effect = "+10% Restoration, 50% Magicka Regen." });

            ArmorDatabase.Add(new Armor { Name = "Storm-Bear Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 43, Weight = 35.0, Value = 1100, Effect = "Resist Shock/Poison 50%." });
            ArmorDatabase.Add(new Armor { Name = "Storm-Bear Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 16, Weight = 7.0, Value = 225, Effect = "Resist Frost 50%." });
            ArmorDatabase.Add(new Armor { Name = "Storm-Bear Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 16, Weight = 6.0, Value = 225, Effect = "Two-Handed 40% better." });
            ArmorDatabase.Add(new Armor { Name = "Storm-Bear Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 21, Weight = 8.0, Value = 550, Effect = "-20% Shout cooldown, 50% Stamina Regen." });

            // ==========================================
            // HEAVY ARMOR - TRIBUNAL (AE)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Indoril Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 34, Weight = 45.0, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Her Hand Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 43, Weight = 38.0, Value = 1500 });
            ArmorDatabase.Add(new Armor { Name = "Ordinator Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 34, Weight = 45.0, Value = 400 });

            // ==========================================
            // HEAVY ARMOR - VIGIL ENFORCER (AE)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Vigil Enforcer Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 36, Weight = 38.0, Value = 500 });
            ArmorDatabase.Add(new Armor { Name = "Vigil Enforcer Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 14, Weight = 7.0, Value = 100 });
            ArmorDatabase.Add(new Armor { Name = "Vigil Enforcer Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 14, Weight = 6.0, Value = 100 });
            ArmorDatabase.Add(new Armor { Name = "Vigil Enforcer Corrupt Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 19, Weight = 8.0, Value = 250 });

            // ==========================================
            // HEAVY ARMOR - ALTERNATIVE ARMORS (AE)
            // ==========================================

            // Silver Armor
            ArmorDatabase.Add(new Armor { Name = "Silver Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 33, Weight = 35.0, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Silver Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 12, Weight = 7.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Silver Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 12, Weight = 5.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Silver Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 17, Weight = 5.0, Value = 200 });

            // Ebony Plate
            ArmorDatabase.Add(new Armor { Name = "Ebony Plate Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 43, Weight = 38.0, Value = 1500 });
            ArmorDatabase.Add(new Armor { Name = "Ebony Plate Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 16, Weight = 7.0, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Ebony Plate Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 16, Weight = 7.0, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Ebony Plate Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 21, Weight = 10.0, Value = 750 });

            // Daedric Plate
            ArmorDatabase.Add(new Armor { Name = "Daedric Plate Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 49, Weight = 50.0, Value = 3200 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Plate Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 18, Weight = 10.0, Value = 625 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Plate Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 18, Weight = 6.0, Value = 625 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Plate Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 23, Weight = 15.0, Value = 1600 });

            // Dragonplate Insulated
            ArmorDatabase.Add(new Armor { Name = "Dragonplate Insulated Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 46, Weight = 40.0, Value = 2125 });
            ArmorDatabase.Add(new Armor { Name = "Dragonplate Insulated Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 17, Weight = 8.0, Value = 425 });
            ArmorDatabase.Add(new Armor { Name = "Dragonplate Insulated Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 17, Weight = 8.0, Value = 425 });
            ArmorDatabase.Add(new Armor { Name = "Dragonplate Insulated Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 22, Weight = 10.0, Value = 1050 });

            // Steel Soldier
            ArmorDatabase.Add(new Armor { Name = "Steel Soldier Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 31, Weight = 35.0, Value = 275 });
            ArmorDatabase.Add(new Armor { Name = "Steel Soldier Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 12, Weight = 7.0, Value = 55 });
            ArmorDatabase.Add(new Armor { Name = "Steel Soldier Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 12, Weight = 4.0, Value = 55 });
            ArmorDatabase.Add(new Armor { Name = "Steel Soldier Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 17, Weight = 5.0, Value = 135 });

            // Dwarven Plate
            ArmorDatabase.Add(new Armor { Name = "Dwarven Plate Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 34, Weight = 45.0, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Dwarven Plate Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 13, Weight = 10.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Dwarven Plate Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 13, Weight = 8.0, Value = 85 });
            ArmorDatabase.Add(new Armor { Name = "Dwarven Plate Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 18, Weight = 12.0, Value = 200 });

            // Orcish Plate
            ArmorDatabase.Add(new Armor { Name = "Orcish Plate Armor (AE)", Slot = "Chest", Category = "Heavy Armor", ArmorRating = 40, Weight = 45.0, Value = 1000 });
            ArmorDatabase.Add(new Armor { Name = "Orcish Plate Boots (AE)", Slot = "Feet", Category = "Heavy Armor", ArmorRating = 15, Weight = 10.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Orcish Plate Gauntlets (AE)", Slot = "Hands", Category = "Heavy Armor", ArmorRating = 15, Weight = 7.0, Value = 200 });
            ArmorDatabase.Add(new Armor { Name = "Orcish Plate Helmet (AE)", Slot = "Head", Category = "Heavy Armor", ArmorRating = 20, Weight = 10.0, Value = 500 });

            // ==========================================
            // HEAVY ARMOR - SPELL KNIGHT (AE)
            // ==========================================

            ArmorDatabase.Add(new Armor
            {
                Name = "Spell Knight Armor (Enchanted) (AE)",
                Slot = "Chest",
                Category = "Heavy Armor",
                LevelVariants = new List<LeveledStat>
                {
                new LeveledStat { MinLevel = 1, ArmorRating = 25, Value = 550, Effect = "+20 Health, +10 Heavy Armor" },
                new LeveledStat { MinLevel = 19, ArmorRating = 31, Value = 1200, Effect = "+35 Health, +12 Heavy Armor" },
                new LeveledStat { MinLevel = 36, ArmorRating = 43, Value = 2800, Effect = "+50 Health, +15 Heavy Armor" }
                }
            });

            ArmorDatabase.Add(new Armor
            {
                Name = "Spell Knight Boots (Enchanted) (AE)",
                Slot = "Feet",
                Category = "Heavy Armor",
                LevelVariants = new List<LeveledStat>
                {
                new LeveledStat { MinLevel = 1, ArmorRating = 10, Value = 150, Effect = "+20 Stamina" },
                new LeveledStat { MinLevel = 19, ArmorRating = 12, Value = 300, Effect = "+35 Stamina" },
                new LeveledStat { MinLevel = 36, ArmorRating = 16, Value = 700, Effect = "+50 Stamina" }
                }
            });

            ArmorDatabase.Add(new Armor
            {
                Name = "Spell Knight Gauntlets (Enchanted) (AE)",
                Slot = "Hands",
                Category = "Heavy Armor",
                LevelVariants = new List<LeveledStat>
                {
                new LeveledStat { MinLevel = 1, ArmorRating = 10, Value = 150, Effect = "+10 Heavy Armor" },
                new LeveledStat { MinLevel = 19, ArmorRating = 12, Value = 300, Effect = "+12 Heavy Armor" },
                new LeveledStat { MinLevel = 36, ArmorRating = 16, Value = 700, Effect = "+15 Heavy Armor" }
                }
            });

            ArmorDatabase.Add(new Armor
            {
                Name = "Spell Knight Helmet (Enchanted) (AE)",
                Slot = "Head",
                Category = "Heavy Armor",
                LevelVariants = new List<LeveledStat>
                {
                new LeveledStat { MinLevel = 1, ArmorRating = 15, Value = 350, Effect = "+20 Magicka" },
                new LeveledStat { MinLevel = 19, ArmorRating = 17, Value = 650, Effect = "+35 Magicka" },
                new LeveledStat { MinLevel = 36, ArmorRating = 21, Value = 1200, Effect = "+50 Magicka" }
                }
            });

            ArmorDatabase.Add(new Armor
            {
                Name = "Shield of Solitude",
                Slot = "Shield",
                Category = "Shield",
                Weight = 12.0,
                LevelVariants = new List<LeveledStat>
                {
                new LeveledStat { MinLevel = 1, ArmorRating = 20, Value = 450, Effect = "Resist Magic 10%, Block 15% better." },
                new LeveledStat { MinLevel = 18, ArmorRating = 24, Value = 685, Effect = "Resist Magic 15%, Block 20% better." },
                new LeveledStat { MinLevel = 40, ArmorRating = 32, Value = 2000, Effect = "Resist Magic 30%, Block 35% better." }
                }
            });

            ArmorDatabase.Add(new Armor
            {
                Name = "Miraak (Heavy) (DB)",
                Slot = "Head",
                Category = "Heavy Armor",
                Weight = 9.0,
                LevelVariants = new List<LeveledStat>
                {
                new LeveledStat { MinLevel = 1, ArmorRating = 23, Value = 455, Effect = "+40 Magicka" },
                new LeveledStat { MinLevel = 45, ArmorRating = 25, Value = 510, Effect = "+60 Magicka" },
                new LeveledStat { MinLevel = 60, ArmorRating = 27, Value = 603, Effect = "+70 Magicka" }
                }
            });

            // ==========================================
            // LIGHT ARMOR - BASIC MATERIALS
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Hide Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 20, Weight = 5.0, Value = 50 });
            ArmorDatabase.Add(new Armor { Name = "Hide Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 5, Weight = 1.0, Value = 10 });
            ArmorDatabase.Add(new Armor { Name = "Hide Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 5, Weight = 1.0, Value = 10 });
            ArmorDatabase.Add(new Armor { Name = "Hide Helmet", Slot = "Head", Category = "Light Armor", ArmorRating = 10, Weight = 1.0, Value = 25 });

            ArmorDatabase.Add(new Armor { Name = "Leather Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 26, Weight = 6.0, Value = 125 });
            ArmorDatabase.Add(new Armor { Name = "Leather Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 9, Weight = 2.0, Value = 25 });
            ArmorDatabase.Add(new Armor { Name = "Leather Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 9, Weight = 2.0, Value = 25 });
            ArmorDatabase.Add(new Armor { Name = "Leather Helmet", Slot = "Head", Category = "Light Armor", ArmorRating = 14, Weight = 2.0, Value = 60 });

            ArmorDatabase.Add(new Armor { Name = "Elven Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 29, Weight = 4.0, Value = 225 });
            ArmorDatabase.Add(new Armor { Name = "Elven Gilded Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 35, Weight = 4.0, Value = 550 });
            ArmorDatabase.Add(new Armor { Name = "Elven Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 10, Weight = 1.0, Value = 45 });
            ArmorDatabase.Add(new Armor { Name = "Elven Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 10, Weight = 1.0, Value = 45 });
            ArmorDatabase.Add(new Armor { Name = "Elven Helmet", Slot = "Head", Category = "Light Armor", ArmorRating = 15, Weight = 1.0, Value = 110 });

            ArmorDatabase.Add(new Armor { Name = "Glass Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 38, Weight = 7.0, Value = 900 });
            ArmorDatabase.Add(new Armor { Name = "Glass Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 13, Weight = 2.0, Value = 190 });
            ArmorDatabase.Add(new Armor { Name = "Glass Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 13, Weight = 2.0, Value = 190 });
            ArmorDatabase.Add(new Armor { Name = "Glass Helmet", Slot = "Head", Category = "Light Armor", ArmorRating = 18, Weight = 2.0, Value = 450 });

            ArmorDatabase.Add(new Armor { Name = "Dragonscale Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 41, Weight = 10.0, Value = 1500 });
            ArmorDatabase.Add(new Armor { Name = "Dragonscale Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 14, Weight = 3.0, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Dragonscale Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 14, Weight = 3.0, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Dragonscale Helmet", Slot = "Head", Category = "Light Armor", ArmorRating = 19, Weight = 4.0, Value = 750 });

            // ==========================================
            // LIGHT ARMOR - UNIQUES & FACTIONS
            // ==========================================

            // Thieves Guild (Standard)
            ArmorDatabase.Add(new Armor { Name = "Thieves Guild Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 28, Weight = 5.0, Value = 398, Effect = "+20 Carry Weight" });
            ArmorDatabase.Add(new Armor { Name = "Thieves Guild Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 8, Weight = 1.5, Value = 135, Effect = "Pickpocket 15% better" });
            ArmorDatabase.Add(new Armor { Name = "Thieves Guild Gloves", Slot = "Hands", Category = "Light Armor", ArmorRating = 8, Weight = 1.5, Value = 135, Effect = "Lockpicking 15% easier" });
            ArmorDatabase.Add(new Armor { Name = "Thieves Guild Hood", Slot = "Head", Category = "Light Armor", ArmorRating = 13, Weight = 1.5, Value = 194, Effect = "Prices 10% better" });

            // Dark Brotherhood
            ArmorDatabase.Add(new Armor { Name = "Shrouded Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 29, Weight = 7.0, Value = 464, Effect = "Resist Poison 50%" });
            ArmorDatabase.Add(new Armor { Name = "Shrouded Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 8, Weight = 2.0, Value = 155, Effect = "Muffle" });
            ArmorDatabase.Add(new Armor { Name = "Shrouded Gloves", Slot = "Hands", Category = "Light Armor", ArmorRating = 8, Weight = 2.0, Value = 155, Effect = "Double Sneak Attack Dmg with 1H" });
            ArmorDatabase.Add(new Armor { Name = "Shrouded Cowl", Slot = "Head", Category = "Light Armor", ArmorRating = 13, Weight = 2.0, Value = 227, Effect = "Bows do 20% more damage" });

            // Artifacts
            ArmorDatabase.Add(new Armor { Name = "Savior's Hide", Slot = "Chest", Category = "Light Armor", ArmorRating = 26, Weight = 6.0, Value = 2679, Effect = "Magic Resist 15%, Poison Resist 50%" });
            ArmorDatabase.Add(new Armor { Name = "Deathbrand Armor (DB)", Slot = "Chest", Category = "Light Armor", ArmorRating = 39, Weight = 7.0, Value = 2616, Effect = "+15 Stamina per Deathbrand item worn" });
            ArmorDatabase.Add(new Armor { Name = "Ancient Falmer Armor (DG)", Slot = "Chest", Category = "Light Armor", ArmorRating = 38, Weight = 7.0, Value = 900 });

            // ==========================================
            // LIGHT ARMOR - LEVELED ITEMS
            // ==========================================

            ArmorDatabase.Add(new Armor
            {
                Name = "Nightingale Armor",
                Slot = "Chest",
                Category = "Light Armor",
                Weight = 9.0,
                LevelVariants = new List<LeveledStat>
                {
                    new LeveledStat { MinLevel = 1, ArmorRating = 26, Value = 645, Effect = "+20 Stamina, Frost Resist 15%" },
                    new LeveledStat { MinLevel = 19, ArmorRating = 30, Value = 1115, Effect = "+30 Stamina, Frost Resist 30%" },
                    new LeveledStat { MinLevel = 32, ArmorRating = 34, Value = 1750, Effect = "+40 Stamina, Frost Resist 50%" }
                }
            });

            ArmorDatabase.Add(new Armor
            {
                Name = "Miraak (Light) (DB)",
                Slot = "Head",
                Category = "Light Armor",
                Weight = 2.0,
                LevelVariants = new List<LeveledStat>
                {
                    new LeveledStat { MinLevel = 1, ArmorRating = 13, Value = 475, Effect = "+40 Magicka" },
                    new LeveledStat { MinLevel = 45, ArmorRating = 15, Value = 535, Effect = "+60 Magicka" },
                    new LeveledStat { MinLevel = 60, ArmorRating = 17, Value = 635, Effect = "+70 Magicka" }
                }
            });

            // ==========================================
            // LIGHT ARMOR - STALHRIM & CHITIN (DB)
            // ==========================================

            // Stalhrim Light
            ArmorDatabase.Add(new Armor { Name = "Stalhrim Light Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 39, Weight = 7.0, Value = 1825 });
            ArmorDatabase.Add(new Armor { Name = "Stalhrim Light Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 13, Weight = 2.0, Value = 360 });
            ArmorDatabase.Add(new Armor { Name = "Stalhrim Light Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 13, Weight = 2.0, Value = 360 });
            ArmorDatabase.Add(new Armor { Name = "Stalhrim Light Helmet", Slot = "Head", Category = "Light Armor", ArmorRating = 18, Weight = 2.0, Value = 910 });

            // Chitin Light
            ArmorDatabase.Add(new Armor { Name = "Chitin Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 29, Weight = 4.0, Value = 225 });
            ArmorDatabase.Add(new Armor { Name = "Chitin Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 10, Weight = 1.0, Value = 45 });
            ArmorDatabase.Add(new Armor { Name = "Chitin Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 10, Weight = 1.0, Value = 45 });
            ArmorDatabase.Add(new Armor { Name = "Chitin Helmet", Slot = "Head", Category = "Light Armor", ArmorRating = 15, Weight = 1.0, Value = 110 });

            // ==========================================
            // LIGHT ARMOR - VAMPIRE & DAWNGUARD (DG)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Vampire Royal Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 30, Weight = 5.0, Value = 1250, Effect = "Magicka Regen 125% faster." });
            ArmorDatabase.Add(new Armor { Name = "Vampire Armor", Slot = "Chest", Category = "Light Armor", ArmorRating = 24, Weight = 4.0, Value = 175 });
            ArmorDatabase.Add(new Armor { Name = "Vampire Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 6, Weight = 1.0, Value = 30 });
            ArmorDatabase.Add(new Armor { Name = "Vampire Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 6, Weight = 1.0, Value = 30 });

            ArmorDatabase.Add(new Armor { Name = "Dawnguard Armor (Light)", Slot = "Chest", Category = "Light Armor", ArmorRating = 31, Weight = 6.0, Value = 250 });
            ArmorDatabase.Add(new Armor { Name = "Dawnguard Boots", Slot = "Feet", Category = "Light Armor", ArmorRating = 11, Weight = 2.0, Value = 45 });
            ArmorDatabase.Add(new Armor { Name = "Dawnguard Gauntlets", Slot = "Hands", Category = "Light Armor", ArmorRating = 11, Weight = 2.0, Value = 45 });

            // ==========================================
            // LIGHT ARMOR - ANNIVERSARY EDITION (AE)
            // ==========================================

            // Amber Armor (Bernstein)
            ArmorDatabase.Add(new Armor { Name = "Amber Armor (AE)", Slot = "Chest", Category = "Light Armor", ArmorRating = 40, Weight = 6.0, Value = 1700 });
            ArmorDatabase.Add(new Armor { Name = "Amber Boots (AE)", Slot = "Feet", Category = "Light Armor", ArmorRating = 14, Weight = 2.0, Value = 340 });
            ArmorDatabase.Add(new Armor { Name = "Amber Gauntlets (AE)", Slot = "Hands", Category = "Light Armor", ArmorRating = 14, Weight = 2.0, Value = 340 });
            ArmorDatabase.Add(new Armor { Name = "Amber Helmet (AE)", Slot = "Head", Category = "Light Armor", ArmorRating = 19, Weight = 2.0, Value = 850 });
            ArmorDatabase.Add(new Armor { Name = "Amber Shield (AE)", Slot = "Shield", Category = "Shield", ArmorRating = 28, Weight = 5.0, Value = 850 });

            // Dark Armor
            ArmorDatabase.Add(new Armor { Name = "Dark Armor (AE)", Slot = "Chest", Category = "Light Armor", ArmorRating = 36, Weight = 7.0, Value = 750 });
            ArmorDatabase.Add(new Armor { Name = "Dark Boots (AE)", Slot = "Feet", Category = "Light Armor", ArmorRating = 12, Weight = 2.0, Value = 150 });
            ArmorDatabase.Add(new Armor { Name = "Dark Gauntlets (AE)", Slot = "Hands", Category = "Light Armor", ArmorRating = 12, Weight = 2.0, Value = 150 });
            ArmorDatabase.Add(new Armor { Name = "Dark Helmet (AE)", Slot = "Head", Category = "Light Armor", ArmorRating = 17, Weight = 2.0, Value = 375 });

            // ==========================================
            // LIGHT ARMOR - ALTERNATIVE ARMORS (AE)
            // ==========================================

            // Daedric Mail
            ArmorDatabase.Add(new Armor { Name = "Daedric Mail Armor (AE)", Slot = "Chest", Category = "Light Armor", ArmorRating = 38, Weight = 10.0, Value = 1500 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Mail Boots (AE)", Slot = "Feet", Category = "Light Armor", ArmorRating = 13, Weight = 3.0, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Mail Gauntlets (AE)", Slot = "Hands", Category = "Light Armor", ArmorRating = 13, Weight = 3.0, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Daedric Mail Helmet (AE)", Slot = "Head", Category = "Light Armor", ArmorRating = 18, Weight = 4.0, Value = 750 });

            // Dragonbone Mail
            ArmorDatabase.Add(new Armor { Name = "Dragonbone Mail Armor (AE)", Slot = "Chest", Category = "Light Armor", ArmorRating = 41, Weight = 15.0, Value = 2000, Effect = "Resist Fire 100%." });

            // Studded Dragonscale
            ArmorDatabase.Add(new Armor { Name = "Studded Dragonscale Armor (AE)", Slot = "Chest", Category = "Light Armor", ArmorRating = 44, Weight = 10.0, Value = 1800 });

            // ==========================================
            // LIGHT ARMOR - DEATHBRAND SET (DB)
            // ==========================================

            ArmorDatabase.Add(new Armor { Name = "Deathbrand Boots (DB)", Slot = "Feet", Category = "Light Armor", ArmorRating = 11, Weight = 2.0, Value = 413, Effect = "+10 Carry Weight per Deathbrand item worn." });
            ArmorDatabase.Add(new Armor { Name = "Deathbrand Gauntlets (DB)", Slot = "Hands", Category = "Light Armor", ArmorRating = 11, Weight = 2.0, Value = 413, Effect = "One-Handed 10% more damage per Deathbrand item worn." });
            ArmorDatabase.Add(new Armor { Name = "Deathbrand Helm (DB)", Slot = "Head", Category = "Light Armor", ArmorRating = 16, Weight = 2.0, Value = 1007, Effect = "Waterbreathing, Armor bonus per Deathbrand item worn." });

            // ==========================================
            // CLOTHING - MAGE ROBES (BASE VARIANTS)
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Novice Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 45, Effect = "Magicka Regen +50%." });
            ArmorDatabase.Add(new Armor { Name = "Apprentice Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 125, Effect = "Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Adept Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 400, Effect = "Magicka Regen +100%." });
            ArmorDatabase.Add(new Armor { Name = "Expert Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 900, Effect = "Magicka Regen +125%." });
            ArmorDatabase.Add(new Armor { Name = "Master Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1800, Effect = "Magicka Regen +150%." });

            // ==========================================
            // CLOTHING - SCHOOL SPECIFIC ROBES
            // ==========================================
            // Destruction
            ArmorDatabase.Add(new Armor { Name = "Novice Robes of Destruction", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 145, Effect = "Destruction spells cost 12% less, Magicka Regen +50%." });
            ArmorDatabase.Add(new Armor { Name = "Apprentice Robes of Destruction", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 310, Effect = "Destruction spells cost 15% less, Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Adept Robes of Destruction", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 750, Effect = "Destruction spells cost 17% less, Magicka Regen +100%." });
            ArmorDatabase.Add(new Armor { Name = "Expert Robes of Destruction", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1400, Effect = "Destruction spells cost 20% less, Magicka Regen +125%." });
            ArmorDatabase.Add(new Armor { Name = "Master Robes of Destruction", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2300, Effect = "Destruction spells cost 22% less, Magicka Regen +150%." });

            // Restoration
            ArmorDatabase.Add(new Armor { Name = "Novice Robes of Restoration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 145, Effect = "Restoration spells cost 12% less, Magicka Regen +50%." });
            ArmorDatabase.Add(new Armor { Name = "Apprentice Robes of Restoration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 310, Effect = "Restoration spells cost 15% less, Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Adept Robes of Restoration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 750, Effect = "Restoration spells cost 17% less, Magicka Regen +100%." });
            ArmorDatabase.Add(new Armor { Name = "Expert Robes of Restoration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1400, Effect = "Restoration spells cost 20% less, Magicka Regen +125%." });
            ArmorDatabase.Add(new Armor { Name = "Master Robes of Restoration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2300, Effect = "Restoration spells cost 22% less, Magicka Regen +150%." });

            // Alteration
            ArmorDatabase.Add(new Armor { Name = "Novice Robes of Alteration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 145, Effect = "Alteration spells cost 12% less, Magicka Regen +50%." });
            ArmorDatabase.Add(new Armor { Name = "Apprentice Robes of Alteration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 310, Effect = "Alteration spells cost 15% less, Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Adept Robes of Alteration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 750, Effect = "Alteration spells cost 17% less, Magicka Regen +100%." });
            ArmorDatabase.Add(new Armor { Name = "Expert Robes of Alteration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1400, Effect = "Alteration spells cost 20% less, Magicka Regen +125%." });
            ArmorDatabase.Add(new Armor { Name = "Master Robes of Alteration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2300, Effect = "Alteration spells cost 22% less, Magicka Regen +150%." });

            // Conjuration
            ArmorDatabase.Add(new Armor { Name = "Novice Robes of Conjuration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 145, Effect = "Conjuration spells cost 12% less, Magicka Regen +50%." });
            ArmorDatabase.Add(new Armor { Name = "Apprentice Robes of Conjuration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 310, Effect = "Conjuration spells cost 15% less, Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Adept Robes of Conjuration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 750, Effect = "Conjuration spells cost 17% less, Magicka Regen +100%." });
            ArmorDatabase.Add(new Armor { Name = "Expert Robes of Conjuration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1400, Effect = "Conjuration spells cost 20% less, Magicka Regen +125%." });
            ArmorDatabase.Add(new Armor { Name = "Master Robes of Conjuration", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2300, Effect = "Conjuration spells cost 22% less, Magicka Regen +150%." });

            // Illusion
            ArmorDatabase.Add(new Armor { Name = "Novice Robes of Illusion", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 145, Effect = "Illusion spells cost 12% less, Magicka Regen +50%." });
            ArmorDatabase.Add(new Armor { Name = "Apprentice Robes of Illusion", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 310, Effect = "Illusion spells cost 15% less, Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Adept Robes of Illusion", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 750, Effect = "Illusion spells cost 17% less, Magicka Regen +100%." });
            ArmorDatabase.Add(new Armor { Name = "Expert Robes of Illusion", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1400, Effect = "Illusion spells cost 20% less, Magicka Regen +125%." });
            ArmorDatabase.Add(new Armor { Name = "Master Robes of Illusion", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2300, Effect = "Illusion spells cost 22% less, Magicka Regen +150%." });

            // ==========================================
            // CLOTHING - UNIQUE ROBES & SPECIALS
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Arch-Mage's Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 3000, Effect = "All spells cost 15% less, +50 Magicka, Magicka Regen +100%." });
            ArmorDatabase.Add(new Armor { Name = "Miraak's Robes (DB)", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1250, Effect = "Absorb 15% Magicka from breath/spells." });
            ArmorDatabase.Add(new Armor { Name = "Telvanni Robes (DB)", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1115, Effect = "Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Temple Priest Robes (DB)", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 55, Effect = "Restoration spells cost 12% less." });
            ArmorDatabase.Add(new Armor { Name = "Tunic of the Unmourned (AE)", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 550, Effect = "Health/Stamina Regen +15%." });

            // ==========================================
            // CLOTHING - FACTION SETS (FULL)
            // ==========================================
            // Thalmor Set
            ArmorDatabase.Add(new Armor { Name = "Thalmor Robes (Hooded)", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 175, Effect = "Destruction spells cost 12% less." });
            ArmorDatabase.Add(new Armor { Name = "Thalmor Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 150, Effect = "Destruction spells cost 12% less." });
            ArmorDatabase.Add(new Armor { Name = "Thalmor Gloves", Slot = "Hands", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 5 });
            ArmorDatabase.Add(new Armor { Name = "Thalmor Boots", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 5 });

            // Psijic Order Set
            ArmorDatabase.Add(new Armor { Name = "Psijic Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 100 });
            ArmorDatabase.Add(new Armor { Name = "Psijic Hood", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 50 });
            ArmorDatabase.Add(new Armor { Name = "Psijic Gloves", Slot = "Hands", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 25 });
            ArmorDatabase.Add(new Armor { Name = "Psijic Boots", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 25 });

            // Mythic Dawn Set
            ArmorDatabase.Add(new Armor { Name = "Mythic Dawn Robes (Enchanted)", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 75, Effect = "Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Mythic Dawn Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 25 });
            ArmorDatabase.Add(new Armor { Name = "Mythic Dawn Boots", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 5 });

            // Cultist Set (Dragonborn)
            ArmorDatabase.Add(new Armor { Name = "Cultist Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 250, Effect = "Magicka Regen +75%." });
            ArmorDatabase.Add(new Armor { Name = "Cultist Boots", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 10 });
            ArmorDatabase.Add(new Armor { Name = "Cultist Gloves", Slot = "Hands", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 10 });
            ArmorDatabase.Add(new Armor { Name = "Cultist Mask", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 50 });

            // Skaal Set (Dragonborn)
            ArmorDatabase.Add(new Armor { Name = "Skaal Coat", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 25 });
            ArmorDatabase.Add(new Armor { Name = "Skaal Boots", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 10 });
            ArmorDatabase.Add(new Armor { Name = "Skaal Gloves", Slot = "Hands", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 10 });
            ArmorDatabase.Add(new Armor { Name = "Skaal Hat", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 10 });

            // ==========================================
            // CLOTHING - CIVILIAN (BASE & UNIQUE)
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Clothes (Common)", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1 });
            ArmorDatabase.Add(new Armor { Name = "Fine Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 25 });
            ArmorDatabase.Add(new Armor { Name = "Merchant's Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 10 });
            ArmorDatabase.Add(new Armor { Name = "Barkeeper's Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 5 });
            ArmorDatabase.Add(new Armor { Name = "Chef's Tunic", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 5 });
            ArmorDatabase.Add(new Armor { Name = "Chef's Hat", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 2 });
            ArmorDatabase.Add(new Armor { Name = "Emperor's Robes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 150, Effect = "The robes of Titus Mede II." });
            ArmorDatabase.Add(new Armor { Name = "Ulfric's Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 50 });
            ArmorDatabase.Add(new Armor { Name = "Wedding Dress", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 100 });
            ArmorDatabase.Add(new Armor { Name = "Party Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 100, Effect = "Elegant embassy attire." });

            // Cicero & Jester
            ArmorDatabase.Add(new Armor { Name = "Cicero's Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1455, Effect = "Prices 20% better, One-Handed +20%." });
            ArmorDatabase.Add(new Armor { Name = "Cicero's Hat", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 227, Effect = "Sneak 35% better." });
            ArmorDatabase.Add(new Armor { Name = "Cicero's Gloves", Slot = "Hands", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 155, Effect = "Double Sneak Attack damage (1H)." });
            ArmorDatabase.Add(new Armor { Name = "Cicero's Boots", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 155, Effect = "Muffle." });
            ArmorDatabase.Add(new Armor { Name = "Jester's Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 464, Effect = "Prices 10% better, One-Handed +12%." });

            // ==========================================
            // CLOTHING - ACCESSORIES & BASE ITEMS
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Shoes", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2 });
            ArmorDatabase.Add(new Armor { Name = "Fine Boots", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 15 });
            ArmorDatabase.Add(new Armor { Name = "Gloves", Slot = "Hands", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 2 });
            ArmorDatabase.Add(new Armor { Name = "Hat", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 2 });
            ArmorDatabase.Add(new Armor { Name = "Mage Hood", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 50, Effect = "+30 Magicka." });

            // ==========================================
            // CLOTHING - ADDITIONAL CIVILIAN & VARIANTS
            // ==========================================
            // Zusätzliche Fußbekleidung
            ArmorDatabase.Add(new Armor { Name = "Boots (Brown)", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2 });
            ArmorDatabase.Add(new Armor { Name = "Boots (Black)", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2 });
            ArmorDatabase.Add(new Armor { Name = "Pleated Shoes", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 5 });
            ArmorDatabase.Add(new Armor { Name = "Cuffed Boots", Slot = "Feet", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 2 });

            // Zusätzliche Kopfbedeckungen
            ArmorDatabase.Add(new Armor { Name = "Fine Hat (Elegant)", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 15 });
            ArmorDatabase.Add(new Armor { Name = "Cowl", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 5 });
            ArmorDatabase.Add(new Armor { Name = "Mourner's Hat", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.5, Value = 5 });

            // Zusätzliche Körperkleidung (Varianten)
            ArmorDatabase.Add(new Armor { Name = "Embroidered Garment", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 20 });
            ArmorDatabase.Add(new Armor { Name = "Mourner's Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 5 });
            ArmorDatabase.Add(new Armor { Name = "Blacksmith's Apron", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 15 });
            ArmorDatabase.Add(new Armor { Name = "Beggar Clothes", Slot = "Chest", Category = "Clothing", ArmorRating = 0, Weight = 1.0, Value = 1 });

            // ==========================================
            // CLOTHING - ALL CIRCLETS (REIFE)
            // ==========================================
            ArmorDatabase.Add(new Armor { Name = "Copper and Moonstone Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 100 });
            ArmorDatabase.Add(new Armor { Name = "Copper and Onyx Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 50 });
            ArmorDatabase.Add(new Armor { Name = "Copper and Sapphire Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 150 });
            ArmorDatabase.Add(new Armor { Name = "Silver and Moonstone Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 250 });
            ArmorDatabase.Add(new Armor { Name = "Silver and Sapphire Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Gold and Emerald Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 600 });
            ArmorDatabase.Add(new Armor { Name = "Gold and Ruby Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 500 });
            ArmorDatabase.Add(new Armor { Name = "Jade and Emerald Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 300 });
            ArmorDatabase.Add(new Armor { Name = "Jade and Sapphire Circlet", Slot = "Head", Category = "Clothing", ArmorRating = 0, Weight = 0.3, Value = 350 });

            // ==========================================
            // JEWELRY - RINGS, AMULETS & CIRCLETS
            // ==========================================

            // Base Rings & Necklaces
            ArmorDatabase.Add(new Armor { Name = "Silver Ring", Slot = "Ring", Category = "Jewelry", ArmorRating = 0, Weight = 0.1, Value = 30 });
            ArmorDatabase.Add(new Armor { Name = "Gold Ring", Slot = "Ring", Category = "Jewelry", ArmorRating = 0, Weight = 0.1, Value = 75 });
            ArmorDatabase.Add(new Armor { Name = "Gold Diamond Ring", Slot = "Ring", Category = "Jewelry", ArmorRating = 0, Weight = 0.1, Value = 900 });
            ArmorDatabase.Add(new Armor { Name = "Silver Sapphire Necklace", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Gold Necklace", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 120 });

            // Amulets of the Nine Divines
            ArmorDatabase.Add(new Armor { Name = "Amulet of Akatosh", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "Magicka Regen +25%." });
            ArmorDatabase.Add(new Armor { Name = "Amulet of Arkay", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "+10 Health." });
            ArmorDatabase.Add(new Armor { Name = "Amulet of Dibella", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "+15 Speech." });
            ArmorDatabase.Add(new Armor { Name = "Amulet of Julianos", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "+10 Magicka." });
            ArmorDatabase.Add(new Armor { Name = "Amulet of Kynareth", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "+10 Stamina." });
            ArmorDatabase.Add(new Armor { Name = "Amulet of Mara", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "Restoration spells cost 10% less." });
            ArmorDatabase.Add(new Armor { Name = "Amulet of Stendarr", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "Block 10% better." });
            ArmorDatabase.Add(new Armor { Name = "Amulet of Talos", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "Shout cooldown reduced 20%." });
            ArmorDatabase.Add(new Armor { Name = "Amulet of Zenithar", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 15, Effect = "Prices 10% better." });

            // Artifact Jewelry
            ArmorDatabase.Add(new Armor { Name = "The Gauldur Amulet", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 1864, Effect = "+30 Health, Magicka, and Stamina." });
            ArmorDatabase.Add(new Armor { Name = "Savos Aren's Amulet", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 757, Effect = "+50 Magicka." });
            ArmorDatabase.Add(new Armor { Name = "Ring of Namira", Slot = "Ring", Category = "Jewelry", ArmorRating = 0, Weight = 0.1, Value = 870, Effect = "+50 Stamina, Cannibalism." });
            ArmorDatabase.Add(new Armor { Name = "Ring of Hircine", Slot = "Ring", Category = "Jewelry", ArmorRating = 0, Weight = 0.1, Value = 1100, Effect = "Extra Werewolf transformations." });
            ArmorDatabase.Add(new Armor { Name = "Ahzidal's Ring of Arcana", Slot = "Ring", Category = "Jewelry", ArmorRating = 0, Weight = 0.1, Value = 1100, Effect = "Grants Ignite and Freeze spells." });
            ArmorDatabase.Add(new Armor { Name = "Locket of Saint Jiub", Slot = "Necklace", Category = "Jewelry", ArmorRating = 0, Weight = 0.5, Value = 1250, Effect = "+50 Carry Weight & Stamina." });

            // Circlets (Reife)
            ArmorDatabase.Add(new Armor { Name = "Copper and Moonstone Circlet", Slot = "Head", Category = "Jewelry", ArmorRating = 0, Weight = 0.3, Value = 100 });
            ArmorDatabase.Add(new Armor { Name = "Silver Sapphire Circlet", Slot = "Head", Category = "Jewelry", ArmorRating = 0, Weight = 0.3, Value = 400 });
            ArmorDatabase.Add(new Armor { Name = "Gold and Emerald Circlet", Slot = "Head", Category = "Jewelry", ArmorRating = 0, Weight = 0.3, Value = 600 });

            // Leveled Mage's Circlet
            ArmorDatabase.Add(new Armor
            {
                Name = "Mage's Circlet",
                Slot = "Head",
                Category = "Jewelry",
                Weight = 0.3,
                LevelVariants = new List<LeveledStat>
                {
            new LeveledStat { MinLevel = 1, Value = 500, Effect = "+20 Magicka" },
            new LeveledStat { MinLevel = 15, Value = 700, Effect = "+40 Magicka" },
            new LeveledStat { MinLevel = 25, Value = 1000, Effect = "+70 Magicka" }
                }
            });

            // SORTIERUNG AM ENDE
            ArmorDatabase = ArmorDatabase.OrderBy(a => a.Name).ToList();

        }

        private void LoadDifficultyData()
        {
            DifficultyDatabase.Clear();
            // Dealt: Schaden den DU machst | Taken: Schaden den GEGNER machen
            DifficultyDatabase.Add(new Difficulty { Name = "Novice", DamageDealtMultiplier = 2.0, DamageTakenMultiplier = 0.5 });
            DifficultyDatabase.Add(new Difficulty { Name = "Apprentice", DamageDealtMultiplier = 1.5, DamageTakenMultiplier = 0.75 });
            DifficultyDatabase.Add(new Difficulty { Name = "Adept", DamageDealtMultiplier = 1.0, DamageTakenMultiplier = 1.0 });
            DifficultyDatabase.Add(new Difficulty { Name = "Expert", DamageDealtMultiplier = 0.75, DamageTakenMultiplier = 1.5 });
            DifficultyDatabase.Add(new Difficulty { Name = "Master", DamageDealtMultiplier = 0.5, DamageTakenMultiplier = 2.0 });
            DifficultyDatabase.Add(new Difficulty { Name = "Legendary", DamageDealtMultiplier = 0.25, DamageTakenMultiplier = 3.0 });
        }

        private void LoadEnchantmentData()
        {
            EnchantmentDatabase.Clear();

            // Alle möglichen Slots für den "None"-Eintrag definieren
            var allSlots = new List<string> { "Weapon", "Head", "Chest", "Hands", "Feet", "Shield", "Necklace", "Ring" };

            // "None" bekommt jetzt alle Slots, damit er nie aus dem Filter fliegt
            EnchantmentDatabase.Add(new Enchantment
            {
                Name = "None",
                AddedValue = 0,
                Description = "No enchantment",
                CompatibleSlots = allSlots
            });

            // ==========================================
            // WEAPON ENCHANTMENTS (Slot: "Weapon")
            // ==========================================
            var weaponOnly = new List<string> { "Weapon" };

            EnchantmentDatabase.Add(new Enchantment { Name = "Fire Damage", AddedValue = 10, CompatibleSlots = weaponOnly, Description = "Burns the target for {0} points." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Frost Damage", AddedValue = 10, CompatibleSlots = weaponOnly, Description = "Deals {0} points of frost damage to Health and Stamina." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Shock Damage", AddedValue = 10, CompatibleSlots = weaponOnly, Description = "Deals {0} points of shock damage to Health and half as much to Magicka." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Chaos Damage (DB)", AddedValue = 15, CompatibleSlots = weaponOnly, Description = "50% chance for each element to do {0} damage." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Magicka Damage", AddedValue = 15, CompatibleSlots = weaponOnly, Description = "Does {0} points of Magicka damage." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Stamina Damage", AddedValue = 15, CompatibleSlots = weaponOnly, Description = "Does {0} points of Stamina damage." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Absorb Health", AddedValue = 8, CompatibleSlots = weaponOnly, Description = "Absorbs {0} points of Health." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Absorb Magicka", AddedValue = 10, CompatibleSlots = weaponOnly, Description = "Absorbs {0} points of Magicka." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Absorb Stamina", AddedValue = 10, CompatibleSlots = weaponOnly, Description = "Absorbs {0} points of Stamina." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Soul Trap", AddedValue = 5, CompatibleSlots = weaponOnly, Description = "Fills a soul gem if the target dies within {0} seconds." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Paralyze", AddedValue = 2, CompatibleSlots = weaponOnly, Description = "Chance to paralyze the target for {0} seconds." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Banish", AddedValue = 0, CompatibleSlots = weaponOnly, Description = "Summoned Daedra up to level {0} are sent back to Oblivion." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Turn Undead", AddedValue = 0, CompatibleSlots = weaponOnly, Description = "Undead up to level {0} flee for 30 seconds." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fear", AddedValue = 0, CompatibleSlots = weaponOnly, Description = "Creatures and people up to level {0} flee for 30 seconds." });

            // ==========================================
            // APPAREL ENCHANTMENTS (According to UESP)
            // ==========================================

            // Fortify Attributes
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Health", AddedValue = 20, CompatibleSlots = new List<string> { "Chest", "Shield", "Necklace", "Ring" }, Description = "Increases Health by {0} points." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Magicka", AddedValue = 20, CompatibleSlots = new List<string> { "Head", "Hands", "Necklace", "Ring" }, Description = "Increases Magicka by {0} points." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Stamina", AddedValue = 20, CompatibleSlots = new List<string> { "Chest", "Feet", "Necklace", "Ring" }, Description = "Increases Stamina by {0} points." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Carry Weight", AddedValue = 15, CompatibleSlots = new List<string> { "Feet", "Hands", "Necklace", "Ring" }, Description = "Carrying capacity increased by {0} points." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Unarmed", AddedValue = 5, CompatibleSlots = new List<string> { "Hands", "Ring" }, Description = "Unarmed strikes do {0} additional damage." });

            // Fortify Combat Skills
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Archery", AddedValue = 15, CompatibleSlots = new List<string> { "Head", "Hands", "Necklace", "Ring" }, Description = "Bows do {0}% more damage." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify One-Handed", AddedValue = 15, CompatibleSlots = new List<string> { "Hands", "Feet", "Necklace", "Ring" }, Description = "One-handed weapons do {0}% more damage." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Two-Handed", AddedValue = 15, CompatibleSlots = new List<string> { "Hands", "Feet", "Necklace", "Ring" }, Description = "Two-handed weapons do {0}% more damage." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Block", AddedValue = 15, CompatibleSlots = new List<string> { "Hands", "Shield", "Necklace", "Ring" }, Description = "Blocks are {0}% more effective." });

            // Fortify Stealth & Utility Skills
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Sneak", AddedValue = 15, CompatibleSlots = new List<string> { "Feet", "Hands", "Necklace", "Ring" }, Description = "Sneaking is {0}% better." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Lockpicking", AddedValue = 15, CompatibleSlots = new List<string> { "Head", "Hands", "Necklace", "Ring" }, Description = "Lockpicking is {0}% easier." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Pickpocket", AddedValue = 15, CompatibleSlots = new List<string> { "Hands", "Feet", "Necklace", "Ring" }, Description = "Pickpocketing is {0}% easier." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Barter", AddedValue = 10, CompatibleSlots = new List<string> { "Necklace" }, Description = "Prices are {0}% better." });

            // Magic School Cost Reduction
            var magicSchools = new List<string> { "Head", "Chest", "Necklace", "Ring" };
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Alteration", AddedValue = 12, CompatibleSlots = magicSchools, Description = "Alteration spells cost {0}% less to cast." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Conjuration", AddedValue = 12, CompatibleSlots = magicSchools, Description = "Conjuration spells cost {0}% less to cast." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Destruction", AddedValue = 12, CompatibleSlots = magicSchools, Description = "Destruction spells cost {0}% less to cast." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Illusion", AddedValue = 12, CompatibleSlots = magicSchools, Description = "Illusion spells cost {0}% less to cast." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Restoration", AddedValue = 12, CompatibleSlots = magicSchools, Description = "Restoration spells cost {0}% less to cast." });

            // Resistances
            var elementalResist = new List<string> { "Chest", "Feet", "Shield", "Necklace", "Ring" };
            EnchantmentDatabase.Add(new Enchantment { Name = "Resist Fire", AddedValue = 15, CompatibleSlots = elementalResist, Description = "Increases Fire Resistance by {0}%." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Resist Frost", AddedValue = 15, CompatibleSlots = elementalResist, Description = "Increases Frost Resistance by {0}%." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Resist Shock", AddedValue = 15, CompatibleSlots = elementalResist, Description = "Increases Shock Resistance by {0}%." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Resist Magic", AddedValue = 8, CompatibleSlots = new List<string> { "Shield", "Necklace", "Ring" }, Description = "Increases Magic Resistance by {0}%." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Resist Poison", AddedValue = 50, CompatibleSlots = new List<string> { "Chest", "Necklace", "Ring" }, Description = "Increases Poison Resistance by {0}%." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Resist Disease", AddedValue = 50, CompatibleSlots = new List<string> { "Chest", "Necklace", "Ring" }, Description = "Increases Disease Resistance by {0}%." });

            // Regeneration & Misc
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Magicka Regen", AddedValue = 20, CompatibleSlots = new List<string> { "Head", "Chest", "Ring" }, Description = "Magicka regenerates {0}% faster." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Stamina Regen", AddedValue = 20, CompatibleSlots = new List<string> { "Chest", "Feet", "Necklace" }, Description = "Stamina regenerates {0}% faster." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Fortify Health Regen", AddedValue = 20, CompatibleSlots = new List<string> { "Chest", "Necklace", "Ring" }, Description = "Health regenerates {0}% faster." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Waterbreathing", AddedValue = 0, CompatibleSlots = new List<string> { "Head", "Necklace", "Ring" }, Description = "Can breathe under water." });
            EnchantmentDatabase.Add(new Enchantment { Name = "Muffle", AddedValue = 0, CompatibleSlots = new List<string> { "Feet" }, Description = "Wearer moves silently." });
        }

        private void LoadSoulGemData()
        {
            SoulGemDatabase.Clear();
            // Wir fügen "None" mit Multiplikator 0 hinzu
            SoulGemDatabase.Add(new SoulGem { Name = "None", Multiplier = 0.0 });

            // Standard Seelensteine
            SoulGemDatabase.Add(new SoulGem { Name = "Petty", Multiplier = 0.1 });
            SoulGemDatabase.Add(new SoulGem { Name = "Lesser", Multiplier = 0.25 });
            SoulGemDatabase.Add(new SoulGem { Name = "Common", Multiplier = 0.5 });
            SoulGemDatabase.Add(new SoulGem { Name = "Greater", Multiplier = 0.75 });
            SoulGemDatabase.Add(new SoulGem { Name = "Grand", Multiplier = 1.0 });
            SoulGemDatabase.Add(new SoulGem { Name = "Black", Multiplier = 1.0 });

            // NEU: Die daedrischen Artefakte (Azura)
            SoulGemDatabase.Add(new SoulGem { Name = "Azura's Star", Multiplier = 1.0 });
            SoulGemDatabase.Add(new SoulGem { Name = "The Black Star", Multiplier = 1.0 });
        }

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

    }
}