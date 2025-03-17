using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Guides.Services;

public sealed class ErrorService : INotifyPropertyChanged
{
    private string? _errorMessage;
    
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string>? OnError;

    public void ShowError(string message)
    {
        ErrorMessage = message;
        OnError?.Invoke(message);
    }

    public void ErrorShown()
    {
        ErrorMessage = null;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}