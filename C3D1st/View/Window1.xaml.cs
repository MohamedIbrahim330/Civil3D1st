using AlignmentTool.ViewModel;
using System.Windows;

namespace C3D1st.View
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            var viewModel = new AlignmentToolViewModel();
            DataContext = viewModel;
        }
    }
}
