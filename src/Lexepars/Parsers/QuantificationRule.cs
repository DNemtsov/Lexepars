namespace Lexepars.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Specifies the expected number of the `item occurrences for the quantified parsers. 
    /// </summary>
    public enum QuantificationRule
    {
        /// <summary>
        /// N or more times.
        /// </summary>
        NOrMore,

        /// <summary>
        /// Exactly N times.
        /// </summary>
        ExactlyN,

        /// <summary>
        /// N or less times.
        /// </summary>
        NOrLess,

        /// <summary>
        /// From N to M times.
        /// </summary>
        NtoM
    }
}
