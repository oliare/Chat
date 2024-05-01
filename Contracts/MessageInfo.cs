namespace Contracts
{

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
            return $"{Name}:  {Message,-20} {Time.ToShortTimeString()}";
        }

    }
}
