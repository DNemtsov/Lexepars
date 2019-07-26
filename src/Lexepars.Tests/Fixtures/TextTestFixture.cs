namespace Lexepars.Tests.Fixtures
{
    internal class TextTestFixture
    {
        public TextTestFixture(string s, string newLine = null)
        {
            _s = s;
            _newLine = newLine;
        }

        private readonly string _s;
        private readonly string _newLine;

        public InputText Advance(int characters)
        {
            var text = new InputText(_s, _newLine);

            text.Advance(characters);

            return text;
        }

        public static implicit operator InputText(TextTestFixture f)
        {
            return new InputText(f._s, f._newLine);
        }
    }
}
