namespace System
{
    public interface IPropertiesMapper<in TSource, in TDest>
    {
        void MapProperties(TSource source, TDest dest);
    }
}
