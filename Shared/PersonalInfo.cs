namespace PKHeX.EncounterSlotDumper
{
    /// <summary>
    /// Stat/misc data for individual species or their associated alternate forme data.
    /// </summary>
    public abstract class PersonalInfo
    {
        /// <summary>
        /// Raw Data
        /// </summary>
        protected readonly byte[] Data;

        protected PersonalInfo(byte[] data) => Data = data;

        /// <summary>
        /// Writes entry to raw bytes.
        /// </summary>
        public abstract byte[] Write();

        /// <summary>
        /// Base HP
        /// </summary>
        public abstract int HP { get; set; }

        /// <summary>
        /// Base Attack
        /// </summary>
        public abstract int ATK { get; set; }

        /// <summary>
        /// Base Defense
        /// </summary>
        public abstract int DEF { get; set; }

        /// <summary>
        /// Base Speed
        /// </summary>
        public abstract int SPE { get; set; }

        /// <summary>
        /// Base Special Attack
        /// </summary>
        public abstract int SPA { get; set; }

        /// <summary>
        /// Base Special Defense
        /// </summary>
        public abstract int SPD { get; set; }

        /// <summary>
        /// Base Stat values
        /// </summary>
        public int[] Stats
        {
            get => new[] { HP, ATK, DEF, SPE, SPA, SPD };
            set
            {
                HP = value[0];
                ATK = value[1];
                DEF = value[2];
                SPE = value[3];
                SPA = value[4];
                SPD = value[5];
            }
        }

        /// <summary>
        /// Amount of HP Effort Values to yield when defeating this entry.
        /// </summary>
        public abstract int EV_HP { get; set; }

        /// <summary>
        /// Amount of Attack Effort Values to yield when defeating this entry.
        /// </summary>
        public abstract int EV_ATK { get; set; }

        /// <summary>
        /// Amount of Defense Effort Values to yield when defeating this entry.
        /// </summary>
        public abstract int EV_DEF { get; set; }

        /// <summary>
        /// Amount of Speed Effort Values to yield when defeating this entry.
        /// </summary>
        public abstract int EV_SPE { get; set; }

        /// <summary>
        /// Amount of Special Attack Effort Values to yield when defeating this entry.
        /// </summary>
        public abstract int EV_SPA { get; set; }

        /// <summary>
        /// Amount of Special Defense Effort Values to yield when defeating this entry.
        /// </summary>
        public abstract int EV_SPD { get; set; }

        /// <summary>
        /// Primary Type
        /// </summary>
        public abstract int Type1 { get; set; }

        /// <summary>
        /// Secondary Type
        /// </summary>
        public abstract int Type2 { get; set; }

        /// <summary>
        /// First Egg Group
        /// </summary>
        public abstract int EggGroup1 { get; set; }

        /// <summary>
        /// Second Egg Group
        /// </summary>
        public abstract int EggGroup2 { get; set; }

        /// <summary>
        /// Catch Rate
        /// </summary>
        public abstract int CatchRate { get; set; }

        /// <summary>
        /// Evolution Stage value (or equivalent for un-evolved).
        /// </summary>
        public virtual int EvoStage { get; set; }

        /// <summary>
        /// Held Items the entry can be randomly encountered with.
        /// </summary>
        public abstract int[] Items { get; set; }

        /// <summary>
        /// Gender Ratio value determining if the entry is a fixed gender or bi-gendered.
        /// </summary>
        public abstract int Gender { get; set; }

        /// <summary>
        /// Amount of Hatching Step Cycles required to hatch if in an egg.
        /// </summary>
        public abstract int HatchCycles { get; set; }

        /// <summary>
        /// Initial Friendship when captured or received.
        /// </summary>
        public abstract int BaseFriendship { get; set; }

        /// <summary>
        /// Experience-Level Growth Rate type
        /// </summary>
        public abstract int EXPGrowth { get; set; }

        public abstract int[] Abilities { get; set; }

        /// <summary>
        /// Escape factor used for fleeing the Safari Zone or calling for help in SOS Battles.
        /// </summary>
        public abstract int EscapeRate { get; set; }

        public virtual int FormeCount { get; set; } = 1;

        protected internal virtual int FormStatsIndex { get; set; }

        public virtual int FormeSprite { get; set; }

        /// <summary>
        /// Base Experience Yield factor
        /// </summary>
        public abstract int BaseEXP { get; set; }

        /// <summary>
        /// Main color ID of the entry. The majority of the Pokémon's color is of this color, usually.
        /// </summary>
        public abstract int Color { get; set; }

        /// <summary>
        /// Height of the entry in meters (m).
        /// </summary>
        public virtual int Height { get; set; } = 0;

        /// <summary>
        /// Mass of the entry in kilograms (kg).
        /// </summary>
        public virtual int Weight { get; set; } = 0;

        /// <summary>
        /// Dual Type IDs used for same-type attack bonuses, weakness, etc.
        /// </summary>
        public int[] Types
        {
            get => new[] { Type1, Type2 };
            set
            {
                if (value.Length != 2) return;
                Type1 = value[0];
                Type2 = value[1];
            }
        }

        /// <summary>
        /// Dual Egg Group IDs used to determine if an egg should be created as a result of both parents sharing at least one group ID.
        /// </summary>
        public int[] EggGroups
        {
            get => new[] { EggGroup1, EggGroup2 };
            set
            {
                if (value.Length != 2) return;
                EggGroup1 = (byte)value[0];
                EggGroup2 = (byte)value[1];
            }
        }

        public int FormeIndex(int species, int forme)
        {
            if (forme <= 0) // no forme requested
                return species;
            if (FormStatsIndex <= 0) // no formes present
                return species;
            if (forme >= FormeCount) // beyond range of species' formes
                return species;

            return FormStatsIndex + forme - 1;
        }

        /// <summary>
        /// Indicates that the entry is exclusively Genderless.
        /// </summary>
        public bool Genderless => Gender == 255;

        /// <summary>
        /// Indicates that the entry is exclusively Female gendered.
        /// </summary>
        public bool OnlyFemale => Gender == 254;

        /// <summary>
        /// Indicates that the entry is exclusively Male gendered.
        /// </summary>
        public bool OnlyMale => Gender == 0;

        /// <summary>
        /// Indicates if the entry has Formes or not.
        ///  </summary>
        public bool HasFormes => FormeCount > 1;

        /// <summary>
        /// Base Stat Total sum of all stats.
        /// </summary>
        public int BST => HP + ATK + DEF + SPE + SPA + SPD;

        public bool IsFormeWithinRange(int forme)
        {
            if (forme == 0)
                return true;
            return forme < FormeCount;
        }

        /// <summary>
        /// Checks to see if the provided Types match the entry's types.
        /// </summary>
        /// <remarks>Input order matters! If input order does not matter, use <see cref="o:IsType(type1, type2)"/>.</remarks>
        /// <param name="type1">First type</param>
        /// <param name="type2">Second type</param>
        /// <returns>Typing is an exact match</returns>
        public bool IsValidTypeCombination(int type1, int type2) => Type1 == type1 && Type2 == type2;

        /// <summary>
        /// Checks if the entry has either type equal to the input type.
        /// </summary>
        /// <param name="type1">Type</param>
        /// <returns>Typing is present in entry</returns>
        public bool IsType(int type1) => Type1 == type1 || Type2 == type1;

        /// <summary>
        /// Checks if the entry has either type equal to both input types.
        /// </summary>
        /// <remarks>Input order does not matter.</remarks>
        /// <param name="type1">Type 1</param>
        /// <param name="type2">Type 2</param>
        /// <returns>Typing is present in entry</returns>
        public bool IsType(int type1, int type2) => (Type1 == type1 || Type2 == type1) && (Type1 == type2 || Type2 == type2);

        /// <summary>
        /// Checks if the entry has either egg group equal to the input type.
        /// </summary>
        /// <param name="group">Egg group</param>
        /// <returns>Egg is present in entry</returns>
        public bool IsEggGroup(int group) => EggGroup1 == group || EggGroup2 == group;
    }
}