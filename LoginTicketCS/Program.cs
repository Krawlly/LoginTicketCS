using System;
using System.Runtime.Serialization;
using Krawlly;

namespace LoginTicketCS
{
    [DataContract]
    internal class Person
    {
        [DataMember]
        internal string userName;

        [DataMember]
        internal int validThrough;
    }

    class Program
    {
        static void Main(string[] args)
        {
            TicketFactory ltf = new TicketFactory("C:\\key.pem");
            Person mrNobody = new Person();
            mrNobody.userName = "Вася";
            mrNobody.validThrough = 60 * 24;
            string result = ltf.CreateTicket(mrNobody);
            Console.WriteLine("Login ticket is:");
            Console.WriteLine(result);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
