using IndiaHacksGame.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IndiaHacksGame
{
    /// <summary>
    /// Interaction logic for StockUIControl.xaml
    /// </summary>
    public partial class StockUIControl : UserControl
    {
        public StockUIControl()
        {
            InitializeComponent();
            this.Loaded += StockUIControl_Loaded;
            _viewModel = new MainViewModel(this);
            
            this.DataContext = _viewModel;
            _timer = new DispatcherTimer();
            _timer.Interval = System.TimeSpan.FromSeconds(MainData.GetHideYourProfitAfterSec());
            _timer.Tick += timer_Tick;
        }

        private void StockUIControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.isPrimary = IsPrimary;
        }

        public bool IsPrimary { get; set; }
        private void timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            ((MainWindow)Window.GetWindow(this)).SetStkProfitVisibility(Visibility.Collapsed);
        }

        MainViewModel _viewModel;
        BuySellDlg buySellDlg_ = new BuySellDlg();
        DispatcherTimer _timer;
        private void BtnSqrOffClick(object sender, RoutedEventArgs e)
        {
            string side = _viewModel.ImbalanceSide;
            int size = _viewModel.ImbalanceQty;
            if (string.IsNullOrEmpty(side))
                return;
            if (size == 0 || size < 0)
                return;
            buySellDlg_.Initilize(side == "Buy" ? "Sell" : "Buy", size);
            buySellDlg_.ShowDialog();
            if (!buySellDlg_.IsCancelled)
            {
                if (side == "Buy")
                    _viewModel.Sell(buySellDlg_.OrderSize);
                else
                    _viewModel.Buy(buySellDlg_.OrderSize);
            }
        }
        internal void StartTimer()
        {
            _timer.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.MainWindow = (MainWindow)Window.GetWindow(this);
        }
    }
}
