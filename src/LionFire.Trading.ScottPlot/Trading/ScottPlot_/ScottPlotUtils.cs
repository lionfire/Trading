using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading;
using ScottPlot;
using ScottPlot.Colormaps;

namespace LionFire.Trading.ScottPlot_;

public static class ScottPlotUtils
{

    public static Plot? CreateVolumeScottPlot(this IEnumerable<IKline>? bars, string? name = null, TimeSpan? timeSpan = null, Plot? plot = null, bool frameless = false)
    {
        plot ??= new();

        plot.Add.Palette = new ScottPlot.Palettes.Penumbra();
        plot.Style.DarkMode();
        plot.ScaleFactor = 0.5f;

        var barsSeries = plot.Add.Bars(bars.Select(b => (double)b.Volume).ToArray());
        barsSeries.Color = new(255, 255, 255, 255);

        // tell the plot to autoscale with no padding beneath the bars
        plot.Axes.Margins(bottom: 0);

        //if (diffPercent > 3.0)
        //{
        //    //candles.Axes.YAxis.Max = highPercent;
        //    //candles.Axes.YAxis.Min = lowPercent;
        //    if (last > first)
        //    {
        //        plot.Style.Background(figure: Color.FromHex("#025836"), data: Color.FromHex("#1b4049"));
        //    }
        //    else
        //    {
        //        plot.Style.Background(figure: Color.FromHex("#823836"), data: Color.FromHex("#4b3049"));
        //    }
        //}
        //else if (diffPercent > 1.0)
        //{
        //    candles.Axes.YAxis.Max = 3;
        //    candles.Axes.YAxis.Min = -3;
        //    if (last > first)
        //    {
        //        plot.Style.Background(figure: Color.FromHex("#023114"), data: Color.FromHex("#0b3049"));
        //    }
        //    else
        //    {
        //        plot.Style.Background(figure: Color.FromHex("#4c212b"), data: Color.FromHex("#0b3049"));
        //    }
        //}
        //else
        {
            //candles.Axes.YAxis.Max = 1;
            //candles.Axes.YAxis.Min = -1;
            plot.Style.Background(figure: Color.FromHex("#22222200"), data: Color.FromHex("#0b304900"));
        }
        plot.Style.ColorAxes(Color.FromHex("#a0acb5"));
        plot.Style.ColorGrids(Color.FromHex("#00000000"));

        if (frameless)
        {
            plot.Layout.Frameless();
        }

        return plot;
    }

    public static Plot CreateScottPlot(this IEnumerable<IKline>? bars, string? name = null, TimeSpan? timeSpan = null, Plot? plot = null, bool frameless = false)
    {
        plot ??= new();
        name ??= "";

        if (bars == null || bars.Count() == 0) { return plot; }

        if (timeSpan == null)
        {
            if (bars.Count() == 1)
            {
                timeSpan = TimeSpan.FromMinutes(1);
            }
            else
            {
                DateTime open = bars.First().OpenTime;
                TimeSpan minTimeSpan = TimeSpan.MaxValue;
                int confirmations = 3;
                foreach (var b in bars.Skip(1))
                {
                    var curTimeSpan = b.OpenTime - open;
                    if (curTimeSpan < minTimeSpan)
                    {
                        minTimeSpan = curTimeSpan;
                    }
                    else if (curTimeSpan == minTimeSpan)
                    {
                        if (--confirmations <= 0)
                        {
                            break;
                        }
                    }
                }
                timeSpan = minTimeSpan;
            }
        }

        //var prices = Generate.RandomOHLCs(30);
        var low = bars.Min(b => (double)b.LowPrice);
        var high = bars.Max(b => (double)b.HighPrice);
        var diff = high - low;
        var first = (double)bars.Last().OpenPrice;
        var last = (double)bars.First().ClosePrice;
        var lowPercent = 100.0 * (double)(low - first) / first;
        var highPercent = 100.0 * (double)(high - first) / first;
        var diffPercent = highPercent - lowPercent;

        bool percentAxis = true;
        List<OHLC> prices;
        if (percentAxis)
        {
            prices = bars.Select(b => new ScottPlot.OHLC(
           lowPercent + diffPercent * (((double)b.OpenPrice - low) / diff),
           lowPercent + diffPercent * (((double)b.HighPrice - low) / diff),
           lowPercent + diffPercent * (((double)b.LowPrice - low) / diff),
           lowPercent + diffPercent * (((double)b.ClosePrice - low) / diff),
           //(double)b.HighPrice,
           //(double)b.LowPrice,
           //(double)b.ClosePrice,
           b.OpenTime, timeSpan.Value)).ToList();
        }
        else
        {
            prices = bars.Select(b => new ScottPlot.OHLC((double)b.OpenPrice, (double)b.HighPrice, (double)b.LowPrice, (double)b.ClosePrice, b.OpenTime, timeSpan.Value)).ToList();
        }
        var candles = plot.Add.Candlestick(prices);
        candles.Axes.YAxis = plot.Axes.Right;
        candles.Axes.YAxis.Label.Text = percentAxis ? "%" : "Price";

        plot.Axes.DateTimeTicksBottom();
        //myPlot.Axes.DateTimeTicks(Edge.Bottom);

        if (diffPercent > 3.0)
        {
            //candles.Axes.YAxis.Max = highPercent;
            //candles.Axes.YAxis.Min = lowPercent;
            if (last > first)
            {
                plot.Style.Background(figure: Color.FromHex("#025836"), data: Color.FromHex("#1b4049"));
            }
            else
            {
                plot.Style.Background(figure: Color.FromHex("#823836"), data: Color.FromHex("#4b3049"));
            }
        }
        else if (diffPercent > 1.0)
        {
            candles.Axes.YAxis.Max = 3;
            candles.Axes.YAxis.Min = -3;
            if (last > first)
            {
                plot.Style.Background(figure: Color.FromHex("#023114"), data: Color.FromHex("#0b3049"));
            }
            else
            {
                plot.Style.Background(figure: Color.FromHex("#4c212b"), data: Color.FromHex("#0b3049"));
            }
        }
        else
        {
            candles.Axes.YAxis.Max = 1;
            candles.Axes.YAxis.Min = -1;
            plot.Style.Background(figure: Color.FromHex("#222222"), data: Color.FromHex("#0b3049"));
        }
        plot.Style.ColorAxes(Color.FromHex("#a0acb5"));
        plot.Style.ColorGrids(Color.FromHex("#0e3d54"));

        plot.Axes.Title.Label.Text = name;
        //myPlot.Axes.Bottom.Label.Text = "Horizontal Axis";
        //myPlot.Axes.Left.Label.Text = "Vertical Axis";

        if (frameless)
        {
            plot.Layout.Frameless();
        }

        return plot;
    }
}
