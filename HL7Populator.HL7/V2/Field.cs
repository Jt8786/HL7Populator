namespace HL7Populator.HL7.V2
{
    using System.Collections.Generic;
    using System.Text;

    public class Field : HL7Object
    {
        public RepetitionList Repetitions { get; set; }

        public Field()
        {
            Repetitions = new RepetitionList();
        }

        public override string this[ControlCharacters controlCharacters]
        {
            get
            {
                StringBuilder results = new StringBuilder();
                for (int i = 1; i <= Repetitions.Count; i++)
                {
                    if (i != 1)
                        results.Append(controlCharacters.RepetitionSeparator);
                    results.Append(Repetitions[i][controlCharacters]);
                }

                return results.ToString();
            }

            set
            {
                foreach (var repetitionSplit in value.Split(controlCharacters.RepetitionSeparator))
                {
                    Repetition repetition = new Repetition();
                    repetition[controlCharacters] = repetitionSplit;

                    Repetitions.Add(repetition);
                }
            }
        }
    }

    public class FieldList : List<Field>
    {
        public new Field this[int i]
        {
            get
            {
                if (i < Count)
                    return base[i];

                return new Field();
            }
            set
            {
                if (Count < i)
                {
                    while (Count < i)
                    {
                        Add(new Field());
                    }
                }

                base[i] = value;
            }
        }
    }
}
