using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace ServiceTuner
{
    public partial class ServiceTuner
    {
        /*成员字段*/
        private string FilePath;

        /*构析函数*/
        public ServiceTuner()
        {
            FilePath = Environment.CurrentDirectory + @"\ServiceTuner.ini";//暂定为本目录下的ini文件
            if (File.Exists(FilePath))
                GetServiceGroups();
            else
                LoadDefaultServiceGroup();
        }

        /*成员函数*/
        //FileDB
        public List<string> GetServiceGroups()
        {
            List<string> serviceGroupNames = new List<string>();
            try
            {
                StreamReader ReadFile_ServiceGroup = new StreamReader(FilePath, Encoding.GetEncoding(936));
                string line;
                while ((line = ReadFile_ServiceGroup.ReadLine()) != null)
                    if (line.StartsWith("#"))//约定组名以#标头
                        serviceGroupNames.Add(line.TrimStart('#'));
                ReadFile_ServiceGroup.Close();
            }
            catch (IOException)
            {
                Error_DBFile();
                return null;
            }
            return serviceGroupNames;
        }
        public List<string> GetServiceGroup(string groupName)
        {
            List<string> serviceNames = new List<string>();

            try
            {
                StreamReader ReadFile_ServiceGroup = new StreamReader(FilePath, Encoding.GetEncoding(936));
                string line;
                while ((line = ReadFile_ServiceGroup.ReadLine()) != null)
                    if (line.StartsWith("#") && line.TrimStart('#') == groupName)
                    {
                        int recordCnt = Convert.ToInt32(ReadFile_ServiceGroup.ReadLine());
                        for (int i = 1; i <= recordCnt; i++)
                        {
                            line = ReadFile_ServiceGroup.ReadLine();
                            if (line == null) throw new IOException();
                            else serviceNames.Add(line);
                        }
                        break;
                    }
                ReadFile_ServiceGroup.Close();
            }
            catch (IOException)
            {
                Error_DBFile();
                return null;
            }

            return serviceNames;
        }
        public bool AddServiceGroup(string groupName, List<string> serviceNames)
        {
            try
            {
                StreamWriter WriteFile_ServiceGroup = new StreamWriter(FilePath, true, Encoding.GetEncoding(936));
                WriteFile_ServiceGroup.Write("#" + groupName + "\r\n");//约定组名以#标头
                WriteFile_ServiceGroup.Write(serviceNames.Count.ToString() + "\r\n");
                foreach (string serviceName in serviceNames)
                    WriteFile_ServiceGroup.Write(serviceName + "\r\n");
                WriteFile_ServiceGroup.Write("\r\n");
                WriteFile_ServiceGroup.Close();
            }
            catch (IOException)
            {
                Error_DBFile();
                return false;
            }
            return true;
        }
        public bool DeleteServiceGroup(string groupName)
        {
            try
            {
                StreamReader ReadFile_ServiceGroup = new StreamReader(FilePath, Encoding.GetEncoding(936));
                string line;
                int ptr = -1;//假定从0行开始计数
                while ((line = ReadFile_ServiceGroup.ReadLine()) != null)
                {
                    ptr++;//假定从0行开始计数
                    if (line.StartsWith("#") && line.TrimStart('#') == groupName)
                    {
                        int recordCnt = Convert.ToInt32(ReadFile_ServiceGroup.ReadLine());
                        recordCnt += 3;//将要删除的行比存储的服务名个数多3

                        ReadFile_ServiceGroup.Close();//强制解除占用

                        List<string> lines = new List<string>(File.ReadAllLines(FilePath, Encoding.GetEncoding(936)));
                        for (int i = 0; i < recordCnt; i++)
                            lines.RemoveAt(ptr);
                        File.WriteAllLines(FilePath, lines.ToArray(), Encoding.GetEncoding(936));//读出-写回
                        return true;
                    }
                }
                ReadFile_ServiceGroup.Close();
            }
            catch (IOException)
            {
                Error_DBFile();
                return false;
            }
            return false;
        }
        public void EditDB()
        {
            ProcessStartInfo info = new ProcessStartInfo("Notepad.exe", FilePath);
            Process.Start(info);
        }
        public void ResetDB()
        {
            LoadDefaultServiceGroup();
        }
        //FileDB-工具函数
        private void LoadDefaultServiceGroup()
        {
            StreamWriter WriteFile_ServiceGroup = new StreamWriter(FilePath, false, Encoding.GetEncoding(936));

            string defaultServiceGroup = "#SQL Server\r\n5\r\nMSSQL$KAHSOLT\r\nSQLBrowser\r\nSQLTELEMETRY$KAHSOLT\r\nSQLWriter\r\nSQLAgent$KAHSOLT\r\n\r\n" +
                "#NVIDIA\r\n5\r\nnvsvc\r\nGfExperienceService\r\nNvNetworkService\r\nNvStreamNetworkSvc\r\nNvStreamSvc\r\n\r\n" +
                "#Hyper-V\r\n8\r\nvmickvpexchange\r\nvmicguestinterface\r\nvmicshutdown\r\nvmicheartbeat\r\nvmicvmsession\r\nvmictimesync\r\nvmicvss\r\nvmicrdv\r\n\r\n" +
                "#VMware\r\n4\r\nVMAuthdService\r\nVMnetDHCP\r\nVMware NAT Service\r\nVMUSBArbService\r\n\r\n" +
                "#Windows Defender\r\n3\r\nSense\r\nWdNisSvc\r\nWinDefend\r\n\r\n" +
                "#Microsoft Office\r\n2\r\nClickToRunSvc\r\nose64\r\n\r\n" +
                "#Rosetta Stone\r\n1\r\nRosettaStoneDaemon\r\n\r\n" +
                "#Thunder\r\n1\r\nXLServicePlatform\r\n\r\n" +
                "#Tencent\r\n2\r\nQPCore\r\nQQMusicService\r\n\r\n";

            WriteFile_ServiceGroup.Write(defaultServiceGroup);
            WriteFile_ServiceGroup.Close();
        }
        private void Error_DBFile()
        {
            if (MessageBox.Show("Fatal Error: 数据文件读写错误！\n是否恢复默认服务组信息？", "系统错误！", MessageBoxButtons.OKCancel) == DialogResult.OK)
                LoadDefaultServiceGroup();
        }

        //Service & Device
        public int GetServices(ListView serviceInfo)
        {
            ServiceController[] serviceControllers = ServiceController.GetServices();

            //表数据更新
            serviceInfo.BeginUpdate();//数据更新，UI暂时挂起
            serviceInfo.Items.Clear();
            foreach (ServiceController service in serviceControllers)
            {
                ListViewItem item = new ListViewItem();
                item.Text = service.DisplayName;//显示名
                item.SubItems.Add(service.ServiceName);//服务名
                item.SubItems.Add(getStatus(service));//运行状态
                item.SubItems.Add(getStartupType(service));//启动类型
                item.SubItems.Add(getCompany(service));//服务厂商
                item.SubItems.Add(getDescription(service));//描述
                item.SubItems.Add(getImageCommands(service.ServiceName));//命令行
                serviceInfo.Items.Add(item);
                service.Close();
            }
            serviceInfo.EndUpdate();//结束数据处理，UI界面一次性绘制
            return serviceInfo.Items.Count;
        }
        public void GetDependingInfo(string serviceName, ref List<string> servicesDependedOn, ref List<string> dependentServices)//此服务依赖于(上级)，依赖于此服务(下级)
        {
            ServiceController[] services = ServiceController.GetServices();
            ServiceController thisService = null;

            foreach (ServiceController service in services)
                if (service.ServiceName == serviceName)
                {
                    thisService = service;
                    break;
                }
                else
                    service.Close();

            if (thisService == null)
            {
                MessageBox.Show("Fatal Error: 查无此服务！", "系统错误！");
                return;
            }

            services = thisService.ServicesDependedOn;
            foreach (ServiceController service in services)
                servicesDependedOn.Add(service.ServiceName);
            services = thisService.DependentServices;
            foreach (ServiceController service in services)
                dependentServices.Add(service.ServiceName);
        }
        public bool GetStatus(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            ServiceController thisService = null;

            foreach (ServiceController service in services)
                if (service.ServiceName == serviceName)
                {
                    thisService = service;
                    break;
                }
                else
                    service.Close();

            if (thisService == null)
            {
                MessageBox.Show("Fatal Error: 查无此服务！", "系统错误！");
                return false;
            }
            else if (thisService.Status != ServiceControllerStatus.Running)
                return false;
            else
                return true;
        }
        public bool StartService(string serviceName)
        {
            try
            {
                ServiceController serviceController = new ServiceController(serviceName);
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
                serviceController.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool RestartService(string serviceName)
        {
            try
            {
                ServiceController serviceController = new ServiceController(serviceName);
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
                serviceController.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool StopService(string serviceName)
        {
            try
            {
                ServiceController serviceController = new ServiceController(serviceName);
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                serviceController.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool ChangeStartupType(string serviceName, int startupType)
        {
            try
            {
                switch (startupType)
                {
                    case 2:
                        Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName).SetValue("Start", 2, RegistryValueKind.DWord);//2=自动
                        break;
                    case 3:
                        Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName).SetValue("Start", 3, RegistryValueKind.DWord);//3=手动
                        break;
                    case 4:
                        Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName).SetValue("Start", 4, RegistryValueKind.DWord);//4=禁用
                        break;
                    default:
                        MessageBox.Show("Fatal Error: startupType错误！", "系统错误！");
                        break;
                }

            }
            catch (System.Security.SecurityException)
            {
                MessageBox.Show("Error: 修改失败！", "启动类型修改回执...");
                return false;
            }
            return true;
        }
        public void ShowImageAttribute(string serviceName)
        {
            try
            {
                string fileName = getImagePath(serviceName);

                SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
                info.cbSize = Marshal.SizeOf(info);
                info.lpVerb = "properties";
                info.lpFile = fileName;
                info.nShow = 5;
                info.fMask = 12u;
                ShellExecuteEx(ref info);
            }
            catch
            {
                MessageBox.Show("Error: 访问失败！", "文件属性查看回执...");
                return;
            }
        }
        public void ShowImageDirectory(string serviceName)
        {
            ProcessStartInfo fileInDirectory = new ProcessStartInfo("Explorer.exe");
            fileInDirectory.Arguments = "/e,/select," + getImagePath(serviceName);
            Process.Start(fileInDirectory);
        }
        //Service & Device-工具函数
        private string getStatus(ServiceController service)
        {
            if (service.Status == ServiceControllerStatus.Running)
                return "正在运行";
            else if (service.Status == ServiceControllerStatus.Stopped)
                return "已停止";
            else if (service.Status == ServiceControllerStatus.Paused)
                return "已暂停";
            else if (service.Status == ServiceControllerStatus.PausePending)
                return "正在暂停...";
            else if (service.Status == ServiceControllerStatus.ContinuePending)
                return "正在继续...";
            else if (service.Status == ServiceControllerStatus.StartPending)
                return "正在启动...";
            else if (service.Status == ServiceControllerStatus.StopPending)
                return "正在停止...";
            return "未知";
        }
        private string getStartupType(ServiceController service)
        {
            try
            {
                string startupType = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + service.ServiceName).GetValue("Start").ToString();
                switch (Convert.ToInt32(startupType))
                {
                    case 0:
                        return "内核";//Boot(by Kernel)
                    case 1:
                        return "系统";//System(by I/O Sub-System)
                    case 2:
                        return "自动";
                    case 3:
                        return "手动";
                    case 4:
                        return "已禁用";
                    default:
                        return "未知";
                }
            }
            catch
            {
                return "(访问失败)";
            }
        }
        private string getCompany(ServiceController service)
        {
            string imageName = getImagePath(service.ServiceName);
            try
            {
                return FileVersionInfo.GetVersionInfo(imageName).CompanyName;
            }
            catch
            {
                return "";
            }
        }
        private string getDescription(ServiceController service)
        {
            try
            {
                string indirectString = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + service.ServiceName).GetValue("Description").ToString();
                if(indirectString.StartsWith("@"))
                {
                    uint buffer = 1024;//缓冲区
                    char[] szOut = new char[buffer];
                    SHLoadIndirectString(indirectString.ToCharArray(), szOut, buffer, null);
                    return new string(szOut);
                }
                else
                    return indirectString;
            }
            catch
            {
                return "";
            }
        }
        private string getImageCommands(string serviceName)
        {
            try
            {
                return Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName).GetValue("ImagePath").ToString();
            }
            catch
            {
                return "";
            }
        }
        private string getImagePath(string serviceName)
        {
            try
            {
                string imageName = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName).GetValue("ImagePath").ToString();
                if (imageName.StartsWith("\""))
                    imageName = imageName.Substring(1, imageName.LastIndexOf('\"') - 1);//有引号直接取引号内的路径
                else
                    imageName = imageName.Split('-')[0].Split('/')[0].Trim('"').TrimEnd(' ');//无引号尝试删除参数
                if (!imageName.EndsWith(".exe"))
                    imageName += ".exe";
                return imageName;
            }
            catch
            {
                return "";
            }
        }


        #region Call DLLs - 支持文件属性
        [StructLayout(LayoutKind.Sequential)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }
        [DllImport("shell32.dll")]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
        #endregion
        #region Call DLLs - 支持间接字符串转换
        [DllImport("Shlwapi.dll")]
        static extern int SHLoadIndirectString(char[] pszSource, char[] pszOutBuf, uint cchOutBuf, object ppvReserved);
        #endregion

    }
}
