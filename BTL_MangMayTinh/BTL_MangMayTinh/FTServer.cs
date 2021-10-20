using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace BTL_MangMayTinh
{
    public partial class Server : Form
    {
        public delegate void UpdateProgressBarStatus( int size, int progress);

        public delegate void UpdateLabelCallBack(string msg);

        private NetworkStream _networkStream = null;
        private Socket _socket = null;
        private TcpListener _tcpListener = null;
        private Stream _streamReader = null;
        private Stream _streamWriter = null;
        private Thread t = null;

        public Server()
        {
            InitializeComponent();
            btnConnect.Click += new EventHandler(btnConnect_Click);
            IPHostEntry ips = Dns.GetHostByName(Dns.GetHostName());
            txbIPServer.Text = ips.AddressList[0].ToString();
            

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fdlg = new OpenFileDialog())
            {
                fdlg.Filter = "All Files (*.*)|*.*";
                fdlg.Title = "Chọn file tải lên";
                fdlg.InitialDirectory = @"";
                if (fdlg.ShowDialog() == DialogResult.OK)
                {
                    txbFileName.Text = fdlg.FileName;
                    
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string s = btnConnect.Text;
            if(s == "Start")
            {
                btnConnect.Text = "Stop";
                t = new Thread(new ThreadStart((StartServer)));
                txbFileName.Clear();
            }
            else if(s == "Stop")
            {
                btnConnect.Text = "Start";
                if(_socket != null && _tcpListener != null)
                {
                    _socket.Close();
                    _tcpListener.Stop();
                }               
            }
        }

        private void StartServer()
        {
            try
            {
                //Set the connection status to waiting
                setConnectionStatus("Listening...");
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse(txbIPServer.Text), int.Parse(txbPort.Text));
                _tcpListener = new TcpListener(ip);
                _tcpListener.Start();
                _socket = _tcpListener.AcceptSocket();
                if (_socket.Connected)
                {
                    NetworkStream ns = new NetworkStream(_socket);
                    StreamReader sr = new StreamReader(ns);
                    StreamWriter sw = new StreamWriter(ns);
                    string fileName = txbFileName.Text;
                    _networkStream = new NetworkStream(_socket);
                    _streamReader = File.OpenRead(fileName);
                    _streamWriter = _networkStream;
                    FileInfo _fileInfo = new FileInfo(fileName);
                    AddListViewItem(fileName);
                    int size = Convert.ToInt32(_fileInfo.Length);                                  
                    byte[] buff = new Byte[2048];
                    int len = 0;
                    sw.WriteLine(fileName);
                    sw.Flush();
                    while ((len = _streamReader.Read(buff, 0, 2048)) > 0)
                    {
                        _streamWriter.Write(buff, 0, len);
                        _streamWriter.Flush();
                    }
                    setConnectionStatus("Sent file!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _socket.Close();
                _networkStream.Close();
                _streamWriter.Close();
                _streamReader.Close();
                _tcpListener.Stop();
            }
        }
        private delegate void dlgAddListViewItem(string fileName);
        private void AddListViewItem(string fileName)
        {
            if (this.lstTransfers.InvokeRequired)
            {
                this.Invoke(new dlgAddListViewItem(AddListViewItem),fileName);
            }
            else
            {
                //Create the LVI for the new transfer.
                ListViewItem i = new ListViewItem();
                i.Text = Guid.NewGuid().ToString();
                i.SubItems.Add(fileName);
                i.SubItems.Add("Upload");
                lstTransfers.Items.Add(i); //Add the item
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Powered by SANSLAB " + Environment.NewLine + Environment.NewLine + "Public Date : 06-Dec-2017", "About",MessageBoxButtons.OKCancel,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
        }

        private void setConnectionStatus(string msg)
        {
            if (InvokeRequired)
            {
                object[] pList = { msg };
                lbMsg.BeginInvoke(new UpdateLabelCallBack(OnUpdateLabel), pList);
            }
            else
            {
                OnUpdateLabel(msg);
            }
        }

      
        private void OnUpdateLabel(String msg)
        {
            lbMsg.Text = msg;
        }

        

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            t.Start();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "Step 1: Enter the port number";
            msg += Environment.NewLine;
            msg += Environment.NewLine;
            msg += "Step 2: Click 'Start' to start server on port";
            msg += Environment.NewLine;
            msg += Environment.NewLine;
            msg += "Step 3: Click 'Browse' to choose file to send";
            msg += Environment.NewLine;
            msg += Environment.NewLine;
            msg += "Step 4: Click 'Send' to send file and listening connection from client";
            MessageBox.Show(msg, "How to use?", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }

        private void toolStripDropDownButton3_ButtonClick(object sender, EventArgs e)
        {

        }

        private void lstTransfers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Server_Load(object sender, EventArgs e)
        {
           
        }

        private void txbPort_Click(object sender, EventArgs e)
        {

        }
        private void txbPort_TextChanged(object sender, EventArgs e)
        {
            this.btnConnect.Enabled = IsNumeric(txbPort.Text);
        }
        private bool IsNumeric( string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            foreach (char c in s)
            {
               
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}