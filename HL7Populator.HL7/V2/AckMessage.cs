using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HL7Populator.HL7.V2
{
    public class AckMessage : Message
    {
        public string AckMessageID
        {
            get
            {
                if (Segments.Count > 0)
                {
                    var msh = Segments.Where(t => t.Name.ToLower() == "msa").FirstOrDefault();
                    if (null != msh)
                        return msh.Fields[2][ControlCharacters.Default];
                }

                return "0";
            }
        }
    }
}
