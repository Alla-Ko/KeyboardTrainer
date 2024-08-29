using KeybordTrainer.Models;
using System.Collections.ObjectModel;
using System.Windows;


namespace KeybordTrainer
{
    /// <summary>
    /// Логика взаимодействия для Results.xaml
    /// </summary>
    public partial class Results : Window
    {
        public ObservableCollection<UserLevel> Levels { get; set; }
        private KeyboardTrainerContext _context;
        public User gamer;
        public Results(User user)
        {
            InitializeComponent();
            _context = new KeyboardTrainerContext();
            gamer = user;
            Levels = new ObservableCollection<UserLevel>();
            DataContext = this;
            LevelsListView.ItemsSource = Levels;
            userNameTextBlock.Text = user.UserName;
            LoadLevels();

        }
        private void LoadLevels()
        {
            Levels.Clear();
            var levels = _context.UserLevels
                                   .Where(ul => ul.UserId == gamer.UserId && ul.Speed > 0)
                                    .OrderBy(ul => ul.TaskId)
                                   .ToList();

            foreach (var level in levels)
            {
                Levels.Add(level);
            }

        }
    }
}
