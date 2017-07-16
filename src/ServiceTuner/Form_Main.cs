using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ServiceTuner
{
    public partial class Form_Main : Form
    {
        /*常量定义*/
        private const int ITEMS_COUNT = 7;

        /*当前状态记录变量*/
        private ToolStripMenuItem currentRefreshFrequency;
        private ToolStripMenuItem currentStartupType;
        private List<SortOrder> currentOrders;
        private bool filterEmpty;

        /*成员字段*/
        private static ServiceTuner serviceTuner = new ServiceTuner();
        private static ListViewSorter listViewSorter = new ListViewSorter();

        /*构造函数*/
        public Form_Main()
        {
            InitializeComponent();
        }
        private void Form_Main_Load(object sender, EventArgs e)
        {
            //显示首页-进程页
            currentRefreshFrequency = 中ToolStripMenuItem;
            currentStartupType = toolStripMenuItem_Auto;
            currentOrders = new List<SortOrder>();
            for (int i = 1; i <= ITEMS_COUNT; i++)
                currentOrders.Add(SortOrder.Descending);
            this.listView_Service.ListViewItemSorter = listViewSorter;
            
            //筛选器预提示
            toolStripTextBox_Filter.Text = "筛选器";
            toolStripTextBox_Filter.Font = new Font(toolStripTextBox_Filter.Font, toolStripTextBox_Filter.Font.Style | FontStyle.Italic);
            filterEmpty = true;

            //展示列表，启动时计
            RefreshService();
            timer.Start();
            this.Focus();
        }

        /*ToolStripMenu-主窗口菜单栏*/
        private void 运行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process runDialog = new Process();
            runDialog.StartInfo.FileName = "C:\\Windows\\System32\\rundll32.exe";
            runDialog.StartInfo.Arguments = "shell32.dll,#61";
            runDialog.Start();
        }
        private void 导出为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName;
            string seperator;

            //导出的文件名
            fileName = "ServiceTuner_";
            saveFileDialog_ListInfo.FileName = fileName + DateTime.Now.ToString("yyyy年MM月dd日hh时mm分ss秒");
            saveFileDialog_ListInfo.ShowDialog();
            fileName = saveFileDialog_ListInfo.FileName;

            if(saveFileDialog_ListInfo.FileName.EndsWith(".csv"))
                seperator = ",";
            else
                seperator = "\t";

            //写出
            StreamWriter streamWriter;
            if (File.Exists(fileName))
                streamWriter = new StreamWriter(fileName,false, Encoding.ASCII);
            else
                streamWriter = File.CreateText(fileName);
            for (int i = 0; i < listView_Service.Items.Count; i++)
            {
                for (int j = 0; j < listView_Service.Items[i].SubItems.Count; j++)
                    streamWriter.Write(listView_Service.Items[i].SubItems[j].Text + seperator);
                streamWriter.WriteLine();
            }
            streamWriter.Flush();
            streamWriter.Close();
        }
        private void 截屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //窗口打印
            IntPtr hWnd = this.Handle;
            IntPtr hscrdc = GetWindowDC(hWnd);
            Control control = FromHandle(hWnd);
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, control.Width, control.Height);
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = Image.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);
            DeleteDC(hmemdc);//删除用过的对象

            //保存的文件名
            string fileName;
                fileName = "ServiceTuner_";
            saveFileDialog_SnapShot.FileName = fileName + DateTime.Now.ToString("yyyy年MM月dd日hh时mm分ss秒");
            saveFileDialog_SnapShot.ShowDialog();
            fileName = saveFileDialog_SnapShot.FileName;
            if (fileName == "")
                return;//空退出则返回

            //写出
            if (saveFileDialog_SnapShot.FileName.EndsWith(".png"))
                bmp.Save(fileName, ImageFormat.Png);
            else if (saveFileDialog_SnapShot.FileName.EndsWith(".jpg"))
                bmp.Save(fileName, ImageFormat.Jpeg);
            else
                bmp.Save(fileName);
        }
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }
        private void 快ToolStripMenuItem_Click(object sender, EventArgs e)//更新
        {
            currentRefreshFrequency.Checked = false;
            快ToolStripMenuItem.Checked = true;
            currentRefreshFrequency = 快ToolStripMenuItem;
            ChangeTimerInterval();
        }
        private void 中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentRefreshFrequency.Checked = false;
            中ToolStripMenuItem.Checked = true;
            currentRefreshFrequency = 中ToolStripMenuItem;
            ChangeTimerInterval();
        }
        private void 慢ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentRefreshFrequency.Checked = false;
            慢ToolStripMenuItem.Checked = true;
            currentRefreshFrequency = 慢ToolStripMenuItem;
            ChangeTimerInterval();
        }
        private void 停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentRefreshFrequency.Checked = false;
            停止ToolStripMenuItem.Checked = true;
            currentRefreshFrequency = 停止ToolStripMenuItem;
            ChangeTimerInterval();
        }
        private void 编辑配置文件IToolStripMenuItem_Click(object sender, EventArgs e)//数据
        {
            serviceTuner.EditDB();
        }
        private void 还原默认配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要还原默认配置文件吗？\n如果“服务组”功能无法正常工作，还原配置文件可以修复此问题。\n\n注意：自定义服务组将被删除！", "还原确认...", MessageBoxButtons.OKCancel) == DialogResult.OK)
                serviceTuner.ResetDB();
        }
        private void 窗口置顶ToolStripMenuItem_Click(object sender, EventArgs e)//选项
        {
            窗口置顶ToolStripMenuItem.Checked = !窗口置顶ToolStripMenuItem.Checked;
            if (窗口置顶ToolStripMenuItem.Checked)
                this.TopMost = true;
            else
                this.TopMost = false;
        }
        private void 透明度ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            窗口半透明ToolStripMenuItem.Checked = !窗口半透明ToolStripMenuItem.Checked;
            if (窗口半透明ToolStripMenuItem.Checked)
                this.Opacity = 0.85;
            else
                this.Opacity = 1.0;
        }
        private void 最小化至托盘ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            最小化至托盘ToolStripMenuItem.Checked = !最小化至托盘ToolStripMenuItem.Checked;
            if (最小化至托盘ToolStripMenuItem.Checked)
                notifyIcon.Visible = true;
            else
                notifyIcon.Visible = false;
        }
        private void 使用说明ToolStripMenuItem_Click(object sender, EventArgs e)//帮助
        {
            string userManaul = "";
            userManaul += "高级功能说明：\n\n";
            userManaul += "1.双击列表中的任意行服务项，可查看此服务的继承关系。\n";
            userManaul += "2.单击列表标头，可切换升降序进行排序。\n";
            userManaul += "3.列表配有右键快捷菜单，默认打开系统通知区图标。\n";
            userManaul += "4.服务组配置文件的格式：前导井号#的组名+组内服务数量+所有服务名；组之间用空行分隔。\n";
            userManaul += "===============================================\n";
            userManaul += "版本缺陷说明：\n\n";
            userManaul += "1.筛选器不机智，因为输入的文本每个字符都会触发一次TextChange事件。\n";
            userManaul += "2.服务描述多为空白，因为C#不能直接解读DLL文件中的间接字符串资源。\n";
            userManaul += "3.文件属性查看和定位有时失效，因为调用C++动态链接库中的函数似乎不是用Administrator身份，导致Explorer访问权限不足。\n";
            userManaul += "\n";

            MessageBox.Show(userManaul, "使用说明...");
        }
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("软件名称：ServiceTuner Press版\n" +
                "版本序号：V1.2\n" +
                "作者信息：Kahsolt\n" +
                "功能简介：服务管理小工具\n\t 作为 DIY任务管理器 的一个子集\n" +
                "发布时间：2016年6月",
                "关于ServiceTuner...", MessageBoxButtons.OK);
        }
        private void toolStripTextBox_Filter_Enter(object sender, EventArgs e)//筛选器
        {
            if(filterEmpty)
            {
                toolStripTextBox_Filter.Text = "";
                toolStripTextBox_Filter.Font = new Font(toolStripTextBox_Filter.Font, toolStripTextBox_Filter.Font.Style ^ FontStyle.Italic);
            }
        }
        private void toolStripTextBox_Filter_Leave(object sender, EventArgs e)
        {
            if (toolStripTextBox_Filter.Text == "")
            {
                filterEmpty = true;
                toolStripTextBox_Filter.Text = "筛选器";
                toolStripTextBox_Filter.Font = new Font(toolStripTextBox_Filter.Font, toolStripTextBox_Filter.Font.Style | FontStyle.Italic);
            }
            else filterEmpty = false;
        }
        private void toolStripTextBox_Filter_TextChanged(object sender, EventArgs e)
        {
            if (!toolStripTextBox_Filter.Focused)//不在用户焦点时系统引起的TextChange，略过
                return;
            else if (toolStripTextBox_Filter.Text == "" && filterEmpty)//在用户焦点时但为空(经历过一次Leave事件)略过
                return;
            
            //在用户焦点，调整当前筛选器状态
            if (toolStripTextBox_Filter.Text == "")
                filterEmpty = true;
            else
                filterEmpty = false;
                
            RefreshService();
        }

        /*ToolStrip-工具栏*/
        private void toolStripButton_Service_Start_Click(object sender, EventArgs e)
        {
            if (listView_Service.SelectedItems.Count == 0)
                return;

            if (serviceTuner.StartService(listView_Service.FocusedItem.SubItems[1].Text))//Index=1是服务名
                listView_Service.FocusedItem.SubItems[2].Text = "正在运行";//Index=2是运行状态
            else
                MessageBox.Show("Error: 启动失败", "服务操作回执...");

            listView_Service_SelectedIndexChanged(sender, e);
        }
        private void toolStripButton_Service_Restart_Click(object sender, EventArgs e)
        {
            if (listView_Service.SelectedItems.Count == 0)
                return;
            if (serviceTuner.RestartService(listView_Service.FocusedItem.SubItems[1].Text))//Index=1是服务名
                listView_Service.FocusedItem.SubItems[2].Text = "正在运行";//Index=2是运行状态
            else
                MessageBox.Show("Error: 重新启动失败", "服务操作回执...");

            listView_Service_SelectedIndexChanged(sender, e);
        }
        private void toolStripButton_Service_Stop_Click(object sender, EventArgs e)
        {
            if (listView_Service.SelectedItems.Count == 0)
                return;

            if (serviceTuner.StopService(listView_Service.FocusedItem.SubItems[1].Text))//Index=1是服务名
                listView_Service.FocusedItem.SubItems[2].Text = "已停止";//Index=2是运行状态
            else
                MessageBox.Show("Error: 停止失败", "服务操作回执...");

            listView_Service_SelectedIndexChanged(sender, e);
        }
        private void toolStripDropDownButton_Service_StartupType_DropDownOpening(object sender, EventArgs e)
        {
            if (listView_Service.SelectedItems.Count == 0)
                return;

            switch (listView_Service.FocusedItem.SubItems[3].Text)//index=3是启动类型
            {
                case "自动":
                    currentStartupType.Checked = false;
                    currentStartupType = toolStripMenuItem_Auto;
                    currentStartupType.Checked = true;
                    break;
                case "手动":
                    currentStartupType.Checked = false;
                    currentStartupType = toolStripMenuItem_Manual;
                    currentStartupType.Checked = true;
                    break;
                case "已禁用":
                    currentStartupType.Checked = false;
                    currentStartupType = toolStripMenuItem_Disabled;
                    currentStartupType.Checked = true;
                    break;
                default:
                    MessageBox.Show("Error: 此项被标记为系统服务,\n为安全起见暂时不支持更改启动类型！", "保护性提示...");
                    break;
            }
        }
        private void toolStripMenuItem_Manual_Click(object sender, EventArgs e)
        {
            if (serviceTuner.ChangeStartupType(listView_Service.FocusedItem.SubItems[1].Text, 3))//index=1是服务名
                listView_Service.FocusedItem.SubItems[3].Text = "手动";
        }
        private void toolStripMenuItem_Auto_Click(object sender, EventArgs e)
        {
            if (serviceTuner.ChangeStartupType(listView_Service.FocusedItem.SubItems[1].Text, 2))//index=1是服务名
                listView_Service.FocusedItem.SubItems[3].Text = "自动";
        }
        private void toolStripMenuItem_Disabled_Click(object sender, EventArgs e)
        {
            if (serviceTuner.ChangeStartupType(listView_Service.FocusedItem.SubItems[1].Text, 4))//Index=1是服务名
                listView_Service.FocusedItem.SubItems[3].Text = "已禁用";
        }
        private void toolStripButton_Service_Refresh_Click(object sender, EventArgs e)
        {
            RefreshService();
        }
        private void toolStripButton_Service_GroupAdd_Click(object sender, EventArgs e)
        {
            if (listView_Service.SelectedItems.Count == 0)
            {
                MessageBox.Show("提示：请先选择列表中的一或多个服务项！", "服务组使用方法...");
                return;
            }

            //事先缓存，防止取名字时自动刷新刷没了
            ListView.SelectedListViewItemCollection selectedServices = listView_Service.SelectedItems;

            TextBox tmp_newName = new TextBox();
            List<string> namesUsed = new List<string>();
            new Form_GetGroupName(ref tmp_newName, ref namesUsed).ShowDialog();//ShowDialog会等待返回
            string newGroupName = tmp_newName.Text;
            tmp_newName.Dispose();

            if (newGroupName == "")
                return;//无名退出返回

            List<string> serviceNames = new List<string>();
            foreach(ListViewItem service in selectedServices)
                serviceNames.Add(service.SubItems[1].Text);//Index=1是服务名

            serviceTuner.AddServiceGroup(newGroupName, serviceNames);
        }
        private void toolStripButton_Service_GroupDelete_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox_Service_Group.Text == "")
                return;

            serviceTuner.DeleteServiceGroup(toolStripComboBox_Service_Group.Text);
            toolStripComboBox_Service_Group.Items.Remove(toolStripComboBox_Service_Group.SelectedItem);

            if (toolStripComboBox_Service_Group.Items.Count == 0)
                toolStripComboBox_Service_Group.SelectedItem = null;
            else
                toolStripComboBox_Service_Group.SelectedIndex = 0;
        }
        private void toolStripLabel_Service_LoadGroup_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox_Service_Group.Text == "")
                return;

            List<string> services = serviceTuner.GetServiceGroup(toolStripComboBox_Service_Group.Text);
            string serviceGroup = "服务组“" + toolStripComboBox_Service_Group.Text + "”共计" + services.Count + "项：\n\n";
            foreach (string service in services)
                serviceGroup += service + "\n";

            MessageBox.Show(serviceGroup, "服务组详细...");
        }
        private void toolStripComboBox_Service_Group_Click(object sender, EventArgs e)
        {
            List<string> serviceGroups = serviceTuner.GetServiceGroups();

            toolStripComboBox_Service_Group.Items.Clear();
            foreach (string serviceGroup in serviceGroups)
                toolStripComboBox_Service_Group.Items.Add(serviceGroup);
        }
        private void toolStripButton_Service_GroupStart_Click(object sender, EventArgs e)
        {
            List<string> services = serviceTuner.GetServiceGroup(toolStripComboBox_Service_Group.Text);

            string receipt = "尝试启动服务组，回执如下：\n\n";
            foreach (string service in services)
            {
                serviceTuner.StartService(service);
                if (serviceTuner.GetStatus(service))
                    receipt += (service + "\t正在运行\n");
                else
                    receipt += (service + "\t已停止\n");
            }
            MessageBox.Show(receipt, "服务组操作回执...");

            RefreshService();
        }
        private void toolStripButton_Service_GroupRestart_Click(object sender, EventArgs e)
        {
            List<string> services = serviceTuner.GetServiceGroup(toolStripComboBox_Service_Group.Text);

            string receipt = "尝试重新启动服务组，回执如下：\n\n";
            foreach (string service in services)
            {
                serviceTuner.RestartService(service);
                if (serviceTuner.GetStatus(service))
                    receipt += (service + "\t正在运行\n");
                else
                    receipt += (service + "\t已停止\n");
            }
            MessageBox.Show(receipt, "服务组操作回执...");

            RefreshService();
        }
        private void toolStripButton_Service_GroupStop_Click(object sender, EventArgs e)
        {
            List<string> services = serviceTuner.GetServiceGroup(toolStripComboBox_Service_Group.Text);

            string receipt = "尝试停止动服务组，回执如下：\n\n";
            foreach (string service in services)
            {
                serviceTuner.StopService(service);
                if (serviceTuner.GetStatus(service))
                    receipt += (service + "\t正在运行\n");
                else
                    receipt += (service + "\t已停止\n");
            }
            MessageBox.Show(receipt, "服务组操作回执...");

            RefreshService();
        }
        private void toolStripButton_Service_ImageAttributes_Click(object sender, EventArgs e)
        {
            if (listView_Service.SelectedItems.Count == 0)
                return;

            serviceTuner.ShowImageAttribute(listView_Service.FocusedItem.SubItems[1].Text);//Index=1是服务名
        }
        private void toolStripButton_Service_ImageDirectory_Click(object sender, EventArgs e)
        {
            if (listView_Service.SelectedItems.Count == 0)
                return;

            serviceTuner.ShowImageDirectory(listView_Service.FocusedItem.SubItems[1].Text);//Index=1是服务名
        }

        /*ListView-服务列表事件*/
        private void listView_Service_DoubleClick(object sender, EventArgs e)
        {
            if (listView_Service.FocusedItem == null)
                return;

            List<string> servicesDependedOn = new List<string>();
            List<string> dependentServices = new List<string>();
            serviceTuner.GetDependingInfo(listView_Service.FocusedItem.SubItems[1].Text, ref servicesDependedOn, ref dependentServices);


            string dependingInfo = "=====服务 " + listView_Service.FocusedItem.SubItems[1].Text + " 的依赖关系=====\n\n";
            dependingInfo += "此服务依赖(上级)：" + servicesDependedOn.Count + "项\n";
            if (servicesDependedOn.Count == 0)
                dependingInfo += "(无)\n";
            else
                foreach (string service in servicesDependedOn)
                    dependingInfo += service + "\n";
            dependingInfo += "\n";//间行
            dependingInfo += "依赖此服务(下级)：" + dependentServices.Count + "项\n";
            if (dependentServices.Count == 0)
                dependingInfo += "(无)\n";
            else
                foreach (string service in dependentServices)
                    dependingInfo += service + "\n";

            MessageBox.Show(dependingInfo,"服务依赖关系...");
            return;
        }
        private void listView_Service_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView_Service.FocusedItem == null)
                return;

            if (listView_Service.FocusedItem.SubItems[3].Text == "已禁用")
            {
                toolStripButton_Service_Start.Enabled = false;//工具栏
                toolStripButton_Service_Restart.Enabled = false;
                toolStripButton_Service_Stop.Enabled = false;
                toolStripMenuItem_Start.Enabled = false;//右键菜单
                toolStripMenuItem_Restart.Enabled = false;
                toolStripMenuItem_Stop.Enabled = false;
                return;
            }
                
            switch (listView_Service.FocusedItem.SubItems[2].Text)
            {
                case "正在运行":
                    toolStripButton_Service_Start.Enabled = false;
                    toolStripButton_Service_Restart.Enabled = true;
                    toolStripButton_Service_Stop.Enabled = true;
                    toolStripMenuItem_Start.Enabled = false;
                    toolStripMenuItem_Restart.Enabled = true;
                    toolStripMenuItem_Stop.Enabled = true;
                    break;
                case "已停止":
                    toolStripButton_Service_Start.Enabled = true;
                    toolStripButton_Service_Restart.Enabled = false;
                    toolStripButton_Service_Stop.Enabled = false;
                    toolStripMenuItem_Start.Enabled = true;
                    toolStripMenuItem_Restart.Enabled = false;
                    toolStripMenuItem_Stop.Enabled = false;
                    break;
                default:
                    break;
            }
        }
        private void listView_Service_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (currentOrders[e.Column] == SortOrder.Ascending)//反序
                currentOrders[e.Column] = SortOrder.Descending;
            else
                currentOrders[e.Column] = SortOrder.Ascending;

            listViewSorter.SortColumn = e.Column;
            listViewSorter.SortOrder = currentOrders[e.Column];
            listView_Service.Sort();
        }

        /*ListView-服务列表右键菜单事件-额外*/
        private void toolStripMenuItem_StartupType_DropDownOpening(object sender, EventArgs e)
        {
            if (listView_Service.FocusedItem == null)
                return;

            switch (listView_Service.FocusedItem.SubItems[3].Text)
            {
                case "自动":
                    toolStripMenuItem_context_Manual.Checked = false;
                    toolStripMenuItem_context_Auto.Checked = true;
                    toolStripMenuItem_context_Disabled.Checked = false;
                    break;
                case "手动":
                    toolStripMenuItem_context_Manual.Checked = true;
                    toolStripMenuItem_context_Auto.Checked = false;
                    toolStripMenuItem_context_Disabled.Checked = false;
                    break;
                case "已禁用":
                    toolStripMenuItem_context_Manual.Checked = false;
                    toolStripMenuItem_context_Auto.Checked = false;
                    toolStripMenuItem_context_Disabled.Checked = true;
                    break;
            }
        }
        private void toolStripMenuItem_CopyServiceName_Click(object sender, EventArgs e)
        {
            string serviceDisplayName = listView_Service.FocusedItem.SubItems[0].Text;//复制显示名
            Clipboard.SetDataObject(serviceDisplayName);
        }
        private void toolStripMenuItem_SearchOnline_Click(object sender, EventArgs e)
        {
            string serviceDisplayName = listView_Service.FocusedItem.SubItems[0].Text;//复制显示名
            string webpage = "https://www.baidu.com/s?ie=UTF-8&wd=" + serviceDisplayName;
            Process.Start(webpage);
        }

        /*Timer-时计*/
        private void timer_Tick(object sender, EventArgs e)
        {
            RefreshService();
        }

        /*NotifyIcon-系统托盘*/
        private void Form_Main_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                if (最小化至托盘ToolStripMenuItem.Checked)
                {
                    this.ShowInTaskbar = false;
                    notifyIcon.ShowBalloonTip(1000);
                    Thread.Yield();
                }
                else
                    this.ShowInTaskbar = true;
        }
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.Activate();
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }
        private void toolStripMenuItem_ShowMain_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.Activate();
            }
        }
        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //工具函数
        protected void ChangeTimerInterval()
        {
            if (currentRefreshFrequency == 停止ToolStripMenuItem)
                timer.Stop();
            else
            {
                if (currentRefreshFrequency == 快ToolStripMenuItem)
                    timer.Interval = 30000;
                else if (currentRefreshFrequency == 中ToolStripMenuItem)
                    timer.Interval = 60000;
                else if (currentRefreshFrequency == 慢ToolStripMenuItem)
                    timer.Interval = 90000;
                else
                    MessageBox.Show("Fatal Error: 刷新速率错误！", "系统错误！");
            }
        }
        protected void RefreshService()
        {
            if(filterEmpty)//如果筛选器为空，直接更新列表
            {
                serviceTuner.GetServices(listView_Service);
                toolStripStatusLabel_Status.Text = "共有" + listView_Service.Items.Count.ToString() + "项";//状态栏更新
            }
            else//否则，经过筛选器筛选
            {
                string filter = toolStripTextBox_Filter.Text;//取得筛选器值
                ListView filterList = new ListView();
                serviceTuner.GetServices(filterList);//获取服务列表到临时ListView

                listView_Service.BeginUpdate();
                listView_Service.Items.Clear();//清空当前列表
                foreach (ListViewItem item in filterList.Items)//筛选合格的加入当前列表
                    foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                        if (subItem.Text.ToLowerInvariant().Contains(filter.ToLowerInvariant()))
                        {
                            listView_Service.Items.Add((ListViewItem)item.Clone());
                            break;
                        }
                listView_Service.EndUpdate();

                toolStripStatusLabel_Status.Text = "选出" + listView_Service.Items.Count.ToString() + "项 (经过筛选器“" + filter + "”)";//状态栏更新
            }
        }

        #region Call DLLs - 支持截图
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        #endregion
    }
}
