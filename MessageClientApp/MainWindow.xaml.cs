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
            if (string.IsNullOrEmpty(usernameBox.Text) 
                || string.IsNullOrWhiteSpace(usernameBox.Text))
                MessageBox.Show("Enter your name before joining the chat");

            else
            {
                string message = "$<Join>";
                byte[] data = Encoding.Unicode.GetBytes(message);
                string name = usernameBox.Text;
                textLabel.Content = name;
                usernameBox.Visibility = Visibility.Hidden;
                textLabel.Visibility = Visibility.Visible;
                MessageBox.Show("Welcome to our chat!!");
                SendMessage(data);
                Listener();
                isJoined = true;
            }
        }

        private async void Listener()
        {
            while (true)
            {
                var res = await client.ReceiveAsync();
                string message = Encoding.Unicode.GetString(res.Buffer);
                MessageInfo clientMsg = JsonSerializer.Deserialize<MessageInfo>(message)!;
                messages.Add(clientMsg);
            }
        }
        private void LeaveButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "$<Leave>";
            byte[] data = Encoding.Unicode.GetBytes(message);
            SendMessage(data);
            usernameBox.Clear();
            msgTextBox.Clear();
            messages.Clear();
            textLabel.Content = "";

            usernameBox.Visibility = Visibility.Visible;
            textLabel.Visibility = Visibility.Hidden;
        }
    }
    public class MessageInfo
    {
        public string Message { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public MessageInfo(string message, string name)
        {
            Message = message;
            Name = name;
            Time = DateTime.Now;
        }
        public override string ToString()
        {
            return $"{Name} -- {Message,-20} {Time.ToShortTimeString()}";
        }
    }
}