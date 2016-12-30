using LionFire.Parsing.String;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.DataGrid;
using LionFire.Trading.Backtesting;

namespace LionFire.Trading.Dash.Wpf
{
    /// <summary>
    /// Interaction logic for BacktestingView.xaml
    /// </summary>
    public partial class BacktestingView : UserControl
    {
        public BacktestingViewModel VM { get { return DataContext as BacktestingViewModel; } }

        public BacktestingView()
        {
            InitializeComponent();
        }

        private void RefreshBacktestResults(bool forceRefresh = false)
        {
            if (!forceRefresh && lastResultsFilterText == ResultsFilterBox.Text) return;
            lastResultsFilterText = ResultsFilterBox.Text;
            var results = LoadResults(ResultsADFilterSlider.Value);

            DataGridCollectionView collectionView = new DataGridCollectionView(results);
            collectionView.SortDescriptions.Add(new SortDescription("AD", ListSortDirection.Descending));
            //ResultsGrid.ItemsSource = collectionView;
            VM.Results = collectionView;
        }

        private List<BacktestResultHandle> LoadResults(double minAD = double.NaN)
        {
            List<BacktestResultHandle> results = new List<BacktestResultHandle>();

            bool gotAD = false;

            var dir = System.IO.Path.Combine(LionFireEnvironment.AppProgramDataDir, @"Results");
            foreach (var path in Directory.GetFiles(dir))
            {
                var str = System.IO.Path.GetFileNameWithoutExtension(path);

                var filters = VM.ResultsFilterBox?.Split(' ');
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

                        }
                        else if (filter.StartsWith("<="))
                        {
                            var filterString = filter.Substring(2);
                            filterString.ParseUnitValue(typeof(BacktestResultHandle), out unit, out val, out pi);
                            if (pi == null) break;
                            curVal = pi.GetValue(handle);

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
                        if (unit == "ad")
                        {
                            gotAD = true;
                        }
                    }
                }
                if (!gotAD && !double.IsNaN(minAD))
                {
                    if (handle.AD < minAD) failedFilter = true;
                }

                foreach (var b in SymbolFilterButtons.Children.OfType<ToggleButton>())
                {
                    if (b.IsChecked == true)
                    {
                        var filter = b.Content as string;
                        if (filter.Length == 3)
                        {
                            if (!handle.Symbol.Contains(filter)) { failedFilter = true; break; }
                        }
                    }
                }
                if (failedFilter) continue;
                results.Add(handle);
            }

            return results;
        }

        private void TimeDelayUpdateResultsFilter(bool forceUpdate = false)
        {
            if (ResultsFilterBox_TextChanged_cts != null)
            {
                ResultsFilterBox_TextChanged_cts.Cancel();
            }

            ResultsFilterBox_TextChanged_cts = new CancellationTokenSource();


            Task.Factory.StartNew(() =>
            {
                refreshResults = DateTime.Now + TimeSpan.FromMilliseconds(400);
                ////while (refreshResults<DateTime.Now) {
                Thread.Sleep(400);
                //}
            }, ResultsFilterBox_TextChanged_cts.Token).ContinueWith(t =>
            {
                if (t.IsCanceled || ResultsFilterBox_TextChanged_cts.Token.IsCancellationRequested) { return; }
                Dispatcher.Invoke(() => RefreshBacktestResults(forceUpdate));
            }
            );
        }

        #region Input Event Handlers

        private void ResultsFilterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RefreshBacktestResults(true);
            }
        }


        private void B_Checked(object sender, RoutedEventArgs e)
        {
            TimeDelayUpdateResultsFilter(true);
        }

        private void ResultsADFilterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeDelayUpdateResultsFilter(true);
        }

        CancellationTokenSource ResultsFilterBox_TextChanged_cts;
        DateTime refreshResults;
        

        string lastResultsFilterText;
        private void ResultsFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimeDelayUpdateResultsFilter();

        }

        private void ResultToScanner_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SessionViewModel.Session.AddScanner(ResultsGrid.SelectedItem as BacktestResultHandle);
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message); // TODO - error handling
            }

            //Debug.WriteLine(e.OriginalSource);
            //Debug.WriteLine(e.Source);
            //Debug.WriteLine((e.Source as FrameworkElement).DataContext);
            //Debug.WriteLine(sender.GetType().FullName);
        }
        private void ResultToBot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SessionViewModel.Session.AddDemoBot(ResultsGrid.SelectedItem as BacktestResultHandle);
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message); // TODO - error handling
            }
            //Debug.WriteLine((e.Source as FrameworkElement).DataContext);
            //Debug.WriteLine(sender.GetType().FullName);
        }

        #endregion


    }
}
