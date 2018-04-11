namespace HL7Populator.HL7.V2
{
    public class ControlCharacters
    {
        public const char Header = (char)11;
        public const char EndOfLine = (char)13;
        public const char EndOfMessage = (char)28;

        public static ControlCharacters Default = new ControlCharacters();

        public char FieldSeparator { get; set; } = '|';
        public char ComponentSeparator { get; set; } = '^';
        public char RepetitionSeparator { get; set; } = '~';
        public char EscapeCharacter { get; set; } = '\\';
        public char SubcomponentSeparator { get; set; } = '&';

        public ControlCharacters()
        {

        }

        public ControlCharacters(string controlCharacters)
        {
            if (controlCharacters.Length < 5)
                throw new HL7Exception("Control Characters lenght must be at least 5");
            var chars = controlCharacters.ToCharArray();
            FieldSeparator = chars[0];
            ComponentSeparator = chars[1];
            RepetitionSeparator = chars[2];
            EscapeCharacter = chars[3];
            SubcomponentSeparator = chars[4];
        }
    }
}