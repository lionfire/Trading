using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Agent.FrameworkProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var agent = new AgentService();
            agent.Run();
            Console.ReadKey();
        }
    }
}
