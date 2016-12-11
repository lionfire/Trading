using System.Collections.Generic;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ILoadHistoricalDataJob
    {
        string AccessToken { get; set; }
        string AccountId { get; set; }
        List<TimedBarStruct> Result { get; set; }

        Task Run();
        string ToString();
    }
}