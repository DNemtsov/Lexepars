using System;

namespace Lexepars
{
    public class Failure<T> : GeneralFailure, IReply<T>
    {
        public Failure(TokenStream unparsedTokens, FailureMessage message)
            : base(unparsedTokens, message)
        {}

        public Failure(TokenStream unparsedTokens, FailureMessages messages)
            : base(unparsedTokens, messages)
        {}

#pragma warning disable CA1000 // Do not declare static members on generic types
        public new static Failure<T> From(IGeneralReply r)
#pragma warning restore CA1000 // Do not declare static members on generic types
        {
            return new Failure<T>(r.UnparsedTokens, r.FailureMessages);
        }

        public T ParsedValue => throw new NotSupportedException($"{UnparsedTokens.Position}: {FailureMessages}");
    }
}