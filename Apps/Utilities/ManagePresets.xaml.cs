using NetworkAnalyzer.Apps.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkAnalyzer.Utilities
{
    public partial class ManagePresets : UserControl
    {
        public ManagePresets()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PresetNamesProperty =
            DependencyProperty.Register(nameof(PresetNames), typeof(ObservableCollection<LatencyMonitorPreset>), typeof(ManagePresets), new PropertyMetadata());

        public static readonly DependencyProperty PresetNameProperty =
            DependencyProperty.Register(nameof(PresetName), typeof(string), typeof(ManagePresets), new PropertyMetadata());

        public static readonly DependencyProperty TargetNameProperty =
            DependencyProperty.Register(nameof(TargetName), typeof(string), typeof(ManagePresets), new PropertyMetadata());

        public static readonly DependencyProperty SavePresetCommandProperty =
            DependencyProperty.Register(nameof(SavePresetCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty DeletePresetCommandProperty =
            DependencyProperty.Register(nameof(DeletePresetCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty AddItemCommandProperty =
            DependencyProperty.Register(nameof(AddItemCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty RemoveItemCommandProperty =
            DependencyProperty.Register(nameof(RemoveItemCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty CloseWindowCommandProperty =
            DependencyProperty.Register(nameof(CloseWindowCommand), typeof(ICommand), typeof(ManagePresets));

        public ObservableCollection<LatencyMonitorPreset> PresetNames
        {
            get => (ObservableCollection<LatencyMonitorPreset>)GetValue(PresetNamesProperty);
            set => SetValue(PresetNamesProperty, value);
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

        public ICommand SavePresetCommand
        {
            get => (ICommand)GetValue(SavePresetCommandProperty);
            set => SetValue(SavePresetCommandProperty, value);
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

        public ICommand CloseWindowCommand
        {
            get => (ICommand)GetValue(CloseWindowCommandProperty);
            set => SetValue(CloseWindowCommandProperty, value);
        }
    }
}
