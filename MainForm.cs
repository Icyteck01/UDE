using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UniversalDB.classes;
using System.Linq;
using JHUI.Forms;
using GameDataBaseEditor.DBReader;

namespace GameDataBaseEditor
{
    public partial class MainForm : JForm
    {
        internal delegate void UpdateProgressDelegate(String value, int min, int max);
        internal event UpdateProgressDelegate progress_bar2;
        private bool locked;
        public int version = 1;
        private eListCollection eLC;
        private string path = null;
        public static int encriptPin = 0;
        public void progress_bar(String value, int min = 0, int max = 0)
        {
            if (progress_bar2 != null)
            {
                progress_bar2(value, min, max);
            }
            progressBar1.Text = value;
            if (min == 0 && max == 0)
            {
                progressBar1.Value = 0;

                comboBox_lists.Enabled = true;
            }
            else
            {
                int val = (100 * min) / max;
                progressBar1.Value = val <= 100 ? val : 100;
            }
        }
        public MainForm()
        {
            InitializeComponent();
            encriptPin = 1984;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog eLoad = new OpenFileDialog();
            eLoad.Filter = "Elements File (*.data)|*.data|All Files (*.*)|*.*";
            eLoad.RestoreDirectory = false;
            if (eLoad.ShowDialog() == DialogResult.OK && File.Exists(eLoad.FileName))
            {
                locked = true;
                path = eLoad.FileName;
                eLC = new eListCollection();
                Dictionary<int, eItemCollection> data = eLC.Load(eLoad.FileName);
                comboBox_lists.Items.Clear();
                foreach (KeyValuePair<int, eItemCollection> sub in data)
                {
                    comboBox_lists.Items.Add(sub.Value.listName);
                }
                locked = false;
                comboBox_lists.SelectedIndex = 0;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (eLC != null)
                if (eLC.Save(MessageBox.Show("Do you want to encrypt the data?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes))
                {
                    MessageBox.Show("Saved!");
                }

        }

        private void comboBox_lists_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (locked) return;
            if (eLC == null) return;
            int selected_index = comboBox_lists.SelectedIndex;
            if (selected_index > -1)
            {
                locked = true;
                listBox_items.Rows.Clear();
                for (int item = 0; item < eLC.Lists[selected_index].elementValues.Count; item++)
                {
                    listBox_items.Rows.Add(new object[] { eLC.getValue(selected_index, item, "id"), eLC.getValue(selected_index, item, "name") });
                    textBox_Id.Text = eLC.Lists[selected_index].listID.ToString();
                    textBox_Name.Text = eLC.Lists[selected_index].listName;
                    listbox_description.Text = "List: " + eLC.Lists[selected_index].listID;
                }
                locked = false;
                listBox_items_SelectionChanged(null, null);
            }
        }

        private void listBox_items_CellValueChanged(object sender, DataGridViewCellEventArgs ea)
        {
            if (locked) { return; }
            if (listBox_items.CurrentCell != null && eLC != null)
            {
                int list = comboBox_lists.SelectedIndex;
                int item = listBox_items.CurrentCell.RowIndex;
                if (list > -1 && item > -1)
                {
                    // DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                    //  for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                    //  {
                    //System.Windows.Forms.Application.DoEvents();
                    // progress_bar("Changing ...", x, listBox_items.SelectedRows.Count);
                    string valeu = Convert.ToString(listBox_items.Rows[ea.RowIndex].Cells[ea.ColumnIndex].Value);
                    eLC.SetValue(list, item, ea.ColumnIndex, 2, valeu);
                    // }
                }
            }
            progress_bar("Ready");
        }

        private void listBox_items_SelectionChanged(object sender, EventArgs ex)
        {
            if (locked) return;
            int selected_index = comboBox_lists.SelectedIndex;
            int rowIndex = listBox_items.CurrentCell.RowIndex;
            if (selected_index > -1 && rowIndex > -1)
            {
                dataGridView_item.SuspendLayout();
                dataGridView_item.Rows.Clear();
                dataGridView_item.Enabled = false;
                List<Settings> setings = eLC.settingsList.Where(x => x.listId == selected_index).ToList();
                for (int e = 0; e < eLC.Lists[selected_index].elementValues[rowIndex].Count; e++)
                {
                    RowData data = eLC.Lists[selected_index].elementValues[rowIndex][e];
                    DataGridViewRow row = (DataGridViewRow)dataGridView_item.RowTemplate.Clone();
                    row.CreateCells(dataGridView_item);
                    row.Cells[0].Value = data.name.ToString();
                    DataGridViewComboBoxCell cbc = new DataGridViewComboBoxCell();
                    int count = 0;
                    int selected = -0;
                    bool isType = data.type == (int)ObjType.Boolean;
                    Settings seting = setings.FirstOrDefault(x => x.RowIndex == e);
                    foreach (ObjType suit in Enum.GetValues(typeof(ObjType)))
                    {
                        cbc.Items.Add(suit.ToString());
                        if (data.type == count) { selected = count; }
                        count++;
                    }
                    row.Cells[1] = cbc;
                    if (isType)
                    {
                        bool val = false;
                        bool.TryParse(data.value.ToString(), out val);

                        DataGridViewCheckBoxCell cbv = new DataGridViewCheckBoxCell();
                        cbv.Value = val;
                        row.Cells[2] = cbv;
                    }
                    else if(seting != null)
                    {
                        try
                        {
                            DataGridViewComboBoxCell cbcc = new DataGridViewComboBoxCell();
                            string[] datas = seting.rowValues.Split(',');
                            int c = 0;
                            foreach(string str in datas)
                            {
                                string[] datax = null;
                                if(str.Contains("="))
                                {
                                    datax = str.Split('=');
                                }
                                else
                                {
                                    datax = str.Split(null);
                                }
                                cbcc.Items.Add(datax[0]+"_"+ datax[1]);
                                if(int.Parse(datax[1]) == int.Parse(data.value.ToString()))
                                    cbcc.Value = cbcc.Items[cbcc.Items.IndexOf(datax[0] + "_" + datax[1])];

                                c++;
                            }
                            row.Cells[2] = cbcc;
                        }
                        catch { row.Cells[2].Value = data.value.ToString(); }
                    }
                    else
                    {
                        row.Cells[2].Value = data.value.ToString();
                    }
                    try { if (selected != -1) { cbc.Value = cbc.Items[selected]; } } catch (Exception) { }
                    dataGridView_item.Rows.Add(row);
                }
                dataGridView_item.Enabled = true;
                dataGridView_item.PerformLayout();
                dataGridView_item.ResumeLayout();
            }
        }

        private void change_value(object sender, DataGridViewCellEventArgs ea)
        {
            if (locked) { return; }
            if (listBox_items.CurrentCell != null && eLC != null)
            {
                int list = comboBox_lists.SelectedIndex;
                int item = listBox_items.CurrentCell.RowIndex;
                if (list > -1 && item > -1)
                {
                    DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                    for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        progress_bar("Changing ...", x, listBox_items.SelectedRows.Count);
                        int idx = listBox_items.SelectedRows[x].Index;
                        string valeu = "";
                        if (dataGridView_item.Rows[ea.RowIndex].Cells[ea.ColumnIndex] is DataGridViewCheckBoxCell)
                        {
                            valeu = Convert.ToString(dataGridView_item.Rows[ea.RowIndex].Cells[ea.ColumnIndex].Value);
                        }
                        else if (dataGridView_item.Rows[ea.RowIndex].Cells[ea.ColumnIndex] is DataGridViewComboBoxCell)
                        {
                            try
                            {
                                valeu = Convert.ToString(dataGridView_item.Rows[ea.RowIndex].Cells[ea.ColumnIndex].Value.ToString().Split('_')[1].ToString());
                            }
                            catch { valeu = Convert.ToString(dataGridView_item.Rows[ea.RowIndex].Cells[ea.ColumnIndex].Value); }
                        }
                        else
                        {
                            valeu = Convert.ToString(dataGridView_item.Rows[ea.RowIndex].Cells[ea.ColumnIndex].Value);
                        }
                        eLC.SetValue(list, idx, ea.RowIndex, ea.ColumnIndex, valeu);
                    }

                    if (ea.ColumnIndex == 1)
                    {
                        listBox_items_SelectionChanged(null, null);
                    }
                }
            }
            progress_bar("Ready");
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (locked) { return; }
            int current_list = comboBox_lists.SelectedIndex;
            if (current_list > -1 && listBox_items.CurrentCell != null)
            {
                locked = true;
                int item = listBox_items.CurrentCell.RowIndex;
                Dictionary<int, RowData> newd = new Dictionary<int, RowData>();
                RowData x1 = new RowData();
                x1.name = "Id";
                x1.type = (int)ObjType.Int;
                x1.value = -1;
                newd[0] = x1;
                x1 = new RowData();
                x1.name = "Name";
                x1.type = (int)ObjType.String;
                x1.value = "New item";
                newd[1] = x1;
                eLC.Lists[current_list].elementValues.Add(9999, newd);
                eLC.Lists[current_list].elementValues = reSortedDictionary(eLC.Lists[current_list].elementValues);
                locked = false;
                comboBox_lists_SelectedIndexChanged(null, null);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (locked) { return; }
            int current_list = comboBox_lists.SelectedIndex;
            if (current_list > -1 && listBox_items.CurrentCell != null)
            {
                int item = listBox_items.CurrentCell.RowIndex;
                DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                locked = true;
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    progress_bar("Deleting ...", x, listBox_items.SelectedRows.Count);
                    int idx = listBox_items.SelectedRows[x].Index;
                    eLC.Lists[current_list].elementValues.Remove(idx);
                }
                for (int i = selected.Count - 1; i >= 0; i--)
                {
                    listBox_items.Rows.Remove(selected[i]);
                }
                eLC.Lists[current_list].elementValues = reSortedDictionary(eLC.Lists[current_list].elementValues);

                locked = false;
            }
            progress_bar("Ready");
        }

        private void cloneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (locked) { return; }
            int current_list = comboBox_lists.SelectedIndex;
            if (current_list > -1 && listBox_items.CurrentCell != null)
            {
                int item = listBox_items.CurrentCell.RowIndex;
                DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                locked = true;
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    progress_bar("Cloning ...", x, listBox_items.SelectedRows.Count);
                    int idx = listBox_items.SelectedRows[x].Index;
                    Dictionary<int, RowData> cloned = eLC.Lists[current_list].elementValues[idx].ToDictionary(k => k.Key, k => new RowData(k.Value));

                    eLC.Lists[current_list].elementValues.Add(9999 - (x + 1), cloned);

                }
                eLC.Lists[current_list].elementValues = reSortedDictionary(eLC.Lists[current_list].elementValues);
                locked = false;
                comboBox_lists_SelectedIndexChanged(null, null);
            }
            progress_bar("Ready");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (locked) { return; }
            int current_list = comboBox_lists.SelectedIndex;
            if (current_list > -1 && listBox_items.CurrentCell != null)
            {
                int item = listBox_items.CurrentCell.RowIndex;
                DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                locked = true;
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    progress_bar("Cloning Row...", x, listBox_items.SelectedRows.Count);
                    int idx = listBox_items.SelectedRows[x].Index;
                    eLC.Lists[current_list].elementValues[idx].Add(9999, new RowData());
                    eLC.Lists[current_list].elementValues[idx] = reSortedDictionary(eLC.Lists[current_list].elementValues[idx]);
                }

                eLC.Lists[current_list].elementValues = reSortedDictionary(eLC.Lists[current_list].elementValues);
                locked = false;
                listBox_items_SelectionChanged(null, null);
            }
            progress_bar("Ready");
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (locked) { return; }
            int current_list = comboBox_lists.SelectedIndex;
            if (current_list > -1 && listBox_items.CurrentCell != null && dataGridView_item.CurrentCell != null)
            {
                int item = listBox_items.CurrentCell.RowIndex;

                DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                locked = true;
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    progress_bar("Deleting Row ...", x, listBox_items.SelectedRows.Count);
                    int idx = listBox_items.SelectedRows[x].Index;
                    DataGridViewSelectedRowCollection selectedrows = dataGridView_item.SelectedRows;
                    for (int i = 0; i < dataGridView_item.SelectedRows.Count; i++)
                    {
                        int xy = dataGridView_item.SelectedRows[i].Index;
                        eLC.Lists[current_list].RemoveRow(idx, xy);
                    }
                }

