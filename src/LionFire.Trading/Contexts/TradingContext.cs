using LionFire.MultiTyping;

namespace LionFire.Trading;

//namespace LionFire.Trading
//{
//    public interface ITradingContext
//    {
//        TradingOptions Options {
//            get;
//        }
//    }

//}
public class TradingContext
{
    public MultiTyped? MultiTyped { get; set; } // Replace with Flex?

    #region Configuration

    public TradingOptions Options { get; set; }

    #endregion

    #region Construction


    public TradingContext(TradingOptions options = null)
    {
        this.Options = options;
    }

    #endregion


}
