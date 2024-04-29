using System.Net;
using System.Net.Sockets;
using System.Text;


public class ChatServer
{
    const short port = 4040;
    const string JOIN = "$<Join>";
    const string LEAVE = "$<Leave>";
    UdpClient server;
    IPEndPoint clientEndPoint = null;
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
                case JOIN:
                    AddMember(clientEndPoint);
                    break;
                case LEAVE:
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
        if (!members.Contains(clientEndPoint))
        {
            members.Add(clientEndPoint);
            Console.WriteLine("Member was added");
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
}
internal class Program
{

    private static void Main(string[] args)
    {
        ChatServer server = new ChatServer();
        server.Start();
    }
}