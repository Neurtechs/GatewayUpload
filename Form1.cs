using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Data.Browsing.Design;
using GatewayUpload.Data;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using static GatewayUpload.Common;


namespace GatewayUpload
{
    public partial class Form1 : Form
    {
        public static List<string> listItems { get; set; }
        private static CancellationTokenSource cSource;
        public static int seqMax = 0;
        public static int seqMaxLast = 0;
        private static string connectionString;

        private static bool bStartup = true;
        //On server
        //private static string user = "Dale";
        //private static string server = "localhost";
        //private static string password = "D@lelieb01";

        //Server from local
        //private static string user = "Dale";
        //private static string server = "neura.dyndns.org,3306";
        //private static string password = "D@lelieb01";

        //Local
        private static string user = "root";
        private static string server = "localhost";
        private static string password = "D@lelieb01";

        //Server from local direct
        //private static string user = "Dale";
        //private static string server = "192.168.1.96,3306";
        //private static string password = "D@lelieb01";


        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Form1()
        {
            InitializeComponent();
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            this.showToolStripMenuItem.Click += showToolStripMenuItem_Click;
            this.exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
           
        }

        //private bool allowVisible;     // ContextMenu's Show command used
        private bool allowClose;       // ContextMenu's Exit command used

        //protected override void SetVisibleCore(bool value)
        //{
        //    if (!allowVisible)
        //    {
        //        value = false;
        //        if (!this.IsHandleCreated) CreateHandle();
        //    }
        //    base.SetVisibleCore(value);
        //}

