using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDataBaseEditor.DBReader
{
    [Serializable]
    public class Settings
    {
        public int listId { get; set; }
        public int RowIndex { get; set; }
        public string rowValues { get; set; }
    }
}
