using System;
using System.Collections.Generic;

namespace UniversalDB.classes
{
    [Serializable]
    public class eItemCollection
    {
        public Dictionary<int, Dictionary<int, RowData>> elementValues = new Dictionary<int, Dictionary<int, RowData>>();
        public string listName;
        public int listID;

        public void RemoveItem(int idx)
        {
            elementValues.Remove(idx);
        }

        public void RemoveRow(int i, int idx)
        {
            if (elementValues.ContainsKey(i))
            {
                elementValues[i].Remove(idx);
                elementValues[i] = resortDic(elementValues[i]);
            }
        }

        public void MoveUp(int ElementIndex, int RowIndex)
        {
            if (elementValues.ContainsKey(ElementIndex) && 0 <= RowIndex - 1)
            {
                RowData x = elementValues[ElementIndex][RowIndex - 1];
                RowData y = elementValues[ElementIndex][RowIndex];
                elementValues[ElementIndex][RowIndex - 1] = y;
                elementValues[ElementIndex][RowIndex] = x;
            }
        }

        public void MoveDown(int ElementIndex, int RowIndex)
        {
            if (elementValues.ContainsKey(ElementIndex) && elementValues[ElementIndex].Count > RowIndex + 1)
            {
                RowData x = elementValues[ElementIndex][RowIndex + 1];
                RowData y = elementValues[ElementIndex][RowIndex];
                elementValues[ElementIndex][RowIndex + 1] = y;
                elementValues[ElementIndex][RowIndex] = x;
            }
        }

        private Dictionary<int, RowData> resortDic(Dictionary<int, RowData> data)
        {
            Dictionary<int, RowData> datanew = new Dictionary<int, RowData>();
            int i = 0;
            foreach (KeyValuePair<int, RowData> entry in data)
            {
                datanew[i] = entry.Value;
                i++;
            }
            return datanew;
        }

        public string GetName(int item, int selectedRow)
        {
            if (elementValues.ContainsKey(item))
            {
                if (elementValues[item].ContainsKey(selectedRow))
                {
                    return elementValues[item][selectedRow].name.ToString();
                }
            }
            return null;
        }

        public string GetType(int item, int selectedRow)
        {
            if (elementValues.ContainsKey(item))
            {
                if (elementValues[item].ContainsKey(selectedRow))
                {
                    ObjType color = (ObjType)System.Enum.Parse(typeof(ObjType), elementValues[item][selectedRow].type.ToString());
                    return Enum.GetName(typeof(ObjType), color).ToLower();
                }
            }
            return null;
        }
        public string GetValue(int item, int selectedRow)
        {
            if (elementValues.ContainsKey(item))
            {
                if(elementValues[item].ContainsKey(selectedRow))
                {
                    return elementValues[item][selectedRow].value.ToString();
                }
            }
            return null;
        }

        public void SetValue(int e, int rowIndex, string newValue)
        {
            if (elementValues.ContainsKey(e))
            {
                if (elementValues[e].ContainsKey(rowIndex))
                {
                   int columnIndex = 2;
                    switch (columnIndex)
                    {
                        case 0:
                            elementValues[e][rowIndex].name = newValue;
                            break;
                        case 1:
                            ObjType color = (ObjType)System.Enum.Parse(typeof(ObjType), newValue);
                            elementValues[e][rowIndex].type = (int)color;
                            break;
                        case 2:
                            switch (elementValues[e][rowIndex].type)
                            {
                                case (int)ObjType.Short:
                                    elementValues[e][rowIndex].value = short.Parse(newValue);
                                    break;
                                case (int)ObjType.Int:
                                    elementValues[e][rowIndex].value = int.Parse(newValue);
                                    break;
                                case (int)ObjType.Float:
                                    elementValues[e][rowIndex].value = float.Parse(newValue);
                                    break;
                                case (int)ObjType.Double:
                                    elementValues[e][rowIndex].value = double.Parse(newValue);
                                    break;
                                case (int)ObjType.Boolean:
                                    elementValues[e][rowIndex].value = bool.Parse(newValue);
                                    break;
                                case (int)ObjType.String:
                                    elementValues[e][rowIndex].value = newValue;
                                    break;
                            }
                            break;
                    }
                }
            }
        }
    }
}