        //protected override void OnFormClosing(FormClosingEventArgs e)
        //{
        //    if (!allowClose)
        //    {
        //        this.Hide();
        //        e.Cancel = true;
        //    }
        //    base.OnFormClosing(e);
        //}

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //allowVisible = true;
            
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowClose = true;
            Application.Exit();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           LogIn();
           if (System.Diagnostics.Debugger.IsAttached)
           {
               lblVersion.Text = "Version: Debug Mode";
           }
           else
           {
               //lblVersion.Text = string.Format("NeuraProcess - v{0}",
               //    ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4));
               lblVersion.Text = "NeuraUpload - version 1.0.1.1";
           }
           StartUp();
           this.WindowState = FormWindowState.Minimized;
        }
        private void btnSetup_Click(object sender, EventArgs e)
        {
            RegistryKey reg = Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue("GatewayRead",Application.ExecutablePath.ToString());
            MessageBox.Show("You have been successfully saved", "Message",
                MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void StartUp()
        {
            //listItems = new List<string>();
            listBox1.Items.Add("Started at " + DateTime.Now.ToString());
            listBox1.Items.Add("-------------------------------");

            Log.Info("Started at " + DateTime.Now.ToString());

            //listBox1.DataSource = listItems;

            listBox1.Refresh();

            CallMain();
            //listBox1.DataSource = listItems;

            listBox1.Items.Add("");
            listBox1.Items.Add("Next Iteration - runat " + DateTime.Now);
            listBox1.Items.Add("--------------");

            listBox1.Refresh();
            timerRead.Interval = 5 * 1000;
            timerRead.Start();
            //if (backgroundWorker1.IsBusy)
                //backgroundWorker1.CancelAsync();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
           StartUp();
            
        }

        public  void  CallMain()
        {
            
            cSource = new CancellationTokenSource();
            var token = cSource.Token;
            //Task t;
            string result = "";
            if (seqMax == 0)
            {
                //get last value in db
                seqMax = GetData.lastSeq();
                seqMaxLast = seqMax;
            }

            seqMax += 1;
            //t = Task.Run(() => FetchData.CallString("from",20, token, out  result), token);
            FetchData.CallString("from", seqMax, out result);
            if (result == "Abort")
            {
                seqMax -= 1;
                listBox1.Items.Add("Timeout or other error");
                Log.Info("Timeout or other error");
                return;
            }
            else if (result == "null")
            {
                seqMax -= 1;
                listBox1.Items.Add("null value");
                //Log.Info("null value");
                return;
            }
            if(result.Length<10){
                seqMax -= 1;
                //listBox1.Items.Add("No new data found after sequence " + seqMax);
                return;
            }
            

            result = result.Replace("[{", "");
            result = result.Replace("}]", "");
            
            string[] sep = { "},{" };
            string[] strList = result.Split(sep, StringSplitOptions.None);
            string[] seq = new string[strList.Length];
            string[] key = new string[strList.Length];
            string[] id = new string[strList.Length];
            string[] val = new string[strList.Length];
            string[] unix = new string[strList.Length];
            string[] error = new string[strList.Length]; //Added 2020/10/10
            string[] shell = new string[strList.Length]; //Added 2020/10/10
            string[] meera = new string[strList.Length]; //Added 2020/10/10
            string[] milo = new string[strList.Length]; //Added 2020/10/10
            string[] mqtt = new string[strList.Length]; //Added 2020/10/10
            string[] openmuc = new string[strList.Length]; //Added 2020/10/10
            string[] errors = new string[strList.Length]; //Added 2021/03/09

            int counter = 0;
            double unixDT = 0;
            DateTime ts = new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc);
            DateTime [] timeStamp = new DateTime[strList.Length];
            foreach (string v in strList)
            {
                //.Add(v);
                listBox1.Items.Add(v);
                Log.Info(v);
                string[] sub = v.Split(',');
                foreach (string s in sub)
                {
                    bool containsSeq = s.Contains("seq");
                    if (containsSeq == true)
                    {
                        seq[counter] = s.Remove(0, 6);
                    }
                    bool containsKey = s.Contains("key");
                    if (containsKey == true)
                    {
                        key[counter] = s.Remove(0, 7);
                        key[counter] = key[counter].Remove(key[counter].Length - 1, 1);
                    }

                    bool containsId = s.Contains("id");

                    if (containsId == true)
                    {
                        id[counter] = s.Remove(0, 6);
                        id[counter] = id[counter].Remove(id[counter].Length - 1, 1);
                    }

                    bool containsVal = s.Contains("val");
                    if (containsVal == true)
                    {
                        //check for valve
                        if (s.Contains("valve") == true)
                        {
                            //skip

                        }
                        else
                        {
                            val[counter] = s.Remove(0, 6);
                        }

                    }
                    
                    bool containsUnix = s.Contains("unix");
                    if (containsUnix == true)
                    {
                        unix[counter] = s.Remove(0, 7);
                       
                    }
                    bool containsError = s.Contains("error");
                    if (containsError == true)
                    {
                        error[counter] = "error";
                    }

                    bool containsErrors = s.Contains("errors");
                    if (containsErrors == true)
                    {
                        errors[counter] = "errors";
                    }

                    bool containsShell = s.Contains("shell");
                    if (containsShell == true)
                    {
                        shell[counter] = "shell";
                    }
                    bool containsMeera = s.Contains("meera");
                    if (containsMeera == true)
                    {
                        meera[counter] = "meera";
                    }
                    bool containsMilo = s.Contains("milo");
                    if (containsMilo == true)
                    {
                        milo[counter] = "milo";
                    }
                    bool containsMqtt = s.Contains("mqtt");
                    if (containsMqtt == true)
                    {
                        mqtt[counter] = "mqtt";
                    }
                    bool containsOpenmuc = s.Contains("openmuc");
                    if (containsOpenmuc == true)
                    {
                        openmuc[counter] = "openmuc";
                    }
                }

                if (val[counter] == null)
                {
                    val[counter] = "0";}
                counter += 1;
            }
            GetData.GetSerial(out DataTable dtGetSerial);
            string filter;
            DataRow[] dr;
            int[] iseq=new int[counter];
            string[] gateway=new string[counter];
            string[] node=new string[counter];
            string[] readingType=new string[counter];
            double[] value=new double[counter];
            string[] nodeSer = new string[counter];
            string[] nodeHardware = new string[counter]; //2021/02/14 11:00
            int[] iReadingType=new int[counter];
            bool[] bValid=new bool[counter];
            string[] status = new string[counter];
            string _node;
            string _nodeSer = "0"; //2021/02/15 10:00
            string _nodeHardware = "0"; //2021/02/15 10:00
            int updates = 0;
            int _nodeType = 0; //2021/02/15 10:00
            for (int i = 0; i < counter; i++)
            {
                if(error[i]=="error"){goto SkipUpdate;} //2020/10/10
                if (shell[i] == "shell") { goto SkipUpdate; } //2020/10/10
                if (meera[i] == "meera") { goto SkipUpdate; } //2020/10/10
                if (milo[i] == "milo") { goto SkipUpdate; } //2020/10/10
                if (mqtt[i] == "mqtt") { goto SkipUpdate; } //2020/10/10
                if (openmuc[i] == "openmuc") { goto SkipUpdate; } //2020/10/10
                if (errors[i] == "errors") { goto SkipUpdate; } //2021/03/09
                iseq[i] = Convert.ToInt32(seq[i]);
                string[] separate = { "/" };
                string[] keySplit = key[i].Split(separate, StringSplitOptions.None);
                gateway[i] = keySplit[0];
                node[i] = keySplit[1];
                readingType[i] = keySplit[2];
                try //added 2020/10/10
                {
                    status[i] = keySplit[3];
                }
                catch (Exception)
                {
                    //Ignore
                }
              
                value[i] = Convert.ToDouble(val[i]);
                unixDT = Convert.ToDouble(unix[i]);
                timeStamp[i] = ts.AddSeconds(unixDT);
                bValid[i] = false;
                
                //filter for gateway
                filter = "GatewaySN = '" + gateway[i] + "'";
                dr = dtGetSerial.Select(filter);
                _node = node[i]; //2021/02/15 10:00
                
                //check for metertype
                for (int j = 0; j < dr.Length; j++)
                {
                    //nodeSer[j] = dr[j]["nodeSN"].ToString();
                    _nodeSer = dr[j]["nodeSN"].ToString();
                    //nodeHardware[j] = dr[j]["HardwareVer"].ToString(); //2021/02/14 11:00
                    _nodeHardware = dr[j]["HardwareVer"].ToString();
                    _nodeType = Convert.ToInt16(dr[j]["NodeType"]);//2021/02/14 11:00
                    //if (node[i] == "enode" && dr[j]["NodeType"].ToString()=="0")

                    try
                    {
                        if (_node.Substring(0, 5) == "enode" && _nodeType.ToString() == "0"
                        && _node == _nodeHardware) //2021/02/14 11:00

                        {
                            bValid[i] = true;
                            //enode
                            if (readingType[i] == "positive" || readingType[i] == "switch")
                            {
                                iReadingType[i] = 0;
                            }
                            else if (readingType[i] == "reverse")
                            {
                                iReadingType[i] = 20;
                            }
                            else if (readingType[i] == "power")
                            {
                                //power meter
                                iReadingType[i] = -1;
                            }
                            else
                            {
                                goto SkipUpdate;
                            }
                            goto UpdateData;
                        }
                        //else if (node[i] == "wnode" && dr[j]["NodeType"].ToString() == "1")
                        else if (_node.Substring(0, 5) == "wnode" && _nodeType.ToString() == "1"
                                                                    && _node == _nodeHardware) //2021/02/14 11:00
                        {
                            if (readingType[i] == "volume" || readingType[i] == "switch")
                            {
                                bValid[i] = true;
                                //wnode
                                iReadingType[i] = 10;
                                value[i] = value[i] / 1000; //Added 2020/10/10
                                goto UpdateData;
                            }
                            else
                            {
                                goto SkipUpdate;
                            }
                        }
                        //else if (node[i]=="gnode" && dr[j]["NodeType"].ToString() == "2")
                        else if (_node.Substring(0, 5) == "gnode" && _nodeType.ToString() == "2"
                                                                   && _node == _nodeHardware) //2021/02/14 11:00
                        {
                            //gnode
                            if (readingType[i] == "power" || readingType[i] == "switch")
                            {
                                bValid[i] = true;
                                goto UpdateData;
                            }
                            else
                            {
                                goto SkipUpdate;
                            }

                        }
                        else
                        {
                            //not a node command or no match
                            //goto NextMeterType;
                        }
                    }
                    catch (Exception)
                    {
                        //Hardware ver probably null
                        goto  SkipUpdate;
                    }
                    NextMeterType:;
                } //for j
                
                UpdateData:;
                if (readingType[i] == "switch" || readingType[i] == "valve")
                {
                    //update the switching table
                    //2020/10/10
                    //if (value[i] > 0)
                    //{
                    //    value[i] = 1;
                    //}

                    //int v = Convert.ToInt32(value[i]);

                    int v = 1; //on/open
                    if (status[i] == "off" || status[i] == "close")
                    {
                        v = 0; //off/close
                    }
                    GetData.SwitchStatusChange(_nodeSer,timeStamp[i],v,iseq[i]);
                }
                else
                {
                    //update thermo table table
                    if (_node.Substring(0, 5) == "gnode" && bValid[i]==true)
                    {
                        //GetData.ThermoChangeStatus(_nodeSer,timeStamp[i],value[i],iseq[i]);
                        value[i] = value[i] / 1000; //Convert to kW
                        ThermoLogic.CheckLogic(_nodeSer, timeStamp[i], value[i], iseq[i]);

                    }
                    else if ((_node.Substring(0, 5) == "enode" ||
                              _node.Substring(0, 5) == "wnode")
                             && bValid[i] == true && iReadingType[i] != -1) 
                    {
                        //reading
                        GetData.InsertReading(_nodeSer, iReadingType[i], value[i], timeStamp[i], iseq[i]);
                    }
                    else if (_node.Substring(0, 5) == "enode" && iReadingType[i] == -1)
                    {
                        //bulk reading
                        GetData.InsertReadingBulk(_nodeSer,timeStamp[i], value[i],iseq[i]);
                    }
                    else
                    {
                        //Not a node
                    }
                   
                }

                updates += 1;
                seqMax = iseq[i];
                if (iseq[i] > seqMax)
                {
                    seqMax = iseq[i];}

                SkipUpdate:;
            }

            listBox1.Items.Add(counter + " new values found after sequence " + seqMax + " " + DateTime.Now);
            Log.Info(counter + " new values found after sequence " + seqMax);
           
            //Check back
            int checkSeq = GetData.lastSeq();
            if (checkSeq == seqMax)
            {
                //write was successful
                listBox1.Items.Add(" Database update successful, added " + updates);
                Log.Info(" Database update successful, added " + updates);
            }
            else
            {
                //write was not successful
                listBox1.Items.Add("Database update not successful");
                Log.Info("Database update not successful ");
            }

        }

        private void timerRead_Tick(object sender, EventArgs e)
        {
            //listItems.Clear();
            //listBox1.DataSource = listItems;
           
            //Thread.Sleep(1000);
            CallMain();



            //listBox1.Items.Add("");
            //listBox1.Items.Add("Next Iteration - runat " + DateTime.Now);
            //listBox1.Items.Add("--------------");


            listBox1.Refresh();
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            seqMaxLast = seqMax;

            if (listBox1.Items.Count > 1000)
            {
                listBox1.Items.Clear();
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            try
            {
                cSource.Cancel();
            }
            catch (Exception exception)
            {

            }
            timerRead.Stop();
            //listItems.Clear();
            //listBox1.DataSource = null;

            listBox1.Items.Add("Stopped by user");
            listBox1.Refresh();
        }

        private void LogIn()
        {
            
            connectionString = "Server = " + server + "; User ID = " + user + "; Password = " +
                               password;
            connectionString = connectionString +
                               ";  Persist Security Info = true; Charset = utf8; Database = Neura; Connection Timeout=1800 ";
            mySqlConnection = new MySqlConnection(connectionString);

            if (mySqlConnection.State == ConnectionState.Closed)
            {
                try
                {
                    mySqlConnection.Open();
                    //MessageBox.Show("Connected to DB");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Check you connection string and comms to DB");
                    MessageBox.Show(ex.Message);

                }
            }
            textBoxUser.Text = user; 
            textBoxServer.Text = server;
            textBoxPassword.Text = password;
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
           LogIn();

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
            listBox1.Height = this.Height - 90;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            ////this.WindowState = FormWindowState.Maximized;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

       

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            allowClose = true;
            Application.Exit();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
        //private void InitializeBackgroundWorker()
        //{
        //    backgroundWorker1.DoWork +=
        //        new DoWorkEventHandler(backgroundWorker1_DoWork);
        //    //backgroundWorker1.RunWorkerCompleted +=
        //    //    new RunWorkerCompletedEventHandler(
        //    //        backgroundWorker1_RunWorkerCompleted);
        //    //backgroundWorker1.ProgressChanged +=
        //    //    new ProgressChangedEventHandler(
        //    //        backgroundWorker1_ProgressChanged);
        //}
        //private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    if (backgroundWorker1.CancellationPending)
        //    {
        //        e.Cancel = true;
        //        timerRead.Start();
        //    }
        //    else
        //    {
        //        if (bStartup == true)
        //        {
        //            LogIn();
        //            StartUp();
        //            bStartup = false;
        //        }
        //        else
        //        {
        //            CallMain();
        //            listBox1.Items.Add("");
        //            listBox1.Items.Add("Next Iteration - runat " + DateTime.Now);
        //            listBox1.Items.Add("--------------");


        //            listBox1.Refresh();
        //            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        //            seqMaxLast = seqMax;
        //        }

        //    }
        //    Thread.Sleep(5000);

        //}
    }
}
