using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

namespace IndiaHacksGame
{
    /// <summary>
    /// Interaction logic for StockInfoDialog.xaml
    /// </summary>
    public partial class StockInfoDialog : MetroWindow
    {
        public StockInfoDialog(List<SymbolData> symbolData, Dictionary<string, string> _companyInformation)
        {
            InitializeComponent();
            txtFirstStock.Text = symbolData[0].SymbolName;
            txtFirstStockDesc.Text = "(" + _companyInformation[symbolData[0].SymbolName] + ")";
            txtSecondStock.Text = symbolData[1].SymbolName;
            txtSecondStockDesc.Text = "(" + _companyInformation[symbolData[1].SymbolName] + ")";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
