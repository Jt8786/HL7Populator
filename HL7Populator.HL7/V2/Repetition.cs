namespace HL7Populator.HL7.V2
{
    using System.Collections.Generic;
    using System.Text;

    public class Repetition : HL7Object
    {
        public ComponentList Components { get; set; }

        public Repetition()
        {
            Components = new ComponentList();
        }

        public override string this[ControlCharacters controlCharacters]
        {
            get
            {
                StringBuilder results = new StringBuilder();
                for (int i = 1; i <= Components.Count; i++)
                {
                    if (i != 1)
                        results.Append(controlCharacters.ComponentSeparator);
                    results.Append(Components[i][controlCharacters]);
                }

                return results.ToString();
            }

            set
            {
                foreach (var componentSplit in value.Split(controlCharacters.ComponentSeparator))
                {
                    Component component = new Component();
                    component[controlCharacters] = componentSplit;

                    Components.Add(component);
                }
            }
        }
    }

    public class RepetitionList : List<Repetition>
    {
        public new Repetition this[int i]
        {
            get
            {
                if (i <= Count)
                    return base[i - 1];

                return new Repetition();
            }
            set
            {
                if (Count <= i)
                {
                    while (Count <= i)
                    {
                        Add(new Repetition());
                    }
                }

                base[i - 1] = value;
            }
        }
    }
}
