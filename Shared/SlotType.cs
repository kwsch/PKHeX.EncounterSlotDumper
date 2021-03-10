using System;

namespace PKHeX.EncounterSlotDumper
{
    /// <summary>
    /// Wild Encounter data <see cref="EncounterSlot"/> Type
    /// </summary>
    [Flags]
    public enum SlotType : ushort
    {
        /// <summary>
        /// Default (un-assigned) encounter slot type.
        /// </summary>
        Any = 0,

        /// <summary>
        /// Slot is encountered via Grass.
        /// </summary>
        Grass = 1,

        /// <summary>
        /// Slot is encountered via Surfing.
        /// </summary>
        Surf = 2,

        /// <summary>
        /// Slot is encountered via Old Rod (Fishing).
        /// </summary>
        Old_Rod = 3,

        /// <summary>
        /// Slot is encountered via Good Rod (Fishing).
        /// </summary>
        Good_Rod = 4,

        /// <summary>
        /// Slot is encountered via Super Rod (Fishing).
        /// </summary>
        Super_Rod = 5,

        /// <summary>
        /// Slot is encountered via Rock Smash.
        /// </summary>
        Rock_Smash = 6,

        /// <summary>
        /// Slot is encountered via Headbutt.
        /// </summary>
        Headbutt = 7,

        /// <summary>
        /// Slot is encountered via a Honey Tree.
        /// </summary>
        HoneyTree = 8,

        /// <summary>
        /// Slot is encountered via the Bug Catching Contest.
        /// </summary>
        BugContest = 9,

        HiddenGrotto = 10,
        GoPark = 11,
        FriendSafari = 12,
        Horde = 13,
        Pokeradar = 14,
        SOS = 15,

        // always used as a modifier to another slot type

        Special = 1 << 6,

        /// <summary>
        /// Slot is encountered in a Swarm.
        /// </summary>
        Swarm = 1 << 7,

        /// <summary>
        /// Slot is encountered in the Safari Zone.
        /// </summary>
        Safari = 1 << 15,

        Grass_Safari = Grass | Safari,
        Surf_Safari = Surf | Safari,
        Old_Rod_Safari = Old_Rod | Safari,
        Good_Rod_Safari = Good_Rod | Safari,
        Super_Rod_Safari = Super_Rod | Safari,
    }
}