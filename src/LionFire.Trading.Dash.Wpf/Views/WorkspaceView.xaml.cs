//#define Live
using LionFire.Applications.Hosting;
using LionFire.Assets;
using LionFire.Execution;
using LionFire.Extensions.Logging;
using LionFire.Templating;
using LionFire.Trading.Applications;
using LionFire.Trading.Bots;
using LionFire.Trading.Dash.Wpf;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Spotware.Connect;
using LionFire.Trading.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LionFire.Parsing.String;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.DataGrid;
using Newtonsoft.Json;
using LionFire.Trading.Backtesting;
using Newtonsoft.Json.Linq;


namespace LionFire.Trading.Dash.Wpf
{
#if UNUSED
    
    public class AppVM : INotifyPropertyChanged
    {
    


    #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
        }

    #endregion

    }
#endif

    public partial class WorkspaceView : UserControl, INotifyPropertyChanged
    {

        List<Task> tasks = new List<Task>(); // TODO: Replace with global task manager

        #region Construction

        
         public WorkspaceView()
        {
            InitializeComponent();



            // TODO
            //foreach (var b in SymbolFilterButtons.Children.OfType<ToggleButton>())
            //{
            //    b.Checked += B_Checked;
            //    b.Unchecked += B_Checked;
            //}
        }

        

        #endregion


        private void WorkspaceTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var x in e.AddedItems)
            {
                Debug.WriteLine("Selected tab: " + x);
            }
            //var header = (string)(Workspace1Tabs.SelectedItem as TabItem)?.Header;

            //if (header == "Results")
            //{
            //    if (ResultsGrid.ItemsSource == null)
            //    {
            //        RefreshBacktestResults();
            //    }
            //}
            
        }

       

        #region StatusText

        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText == value) return;
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }
        private string statusText;

        #endregion

        #region Misc


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        #endregion
             
    }
}
#if OLDXAML

    <xcad:DockingManager Grid.Row="1"
                       AllowMixedOrientation="True"
                       BorderBrush="Black"
                       BorderThickness="1"
                       Theme="{Binding ElementName=_themeCombo, Path=SelectedItem.Tag}">
      <xcad:DockingManager.DocumentHeaderTemplate>
        <DataTemplate>
          <StackPanel Orientation="Horizontal">
            <!--<Image Source="{Binding IconSource}" Margin="0,0,4,0"/>-->
            <TextBlock Text="{Binding Title}" />
          </StackPanel>
        </DataTemplate>
      </xcad:DockingManager.DocumentHeaderTemplate>
       <xcad:DockingManager.LayoutItemTemplateSelector>
                <local:LayoutItemTemplateSelector>
                    <local:LayoutItemTemplateSelector.Template>
                        <DataTemplate>
                            <ContentControl cal:View.Model="{Binding .}" IsTabStop="False"/>
                        </DataTemplate>
                    </local:LayoutItemTemplateSelector.Template>
                </local:LayoutItemTemplateSelector>
            </xcad:DockingManager.LayoutItemTemplateSelector>
      <xcad:LayoutRoot x:Name="_layoutRoot">
        <xcad:LayoutPanel Orientation="Horizontal" >
          <!--<xcad:LayoutAnchorablePane DockWidth="200">
                    <xcad:LayoutAnchorable ContentId="properties" Title="Properties" CanHide="True" CanClose="False" AutoHideWidth="240">
                        <xctk:PropertyGrid NameColumnWidth="90" SelectedObject="{Binding ElementName=_layoutRoot, Path=LastFocusedDocument.Content}"/>
                    </xcad:LayoutAnchorable>
                </xcad:LayoutAnchorablePane>-->
          <xcad:LayoutDocumentPaneGroup >
            <xcad:LayoutDocumentPane >
              <xcad:LayoutDocument ContentId="session1" Title="Session" >
                <DockPanel DataContext="{Binding VM, ElementName=uc}">
                  <DockPanel DockPanel.Dock="Bottom">
                    <DockPanel LastChildFill="False">
                      <TextBlock Text="{Binding StatusText, ElementName=uc}" DockPanel.Dock="Right"></TextBlock>
                      <CheckBox IsChecked="{Binding Workspace.IsTradeApiEnabled}" >Trade API</CheckBox>
                      <CheckBox IsChecked="{Binding Workspace.IsLiveEnabled}" >Live</CheckBox>
                    </DockPanel>
                  </DockPanel>
                  <TabControl x:Name="Sessions" DockPanel.Dock="Top" ItemsSource="{Binding Sessions}">
                    <TabControl.ItemTemplate>
                      <DataTemplate>
                        <TabControl x:Name="WorkspaceTabs"  DockPanel.Dock="Top" SelectedIndex="2"  SelectionChanged="WorkspaceTabs_SelectionChanged">
                          <!--<TabItem Header="W">
                                            <cef:ChromiumWebBrowser  Address="https://www.tradingview.com/widget/"></cef:ChromiumWebBrowser>
                                        </TabItem>-->
                          <TabItem Header="Symbols">
                            <DockPanel>
                              <xcdg:DataGridControl x:Name="SymbolsGrid" ItemsSource="{Binding Symbols}" AutoCreateColumns="False">
                                <xcdg:DataGridControl.View>
                                  <xcdg:TableflowView UseDefaultHeadersFooters="False">
                                    <xcdg:TableflowView.FixedHeaders>
                                      <DataTemplate>
                                        <xcdg:ColumnManagerRow />
                                      </DataTemplate>
                                    </xcdg:TableflowView.FixedHeaders>
                                  </xcdg:TableflowView>
                                </xcdg:DataGridControl.View>
                                <xcdg:DataGridControl.Columns>
                                  <xcdg:Column FieldName="Subscribed" DisplayMemberBinding="{Binding}">
                                    <xcdg:Column.CellContentTemplate>
                                      <DataTemplate>
                                        <CheckBox IsChecked="{Binding  Subscribed}"/>
                                      </DataTemplate>
                                    </xcdg:Column.CellContentTemplate>
                                  </xcdg:Column>
                                  <xcdg:Column FieldName="Code"  DisplayMemberBinding="{Binding Symbol.Code}" Title="Symbol" >

                                  </xcdg:Column>
                                  <xcdg:Column FieldName="Bid" Title="Bid" />
                                  <xcdg:Column FieldName="Ask" Title="Ask" />

                                </xcdg:DataGridControl.Columns>
                              </xcdg:DataGridControl>
                            </DockPanel>
                          </TabItem>
                          <TabItem Header="Bots">
                            <xcdg:DataGridControl x:Name="BotsGrid" ItemsSource="{Binding Bots}">
                            </xcdg:DataGridControl>
                          </TabItem>

                          <TabItem Header="Historical Data">
                            <local:HistoricalData></local:HistoricalData>
                          </TabItem>
                          <TabItem Header="Scanners">
                            <xcdg:DataGridControl  x:Name="ScannersGrid"  ItemsSource="{Binding Scanners}" AutoCreateColumns="False">
                              <xcdg:DataGridControl.Columns>
                                <xcdg:Column FieldName="Self">
                                  <xcdg:Column.CellContentTemplate>
                                    <DataTemplate>
                                      <StackPanel Orientation="Horizontal">
                                        <ToggleButton IsChecked="{Binding IsScanEnabled}">Scan</ToggleButton>
                                        <ToggleButton IsChecked="{Binding IsLiveEnabled}">Live</ToggleButton>
                                        <ToggleButton IsChecked="{Binding IsDemoEnabled}">Demo</ToggleButton>
                                      </StackPanel>
                                    </DataTemplate>

                                  </xcdg:Column.CellContentTemplate>
                                </xcdg:Column>
                                <xcdg:Column FieldName="Type" Title="Type" />
                                <xcdg:Column FieldName="Id" Title="Id" />
                                <xcdg:Column FieldName="State" Title="State" DisplayMemberBinding="{Binding Bot.State.Value}"/>

                                <xcdg:Column FieldName="Symbol" Title="Symbol" />
                                <xcdg:Column FieldName="AD" Title="AD" />
                                <xcdg:Column FieldName="TPM" Title="TPM" />
                                <xcdg:Column FieldName="Days" Title="Days" />

                                <xcdg:Column FieldName="OpenLong" Title="L" DisplayMemberBinding="{Binding SignalBot.Indicator.OpenLongPoints.LastValue}"/>
                                <xcdg:Column FieldName="CloseLong" Title="Lx" DisplayMemberBinding="{Binding SignalBot.Indicator.CloseLongPoints.LastValue}"/>
                                <xcdg:Column FieldName="LongAt" Title="Long @" DisplayMemberBinding="{Binding LongPosition.EntryPrice}" />
                                <xcdg:Column FieldName="LongSL" Title="Long SL" DisplayMemberBinding="{Binding LongPosition.StopLoss}" />
                                <xcdg:Column FieldName="LongTP" Title="Long TP" DisplayMemberBinding="{Binding LongPosition.TakeProfit}" />

                                <xcdg:Column FieldName="OpenShort" Title="S"  DisplayMemberBinding="{Binding SignalBot.Indicator.OpenShortPoints.LastValue}"/>
                                <xcdg:Column FieldName="CloseShort" Title="Sx" DisplayMemberBinding="{Binding SignalBot.Indicator.CloseShortPoints.LastValue}"/>
                                <xcdg:Column FieldName="ShortAt" Title="Short @"  DisplayMemberBinding="{Binding ShortPosition.EntryPrice}" />
                                <xcdg:Column FieldName="ShortSL" Title="Short SL" DisplayMemberBinding="{Binding ShortPosition.StopLoss}"/>
                                <xcdg:Column FieldName="Short TP" Title="Short TP" DisplayMemberBinding="{Binding ShortPosition.TakeProfit}"/>

                              </xcdg:DataGridControl.Columns>
                            </xcdg:DataGridControl>
                          </TabItem>

                          <TabItem Header="Results">
                            <local:BacktestingView></local:BacktestingView>

                          </TabItem>

                          <TabItem Header="Overview">
                          </TabItem>

                          <TabItem Header="Optimization">
                          </TabItem>

                          <TabItem Header="Alerts">
                            <DockPanel>
                              <Button DockPanel.Dock="Bottom">Add</Button>
                              <ListView ItemTemplate="{StaticResource AlertItem}" ItemsSource="{Binding VM.Workspace.Alerts, ElementName=uc}">
                              </ListView>
                            </DockPanel>
                          </TabItem>


                          <TabItem Header="Settings">
                            <xctk:PropertyGrid NameColumnWidth="140" SelectedObject="{Binding Workspace.Settings}"/>
                          </TabItem>

                          <TabItem Header="Info">
                            <DockPanel>
                              <xctk:PropertyGrid NameColumnWidth="140" SelectedObject="{Binding Workspace.Info}"/>
                            </DockPanel>
                          </TabItem>

                        </TabControl>
                      </DataTemplate>
                    </TabControl.ItemTemplate>
                  </TabControl>

                </DockPanel>


              </xcad:LayoutDocument>
              <!--<xcad:LayoutDocument ContentId="workspace2" Title="Workspace 2">

                        </xcad:LayoutDocument>-->
            </xcad:LayoutDocumentPane>
          </xcad:LayoutDocumentPaneGroup >
          <!--<xcad:LayoutAnchorablePaneGroup DockWidth="125">
                    <xcad:LayoutAnchorablePane>
                        <xcad:LayoutAnchorable ContentId="alarms" Title="Alarms" >
                            <ListBox>
                                <s:String>Alarm 1</s:String>
                                <s:String>Alarm 2</s:String>
                                <s:String>Alarm 3</s:String>
                            </ListBox>
                        </xcad:LayoutAnchorable>
                        <xcad:LayoutAnchorable ContentId="journal" Title="Journal" >
                            <RichTextBox>
                                <FlowDocument>
                                    <Paragraph FontSize="14" FontFamily="Segoe">
                                        This is the content of the Journal Pane.
                                        <LineBreak/>
                                        A
                                        <Bold>RichTextBox</Bold> has been added here
                                    </Paragraph>
                                </FlowDocument>
                            </RichTextBox>
                        </xcad:LayoutAnchorable>
                    </xcad:LayoutAnchorablePane>
                </xcad:LayoutAnchorablePaneGroup>-->
        </xcad:LayoutPanel>

        <xcad:LayoutRoot.LeftSide>
          <xcad:LayoutAnchorSide>
            <xcad:LayoutAnchorGroup>
              <xcad:LayoutAnchorable Title="Master Control" ContentId="mastercontrol">
                <DockPanel  DockPanel.Dock="Top" LastChildFill="False">
                  <DockPanel DockPanel.Dock="Right" >
                    <CheckBox >
                      <StackPanel Orientation="Horizontal">
                        <TextBlock Text="{Binding AppVM.NumberOfLiveBots, ElementName=uc}"></TextBlock>
                        <TextBlock> Live Bots Enabled</TextBlock>
                      </StackPanel>
                    </CheckBox>
                  </DockPanel>
                </DockPanel>
              </xcad:LayoutAnchorable>
              <xcad:LayoutAnchorable Title="Contacts" ContentId="contacts" >
                <TextBlock Text="Contacts Content" Margin="10" FontSize="18" FontWeight="Black" TextWrapping="Wrap"/>
              </xcad:LayoutAnchorable>
            </xcad:LayoutAnchorGroup>
          </xcad:LayoutAnchorSide>
        </xcad:LayoutRoot.LeftSide>
      </xcad:LayoutRoot>
    </xcad:DockingManager>

  </DockPanel>

#endif