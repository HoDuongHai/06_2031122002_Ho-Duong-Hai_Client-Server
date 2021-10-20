using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {

        public delegate void UpdateProgress(int size, int progress);
        public delegate void UpdateLabelCallBack(string msg);

        private NetworkStream _networkStream;
        private TcpClient _tcpClient;
        private Stream _streamReader;
        private Stream _streamWriter;
        SaveFileDialog saveDialog = new SaveFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
          Thread t = new Thread(new ThreadStart(SaveFile));
          t.Start();
        }
        private delegate void dlgSaveFile();
        public void SaveFile()
        {
            if (this.btnSave.InvokeRequired)
            {
                this.Invoke(new dlgSaveFile(SaveFile));
            }
            else
            {
                if (saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txbFileName.Text = saveDialog.FileName;

                }
                try
                {
                    _networkStream = _tcpClient.GetStream();
                    _streamReader = _networkStream;
                    _streamWriter = File.OpenWrite(txbFileName.Text);
                    byte[] buff = new Byte[1024];
                    int len = 0;
                    setConnectionStatus("Receiving...");
                    while ((len = _streamReader.Read(buff, 0, 1024)) > 0)
                    {
                        _streamWriter.Write(buff, 0, len);
                        _streamWriter.Flush();
                    }
                    setConnectionStatus("Received file!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _networkStream.Close();
                    _streamWriter.Close();
                    _streamReader.Close();
                }
            }
           
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
        private void btnConnect_Click(object sender, EventArgs e)
        {
            
            try
            {
                string s = btnConnect.Text;
                if (s == "Connect")
                {
                    txbFileName.Clear();
                    IPEndPoint ip = new IPEndPoint(IPAddress.Parse(txbIPServer.Text), int.Parse(txbPort.Text));
                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(ip);
                    btnConnect.Text = "Disconnect";
                    StreamReader sr = new StreamReader(_tcpClient.GetStream());
                    StreamWriter sw = new StreamWriter(_tcpClient.GetStream());
                    string filePath = sr.ReadLine();
                    lbMsg.Text = "File from Server " + filePath;
                    saveDialog.FileName = filePath;
                    saveDialog.Title = "Download file from Server";
                    saveDialog.ShowHelp = true;
                }
                else if(s == "Disconnect")
                {
                    btnConnect.Text = "Connect";
                    _networkStream.Close();
                    _streamWriter.Close();
                    _streamReader.Close();
                }
                
            }
           catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "Step 1: Enter the Ip Server Address and port number ";
            msg += Environment.NewLine;
            msg += Environment.NewLine;
            msg += "Step 2: Click 'Connect' to connect to server";
            msg += Environment.NewLine;
            msg += Environment.NewLine;
            msg += "Step 3: Click 'Save' to choose path to save file";
            MessageBox.Show(msg, "How to use?", MessageBoxButtons.OKCancel, MessageBoxIcon.Information,MessageBoxDefaultButton.Button1);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Powered by SANSLAB" + Environment.NewLine + Environment.NewLine + "Public Date : 06-Dec-2017", "About",MessageBoxButtons.OKCancel,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1);
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void txbIPServer_Click(object sender, EventArgs e)
        {

        }

        private void txbPort_Click(object sender, EventArgs e)
        {

        }
        private void txbPort_TextChanged(object sender, EventArgs e)
        {
            this.btnConnect.Enabled = IsNumeric(txbPort.Text);
        }
        private void txbIpServer_TextChanged(object sender, EventArgs e)
        {
            this.txbPort.Enabled = IsNumeric(txbIPServer.Text);
        }
        private bool IsNumeric(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            foreach (char c in s)
            {

                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }

            return true;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
