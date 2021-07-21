using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Utils.Csf.Struct
{
    /// <summary>
    /// CSF文件的标签结构
    /// </summary>
    public class CsfLabel
    {
        public string Label = string.Empty;
        public List<CsfValue> Values = new();
    }
}
