using System.ComponentModel;

namespace KeybordTrainer.Models;

public partial class User : INotifyPropertyChanged
{
    public int UserId { get; set; }
    private string _userName;


    public string UserName
    {
        get => _userName;
        set
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public virtual ICollection<UserLevel> UserLevels { get; set; } = new List<UserLevel>();
}
