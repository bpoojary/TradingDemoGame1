using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IndiaHacksGame
{
    /// <summary>
    /// Interaction logic for BuySellDlg.xaml
    /// </summary>
    public partial class BuySellDlg : MetroWindow
    {
        public BuySellDlg()
        {
            InitializeComponent();
            numSize.Maximum = MainData.GetMaximumOrderSizeInBuySellDlg();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbSide.SelectedIndex ==0)
            {
                btnBuySell.Content = "Buy";
                this.Title = "Buy";
            }
            else
            {
                btnBuySell.Content = "Sell";
                this.Title = "Sell";
            }
        }

        internal void Initilize(string side, int size = 500)
        {
            IsCancelled = false;
            if (side == "Buy")
                cmbSide.SelectedIndex = 0;
            else
                cmbSide.SelectedIndex = 1;
            numSize.Value = size;
        }
        public int OrderSize { get; set; }
        public string Side { get; set; }
        private void btnBuySell_Click(object sender, RoutedEventArgs e)
        {
            OrderSize = (int)numSize.Value;
            Side = cmbSide.SelectedIndex == 0 ? "Buy" : "Sell";
            this.Hide();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            IsCancelled = true;
            Hide();
        }
        public bool IsCancelled { get; set; }
    }
}
