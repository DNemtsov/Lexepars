namespace Lexepars
{
    public interface ILinedInputText : IInputText
    {
        bool ReadLine();
        bool EndOfLine { get; }
    }
}
