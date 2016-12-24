#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Templating;
using LionFire.Execution;
using System.Collections.ObjectModel;

namespace LionFire.Trading.Bots
{
    public enum PositionEventKind
    {
        Unspecified,
        Opened = 1 << 0,
        Closed = 1 << 1,
        Modified = 1 << 2,
        StoppedOut = 1 << 3,
        PartialPosition = 1 << 4,
    }

    public class PositionEvent
    {
        public Position Position { get; set; }
        public PositionEventKind Kind { get; set; }
    }

    public interface IHasCustomFitness
    {
#if !cAlgo
        double GetFitness(GetFitnessArgs args);
#endif
    }
    public interface IBot : ITemplateInstance
#if !cAlgo
        , IStartable
        , IStoppable
        , IAccountParticipant
        , IExecutable
#endif
    {

        new TBot Template { get; set; }

        string Version { get; set; }

        TradeResult ClosePosition(Position position);
        TradeResult ModifyPosition(Position position, double? StopLoss, double? TakeProfit);

        Microsoft.Extensions.Logging.ILogger Logger { get; }

        BotMode Mode { get; set; }

#if !cAlgo
        Positions BotPositions { get; }
        event Action<PositionEvent> BotPositionChanged;
#endif
    }

    public static class VersionUtils
    {
        public static string GetMinorCompatibilityVersion(this string version)
        {
            if (version == null) return null;

            int index = version.IndexOf('.');
            if (index == -1 || index == version.Length - 1) return version;
            index = version.IndexOf('.', index+1);
            if (index == -1) return version;

            return version.Substring(0, index);
        }
    }
}
