using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Agent.Program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var agent = new AgentService();
            agent.Run();
            Console.ReadKey();
        }
    }
}
