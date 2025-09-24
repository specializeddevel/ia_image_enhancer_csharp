using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageProcessor.Core;
using System;
using System.Collections.ObjectModel;

namespace ImageProcessor.UI.ViewModels;

public partial class ProfilesViewModel : ObservableObject
{
    private readonly ProfileService _profileService;

    public event EventHandler<(string ProfileName, ProcessingOptions Options)?>? CloseRequested;

    [ObservableProperty]
    private string? _selectedProfileName;

    public ObservableCollection<string> ProfileNames { get; }

    public ProfilesViewModel(ProfileService profileService)
    {
        _profileService = profileService;
        ProfileNames = new ObservableCollection<string>();
        LoadProfileNames();
    }

    private void LoadProfileNames()
    {
        var names = _profileService.GetProfileNames();
        ProfileNames.Clear();
        foreach (var name in names)
        {
            ProfileNames.Add(name);
        }
    }

    [RelayCommand]
    private void LoadProfile()
    {
        if (string.IsNullOrEmpty(SelectedProfileName)) return;

        var loadedOptions = _profileService.LoadProfile(SelectedProfileName);
        if (loadedOptions == null) return;

        CloseRequested?.Invoke(this, (SelectedProfileName, loadedOptions));
    }

    [RelayCommand]
    private void DeleteProfile()
    {
        if (string.IsNullOrEmpty(SelectedProfileName)) return;

        _profileService.DeleteProfile(SelectedProfileName);
        LoadProfileNames();
        SelectedProfileName = null;
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, null);
    }
}
