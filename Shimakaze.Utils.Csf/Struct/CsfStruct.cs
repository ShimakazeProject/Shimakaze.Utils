using System.Collections.Generic;

namespace Shimakaze.Utils.Csf.Struct
{
    public class CsfStruct
    {
        public CsfHead Head;
        public List<CsfLabel> Data = new();
    }
}
