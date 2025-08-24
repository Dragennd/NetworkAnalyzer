using NetworkAnalyzer.Apps.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkAnalyzer.Apps.Utilities
{
    public partial class ManagePresets : UserControl
    {
        public ManagePresets()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TargetProfilesProperty =
            DependencyProperty.Register(nameof(TargetProfiles), typeof(ObservableCollection<LatencyMonitorPreset>), typeof(ManagePresets), new PropertyMetadata());

        public static readonly DependencyProperty SelectedPresetProperty =
            DependencyProperty.Register(nameof(SelectedPreset), typeof(LatencyMonitorPreset), typeof(ManagePresets), new PropertyMetadata());

        public static readonly DependencyProperty PresetNameProperty =
            DependencyProperty.Register(nameof(PresetName), typeof(string), typeof(ManagePresets), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TargetNameProperty =
            DependencyProperty.Register(nameof(TargetName), typeof(string), typeof(ManagePresets), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsNonDefaultPresetSelectedProperty =
            DependencyProperty.Register(nameof(IsNonDefaultPresetSelected), typeof(bool), typeof(ManagePresets), new PropertyMetadata(false));

        public static readonly DependencyProperty SavePresetCommandProperty =
            DependencyProperty.Register(nameof(SavePresetCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty NewPresetCommandProperty =
            DependencyProperty.Register(nameof(NewPresetCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty DeletePresetCommandProperty =
            DependencyProperty.Register(nameof(DeletePresetCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty AddItemCommandProperty =
            DependencyProperty.Register(nameof(AddItemCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty RemoveItemCommandProperty =
            DependencyProperty.Register(nameof(RemoveItemCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty ClosePresetWindowCommandProperty =
            DependencyProperty.Register(nameof(ClosePresetWindowCommand), typeof(ICommand), typeof(ManagePresets));

        public ObservableCollection<LatencyMonitorPreset> TargetProfiles
        {
            get => (ObservableCollection<LatencyMonitorPreset>)GetValue(TargetProfilesProperty);
            set => SetValue(TargetProfilesProperty, value);
        }

        public LatencyMonitorPreset SelectedPreset
        {
            get => (LatencyMonitorPreset)GetValue(SelectedPresetProperty);
            set => SetValue(SelectedPresetProperty, value);
        }

        public string PresetName
        {
            get => (string)GetValue(PresetNameProperty);
            set => SetValue(PresetNameProperty, value);
        }

        public string TargetName
        {
            get => (string)GetValue(TargetNameProperty);
            set => SetValue(TargetNameProperty, value);
        }

        public bool IsNonDefaultPresetSelected
        {
            get => (bool)GetValue(IsNonDefaultPresetSelectedProperty);
            set => SetValue(IsNonDefaultPresetSelectedProperty, value);
        }

        public ICommand SavePresetCommand
        {
            get => (ICommand)GetValue(SavePresetCommandProperty);
            set => SetValue(SavePresetCommandProperty, value);
        }

        public ICommand NewPresetCommand
        {
            get => (ICommand)GetValue(NewPresetCommandProperty);
            set => SetValue(NewPresetCommandProperty, value);
        }

        public ICommand DeletePresetCommand
        {
            get => (ICommand)GetValue(DeletePresetCommandProperty);
            set => SetValue(DeletePresetCommandProperty, value);
        }

        public ICommand AddItemCommand
        {
            get => (ICommand)GetValue(AddItemCommandProperty);
            set => SetValue(AddItemCommandProperty, value);
        }

        public ICommand RemoveItemCommand
        {
            get => (ICommand)GetValue(RemoveItemCommandProperty);
            set => SetValue(RemoveItemCommandProperty, value);
        }

        public ICommand ClosePresetWindowCommand
        {
            get => (ICommand)GetValue(ClosePresetWindowCommandProperty);
            set => SetValue(ClosePresetWindowCommandProperty, value);
        }
    }
}