                eLC.Lists[current_list].elementValues = reSortedDictionary(eLC.Lists[current_list].elementValues);
                locked = false;
                listBox_items_SelectionChanged(null, null);
            }
            progress_bar("Ready");
        }

        private Dictionary<int, eItemCollection> reSortedDictionary(Dictionary<int, eItemCollection> data)
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

        private Dictionary<int, RowData> reSortedDictionary(Dictionary<int, RowData> data)
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

        private Dictionary<int, Dictionary<int, RowData>> reSortedDictionary(Dictionary<int, Dictionary<int, RowData>> data)
        {
            Dictionary<int, Dictionary<int, RowData>> datanew = new Dictionary<int, Dictionary<int, RowData>>();
            int i = 0;
            foreach (KeyValuePair<int, Dictionary<int, RowData>> entry in data)
            {
                datanew[i] = entry.Value;
                i++;
            }
            return datanew;
        }

        private void SaveList(object sender, EventArgs e)
        {
            if (locked) { return; }
            int selected_index = comboBox_lists.SelectedIndex;
            if (selected_index > -1)
            {
                int listID = 0;
                bool tryListId = int.TryParse(textBox_Id.Text.ToString(), out listID);
                if (tryListId)
                    eLC.Lists[selected_index].listID = listID;
                else
                {
                    MessageBox.Show("ListID must be numeric!");
                }
                string listName = textBox_Name.Text;
                eLC.Lists[selected_index].listName = listName;
                comboBox_lists.Items[selected_index] = listName;
            }
        }

        private void DeleteList(object sender, EventArgs e)
        {
            if (locked) { return; }
            int selected_index = comboBox_lists.SelectedIndex;
            if (selected_index > -1)
            {
                if (eLC.Lists.Count > 1)
                {
                    eLC.Lists.Remove(selected_index);
                    eLC.Lists = reSortedDictionary(eLC.Lists);
                    locked = true;
                    comboBox_lists.Items.Clear();
                    foreach (KeyValuePair<int, eItemCollection> sub in eLC.Lists)
                    {
                        comboBox_lists.Items.Add(sub.Value.listName);
                    }
                    locked = false;
                    comboBox_lists.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("You must keep at least one list in editor.");
                }
            }
        }

        private void AddNewList(object sender, EventArgs e)
        {
            if (locked) { return; }
            int selected_index = comboBox_lists.SelectedIndex;
            if (selected_index > -1)
            {
                int listID = 0;
                string listName = textBox_Name.Text;
                bool tryListId = int.TryParse(textBox_Id.Text.ToString(), out listID);
                if (tryListId)
                {
                    locked = true;
                    Dictionary<int, eItemCollection> newList = new Dictionary<int, eItemCollection>();
                    eItemCollection eItemCollection = new eItemCollection();
                    eItemCollection.listID = eLC.Lists.Count;
                    eItemCollection.listName = listName;

                    Dictionary<int, RowData> newd = new Dictionary<int, RowData>();
                    RowData x1 = new RowData();
                    x1.name = "Id";
                    x1.type = (int)ObjType.Int;
                    x1.value = -1;
                    newd[0] = x1;
                    x1 = new RowData();
                    x1.name = "Name";
                    x1.type = (int)ObjType.String;
                    x1.value = "New item";
                    newd[1] = x1;
                    eLC.Lists.Add(9999, eItemCollection);
                    eLC.Lists[9999].elementValues.Add(9999, newd);
                    eLC.Lists[9999].elementValues = reSortedDictionary(eLC.Lists[9999].elementValues);
                    eLC.Lists = reSortedDictionary(eLC.Lists);
                    locked = false;
                    comboBox_lists.Items.Add(listName);
                    comboBox_lists.SelectedIndex = eLC.Lists.Count - 1;
                }
                else
                {
                    MessageBox.Show("ListID must be numeric!");
                }

            }
        }

        private void EncriptPinChanged(object sender, EventArgs e)
        {

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog eSave = new SaveFileDialog();
            eSave.RestoreDirectory = false;
            eSave.Filter = "Elements File (*.data)|*.data|All Files (*.*)|*.*";
            if (eSave.ShowDialog() == System.Windows.Forms.DialogResult.OK && eSave.FileName != "")
            {
                if (eLC.SaveAs(eSave.FileName, MessageBox.Show("Do you want to encrypt the data?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes))
                {
                    MessageBox.Show("Saved!");
                }
            }
        }

        private void AddDefinition(object sender, EventArgs e)
        {
            if (loading)
                return;
            Settings data = new Settings();
            data.listId = -1;
            data.RowIndex = -1;
            data.rowValues = "";
            eLC.settingsList.Add(data);
            elementIntoTab_SelectedIndexChanged(null, null);
        }

        private void DelDefinition_Click(object sender, EventArgs e)
        {
            if (loading || DefinitionItems.CurrentCell == null) return;
            int selected_index = DefinitionItems.CurrentCell.RowIndex;
            try
            {
                eLC.settingsList.RemoveAt(selected_index);
                elementIntoTab_SelectedIndexChanged(null, null);
            }
            catch { }

        }

        private bool loading = false;
        private void elementIntoTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            loading = true;
            DefinitionItems.SuspendLayout();
            DefinitionItems.Rows.Clear();
            DefinitionItems.Enabled = false;
            foreach (Settings seting in eLC.settingsList)
            {
                DataGridViewRow row = (DataGridViewRow)DefinitionItems.RowTemplate.Clone();
                row.CreateCells(DefinitionItems);
                DataGridViewComboBoxCell cbc = new DataGridViewComboBoxCell();
                int c = 0;
                foreach (KeyValuePair<int, eItemCollection> sub in eLC.Lists)
                {
                    cbc.Items.Add(sub.Value.listName + "_" + c);
                    if (seting.listId == c)
                        cbc.Value = sub.Value.listName+"_"+ c;

                    c++;
                }
                row.Cells[0] = cbc;
                if (seting.listId != -1)
                {
                    try
                    {
                        DataGridViewComboBoxCell cbcx = new DataGridViewComboBoxCell();
                        foreach (KeyValuePair<int, RowData> sub in eLC.Lists[seting.listId].elementValues[0])
                        {
                            RowData data = sub.Value;
                            cbcx.Items.Add(data.name + "_" + sub.Key);
                            if (seting.RowIndex == sub.Key)
                                cbcx.Value = data.name + "_" + sub.Key;
                        }
                        row.Cells[1] = cbcx;
                    }
                    catch { row.Cells[1].Value = seting.RowIndex.ToString(); }
                }
                else
                {
                    row.Cells[1].Value = seting.RowIndex.ToString();
                }
                row.Cells[2].Value = seting.rowValues;
                DefinitionItems.Rows.Add(row);
            }
            DefinitionItems.Enabled = true;
            DefinitionItems.PerformLayout();
            DefinitionItems.ResumeLayout();
            loading = false;
        }

        private void DefinitionItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (loading || DefinitionItems.CurrentCell == null) return;
            int selected_index = DefinitionItems.CurrentCell.RowIndex;
            int row = DefinitionItems.CurrentCell.ColumnIndex;
            try
            {
                loading = true;
                string value = DefinitionItems.Rows[selected_index].Cells[row].Value.ToString();
                if (row == 0 && DefinitionItems.Rows[selected_index].Cells[row] is DataGridViewComboBoxCell)
                {
                    eLC.settingsList[selected_index].listId = int.Parse(DefinitionItems.Rows[selected_index].Cells[row].Value.ToString().Split('_')[1]);
                    eLC.settingsList[selected_index].RowIndex = 0;
                    elementIntoTab_SelectedIndexChanged(null, null);
                }
                else
                {
                    if (row == 0)
                    {
                        eLC.settingsList[selected_index].listId = int.Parse(value);
                        eLC.settingsList[selected_index].RowIndex = 0;
                        elementIntoTab_SelectedIndexChanged(null, null);
                    }
                }

                if (row == 1 && DefinitionItems.Rows[selected_index].Cells[row] is DataGridViewComboBoxCell)
                    eLC.settingsList[selected_index].RowIndex = int.Parse(DefinitionItems.Rows[selected_index].Cells[row].Value.ToString().Split('_')[1]);
                else
                     if (row == 1)
                        eLC.settingsList[selected_index].RowIndex = int.Parse(value);


                if (row == 2)
                     eLC.settingsList[selected_index].rowValues = value;
            }
            catch
            {

            }
            loading = false;
        }

        private void moveUPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (locked) { return; }
            int current_list = comboBox_lists.SelectedIndex;
            if (current_list > -1 && listBox_items.CurrentCell != null && dataGridView_item.CurrentCell != null)
            {
                int item = listBox_items.CurrentCell.RowIndex;

                DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                locked = true;
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    progress_bar("Moving Row ...", x, listBox_items.SelectedRows.Count);
                    int idx = listBox_items.SelectedRows[x].Index;
                    DataGridViewSelectedRowCollection selectedrows = dataGridView_item.SelectedRows;
                    for (int i = 0; i < dataGridView_item.SelectedRows.Count; i++)
                    {
                        int xy = dataGridView_item.SelectedRows[i].Index;
                        eLC.Lists[current_list].MoveUp(idx, xy);
                    }
                }

                eLC.Lists[current_list].elementValues = reSortedDictionary(eLC.Lists[current_list].elementValues);
                locked = false;
                listBox_items_SelectionChanged(null, null);
            }
            progress_bar("Ready");
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (locked) { return; }
            int current_list = comboBox_lists.SelectedIndex;
            if (current_list > -1 && listBox_items.CurrentCell != null && dataGridView_item.CurrentCell != null)
            {
                int item = listBox_items.CurrentCell.RowIndex;

                DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                locked = true;
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    progress_bar("Moving Row ...", x, listBox_items.SelectedRows.Count);
                    int idx = listBox_items.SelectedRows[x].Index;
                    DataGridViewSelectedRowCollection selectedrows = dataGridView_item.SelectedRows;
                    for (int i = 0; i < dataGridView_item.SelectedRows.Count; i++)
                    {
                        int xy = dataGridView_item.SelectedRows[i].Index;
                        eLC.Lists[current_list].MoveDown(idx, xy);
                    }
                }

                eLC.Lists[current_list].elementValues = reSortedDictionary(eLC.Lists[current_list].elementValues);
                locked = false;
                listBox_items_SelectionChanged(null, null);
            }
            progress_bar("Ready");
        }
    }
}
