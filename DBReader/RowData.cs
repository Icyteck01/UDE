using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversalDB.classes
{
    [Serializable]
    public class RowData
    {
       public string name = "UNAMED";
       public int type = 0;
       public object value = 1;

        public RowData()
        {
        }

        public RowData(RowData t)
        {
            name = t.name;
            type = t.type;
            value = t.value;
        }
    }
}
