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
        Any,

        /// <summary>
        /// Slot is encountered via Grass.
        /// </summary>
        Grass,

        /// <summary>
        /// Slot is encountered via Surfing.
        /// </summary>
        Surf,

        /// <summary>
        /// Slot is encountered via Old Rod (Fishing).
        /// </summary>
        Old_Rod,

        /// <summary>
        /// Slot is encountered via Good Rod (Fishing).
        /// </summary>
        Good_Rod,

        /// <summary>
        /// Slot is encountered via Super Rod (Fishing).
        /// </summary>
        Super_Rod,

        /// <summary>
        /// Slot is encountered via Rock Smash.
        /// </summary>
        Rock_Smash,

        /// <summary>
        /// Slot is encountered via Headbutt.
        /// </summary>
        Headbutt,

        /// <summary>
        /// Slot is encountered via a Honey Tree.
        /// </summary>
        HoneyTree,

        /// <summary>
        /// Slot is encountered via the Bug Catching Contest.
        /// </summary>
        BugContest,

        // always used as a modifier to another slot type

        Special = 1 << 13,

        /// <summary>
        /// Slot is encountered in a Swarm.
        /// </summary>
        Swarm = 1 << 14,

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