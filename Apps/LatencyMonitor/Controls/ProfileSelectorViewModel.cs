using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using System.Collections.ObjectModel;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Controls
{
    internal partial class ProfileSelectorViewModel : ObservableValidator
    {
        #region Control Properties
        public ObservableCollection<LatencyMonitorTargetProfiles> TargetProfiles { get; set; }

        [ObservableProperty]
        public LatencyMonitorTargetProfiles selectedTargetProfiles;

        [ObservableProperty]
        public bool isTargetProfileListScreenVisible = true;

        [ObservableProperty]
        public bool isNewTargetProfileScreenVisible = false;

        [ObservableProperty]
        public bool isUserTargetsChecked = true;

        [ObservableProperty]
        public bool isTracerouteChecked = false;

        [ObservableProperty]
        public string name = string.Empty;

        [ObservableProperty]
        public string target1 = string.Empty;

        [ObservableProperty]
        public string target2 = string.Empty;

        [ObservableProperty]
        public string target3 = string.Empty;

        [ObservableProperty]
        public string target4 = string.Empty;

        [ObservableProperty]
        public string target5 = string.Empty;

        [ObservableProperty]
        public int hops = 0;
        #endregion

        public ProfileSelectorViewModel()
        {
            TargetProfiles = new();
            SelectedTargetProfiles = new();
        }

        [RelayCommand]
        public void NewTargetProfile()
        {
            IsTargetProfileListScreenVisible = false;
            IsNewTargetProfileScreenVisible = true;
            SelectedTargetProfiles = null;
        }

        [RelayCommand]
        public void EditTargetProfile()
        {
            IsTargetProfileListScreenVisible = false;
            IsNewTargetProfileScreenVisible = true;

            Name = SelectedTargetProfiles.ProfileName;
            Hops = SelectedTargetProfiles.Hops;
            Target1 = SelectedTargetProfiles.Target1;
            Target2 = SelectedTargetProfiles.Target2;
            Target3 = SelectedTargetProfiles.Target3;
            Target4 = SelectedTargetProfiles.Target4;
            Target5 = SelectedTargetProfiles.Target5;

            if (SelectedTargetProfiles.ReportType == ReportType.UserTargets)
            {
                IsUserTargetsChecked = true;
                IsTracerouteChecked = false;
            }
            else
            {
                IsUserTargetsChecked = false;
                IsTracerouteChecked = true;
            }
        }

        [RelayCommand]
        public async Task RemoveTargetProfileAsync()
        {
            var dbHandler = new DatabaseHandler();
            await dbHandler.DeleteSelectedProfilesAsync(SelectedTargetProfiles);
            await LoadTargetProfilesAsync();
        }

        [RelayCommand]
        public async Task SaveTargetProfileAsync()
        {
            var dbHandler = new DatabaseHandler();

            if (SelectedTargetProfiles == null)
            {
                var newData = new LatencyMonitorTargetProfiles()
                {
                    ProfileName = Name,
                    Hops = Hops,
                    Target1 = Target1,
                    Target2 = Target2,
                    Target3 = Target3,
                    Target4 = Target4,
                    Target5 = Target5
                };

                if (IsUserTargetsChecked)
                {
                    newData.ReportType = ReportType.UserTargets;
                }
                else
                {
                    newData.ReportType = ReportType.Traceroute;
                }

                await dbHandler.NewLatencyMonitorTargetProfile(newData);

                IsTargetProfileListScreenVisible = true;
                IsNewTargetProfileScreenVisible = false;

                ClearChangeTargetProfilesForm();
                await LoadTargetProfilesAsync();
            }
            else
            {
                SelectedTargetProfiles.ProfileName = Name;
                SelectedTargetProfiles.Hops = Hops;
                SelectedTargetProfiles.Target1 = Target1;
                SelectedTargetProfiles.Target2 = Target2;
                SelectedTargetProfiles.Target3 = Target3;
                SelectedTargetProfiles.Target4 = Target4;
                SelectedTargetProfiles.Target5 = Target5;

                if (IsUserTargetsChecked)
                {
                    SelectedTargetProfiles.ReportType = ReportType.UserTargets;
                }
                else
                {
                    SelectedTargetProfiles.ReportType = ReportType.Traceroute;
                }

                await dbHandler.UpdateLatencyMonitorTargetProfile(SelectedTargetProfiles);

                IsTargetProfileListScreenVisible = true;
                IsNewTargetProfileScreenVisible = false;

                ClearChangeTargetProfilesForm();
                await LoadTargetProfilesAsync();
            }
        }

        [RelayCommand]
        public async Task CancelChangeTargetProfileAsync()
        {
            IsTargetProfileListScreenVisible = true;
            IsNewTargetProfileScreenVisible = false;

            ClearChangeTargetProfilesForm();
            await LoadTargetProfilesAsync();
        }

        public async Task LoadTargetProfilesAsync()
        {
            var dbHandler = new DatabaseHandler();

            TargetProfiles.Clear();

            foreach (var profile in await dbHandler.GetLatencyMonitorTargetProfilesAsync())
            {
                TargetProfiles.Add(profile);
            }
        }

        #region Private Methods
        private void ClearChangeTargetProfilesForm()
        {
            Name = string.Empty;
            Hops = 0;
            Target1 = string.Empty;
            Target2 = string.Empty;
            Target3 = string.Empty;
            Target4 = string.Empty;
            Target5 = string.Empty;
        }
        #endregion
    }
}
