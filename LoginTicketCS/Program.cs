using System;
using Krawlly;

namespace LoginTicketCS
{
    class Program
    {
        static void Main(string[] args)
        {
            TicketFactory ltf = new TicketFactory("C:\\key.pem");
            LoginTicket mrNobody = new LoginTicket("Вася", 60*24);
            string result = ltf.CreateTicket(mrNobody);
            Console.WriteLine("Login ticket is:");
            Console.WriteLine(result);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
