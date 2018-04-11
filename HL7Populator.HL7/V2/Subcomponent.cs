using System.Collections.Generic;

namespace HL7Populator.HL7.V2
{
    public class Subcomponent : HL7Object
    {
        private string _value { get; set; }

        public override string this[ControlCharacters characters]
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }
    }

    public class SubcomponentList : List<Subcomponent>
    {
        public new Subcomponent this[int i]
        {
            get
            {
                if (i <= Count)
                    return base[i - 1];

                return new Subcomponent();
            }
            set
            {
                if (Count <= i)
                {
                    while (Count <= i)
                    {
                        Add(new Subcomponent());
                    }
                }

                base[i - 1] = value;
            }
        }
    }
}
