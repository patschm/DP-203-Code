using System.Windows.Controls;
using IOTDevices.Infra;
using IOTDevices.ViewModels;

namespace IOTDevices;
/// <summary>
/// Interaction logic for MovementSensorView.xaml
/// </summary>
public partial class MovementSensorView : UserControl
{
    public MovementSensorView()
    {
        InitializeComponent();
        DataContext = ViewModelLocator.GetViewModel<MovementViewModel>();
    }
}

