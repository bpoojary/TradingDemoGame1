using IndiaHacksGame.Data;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace IndiaHacksGame.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        UserControl _view;
        public MainViewModel(UserControl view)
        {
            _view = view;

            string[] lables = new string[MainData.GetDurationInSeconds()];
            for (int i = 0; i < MainData.GetDurationInSeconds(); i++)
            {
                lables[i] = (i * MainData.GetUpdateTickerInSeconds()).ToString();
            }
            Labels = lables;
            var cultureInfo = new CultureInfo("hi-IN");
            var numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
            numberFormatInfo.CurrencySymbol = "₹";
            YFormatter = value => value.ToString("C", numberFormatInfo);
            Cash = MainData.GetCash();
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { MainData.GetStartingPrice() }
                },
            };
            YourOrders = new ObservableCollection<GridData>();
            AskBidDataCollection = new ObservableCollection<AskBidData>();
            
            Open = MainData.GetStartingPrice();
            CurrentPrice = MainData.GetStartingPrice();
            StockName = MainData.GetStockName();
            IntraDayStockName = "INTRADAY ■ " + StockName;
        }

        internal void Sell(int orderSize)
        {
            double currentPrice = CurrentPrice;
            CreateOrder("S", orderSize, currentPrice);
        }

        private void CreateOrder(string side, int orderSize, double currentPrice)
        {
            YourOrders.Add(new GridData() { Sym = StockName, Side = side, Price = currentPrice, Size = orderSize, OrderStamp = DateTime.Now, State = "Executed", OrderId = GetOrderId() });
            BuyQty = YourOrders.Sum(c => c.Side == "B" ? c.Size : 0); //BuyQty
            SellQty = YourOrders.Sum(c => c.Side == "S" ? c.Size : 0);//SellQty
            //Imbalance
            ImbalanceQty = Math.Abs(BuyQty - SellQty);
            if (ImbalanceQty == 0)
            {
                ImbalanceSide = "";
            }
            else
            {
                if (BuyQty > SellQty)
                    ImbalanceSide = "Buy";
                else
                    ImbalanceSide = "Sell";
            }
            NotionalProfit = CalculateNotionalProfitLoss();
            Cash = MainData.GetCash() + NotionalProfit;
        }

        internal void Buy(int orderSize)
        {
            double currentPrice = CurrentPrice;
            CreateOrder("B", orderSize, currentPrice);
        }

        public void Start()
        {
            CurrentNews = String.Empty;
            MainWindow.CountOfResult = 0;
            Initialize();
            StartTimer();
            IsGameRunning = true;
            Progress = 0;
            TimerCount = 0;
            UpdateAskBidCollection(MainData.GetStartingPrice());
            High = MainData.GetStartingPrice();
            Low = MainData.GetStartingPrice();
        }

        private void UpdateAskBidCollection(double seed)
        {
            bool add = false;
            if (AskBidDataCollection.Count == 0)
            {
                add = true;
            }
            var data = AskBidData.GetAskBidCollection(seed);
            for (int i = 0; i < 8; i++)
            {
                if (add)
                    AskBidDataCollection.Add(data[i]);
                else
                {
                    AskBidDataCollection[i].Bid = data[i].Bid;
                    AskBidDataCollection[i].Ask = data[i].Ask;
                    AskBidDataCollection[i].BidQty = data[i].BidQty;
                    AskBidDataCollection[i].AskQty = data[i].AskQty;
                    AskBidDataCollection[i].AskPercentage = data[i].AskPercentage;
                    AskBidDataCollection[i].BidPercentage = data[i].BidPercentage;
                }
            }
            OnPropertyChanged("AskBidDataCollection");
        }

        private void Initialize()
        {
            _data = new MainData();
            Profit = 0.0;
            _data.Initialize(StockName, SymbolDataObject, isPrimary);
            //StockName = _data.StockName;
            IntraDayStockName = "INTRADAY ■ " + StockName;
            SeriesCollection[0].Values.Clear();
            OnPropertyChanged("SeriesCollection");
            SeriesCollection[0].Values.Add(MainData.GetStartingPrice());
            OnPropertyChanged("SeriesCollection");
            YourOrders.Clear();
            SellQty = 0;
            BuyQty = 0;
            ImbalanceQty = 0;
            ImbalanceSide = string.Empty;
            Cash = MainData.GetCash();
            NotionalProfit = 0;
        }
        private void StartTimer()
        {
            Close = _data.Price.Peek() - 1;
            CurrentPrice = _data.Price.Peek();
            if (_frequencyTimer == null)
            {
                _frequencyTimer = new Timer();
                _frequencyTimer.Interval = MainData.GetUpdateTickerInSeconds() * 1000;
                _frequencyTimer.Elapsed += _timer_Elapsed;
            }
            _realTimerCounter = 0;
            if (_realTimer == null)
            {
                _realTimer = new Timer();
                _realTimer.Interval = 1000;
                _realTimer.Elapsed += _realTimer_Elapsed;
            }
            _realTimer.Enabled = true;
            _realTimer.Start();
            _frequencyTimer.Enabled = true;
            _frequencyTimer.Start();
        }
        int _realTimerCounter = 0;
        private void _realTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _realTimerCounter++;
            if(_realTimerCounter==19 && isPrimary)
            {
                CurrentNews = SymbolDataObject.News.Content;
            }
            else if (_realTimerCounter == 39 && !isPrimary)
            {
                CurrentNews = SymbolDataObject.News.Content;
            }
            if (_realTimerCounter == MainData.GetDurationInSeconds())
            {
                TimerCount = MainData.GetDurationInSeconds();
                Progress = 100;
                if (IsGameRunning)
                {
                    IsGameRunning = false;
                    Stop();
                }
                return;
            }
            else
            {
                TimerCount++;
                Progress = TimerCount * 100 / MainData.GetDurationInSeconds();
            }
        }

        private static object syncObject = new object();
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (syncObject)
            {
                if (_data.Price.Count > 0)
                {
                    var price = _data.Price.Dequeue();
                    CurrentPrice = price;
                    NotionalProfit = CalculateNotionalProfitLoss();
                    Cash = MainData.GetCash() + NotionalProfit;
                }
            }
        }
        private void Stop()
        {
            //HideChildWindow();
            IsGameRunning = false;
            _realTimer.Enabled = false;
            _realTimer.Stop();
            _frequencyTimer.Stop();
            _frequencyTimer.Enabled = false;
            NotionalProfit = CalculateNotionalProfitLoss();
            Cash = MainData.GetCash() + NotionalProfit;
            CalculateProfitLoss();
            lock(MainWindow.SyncObj)
            {
                MainWindow.CountOfResult++;
                MainWindow.UpdateLeadersBoard();
                FocusOnName();
            }
        }
        public bool isPrimary { get; set; }
        //private void HideChildWindow()
        //{
        //    _view.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
        //    {
        //        _mainWindow.HideChildWindow();
        //    }
        //    ));
        //}
        private void FocusOnName()
        {
            _view.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                MainWindow.FocusOnName();
            }
            ));
        }

        
        private void CalculateProfitLoss()
        {
            _view.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {

                int sizeOfBuy = YourOrders.Sum(o => (o.Side == "B") ? o.Size : 0);
                int sizeOfSell = YourOrders.Sum(o => (o.Side == "S") ? o.Size : 0);
                if (sizeOfBuy != sizeOfSell) //Square off any open positions
                {
                    if (sizeOfBuy > sizeOfSell)
                    {
                        CreateOrder("S", sizeOfBuy - sizeOfSell, CurrentPrice);
                    }
                    else if (sizeOfBuy < sizeOfSell)
                    {
                        CreateOrder("B", -sizeOfBuy + sizeOfSell, CurrentPrice);
                    }
                }
                //now position of open and sell should be same
                double buyAmount = YourOrders.Sum(o => (o.Side == "B") ? o.Size * o.Price : 0);
                double sellAmount = YourOrders.Sum(o => (o.Side == "S") ? o.Size * o.Price : 0);
                Profit = Math.Round(sellAmount - buyAmount, 2);
            }));
        }
        private double CalculateNotionalProfitLoss()
        {
            var cloned = YourOrders.ToList();
            int sizeOfBuy = cloned.Sum(o => (o.Side == "B") ? o.Size : 0);
            int sizeOfSell = cloned.Sum(o => (o.Side == "S") ? o.Size : 0);
            if (sizeOfBuy == 0 && sizeOfSell == 0)
                return 0;
            if (sizeOfBuy != sizeOfSell) //Square off any open positions
            {
                if (sizeOfBuy > sizeOfSell)
                {
                    cloned.Add(new GridData() { Sym = StockName, Side = "S", Price = CurrentPrice, Size = sizeOfBuy - sizeOfSell, OrderStamp = DateTime.Now, State = "Executed", OrderId = GetOrderId() });
                }
                else if (sizeOfBuy < sizeOfSell)
                {
                    cloned.Add(new GridData() { Sym = StockName, Side = "B", Price = CurrentPrice, Size = -sizeOfBuy + sizeOfSell, OrderStamp = DateTime.Now, State = "Executed", OrderId = GetOrderId() });
                }
            }
            //now position of open and sell should be same
            double buyAmount = cloned.Sum(o => (o.Side == "B") ? o.Size * o.Price : 0);
            double sellAmount = cloned.Sum(o => (o.Side == "S") ? o.Size * o.Price : 0);
            return Math.Round(sellAmount - buyAmount, 2);
        }
        private int GetPortfolio()
        {
            return -1;
        }
        private int GetSellSize()
        {
            return -1;
        }
        private int GetBuySize()
        {
            return -1;
        }
        private int GetImbalanceStock()
        {
            return -1;
        }
        MainData _data;
        Timer _frequencyTimer;
        Timer _realTimer;
        #region Bindings
        Random _random = new Random();
        public double CurrentPrice
        {
            get
            {
                return _currentPrice;
            }
            set
            {
                _currentPrice = Math.Round(value, 2);
                Change = Math.Round(-Close + value, 2);
                ChangePercentage = Math.Round(Change * 100 / Close, 2);
                High = High < _currentPrice ? _currentPrice : High;
                Low = Low > _currentPrice ? _currentPrice : Low;
                if (_data != null && _data.Price != null && _data.Price.Count > 0)
                {
                    SeriesCollection[0].Values.Add(_currentPrice);
                    UpdateAskBidCollection(_data.Price.Peek());
                }
                OnPropertyChanged("CurrentPrice");
            }
        }
        private double _open;
        public double Open
        {
            get
            {
                return _open;
            }
            set
            {
                _open = value;
                OnPropertyChanged("Open");
            }
        }
        public double Change
        {
            get
            {
                return _change;
            }
            set
            {
                _change = value;
                OnPropertyChanged("Change");
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
        public double ChangePercentage
        {
            get
            {
                return _changePercentage;
            }
            set
            {
                _changePercentage = value;
                OnPropertyChanged("ChangePercentage");
            }
        }
        private string _currentNews = string.Empty;
        public string CurrentNews
        {
            get
            {
                return _currentNews;
            }
            set
            {
                _currentNews = value;
                OnPropertyChanged("CurrentNews");
            }
        }
        public string StockName
        {
            get
            {
                return _stockName;
            }
            set
            {
                _stockName = value;
                FormattedStockName = value;
                OnPropertyChanged("StockName");
            }
        }
        public string FormattedStockName
        {
            get
            {
                return "(" + _stockName + ")";
            }
            set
            {
                _stockName = value;
                OnPropertyChanged("FormattedStockName");
            }
        }
        string _intraDayStockName = string.Empty;
        public string IntraDayStockName
        {
            get
            {
                return _intraDayStockName;
            }
            set
            {
                _intraDayStockName = value;
                OnPropertyChanged("IntraDayStockName");
            }
        }
        public double Close
        {
            get
            {
                return _close;
            }
            set
            {
                _close = value;
                OnPropertyChanged("Close");
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
                MainWindow.TimerCount = value;
                OnPropertyChanged("TimerCount");
            }
        }
        public double High
        {
            get
            {
                return _high;
            }
            set
            {
                _high = value;
                OnPropertyChanged("High");
            }
        }
        public double Low
        {
            get
            {
                return _low;
            }
            set
            {
                _low = value;
                OnPropertyChanged("Low");
            }
        }
        bool _isGameRunning;
        public bool IsGameRunning
        {
            get
            {
                return _isGameRunning;
            }
            set
            {
                _isGameRunning = value;
                MainWindow.IsGameRunning = value;
                OnPropertyChanged("IsGameRunning");
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
                MainWindow.Progress = value;
                OnPropertyChanged("Progress");
            }
        }


        
        public int BuyQty
        {
            get
            {
                return _buyQty;
            }
            set
            {
                _buyQty = value;
                OnPropertyChanged("BuyQty");
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
                _cash = value;
                if(MainWindow!=null)
                MainWindow.Cash = value;
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
                _notionalProfit = value;
                MainWindow.NotionalProfit = value;
                OnPropertyChanged("NotionalProfit");
            }
        }
        public int ImbalanceQty
        {
            get
            {
                return _imbalanceQty;
            }
            set
            {
                _imbalanceQty = value;
                OnPropertyChanged("ImbalanceQty");
            }
        }
        public string ImbalanceSide
        {
            get
            {
                return _imbalanceSide;
            }
            set
            {
                _imbalanceSide = value;
                OnPropertyChanged("ImbalanceSide");
            }
        }
        public int SellQty
        {
            get
            {
                return _sellQty;
            }
            set
            {
                _sellQty = value;
                OnPropertyChanged("SellQty");
            }
        }
        public ObservableCollection<GridData> YourOrders { get; set; }
        RelayCommand<object> _buyCommand;
        public ICommand BuyCommand
        {
            get
            {
                if (_buyCommand == null)
                {
                    _buyCommand = new RelayCommand<object>(param => this.Buy(),
                        param => this.CanBuy);
                }
                return _buyCommand;
            }
        }

        RelayCommand<object> _sellCommand;
        public ICommand SellCommand
        {
            get
            {
                if (_sellCommand == null)
                {
                    _sellCommand = new RelayCommand<object>(param => this.Sell(),
                        param => this.CanSell);
                }
                return _sellCommand;
            }
        }
        private string GetOrderId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        }
        private void Buy()
        {
            CreateOrder("B", 100, CurrentPrice);
        }
        private void Sell()
        {
            CreateOrder("S", 100, CurrentPrice);
        }
        public ObservableCollection<AskBidData> AskBidDataCollection { get; set; }
  
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public bool CanBuy { get { return IsGameRunning; } }

        public bool CanSell { get { return IsGameRunning; } }

        public MainWindow MainWindow { get; set; }
        public SymbolData SymbolDataObject { get; internal set; }

        private double _change;
        private double _currentPrice;
        private double _changePercentage;
        private string _stockName;
        private double _close = MainData.GetStartingPrice() - 1;
        private double _high = MainData.GetStartingPrice();
        private double _low = MainData.GetStartingPrice();
        private double _profit = 0.0;


        private int _buyQty = 0;
        private int _sellQty = 0;
        private int _imbalanceQty = 0;
        private string _imbalanceSide = string.Empty;
        private double _cash = MainData.GetCash();
        private double _notionalProfit = 0;
        #endregion
    }
}
