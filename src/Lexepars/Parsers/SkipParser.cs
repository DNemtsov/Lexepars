﻿using System.Text;

namespace Lexepars.Parsers
{
    /// <summary>
    /// General parser which skips over one or more `items.
    /// </summary>
    public class SkipParser : Parser
    {
        /// <summary>
        /// Creates a new instance of <see cref="SkipParser"/>.
        /// </summary>
        /// <param name="items">The item parsers. Not null. Not empty.</param>
        public SkipParser(params IGeneralParser[] items)
        {
            ArgumentCheck.NotNullOrEmptyOrWithNulls(items, nameof(items));

            _items = items;
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Stream of tokens to parse. Not null.</param>
        /// <returns>Parsing reply. Not null.</returns>
        public override IGeneralReply ParseGenerally(TokenStream tokens)
        {
            IGeneralReply reply = null;

            var unparsedTokens = tokens;

            foreach (var item in _items)
            {
                reply = item.ParseGenerally(unparsedTokens);

                if (!reply.Success)
                    return reply;

                unparsedTokens = reply.UnparsedTokens;
            }

            return reply;
        }

        /// <summary>
        /// Builds the parser expression.
        /// </summary>
        /// <returns>Expression string. Not null.</returns>
        protected override string BuildExpression()
        {
            var sb = new StringBuilder("<SKIP ");

            sb.Append(string.Join<IGeneralParser>(" ", _items));
            sb.Append(">");

            return sb.ToString();
        }

        private readonly IGeneralParser[] _items;
    }
}
