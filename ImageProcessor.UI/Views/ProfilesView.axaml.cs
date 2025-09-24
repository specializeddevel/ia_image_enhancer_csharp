using Avalonia.Controls;
using ImageProcessor.Core;
using ImageProcessor.UI.ViewModels;
using System;

namespace ImageProcessor.UI.Views;

public partial class ProfilesView : Window
{
    // Parameterless constructor for XAML designer preview
    public ProfilesView()
    { 
        InitializeComponent();
    }

    // Constructor used by Dependency Injection at runtime
    public ProfilesView(ProfilesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += (object? sender, (string ProfileName, ProcessingOptions Options)? result) => Close(result);
    }
}
