namespace Chatting_Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Server server = new();
            await server.StartServer();
        }
    }
}
