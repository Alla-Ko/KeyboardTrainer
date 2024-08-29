using KeybordTrainer.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeybordTrainer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<User> Users { get; set; }
        private KeyboardTrainerContext _context;
        public MainWindow()
        {
            InitializeComponent();
            _context = new KeyboardTrainerContext();
            Users = new ObservableCollection<User>();
            DataContext = this;
            PlayersListView.ItemsSource = Users;
            LoadUsers();
            Users.CollectionChanged += Users_CollectionChanged;
        }
        private void LoadUsers()
        {
            Users.Clear();
            using var context = new KeyboardTrainerContext();

            try
            {
                var users = context.Users.ToList();

                foreach (var user in users)
                {
                    Users.Add(user);
                    user.PropertyChanged += User_PropertyChanged; // Підписуємось на PropertyChanged кожного користувача

                }
            }
            catch (Exception ex) { }

        }
        private void User_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(User.UserName))
            {
                var user = sender as User;
                if (user != null)
                {
                    var userToUpdate = _context.Users.Find(user.UserId);
                    if (userToUpdate != null)
                    {
                        _context.Entry(userToUpdate).CurrentValues.SetValues(user);
                        _context.SaveChanges();
                    }
                }
            }
        }
        private void Users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (User newUser in e.NewItems)
                {

                    _context.Users.Add(newUser);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (User removedUser in e.OldItems)
                {

                    var userToRemove = _context.Users.Find(removedUser.UserId);
                    if (userToRemove != null)
                    {
                        _context.Users.Remove(userToRemove);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (User oldUser in e.OldItems)
                {

                    var userToUpdate = _context.Users.Find(oldUser.UserId);
                    if (userToUpdate != null)
                    {
                        _context.Entry(userToUpdate).CurrentValues.SetValues(oldUser);
                    }
                }
            }

            _context.SaveChanges();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as User;

            if (user != null)
            {
                var customMessageBox = new CustomMessageBox("Are you sure you want to delete this user?", "Delete", "Cancel");
                if (customMessageBox.ShowDialog() == true && customMessageBox.IsConfirmed)
                {
                    Users.Remove(user);
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string newUserName = NewNameTextBox.Text;
            bool userExists = Users.Any(u => u.UserName.Equals(newUserName, StringComparison.OrdinalIgnoreCase));



            if (userExists)
            {
                var customMessageBox = new CustomMessageBox("Such user already exists");
                customMessageBox.ShowDialog();
            }
            else
            {
                User newUser = new User { UserName = newUserName };
                Users.Add(newUser);
            }
        }
        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Отримати ScrollViewer з шаблону ListView
            ScrollViewer scrollViewer = GetScrollViewer(PlayersListView);

            if (scrollViewer != null)
            {
                // Прокручувати вміст залежно від напрямку коліщатка миші
                if (e.Delta > 0)
                    scrollViewer.LineUp();
                else
                    scrollViewer.LineDown();

                // Заборонити подальше оброблення події іншими елементами
                e.Handled = true;
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer)
            {
                return depObj as ScrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                ScrollViewer result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = PlayersListView.SelectedItem as User;
            if (selectedUser != null)
            {

                GameWindow gameWindow = new GameWindow(selectedUser);


                gameWindow.ShowDialog();

                if (!gameWindow.changeUser)
                {
                    Close();
                }
                else
                {
                    PlayersListView.UnselectAll();

                }
            }
            else
            {
                CustomMessageBox messageBox = new CustomMessageBox("Choose a player please!");
                messageBox.ShowDialog();
            }
        }

        private void PlayersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PlayersListView.SelectedItem != null)
            {
                StartButton_Click(sender, e);
            }
        }
    }
}