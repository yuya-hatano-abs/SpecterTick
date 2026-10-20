using System;
using System.Collections.Generic;

namespace SpecterTick.Core
{
    internal sealed class ActivatedSwitchSet : IEquatable<ActivatedSwitchSet>
    {
        internal static readonly ActivatedSwitchSet Empty = new ActivatedSwitchSet(Array.Empty<SwitchId>());

        private readonly SwitchId[] _ids;
        private readonly HashSet<SwitchId> _lookup;

        internal ActivatedSwitchSet(IEnumerable<SwitchId> switchIds)
        {
            if (switchIds == null)
            {
                throw new ArgumentNullException(nameof(switchIds));
            }

            var list = new List<SwitchId>();
            var lookup = new HashSet<SwitchId>();
            foreach (var id in switchIds)
            {
                if (lookup.Add(id))
                {
                    list.Add(id);
                }
            }

            _ids = list.ToArray();
            _lookup = lookup;
        }

        private ActivatedSwitchSet(SwitchId[] ids, HashSet<SwitchId> lookup)
        {
            _ids = ids;
            _lookup = lookup;
        }

        internal ActivatedSwitchSet Add(SwitchId switchId)
        {
            if (_lookup.Contains(switchId))
            {
                return this;
            }

            var nextIds = new SwitchId[_ids.Length + 1];
            Array.Copy(_ids, nextIds, _ids.Length);
            nextIds[_ids.Length] = switchId;
            var nextLookup = new HashSet<SwitchId>(_lookup) { switchId };
            return new ActivatedSwitchSet(nextIds, nextLookup);
        }

        internal bool Contains(SwitchId switchId)
        {
            return _lookup.Contains(switchId);
        }

        internal int Count => _ids.Length;

        public bool Equals(ActivatedSwitchSet other)
        {
            if (other == null)
            {
                return false;
            }

            if (_lookup.Count != other._lookup.Count)
            {
                return false;
            }

            foreach (var id in _lookup)
            {
                if (!other._lookup.Contains(id))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is ActivatedSwitchSet other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = _lookup.Count;
                for (var i = 0; i < _ids.Length; i++)
                {
                    hash += _ids[i].GetHashCode();
                }

                return hash;
            }
        }
    }
}
