namespace Lexepars
{
    public interface IParser<out T> : IGeneralParser
    {
        IReply<T> Parse(TokenStream tokens);
    }
}