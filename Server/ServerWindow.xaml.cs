using Common;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Server
{
    public partial class ServerWindow : Window
    {
        private struct Client
        {
            public EndPoint endPoint;
            public string name;
        }

        private ArrayList clientList;

        private Socket serverSocket;
        
        private byte[] dataStream = new byte[1024];
        
        private delegate void UpdateStatusDelegate(string status);
        private UpdateStatusDelegate updateStatusDelegate = null;

        public ServerWindow()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            try
            {                
                clientList = new ArrayList();
                
                updateStatusDelegate = new UpdateStatusDelegate(UpdateLog);
                
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 50000); // Parse("192.168.0.100")
                                
                serverSocket.Bind(server);                

                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
                
                EndPoint epSender = clients;
                
                serverSocket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);

                tbState.Text = "Listening...";

                //IPEndPoint localIpEndPoint = serverSocket.LocalEndPoint as IPEndPoint;
                //tbServerIP.Text = localIpEndPoint.Address.ToString();
                //tbServerIP.Text = server.Address.ToString();
            }
            catch (Exception ex)
            {
                tbState.Text = "Error";
                MessageBox.Show("Load Error: " + ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateLog(string entry)
        {
            Dispatcher.Invoke(() =>
            {
                lbLog.Items.Add(entry);
            });
        }

        private void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                byte[] data;
                
                Packet receivedData = new Packet(dataStream);
                
                Packet sendData = new Packet();
                
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
                
                EndPoint epSender = clients;
                
                serverSocket.EndReceiveFrom(asyncResult, ref epSender);
                
                sendData.ChatDataId = receivedData.ChatDataId;
                sendData.ChatName = receivedData.ChatName;

                switch (receivedData.ChatDataId)
                {
                    case DataId.Message:
                        sendData.ChatMessage = string.Format("{0}: {1}", receivedData.ChatName, receivedData.ChatMessage);
                        break;

                    case DataId.LogIn:                        
                        Client client = new Client();
                        client.endPoint = epSender;
                        client.name = receivedData.ChatName;
                        
                        clientList.Add(client);

                        sendData.ChatMessage = string.Format("-- {0} is online --", receivedData.ChatName);
                        break;

                    case DataId.LogOut:                        
                        foreach (Client c in clientList)
                        {
                            if (c.endPoint.Equals(epSender))
                            {
                                clientList.Remove(c);
                                break;
                            }
                        }

                        sendData.ChatMessage = string.Format("-- {0} has gone offline --", receivedData.ChatName);
                        break;
                }
                
                data = sendData.GetDataStream();

                foreach (Client client in clientList)
                {
                    if (client.endPoint != epSender || sendData.ChatDataId != DataId.LogIn)
                    {                        
                        serverSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, client.endPoint, new AsyncCallback(SendData), client.endPoint);
                    }
                }
               
                serverSocket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);
                                
                updateStatusDelegate(sendData.ChatMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ReceiveData Error: " + ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SendData(IAsyncResult asyncResult)
        {
            try
            {
                serverSocket.EndSend(asyncResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show("SendData Error: " + ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}