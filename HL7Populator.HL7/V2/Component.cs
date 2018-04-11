namespace HL7Populator.HL7.V2
{
    using System.Collections.Generic;
    using System.Text;

    public class Component : HL7Object
    {
        public SubcomponentList Subcomponents { get; set; }

        public Component()
        {
            Subcomponents = new SubcomponentList();
        }

        public override string this[ControlCharacters controlCharacters]
        {
            get
            {
                StringBuilder results = new StringBuilder();
                for (int i = 1; i <= Subcomponents.Count; i++)
                {
                    if (i != 1)
                        results.Append(controlCharacters.SubcomponentSeparator);
                    results.Append(Subcomponents[i][controlCharacters]);
                }

                return results.ToString();
            }

            set
            {
                foreach (var subcomponentSplit in value.Split(controlCharacters.SubcomponentSeparator))
                {
                    Subcomponent subcomponent = new Subcomponent();
                    subcomponent[controlCharacters] = subcomponentSplit;

                    Subcomponents.Add(subcomponent);
                }
            }
        }
    }

    public class ComponentList : List<Component>
    {
        public new Component this[int i]
        {
            get
            {
                if (i <= Count)
                    return base[i - 1];

                return new Component();
            }
            set
            {
                if (Count <= i)
                {
                    while (Count <= i)
                    {
                        Add(new Component());
                    }
                }

                base[i - 1] = value;
            }
        }
    }
}
