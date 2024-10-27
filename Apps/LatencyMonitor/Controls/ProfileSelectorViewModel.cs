using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Controls
{
    internal partial class ProfileSelectorViewModel : ObservableValidator
    {
        #region Control Properties
        public ObservableCollection<LatencyMonitorTargetProfiles> TargetProfiles { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditTargetProfileCommand))]
        [NotifyCanExecuteChangedFor(nameof(RemoveTargetProfileCommand))]
        public LatencyMonitorTargetProfiles selectedTargetProfile = null;

        [ObservableProperty]
        public bool isTargetProfileListScreenVisible = true;

        [ObservableProperty]
        public bool isNewTargetProfileScreenVisible = false;

        [ObservableProperty]
        public bool isUserTargetsChecked = true;

        [ObservableProperty]
        public bool isTracerouteChecked = false;

        [ObservableProperty]
        [Required]
        public string name = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "The field cannot be empty.\nPlease enter a valid IP Address or DNS Name.")]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string target1 = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string target2 = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string target3 = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string target4 = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string target5 = string.Empty;

        [ObservableProperty]
        [ConditionalRequired(
            nameof(IsTracerouteChecked),
            ErrorMessage = "The field cannot be empty.\nPlease enter a number from 1 to 255.")]
        [RegularExpression(
            @"^(?:[1-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$",
            ErrorMessage = "The number entered is not valid.\nPlease enter a number from 1 to 255.")]
        public int hops = 30;
        #endregion

        public ProfileSelectorViewModel()
        {
            TargetProfiles = new();
        }

        [RelayCommand]
        public async Task NewTargetProfile()
        {
            IsTargetProfileListScreenVisible = false;
            IsNewTargetProfileScreenVisible = true;
            SelectedTargetProfile = null;
            await GetTargetProfilesAsync();
        }

        [RelayCommand(CanExecute = nameof(GetStatusForEditAndRemoveButtons))]
        public void EditTargetProfile()
        {
            if (SelectedTargetProfile != null)
            {
                IsTargetProfileListScreenVisible = false;
                IsNewTargetProfileScreenVisible = true;

                Name = SelectedTargetProfile.ProfileName;
                Hops = SelectedTargetProfile.Hops;
                Target1 = SelectedTargetProfile.Target1;
                Target2 = SelectedTargetProfile.Target2;
                Target3 = SelectedTargetProfile.Target3;
                Target4 = SelectedTargetProfile.Target4;
                Target5 = SelectedTargetProfile.Target5;

                if (SelectedTargetProfile.ReportType == ReportType.UserTargets)
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
        }

        [RelayCommand(CanExecute = nameof(GetStatusForEditAndRemoveButtons))]
        public async Task RemoveTargetProfileAsync()
        {
            if (SelectedTargetProfile != null)
            {
                var dbHandler = new DatabaseHandler();
                await dbHandler.DeleteSelectedProfilesAsync(SelectedTargetProfile);
                await GetTargetProfilesAsync();
            }
        }

        [RelayCommand]
        public async Task SaveTargetProfileAsync()
        {
            var dbHandler = new DatabaseHandler();

            if (await ValidateUserInputAsync() == false)
            {
                return;
            }

            if (SelectedTargetProfile == null)
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
                await GetTargetProfilesAsync();
            }
            else
            {
                SelectedTargetProfile.ProfileName = Name;
                SelectedTargetProfile.Hops = Hops;
                SelectedTargetProfile.Target1 = Target1;
                SelectedTargetProfile.Target2 = Target2;
                SelectedTargetProfile.Target3 = Target3;
                SelectedTargetProfile.Target4 = Target4;
                SelectedTargetProfile.Target5 = Target5;

                if (IsUserTargetsChecked)
                {
                    SelectedTargetProfile.ReportType = ReportType.UserTargets;
                }
                else
                {
                    SelectedTargetProfile.ReportType = ReportType.Traceroute;
                }

                await dbHandler.UpdateLatencyMonitorTargetProfile(SelectedTargetProfile);

                IsTargetProfileListScreenVisible = true;
                IsNewTargetProfileScreenVisible = false;

                ClearChangeTargetProfilesForm();
                await GetTargetProfilesAsync();
            }
        }

        [RelayCommand]
        public async Task CancelChangeTargetProfileAsync()
        {
            IsTargetProfileListScreenVisible = true;
            IsNewTargetProfileScreenVisible = false;

            ClearChangeTargetProfilesForm();
            await GetTargetProfilesAsync();
        }

        public async Task GetTargetProfilesAsync()
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
            SelectedTargetProfile = null;
        }

        private async Task<bool> ValidateUserInputAsync()
        {
            bool status = true;

            // Validate all of the user input fields against regex expressions
            ValidateAllProperties();

            // If the user input fields have errors based on their attributes, return false
            if (HasErrors)
            {
                status = false;
            }

            return await Task.FromResult(status);
        }

        private bool GetStatusForEditAndRemoveButtons()
        {
            if (SelectedTargetProfile == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
