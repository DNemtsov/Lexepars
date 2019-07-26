namespace Lexepars
{
    public interface IReply<out T> : IGeneralReply
    {
        T ParsedValue { get; }
    }
}