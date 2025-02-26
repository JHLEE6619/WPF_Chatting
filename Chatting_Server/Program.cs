namespace Chatting_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new();
            server.StartServer();
        }
    }
}
