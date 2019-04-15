using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MyRegViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            RegistryKey[] regs = new RegistryKey[] { Registry.ClassesRoot, Registry.CurrentUser, //};
                Registry.LocalMachine, Registry.Users, Registry.CurrentConfig};
            listView1.Columns.Add("Имя", 100);
            listView1.Columns.Add("Тип", 100);
            listView1.Columns.Add("Значение", 150);
            listView1.View = View.Details;
            toolStripStatusLabel1.Text = regs[0].Name;
            foreach (RegistryKey key in regs)
            {
                TreeNode node = new TreeNode(key.Name);
                node.Tag = key;
                await Task.Run(() => AddNodes(node, key, 0));
                treeView1.Nodes.Add(node);
            }
        }

        private void AddNodes(TreeNode parentNode, RegistryKey key, int depth)
        {
            if (depth < 2)
            {
                depth++;
                string[] subKeys = key.GetSubKeyNames();
                if (subKeys.Length > 0)
                {
                    for (int i = 0; i < subKeys.Length; i++)
                    {
                        try
                        {
                            TreeNode node = new TreeNode(subKeys[i]);
                            UpdateTreeView(parentNode, node);
                            RegistryKey subKey = key.OpenSubKey(subKeys[i]);
                            node.Tag = subKey;
                            AddNodes(node, subKey, depth);
                        }
                        catch { }
                    }
                }
            }
            
        }

        private void UpdateTreeView(TreeNode parentNode, TreeNode childNode)
        {
            if (treeView1.InvokeRequired)
            {
                treeView1.Invoke(new Action<TreeNode, TreeNode>(UpdateTreeView), parentNode, childNode);
            }
            else
            {
                parentNode.Nodes.Add(childNode);
            }
        }

        private async void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listView1.Items.Clear();
            toolStripStatusLabel1.Text = e.Node.FullPath;
            TreeNode node = e.Node;
            if (node.Tag is RegistryKey)
            {
                RegistryKey selectedKey = node.Tag as RegistryKey;
                string[] names = selectedKey.GetValueNames();
                foreach (string str in names)
                {
                    try
                    {
                        ListViewItem item = null;
                        if (string.IsNullOrEmpty(str))
                            item = new ListViewItem("(По умолчанию)");
                        else
                            item = new ListViewItem(str);
                        RegistryValueKind valueKind = selectedKey.GetValueKind(str);
                        item.SubItems.Add(valueKind.ToString());
                        object keyValue = selectedKey.GetValue(str);
                        string stringValue = "";
                        if (keyValue == null || string.IsNullOrEmpty(keyValue.ToString()))
                        {
                            stringValue = "(значение не присвоено)";
                        }
                        else
                        if (valueKind == RegistryValueKind.Binary)
                        {
                            byte[] buff = (byte[])keyValue;
                            string temp = "";
                            foreach (byte elem in buff)
                            {
                                temp += $"{elem:X4}";
                            }
                            stringValue = temp;
                        }
                        else
                        {
                            stringValue = keyValue.ToString();
                        }
                        item.SubItems.Add(stringValue);
                        listView1.Items.Add(item);
                    }
                    catch { }
                }
                
                if(e.Node.FullPath.Contains(@"\"))
                {
                    node.Nodes.Clear();
                    await Task.Run(() => AddNodes(node, selectedKey, 0));
                }
            }
        }
    }
}
