using System;

namespace Lexepars
{
    public class TokenizationException : Exception
    {
        public TokenizationException(IInputText inputText, string message) : base(message)
        {
            InputText = inputText;
        }

        public IInputText InputText { get; }
    }
}
