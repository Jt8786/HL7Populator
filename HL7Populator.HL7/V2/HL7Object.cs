namespace HL7Populator.HL7.V2
{
    public abstract class HL7Object
    {
        public abstract string this[ControlCharacters controlCharacters] { get; set; }
    }
}
