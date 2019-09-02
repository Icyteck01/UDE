using GameDataBaseEditor;
using GameDataBaseEditor.DBReader;
using MsgPack.Serialization;
using PWDataEditorPaied;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniversalDB.classes
{
    [Serializable]
    public class eListCollection
    {
        public Dictionary<int, eItemCollection> Lists = new Dictionary<int, eItemCollection>();
        public List<Settings> settingsList = new List<Settings>();
        private string loadedFile;

        public Dictionary<int, eItemCollection> Load(string elFile)
        {
            if (File.Exists(elFile))
            {
                this.loadedFile = elFile;
                Lists = new Dictionary<int, eItemCollection>();
                using (var memStream = new MemoryStream())
                {
                    var serializer = SerializationContext.Default.GetSerializer<Dictionary<int, eItemCollection>>();
                    byte[] file = File.ReadAllBytes(elFile);
                    memStream.Write(file, 0, file.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    BinaryReader reader = new BinaryReader(memStream);
                    if (reader.ReadInt32() == 0) { 
                        Lists = serializer.Unpack(memStream);
                        try
                        {
                            var serializer3 = SerializationContext.Default.GetSerializer<Settings[]>();
                            int size = reader.ReadInt32();
                            Settings[] settingarr = serializer3.Unpack(new MemoryStream(reader.ReadBytes(size)));
                            settingsList = new List<Settings>(settingarr);
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        ByteArray ba = new ByteArray();
                        int listLenght = reader.ReadInt32();
                        byte[] readCount = reader.ReadBytes(reader.ReadInt32());
                        ba.Write(readCount, 0, readCount.Length);
                        for (int i = 0; i < listLenght; i++)
                        {
                            var serializer2 = SerializationContext.Default.GetSerializer<eItemCollection>();
                            int listId = ba.ReadInt32();
                            int encriptPIN = ba.ReadInt32();
                            int len = ba.ReadInt32();
                            int pos = ba.ReadInt32();
                            byte[] data = ByteArray.x(ba.read(pos, len), encriptPIN);
                            Lists.Add(i, serializer2.Unpack(new MemoryStream(data)));
                        }

                        try
                        {
                            var serializer3 = SerializationContext.Default.GetSerializer<Settings[]>();
                            int len = ba.ReadInt32();
                            int pos = ba.ReadInt32();
                            byte[] data = ByteArray.x(ba.read(pos, len), MainForm.encriptPin);
                            Settings[] settingarr = serializer3.Unpack(new MemoryStream(data));
                            settingsList = new List<Settings>(settingarr);
                        }
                        catch
                        {

                        }
                    }
                    return Lists;
                }
            }


            return null;
        }

        private Dictionary<int, eItemCollection> resortDic(Dictionary<int, eItemCollection> data)
        {
            Dictionary<int, eItemCollection> datanew = new Dictionary<int, eItemCollection>();
            int i = 0;
            foreach (KeyValuePair<int, eItemCollection> entry in data)
            {
                datanew[i] = entry.Value;
                i++;
            }
            return datanew;
        }

        public bool Save(bool encrypt)
        {
            if (loadedFile == null) return false;

            Lists = resortDic(Lists);
            if (!encrypt)
            {
                var serializer = SerializationContext.Default.GetSerializer<Dictionary<int, eItemCollection>>();
                using (var memStream = new MemoryStream())
                {
                    using (BinaryWriter rwiter = new BinaryWriter(memStream))
                    {
                        rwiter.Write(0);
                        byte[] data = null;
                        try
                        {
                            using (var ms = new MemoryStream())
                            {
                                serializer.Pack(ms, Lists);
                                data = ms.ToArray();
                            }
                        }
                        catch { }
                        if (data != null)
                        {
                            rwiter.Write(data);
                            byte[] bytestowrite = new byte[0];
                            using (var ms = new MemoryStream())
                            {
                                var serializer3 = SerializationContext.Default.GetSerializer<Settings[]>();
                                serializer3.Pack(ms, settingsList.ToArray());
                                bytestowrite = ms.ToArray();
                                rwiter.Write(bytestowrite.Length);
                                rwiter.Write(bytestowrite);
                            }             
                            File.WriteAllBytes(loadedFile, memStream.ToArray());
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                using (var memStream = new MemoryStream())
                {
                    using (BinaryWriter rwiter = new BinaryWriter(memStream))
                    {
                        rwiter.Write(1);
                        byte[] data = null;
                        try
                        {
                           
                            int initial = MainForm.encriptPin;
                            rwiter.Write(Lists.Count);
                            ByteArray ba = new ByteArray();
                            for (int i = 0; i < Lists.Count; i++)
                            {
                                eItemCollection obj = Lists[i];
                                using (var ms = new MemoryStream())
                                {
                                    var serializer2 = SerializationContext.Default.GetSerializer<eItemCollection>();
                                    serializer2.Pack(ms, obj);
                                    byte[] bytestowrite = ByteArray.x(ms.ToArray(), initial);
                                    ba.writeInt(obj.listID);
                                    ba.writeInt(initial);
                                    ba.writeInt(bytestowrite.Length);
                                    ba.writeInt(ba.bytesAvailable());
                                    ba.writeBytes(bytestowrite);
                                    initial++;
                                }
                            }
                            using (var ms = new MemoryStream())
                            {
                                var serializer3 = SerializationContext.Default.GetSerializer<Settings[]>();
                                serializer3.Pack(ms, settingsList.ToArray());
                                byte[] bytestowrite = ByteArray.x(ms.ToArray(), initial);
                                ba.writeBytes(bytestowrite);
                            }
                            data = ba.Consume();
                        }
                        catch { }
                        if (data != null)
                        {
                            rwiter.Write(data.Length);
                            rwiter.Write(data);
                            File.WriteAllBytes(loadedFile, memStream.ToArray());
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        public bool SaveAs(string path, bool encrypt)
        {
            if (path == null) return false;


            if (!encrypt)
            {
                var serializer = SerializationContext.Default.GetSerializer<Dictionary<int, eItemCollection>>();
                using (var memStream = new MemoryStream())
                {
                    using (BinaryWriter rwiter = new BinaryWriter(memStream))
                    {
                        rwiter.Write(0);
                        byte[] data = null;
                        try
                        {
                            using (var ms = new MemoryStream())
                            {
                                serializer.Pack(ms, Lists);
                                data = ms.ToArray();
                            }
                        }
                        catch { }
                        if (data != null)
                        {
                            rwiter.Write(data);
                            File.WriteAllBytes(path, memStream.ToArray());
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                using (var memStream = new MemoryStream())
                {
                    using (BinaryWriter rwiter = new BinaryWriter(memStream))
                    {
                        rwiter.Write(1);
                        byte[] data = null;
                        try
                        {

                            int initial = MainForm.encriptPin;
                            rwiter.Write(Lists.Count);
                            ByteArray ba = new ByteArray();
                            for (int i = 0; i < Lists.Count; i++)
                            {
                                eItemCollection obj = Lists[i];
                                using (var ms = new MemoryStream())
                                {
                                    var serializer2 = SerializationContext.Default.GetSerializer<eItemCollection>();
                                    serializer2.Pack(ms, obj);
                                    byte[] bytestowrite = ByteArray.x(ms.ToArray(), initial);
                                    ba.writeInt(obj.listID);
                                    ba.writeInt(initial);
                                    ba.writeInt(bytestowrite.Length);
                                    ba.writeInt(ba.bytesAvailable());
                                    ba.writeBytes(bytestowrite);
                                    initial++;
                                }
                            }
                            data = ba.Consume();
                        }
                        catch { }
                        if (data != null)
                        {
                            rwiter.Write(data.Length);
                            rwiter.Write(data);
                            File.WriteAllBytes(loadedFile, memStream.ToArray());
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        public object getValue(int l, int e, string v)
        {
            if(Lists.ContainsKey(l))
                if(Lists[l].elementValues.ContainsKey(e))
                {
                    foreach(KeyValuePair<int, RowData> data in Lists[l].elementValues[e])
                    {
                        if (data.Value.name.ToLower().Equals(v.ToLower()))
                            return data.Value.value;
                    }
                }
            return null;
        }

        public object getValueX(int l, int e, string v)
        {
            if (Lists.ContainsKey(l))
                if (Lists[l].elementValues.ContainsKey(e))
                {
                    foreach (KeyValuePair<int, RowData> data in Lists[l].elementValues[e])
                    {
                        if (data.Value.name.ToLower().Equals(v.ToLower()))
                        {
                            string newValue = data.Value.value.ToString();
                            switch (data.Value.type)
                            {
                                case (int)ObjType.Short:
                                    return short.Parse(newValue);
                                case (int)ObjType.Int:
                                    return int.Parse(newValue);
                                case (int)ObjType.Float:
                                    return float.Parse(newValue);
                                case (int)ObjType.Double:
                                    return double.Parse(newValue);
                                case (int)ObjType.Boolean:
                                    return bool.Parse(newValue);
                                case (int)ObjType.String:
                                    return string.IsNullOrEmpty(newValue) ? "" : newValue;
                            }
                        }
                    }
                }
            return null;
        }

        public void SetValue(int l, int e, int rowIndex, int columnIndex, string newValue)
        {
            if (Lists.ContainsKey(l))
                if (Lists[l].elementValues.ContainsKey(e))
                    if(Lists[l].elementValues[e].ContainsKey(rowIndex))
                    {
                        switch (columnIndex)
                        {
                            case 0:
                                Lists[l].elementValues[e][rowIndex].name = newValue;
                               break;
                            case 1:
                                ObjType color = (ObjType)System.Enum.Parse(typeof(ObjType), newValue);
                                Lists[l].elementValues[e][rowIndex].type = (int)color;
                                break;
                            case 2:
                                 switch(Lists[l].elementValues[e][rowIndex].type)
                                {
                                    case (int)ObjType.Short:
                                        Lists[l].elementValues[e][rowIndex].value = short.Parse(newValue);
                                        break;
                                    case (int)ObjType.Int:
                                        Lists[l].elementValues[e][rowIndex].value = int.Parse(newValue);
                                        break;
                                    case (int)ObjType.Float:
                                        Lists[l].elementValues[e][rowIndex].value = float.Parse(newValue);
                                        break;
                                    case (int)ObjType.Double:
                                        Lists[l].elementValues[e][rowIndex].value = double.Parse(newValue);
                                        break;
                                    case (int)ObjType.Boolean:
                                        Lists[l].elementValues[e][rowIndex].value = bool.Parse(newValue);
                                        break;
                                    case (int)ObjType.String:
                                        Lists[l].elementValues[e][rowIndex].value = newValue;
                                        break;
                                }
                                break;
                        }
                    }
        }

        public string GetType(int l, int selectedRow)
        {
            throw new NotImplementedException();
        }
    }
}
