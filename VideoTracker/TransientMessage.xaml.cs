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
using System.Windows.Shapes;

namespace VideoTracker
{
    /// <summary>
    /// Interaction logic for TransientMessage.xaml
    /// </summary>
    public partial class TransientMessage : Window, IDisposable
    {
        public string message;

        public TransientMessage(Window parent, string message)
        {
            InitializeComponent();
            this.Owner = parent;
            this.Message.Content = message;
            this.Show();
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
