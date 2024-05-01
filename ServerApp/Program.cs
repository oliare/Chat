using Contracts;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;


public class ChatServer
{
    const short port = 4040;

    const int MAX_MEMBERS = 2;
    UdpClient server;
    IPEndPoint? clientEndPoint = null;
    List<IPEndPoint> members;

    public ChatServer()
    {
        server = new UdpClient(port);
        members = new List<IPEndPoint>();
    }
    public void Start()
    {
        while (true)
        {
            byte[] data = server.Receive(ref clientEndPoint);
            string message = Encoding.Unicode.GetString(data);

            switch (message)
            {
                case Commands.JOIN:
                    AddMember(clientEndPoint);
                    SendNotification(clientEndPoint, Commands.JOINC);
                    break;
                case Commands.LEAVE:
                    RemoveMember(clientEndPoint);

                    break;
                default:
                    Console.WriteLine($"Got message {message,-20} from : {clientEndPoint} " +
                        $"at {DateTime.Now.ToShortTimeString()}");
                    SendToAll(data);
                    break;
            }
        }
    }

    private void AddMember(IPEndPoint clientEndPoint)
    {
        if (members.Count < MAX_MEMBERS)
        {
            if (!members.Contains(clientEndPoint))
            {
                members.Add(clientEndPoint);
                Console.WriteLine("Member was added");
            }
            else
                Console.WriteLine("Member is already exists");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The limit of participants has been reached");
            Console.ResetColor();
            SendNotification(clientEndPoint, Commands.FULL);
        }
    }
    private void RemoveMember(IPEndPoint clientEndPoint)
    {
        if (members.Contains(clientEndPoint))
        {
            members.Remove(clientEndPoint);
            Console.WriteLine("Member deleted");
        }

    }
    private async void SendToAll(byte[] data)
    {
        foreach (var member in members)
        {
            await server.SendAsync(data, data.Length, member);
        }
    }
    private async void SendNotification(IPEndPoint clientEndPoint, string msg)
    {
        MessageInfo clientMsg = new MessageInfo(msg, "");
        string message = JsonSerializer.Serialize<MessageInfo>(clientMsg);
        byte[] data = Encoding.Unicode.GetBytes(message);

        await server.SendAsync(data, data.Length, clientEndPoint);
    }

}
internal class Program
{
    private static void Main(string[] args)
    {
        ChatServer server = new ChatServer();
        server.Start();
    }
}
