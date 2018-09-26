using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Client
{
    public partial class ClientWindow : Window
    {
        public ClientWindow()
        {
            InitializeComponent();
            displayMessageDelegate = new DisplayMessageDelegate(DisplayMessage);
        }
        
        private Socket clientSocket;
        
        private string name;
        
        private EndPoint epServer;
        
        private byte[] dataStream = new byte[1024];
        
        private delegate void DisplayMessageDelegate(string message);
        private DisplayMessageDelegate displayMessageDelegate = null;

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                name = tbUserName.Text.Trim();
                
                Packet sendData = new Packet();
                sendData.ChatName = name;
                sendData.ChatMessage = null;
                sendData.ChatDataId = DataId.LogIn;
                
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress serverIP = IPAddress.Parse(tbServerIP.Text.Trim());

                IPEndPoint server = new IPEndPoint(serverIP, 50000);
                
                epServer = server;
                
                byte[] data = sendData.GetDataStream();
                
                clientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, epServer, new AsyncCallback(SendData), null);
                                
                dataStream = new byte[1024];
                
                clientSocket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(ReceiveData), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message, "Client Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }        

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Packet sendData = new Packet();
                sendData.ChatName = name;
                sendData.ChatMessage = tbNewMessage.Text.Trim();
                sendData.ChatDataId = DataId.Message;
                
                byte[] byteData = sendData.GetDataStream();
                
                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(SendData), null);

                tbNewMessage.Clear();                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send Error: " + ex.Message, "Client Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (clientSocket != null)
                {
                    Packet sendData = new Packet();
                    sendData.ChatDataId = DataId.LogOut;
                    sendData.ChatName = name;
                    sendData.ChatMessage = null;
                    
                    byte[] byteData = sendData.GetDataStream();
                    
                    clientSocket.SendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer);

                    clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Closing Error: " + ex.Message, "Client Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }        

        private void SendData(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send Data: " + ex.Message, "Client Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);
                
                Packet receivedData = new Packet(dataStream);
                
                if (receivedData.ChatMessage != null)
                    displayMessageDelegate(receivedData.ChatMessage);
                                
                dataStream = new byte[1024];
                
                clientSocket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(ReceiveData), null);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show("Receive Data: " + ex.Message, "Client Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                lbMessages.Items.Add(message);
            });
        }
    }
}
