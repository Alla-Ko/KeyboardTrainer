using KeybordTrainer.Models;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KeybordTrainer
{
    /// <summary>
    /// Логика взаимодействия для SaveProgres.xaml
    /// </summary>
    public partial class SaveProgres : Window
    {
        public UserLevel userLevel;
        public bool saveResult = false;
        private List<Uri> happyCats;
        private List<Uri> sadCats;
        private List<string> happyWords;
        private List<string> sadWords;

        private int maxNumberOfFails = 3;

        public SaveProgres(UserLevel userLevel)
        {
            InitializeComponent();
            this.userLevel = userLevel;
            LevelTextBlock.Text = userLevel.TaskId.ToString();
            SpeedTextBlock.Text = userLevel.Speed.ToString();
            FailsTextBlock.Text = userLevel.Fails.ToString();
            happyCats = new List<Uri>();
            sadCats = new List<Uri>();
            happyWords = new List<string>();
            sadWords = new List<string>();
            downloadCats();
            randomCat();

        }

        private void randomCat()
        {
            using var context = new KeyboardTrainerContext();
            int maxTaskIdInTaskTable = context.Tasks.Max(t => t.Id);

            if (userLevel.Fails < maxNumberOfFails)
            {
                if (userLevel.TaskId == maxTaskIdInTaskTable)
                {
                    ResultCatImage.Source = new BitmapImage(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/!.jpeg", UriKind.RelativeOrAbsolute));
                    motivationTextBlock.Text = "Congratulations on completing the final level! You've done an amazing job, mastering all the challenges along the way. Now, you're ready to type with all ten fingers like a true pro. Keep up the great work and enjoy your newfound speed and precision! Well done!";
                    return;
                }
                Random random = new Random();
                int index = random.Next(happyCats.Count);
                Uri selectedImageUri = happyCats[index];
                ResultCatImage.Source = new BitmapImage(selectedImageUri);
                index = random.Next(happyWords.Count);
                motivationTextBlock.Text = happyWords[index];
            }
            else
            {
                Random random = new Random();
                int index = random.Next(sadCats.Count);
                Uri selectedImageUri = sadCats[index];
                ResultCatImage.Source = new BitmapImage(selectedImageUri);
                index = random.Next(sadWords.Count);
                motivationTextBlock.Text = sadWords[index];
                YesButton.Visibility = Visibility.Collapsed;
                NoButton.Content = "I'll try again";

            }



        }

        private void downloadCats()
        {
            happyCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/+.jpeg", UriKind.RelativeOrAbsolute));
            happyCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/++.jpeg", UriKind.RelativeOrAbsolute));
            happyCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/+++.jpeg", UriKind.RelativeOrAbsolute));
            happyCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/++++.jpeg", UriKind.RelativeOrAbsolute));
            happyCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/+++++.jpeg", UriKind.RelativeOrAbsolute));

            sadCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/-.jpeg", UriKind.RelativeOrAbsolute));
            sadCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/--.jpeg", UriKind.RelativeOrAbsolute));
            sadCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/---.jpeg", UriKind.RelativeOrAbsolute));
            sadCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/----.jpeg", UriKind.RelativeOrAbsolute));
            sadCats.Add(new Uri("pack://application:,,,/KeybordTrainer;component/res/images/-----.jpeg", UriKind.RelativeOrAbsolute));


            happyWords.Add("Congratulations on conquering this level! You're making fantastic progress. Keep up the great work!");
            happyWords.Add("Well done on completing another level! Your skills are improving with every step. Keep it going!");
            happyWords.Add("Bravo! You've successfully passed this level. You're getting closer to mastering the challenge. Keep pushing forward!");
            happyWords.Add("Great job on finishing this level! Your hard work is paying off. On to the next challenge!");
            happyWords.Add("Awesome work! You've nailed this level. You're one step closer to becoming a typing master!");

            sadWords.Add("Don't worry, you’re making progress! Every mistake is a step towards improvement. Try again, you've got this!");
            sadWords.Add("It’s okay to stumble—what matters is that you keep trying. You’re getting better with every attempt. Give it another shot!");
            sadWords.Add("You’re doing great! Sometimes it takes a few tries, but persistence will pay off. Keep going, you’re closer than you think!");
            sadWords.Add("Remember, practice makes perfect. You’re on the right track—just keep pushing forward. Another try will bring you success!");
            sadWords.Add("Mistakes are just part of the learning process. Don’t get discouraged—you have the skills to overcome this challenge. Let’s give it another go!");
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            saveResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
