namespace HL7Populator.HL7.V2
{
    using System.Collections.Generic;
    using System.Text;

    public class Segment : HL7Object
    {
        public FieldList Fields { get; set; }
        public string Name
        {
            get
            {
                if (Fields.Count > 0)
                    return Fields[0][ControlCharacters.Default];
                return string.Empty;
            }
        }

        public Segment()
        {
            Fields = new FieldList();
        }

        public override string this[ControlCharacters controlCharacters]
        {
            get
            {
                StringBuilder results = new StringBuilder();
                for (int i = 0; i < Fields.Count; i++)
                {
                    if (i != 0)
                        results.Append(controlCharacters.FieldSeparator);
                    results.Append(Fields[i][controlCharacters]);
                }

                return results.ToString();
            }

            set
            {
                foreach (var fieldSplit in value.Split(controlCharacters.FieldSeparator))
                {
                    Field field = new Field();
                    field[controlCharacters] = fieldSplit;

                    Fields.Add(field);
                }
            }
        }
    }
}
