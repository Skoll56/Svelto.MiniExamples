﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    /// <summary>
    /// Todo: EntityReference shouldn't map EGIDs as dictionaries keys but directly the indices in the EntityDB arrays
    /// </summary>
    [Serialization.DoNotSerialize]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct EntityReference : IEquatable<EntityReference>
    {
        [FieldOffset(0)] public readonly uint uniqueID;
        [FieldOffset(4)] public readonly uint version;
        [FieldOffset(0)] readonly ulong _GID;

        internal uint index => uniqueID - 1;

        public static bool operator ==(EntityReference obj1, EntityReference obj2)
        {
            return obj1._GID == obj2._GID;
        }

        public static bool operator !=(EntityReference obj1, EntityReference obj2)
        {
            return obj1._GID != obj2._GID;
        }

        public EntityReference(uint uniqueId) : this(uniqueId, 0) {}

        public EntityReference(uint uniqueId, uint version) : this()
        {
            _GID = MAKE_GLOBAL_ID(uniqueId, version);
        }

        public bool Equals(EntityReference other)
        {
            return _GID == other._GID;
        }

        public bool Equals(EntityReference x, EntityReference y)
        {
            return x._GID == y._GID;
        }

        public override string ToString()
        {
            return "id:".FastConcat(uniqueID).FastConcat(" version:").FastConcat(version);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGID ToEGID(EntitiesDB entitiesDB)
        {
            DBC.ECS.Check.Require(this != Invalid, "Invalid Reference Used");

            return entitiesDB.GetEGID(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ToEGID(EntitiesDB entitiesDB, out EGID egid)
        {
            DBC.ECS.Check.Require(this != Invalid, "Invalid Reference Used");

            return entitiesDB.TryGetEGID(this, out egid);
        }

        static ulong MAKE_GLOBAL_ID(uint uniqueId, uint version)
        {
            return (ulong)version << 32 | ((ulong)uniqueId & 0xFFFFFFFF);
        }

        public static EntityReference Invalid => new EntityReference(0, 0);
    }
}
