using IndiaHacksGame.ViewModel;
using MahApps.Metro.Controls;
using System.Configuration;
using System.Windows;
using System.Windows.Threading;
using System;
using System.Windows.Input;
using System.ComponentModel;
using IndiaHacksGame.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IndiaHacksGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged 
    {
        Dictionary<string, string> _companyInformation = new Dictionary<string, string>();
        Dictionary<string, List<CompanyNews>> _newsData = new Dictionary<string, List<CompanyNews>>();
        public MainWindow()
        {
            InitializeComponent();
            LeadersBoard = new ObservableCollection<PlayersData>();
            this.DataContext = this;
            Title = ConfigurationManager.AppSettings["Title"].ToString();
            FocusOnName();
            News = ConfigurationManager.AppSettings["News"].ToString();
            LoadSymbolData();
        }

        private void LoadSymbolData()
        {
            string txt = System.IO.File.ReadAllText("company.csv");
            string[] companyLines = txt.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach(var line in companyLines)
            {
                string[] info = line.Split(',');
                if(!_companyInformation.ContainsKey(info[0]))
                {
                    _companyInformation.Add(info[0], line.Substring(info[0].Length+1));
                }
            }
            string newsTxt = System.IO.File.ReadAllText("news.csv");
            string[] newsLines = newsTxt.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            bool firstLine = true;
            foreach (var line in newsLines)
            {
                if(firstLine)
                {
                    firstLine = false;
                    continue;
                }
                string[] info = line.Split(',');
                string company = info[0];
                if (!_newsData.ContainsKey(company))
                {
                    List<CompanyNews> data = new List<CompanyNews>();
                    data.Add(new CompanyNews() {  Content =info[2] , isPositive = info[1].Equals("+"), SwingPercentage = Convert.ToDouble(info[3])});
                    _newsData.Add(company, data);
                }
                else
                {
                    var companyNewsData = new CompanyNews() { Content = info[2], isPositive = info[1].Equals("+"), SwingPercentage = Convert.ToDouble(info[3]) };
                    _newsData[company].Add(companyNewsData);
                }

            }
        }

        public string News
        {
            get
            {
                return _news;
            }
            set
            {
                _news = value;
                OnPropertyChanged("News");
            }
        }
        //private void MetroWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (btnBuy.IsEnabled && btnBuy.Command.CanExecute(sender) || btnSell.IsEnabled && btnSell.Command.CanExecute(sender))
        //    {
        //        if (e.Key == System.Windows.Input.Key.Left)
        //        {
        //            if (btnBuy.IsEnabled && btnBuy.Command.CanExecute(sender))
        //                btnBuy.Command.Execute(sender);

        //        }
        //        else if (e.Key == System.Windows.Input.Key.Right)
        //        {
        //            if (btnSell.IsEnabled && btnSell.Command.CanExecute(sender))
        //                btnSell.Command.Execute(sender);
        //        }
        //        else if (e.Key == System.Windows.Input.Key.B)
        //        {
        //            buySellDlg_.Initilize("Buy");
        //            buySellDlg_.ShowDialog();
        //            if (!buySellDlg_.IsCancelled)
        //                _viewModel.Buy(buySellDlg_.OrderSize);
        //        }
        //        else if (e.Key == System.Windows.Input.Key.S)
        //        {
        //            buySellDlg_.Initilize("Sell");
        //            buySellDlg_.ShowDialog();
        //            if (!buySellDlg_.IsCancelled)
        //                _viewModel.Sell(buySellDlg_.OrderSize);
        //        }
        //    }
        //}
        //public void HideChildWindow()
        //{
        //    buySellDlg_.IsCancelled = true;
        //    buySellDlg_.Hide();
        //}
        public void FocusOnName()
        {
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        public void SetStkProfitVisibility(Visibility visibility)
        {
            stkProfit.Visibility = visibility;
        }
        private void txtName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(!string.IsNullOrEmpty(txtName.Text) && stkProfit.Visibility !=Visibility.Collapsed)
            {
                stkProfit.Visibility = Visibility.Collapsed;
            }
        }

        private void txtName_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!_firstLaunchDone)
            {
                stkProfit.Visibility = Visibility.Collapsed;
                _firstLaunchDone = true;
            }
            else
            {
                
                
                stkProfit.Visibility = Visibility.Visible;
                stockUIControl1.StartTimer();
                stockUIControl2.StartTimer();
            }
        }
        public ObservableCollection<PlayersData> LeadersBoard { get; set; }
        public static object SyncObj = new object();
        public void UpdateLeadersBoard()
        {
            lock(SyncObj)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (CountOfResult != 2)
                        return;
                    CountOfResult = 0;
                    Profit = ((MainViewModel)stockUIControl1.DataContext).Profit + ((MainViewModel)stockUIControl2.DataContext).Profit;
                    if (LeadersBoard.Count == 0)
                    {
                        LeadersBoard.Add(new PlayersData() { Rank = 1, Name = CurrentPlayerName, Profit = Profit });
                    }
                    else
                    {
                        //get current players profit
                        double currentProfit = Profit;
                        string currentPlayersName = CurrentPlayerName;
                        if (LeadersBoard.Count < 3)
                        {
                            LeadersBoard.Add(new PlayersData() { Rank = -1, Name = CurrentPlayerName, Profit = Profit });
                        }
                        //First check if current profit comes in top 3
                        else if (LeadersBoard.Min(c => c.Profit) < currentProfit)
                        {
                            LeadersBoard.Add(new PlayersData() { Rank = -1, Name = CurrentPlayerName, Profit = Profit });
                        }
                        //adjust the ranking
                        List<double> leadersBoardProfits = LeadersBoard.Select(c => c.Profit).OrderByDescending(d => d).ToList();
                        foreach (var p in LeadersBoard)
                        {
                            int indexOfProfit = leadersBoardProfits.IndexOf(p.Profit);
                            if (indexOfProfit != -1)
                            {
                                p.Rank = indexOfProfit + 1;
                                leadersBoardProfits[indexOfProfit] = Double.MinValue;
                            }
                        }
                        if (LeadersBoard.Count > 3)
                            LeadersBoard.Remove(LeadersBoard.First(cp => cp.Rank == LeadersBoard.Max(c => c.Rank)));
                        List<PlayersData> updated = LeadersBoard.ToList().OrderBy(c => c.Rank).ToList();
                        LeadersBoard.Clear();
                        foreach (var p in updated)
                            LeadersBoard.Add(p);
                    }
                    CurrentPlayerName = string.Empty;
                }));
            }
        }
        private string _currentPlayerName = string.Empty;
        public string CurrentPlayerName
        {
            get
            {
                return _currentPlayerName;
            }
            set
            {
                _currentPlayerName = value;
                OnPropertyChanged("CurrentPlayerName");
            }
        }
        double _progress = 0;
        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }
        RelayCommand<object> _playCommand;
        public ICommand PlayCommand
        {
            get
            {
                if (_playCommand == null)
                {
                    _playCommand = new RelayCommand<object>(param => this.Play(), param => this.CanPlay);
                }
                return _playCommand;
            }
        }

        public bool CanPlay { get { return !string.IsNullOrEmpty(CurrentPlayerName); } }

        private void Play()
        {
            var symbolData = Get2SymbolData();
            ((MainViewModel)stockUIControl1.DataContext).StockName = symbolData[0].SymbolName;
            ((MainViewModel)stockUIControl1.DataContext).SymbolDataObject = symbolData[0];
            ((MainViewModel)stockUIControl2.DataContext).StockName = symbolData[1].SymbolName;
            ((MainViewModel)stockUIControl2.DataContext).SymbolDataObject = symbolData[1];
            ((MainViewModel)stockUIControl1.DataContext).Start();
            ((MainViewModel)stockUIControl2.DataContext).Start();
        }
        public int CountOfResult { get; set; }
        private List<SymbolData> Get2SymbolData()
        {
            List<SymbolData> symbolData = new List<SymbolData>();

            //var symbolsToUse = _companyInformation.Keys.ToList().Take(2).ToList();
            List<string> symbolsToUse = new List<string>();
            var tempList = _companyInformation.Keys.ToList();
            Random random = new Random();
            int index = random.Next(tempList.Count);
            symbolsToUse.Add(tempList[index]);
            tempList.RemoveAt(index);
            index = random.Next(tempList.Count);
            symbolsToUse.Add(tempList[index]);
            foreach (var symbol in symbolsToUse)
            {
                var news = _newsData[symbol].ToList();
                index = random.Next(news.Count);

                symbolData.Add(new SymbolData() { SymbolName = symbol, News = news[index] });
            }
            StockInfoDialog dlg = new StockInfoDialog(symbolData, _companyInformation);
            dlg.ShowDialog();
            return symbolData;
        }

        public bool IsGameRunning
        {
            get
            {
                return _isGameRunning;
            }
            set
            {
                _isGameRunning = value;
                OnPropertyChanged("IsGameRunning");
            }
        }
        int _timerCount = 0;
        public int TimerCount
        {
            get
            {
                return _timerCount;
            }
            set
            {
                _timerCount = value;
                TimerReverseCount = 60 - value;
                OnPropertyChanged("TimerCount");
            }
        }
        int _timerReverseCount = 0;
        public int TimerReverseCount
        {
            get
            {
                return _timerReverseCount;
            }
            set
            {
                _timerReverseCount = value;
                OnPropertyChanged("TimerReverseCount");
            }
        }
        public double Cash
        {
            get
            {
                return _cash;
            }
            set
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _cash = ((MainViewModel)stockUIControl1.DataContext).Cash +
                       ((MainViewModel)stockUIControl2.DataContext).Cash;
                }
                ));
               
                OnPropertyChanged("Cash");
            }
        }
        public double NotionalProfit
        {
            get
            {
                return _notionalProfit;
            }
            set
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _notionalProfit = ((MainViewModel)stockUIControl1.DataContext).NotionalProfit +
                     ((MainViewModel)stockUIControl2.DataContext).NotionalProfit;
                }
                ));
                OnPropertyChanged("NotionalProfit");
            }
        }
        public double Profit
        {
            get
            {
                return _profit;
            }
            set
            {
                _profit = value;
                IsProfitValueNegative = value < 0;
                OnPropertyChanged("Profit");
            }
        }
        bool _isProfitValueNegative = false;
        public bool IsProfitValueNegative
        {
            get
            {
                return _isProfitValueNegative;
            }
            set
            {
                _isProfitValueNegative = value;
                OnPropertyChanged("IsProfitValueNegative");
            }
        }
        private double _notionalProfit = 0;
        private string _news = string.Empty;
        bool _isGameRunning;
        bool _firstLaunchDone = false;
        private double _cash = MainData.GetCash()*2;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private double _profit = 0.0;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
