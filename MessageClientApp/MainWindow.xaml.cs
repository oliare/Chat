using Contracts;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace MessageClientApp
{

    public partial class MainWindow : Window
    {
        IPEndPoint serverPoint;
        UdpClient client;
        ObservableCollection<MessageInfo> messages;
        private bool isJoined = false;

        public MainWindow()
        {
            InitializeComponent();
            string serverAddress = ConfigurationManager.AppSettings["serverAddress"]!;
            short serverPort = short.Parse(ConfigurationManager.AppSettings["serverPort"]!);
            serverPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
            client = new UdpClient();
            messages = new ObservableCollection<MessageInfo>();

            this.DataContext = messages;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = msgTextBox.Text;
            if (!isJoined)
            {
                MessageBox.Show("You must join the chat before sending a message");
                return;
            }
            if (string.IsNullOrEmpty(msg))
            {
                MessageBox.Show("Here is nothing to send");
                return;
            }
            else
            {
                msgTextBox.Text = "";
                MessageInfo clientMsg = new MessageInfo(msg, usernameBox.Text);
                string message = JsonSerializer.Serialize<MessageInfo>(clientMsg);
                byte[] data = Encoding.Unicode.GetBytes(message);
                SendMessage(data);
            }
        }
        private async void SendMessage(byte[] data)
        {
            await client.SendAsync(data, data.Length, serverPoint);
        }
        private void msgTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
            }
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(usernameBox.Text)
              || string.IsNullOrWhiteSpace(usernameBox.Text))
                    MessageBox.Show("Enter your name before joining the chat");
                else
                {

                    byte[] data = Encoding.Unicode.GetBytes(Commands.JOIN);
                    string name = usernameBox.Text;
                    textLabel.Content = name;
                    usernameBox.Visibility = Visibility.Hidden;
                    textLabel.Visibility = Visibility.Visible;
                    SendMessage(data);
                    Listener();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                client.Dispose();
            }

        }
        private async void Listener()
        {
            while (true)
            {

                var res = await client.ReceiveAsync();
                string message = Encoding.Unicode.GetString(res.Buffer);
                MessageInfo clientMsg = JsonSerializer.Deserialize<MessageInfo>(message)!;
                if (clientMsg.Message == Commands.FULL)
                {
                    MessageBox.Show("The chat is currently full :(");
                    usernameBox.Visibility = Visibility.Visible;
                    return;
                }
                if (clientMsg.Message == Commands.JOINC)
                {
                    if (!isJoined)
                    {
                        MessageBox.Show("Welcome to our chat!!");
                        isJoined = true;
                    }
                    continue;
                }
                messages.Add(clientMsg);
            }

        }
        private void Leave()
        {
            string message = Commands.LEAVE;
            byte[] data = Encoding.Unicode.GetBytes(message);
            SendMessage(data);
            isJoined = false;
        }
        private void LeaveButton_Click(object sender, RoutedEventArgs e)
        {
            Leave();
            usernameBox.Clear();
            msgTextBox.Clear();
            messages.Clear();
            textLabel.Content = "";

            usernameBox.Visibility = Visibility.Visible;
            textLabel.Visibility = Visibility.Hidden;
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = (MessageInfo)listBox.SelectedItem;

            if (selected != null)
            {
                var win = new PrivateMsg(selected.Name);
                win.usernameBox.Text = selected.Name;
                win.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Leave();
            client.Dispose();
        }
    }

}