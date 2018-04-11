namespace HL7Populator.HL7.V2
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Message : HL7Object
    {
        public List<Segment> Segments { get; set; }
        public ControlCharacters ControlCharacters { get; set; }
        public string MessageID
        {
            get
            {
                if (Segments.Count > 0)
                {
                    var msh = Segments.Where(t => t.Name.ToLower() == "msh").FirstOrDefault();
                    if (null != msh)
                        return msh.Fields[9][ControlCharacters.Default];
                }


                return "0";
            }
        }

        public Message()
        {
            Segments = new List<Segment>();
            ControlCharacters = new ControlCharacters();
        }

        public Message(string message)
        {
            Segments = new List<Segment>();
            ControlCharacters = GetControlCharacters(message);
            SetSegments(message, ControlCharacters);
        }

        public override string this[ControlCharacters controlCharacters]
        {
            get
            {
                StringBuilder results = new StringBuilder();

                results.Append(ControlCharacters.Header);

                for (int i = 0; i < Segments.Count; i++)
                {
                    results.Append(Segments[i][controlCharacters]);
                    results.Append(ControlCharacters.EndOfLine);
                }

                results.Append(ControlCharacters.EndOfMessage);
                results.Append(ControlCharacters.EndOfLine);

                return results.ToString();
            }

            set
            {
                SetSegments(value, controlCharacters);
            }
        }

        private ControlCharacters GetControlCharacters(string message)
        {
            // Remove HL7 control characters
            message = message.Replace(ControlCharacters.Header.ToString(), string.Empty).Replace(ControlCharacters.EndOfMessage.ToString(), string.Empty);

            if (!message.ToLower().StartsWith("msh"))
                throw new HL7Exception("Messages must begin with MSH");
            if (message.Length < 8)
                throw new HL7Exception("Messages must have a length greater than 8 for control characters");

            return new ControlCharacters(message.Substring(3, 5));
        }

        private void SetSegments(string value, ControlCharacters controlCharacters)
        {
            foreach (var segment in value.Split(ControlCharacters.EndOfLine))
            {
                Segment seg = new Segment();
                seg[controlCharacters] = segment;

                Segments.Add(seg);
            }
        }
    }
}
