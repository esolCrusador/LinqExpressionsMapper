using System;
using System.Collections.Generic;

namespace LinqExpressionsMapper.Models
{
    internal class PairId<TSourceId, TDestId> : IEqualityComparer<PairId<TSourceId, TDestId>>, IEquatable<PairId<TSourceId, TDestId>>
    {
        public PairId(TSourceId sourceId, TDestId destId)
        {
            SourceId = sourceId;
            DestId = destId;
        }

        public TSourceId SourceId { get; private set; }

        public TDestId DestId { get; private set; }

        #region IEqualityComparer<PairId<TSourceId, TDestId>> members

        public bool Equals(PairId<TSourceId, TDestId> x, PairId<TSourceId, TDestId> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.SourceId.Equals(y.SourceId) && x.DestId.Equals(y.DestId);
        }

        public int GetHashCode(PairId<TSourceId, TDestId> obj)
        {
            unchecked
            {
                return (obj.SourceId.GetHashCode() * 397) ^ obj.DestId.GetHashCode();
            }
        }

        #endregion

        public bool Equals(PairId<TSourceId, TDestId> other)
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

            return Equals(this, (PairId<TSourceId, TDestId>)obj);
        }

        public override string ToString()
        {
            return "<" + SourceId + "," + DestId + ">";
        }
    }

    internal class PairId : PairId<Guid, Guid>
    {
        private string _name;

        private PairId(Guid sourceId, Guid destId) : base(sourceId, destId)
        {

        }

        public static PairId GetId<TSource, TDset>()
        {
            Type sourceType = typeof(TSource);
            Type destType = typeof(TDset);

            var pairId = new PairId(sourceType.GUID, destType.GUID)
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
