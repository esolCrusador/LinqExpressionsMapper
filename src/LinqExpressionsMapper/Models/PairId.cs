using System;
using System.Collections.Generic;

namespace LinqExpressionsMapper.Models
{
    internal class PairIdBase : IEqualityComparer<PairIdBase>, IEquatable<PairIdBase>
    {
        public PairIdBase(Type sourceId, Type destId)
        {
            SourceId = sourceId;
            DestId = destId;
        }

        public Type SourceId { get; private set; }

        public Type DestId { get; private set; }

        #region IEqualityComparer<PairId<TSourceId, TDestId>> members

        public bool Equals(PairIdBase x, PairIdBase y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.SourceId == y.SourceId && x.DestId == y.DestId;
        }

        public int GetHashCode(PairIdBase obj)
        {
            unchecked
            {
                return (obj.SourceId.GetHashCode() * 397) ^ obj.DestId.GetHashCode();
            }
        }

        #endregion

        public bool Equals(PairIdBase other)
        {
            return Equals(this, other);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals(this, (PairIdBase)obj);
        }

        public override string ToString()
        {
            return "<" + SourceId + "," + DestId + ">";
        }
    }

    internal class PairId : PairIdBase
    {
        private string _name;

        private PairId(Type sourceId, Type destId) : base(sourceId, destId)
        {

        }

        public static PairId GetId<TSource, TDset>()
        {
            Type sourceType = typeof(TSource);
            Type destType = typeof(TDset);

            return GetId(sourceType, destType);
        }

        public static PairId GetId(Type sourceType, Type destType)
        {
            var pairId = new PairId(sourceType, destType)
            {
                _name = "<" + sourceType.Name + "," + destType.Name + ">"
            };

            return pairId;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
