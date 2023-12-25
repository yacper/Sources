using System;
using System.Collections.Generic;
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

namespace Sparks.Scripts.Custom
{
    /// <summary>
    /// PositionLog.xaml 的交互逻辑
    /// </summary>
    public partial class PositionLogView : UserControl
    {
        public PositionLogView(object datacontext)
        {
            DataContext = datacontext;
            InitializeComponent();
        }

        public int LastSteps { get; set; } = 10;

        private void LoadPosition(object sender, RoutedEventArgs e)
        {
            (DataContext as Grid_IB).LoadPositionLog();
        }

        private void SavePosition(object sender, RoutedEventArgs e)
        {
            (DataContext as Grid_IB).SavePositionLog();
        }
    }
}
