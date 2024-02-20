using System.Windows.Controls;
using IOTDevices.Infra;
using IOTDevices.ViewModels;

namespace IOTDevices;
/// <summary>
/// Interaction logic for PressureSensorView.xaml
/// </summary>
public partial class PressureSensorView : UserControl
{
    public PressureSensorView()
    {
        InitializeComponent();
        DataContext = ViewModelLocator.GetViewModel<PressureViewModel>();
    }
}
