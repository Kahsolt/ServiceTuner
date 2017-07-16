using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ServiceTuner
{
    public partial class Form_GetGroupName : Form
    {
        private TextBox newName;
        private List<string> nameList;

        /*构造函数*/
        public Form_GetGroupName(ref TextBox name,ref List<string> names)
        {
            InitializeComponent();
            newName = name;
            nameList = names;
        }
        private void Form_GetGroupName_Load(object sender, EventArgs e)
        {
            textBox_NewGroupName.Focus();
        }

        /*事件响应函数*/
        private void button_NewGroupName_Click(object sender, EventArgs e)
        {
            newName.Text = textBox_NewGroupName.Text;
            if (newName.Text == "")
            {
                MessageBox.Show("组名不能为空啊！","出错啦...");
                return;
            }
            else if(newName.Text.StartsWith("#"))
            {
                MessageBox.Show("组名不能以井号#开头！", "出错啦...");
                return;
            }
            foreach(string name in nameList)
                if(newName.Text == name)
                {
                    MessageBox.Show("已经有叫这个名字的组啦！", "出错啦...");
                    return;
                }
            this.Dispose();
        }


    }
}
