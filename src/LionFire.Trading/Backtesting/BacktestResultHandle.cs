#if !cAlgo

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
#if !cAlgo
using LionFire.Parsing.String;
#endif
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{
    public class BacktestResultHandle : IReadHandle<BacktestResult> // TODO: Use IReadHandle or something
    {
        public static implicit operator BacktestResultHandle(BacktestResult r)
        {
            return new BacktestResultHandle { Object = r };
        }

        public bool HasObject { get { return obj != null; } }

        public BacktestResult Object
        {
            get
            {
                if (obj == null && Path != null)
                {
                    try
                    {
                        obj = Newtonsoft.Json.JsonConvert.DeserializeObject<BacktestResult>(System.IO.File.ReadAllText(Path));
                    }
                    catch { }
                }
                return obj;
            }
            set { obj = value; }
        }
        private BacktestResult obj;

        public BacktestResultHandle Self { get { return this; } } // REVIEW - another way to get context from datagrid: ancestor row?
        public string Path { get; set; }

        [Unit("id=")]
        public string Id { get; set; }

        [Unit("bot=")]
        public string Bot { get; set; }

        [Unit("sym=")]
        public string Symbol { get; set; }

        [Unit("tf=")]
        public string TimeFrame { get { return timeFrame ?? Object?.TimeFrame; } set { timeFrame = value; } }
        private string timeFrame;

        /// <summary>
        /// AROI vs Max Equity Drawdown
        /// </summary>
        [Unit("ad")]
        public double AD { get; set; }

        [Unit("adwt")]
        public string AverageDaysPerWinningTrade { get; set; }

        /// <summary>
        /// Trades Per month
        /// </summary>
        [Unit("tpm")]
        public double TPM { get; set; }

        [Unit("d")]
        public double Days { get; set; }

        public static List<BacktestResultHandle> Find(string filterText, Predicate<BacktestResultHandle> filterPredicate = null)
        {
            

            List<BacktestResultHandle> results = new List<BacktestResultHandle>();

            //bool gotAD = false;
            var filters = filterText?.Split(' ');

            var dir = System.IO.Path.Combine(LionFireEnvironment.AppProgramDataDir, @"Results");
            foreach (var path in Directory.GetFiles(dir))
            {
                var str = System.IO.Path.GetFileNameWithoutExtension(path);

                
                bool failedFilter = false;
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        if (filter.StartsWith(">")
                            || filter.StartsWith("<")
                            || filter.StartsWith("=")
                            ) continue;

                        if (!str.Contains(filter))
                        {
                            failedFilter = true;
                            break;
                        }
                    }
                }
                if (failedFilter) continue;

                var handle = new BacktestResultHandle();
                handle.Path = path;
                handle.AssignFromString(str);


                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        string unit = null;
                        object val;
                        object curVal;
                        PropertyInfo pi;
                        if (filter.StartsWith(">="))
                        {
                            var filterString = filter.Substring(2);
                            filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                            if (pi == null) break;
                            curVal = pi.GetValue(handle);
                            switch (pi.PropertyType.Name)
                            {
                                case "Double":
                                    if (!((double)curVal >= (double)val)) failedFilter = true;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else if (filter.StartsWith("<="))
                        {
                            var filterString = filter.Substring(2);
                            filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                            if (pi == null) break;
                            curVal = pi.GetValue(handle);
                            switch (pi.PropertyType.Name)
                            {
                                case "Double":
                                    if (!((double)curVal <= (double)val)) failedFilter = true;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else if (filter.StartsWith(">"))
                        {
                            var filterString = filter.Substring(1);
                            filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                            if (pi == null) break;
                            curVal = pi.GetValue(handle);
                            switch (pi.PropertyType.Name)
                            {
                                case "Double":
                                    if (!((double)curVal > (double)val)) failedFilter = true;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else if (filter.StartsWith("<"))
                        {
                            var filterString = filter.Substring(1);
                            filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                            if (pi == null) break;
                            curVal = pi.GetValue(handle);
                            switch (pi.PropertyType.Name)
                            {
                                case "Double":
                                    if (!((double)curVal < (double)val)) failedFilter = true;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else if (filter.StartsWith("="))
                        {
                            var filterString = filter.Substring(1);
                            filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                            if (pi == null) break;
                            curVal = pi.GetValue(handle);
                            switch (pi.PropertyType.Name)
                            {
                                //case "Double":
                                //    if (!((double)curVal == (double)val)) failedFilter = true;
                                //    break;
                                default:
                                    if (curVal != val) failedFilter = true;
                                    break;
                            }
                        }
                    }
                }

                if (filterPredicate != null && !filterPredicate(handle)) failedFilter = true;

                if (failedFilter) continue;
                results.Add(handle);
            }

            return results;
        }


        public override string ToString()
        {
            return this.ToXamlAttribute();
        }
    }

}

#endif