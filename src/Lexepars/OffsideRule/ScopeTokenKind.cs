namespace Lexepars.OffsideRule
{
    /// <summary>
    /// Base class for the scope control token kinds. Should be inherited.
    /// </summary>
    /// <remarks>Scope tokens are used in the cases where there's no explicit scope indicators, such as { and }.</remarks>
    public abstract class ScopeTokenKind : NullLexemeTokenKind
    {
        /// <summary>
        /// The standard token emitted at the beginning of a scope.
        /// </summary>
        public static readonly ScopeBeginTokenKind ScopeBegin = new ScopeBeginTokenKind();
        
        /// <summary>
        /// The standard token emitted at the beginning of a scope.
        /// </summary>
        public static readonly ScopeEndTokenKind ScopeEnd = new ScopeEndTokenKind();

        /// <summary>
        /// The standard token emitted if the scope is inconsistent.
        /// Occurrence of this token is a fatal error, so lexer should stop after emitting a token of this kind.
        /// </summary>
        public static readonly ScopeInconsistentTokenKind ScopeInconsistent = new ScopeInconsistentTokenKind();

        /// <summary>
        /// Creates a new instance of <see cref="ScopeTokenKind"/>
        /// </summary>
        protected ScopeTokenKind(string name)
            : base(name)
        { }
    }

    /// <summary>
    /// Scope begin token kind.
    /// </summary>
    /// <remarks>Scope tokens are used in the cases where there are no explicit scope indicators, such as '{' and '}'.</remarks>
    public class ScopeBeginTokenKind : ScopeTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="ScopeBeginTokenKind"/>
        /// </summary>
        public ScopeBeginTokenKind()
            : base("scope begin")
        { }
    }

    /// <summary>
    /// Scope end token kind.
    /// </summary>
    /// <remarks>Scope tokens are used in the cases where there are no explicit scope indicators, such as '{' and '}'.</remarks>
    public class ScopeEndTokenKind : ScopeTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="ScopeEndTokenKind"/>
        /// </summary>
        public ScopeEndTokenKind()
            : base("scope end")
        { }
    }

    /// <summary>
    /// Inconsistent scope token kind. Is used to indicate scope inconsistency, e.g. improper offside-rule indent level.
    /// Occurrence of this token is a fatal error, so lexer should stop after emitting a token of this kind.
    /// </summary>
    /// <remarks>Scope tokens are used in the cases where there are no explicit scope indicators, such as '{' and '}'.</remarks>
    public class ScopeInconsistentTokenKind : ScopeTokenKind
    {
        /// <summary>
        /// Creates a new instance of <see cref="ScopeInconsistentTokenKind"/>
        /// </summary>
        public ScopeInconsistentTokenKind()
            : base("scope is inconsistent")
        { }
    }
}
