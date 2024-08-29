using KeybordTrainer.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KeybordTrainer
{
    /// <summary>
    /// Логика взаимодействия для GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        CancellationToken token;
        private CancellationTokenSource cancellationTokenSourceMp3;
        CancellationToken tokenMp3;
        private bool isTimerStarted = false;

        private DateTime startTime;
        public User gamer;
        public bool changeUser = false;
        private string currentTask = "";
        private int maxNumberOfFails = 100;
        private int errorCount = 0;
        private Brush ColorPressed;
        private Brush ColorNormal;
        private Brush PinkieColor;//
        private Brush RingFingerColor;
        private Brush MiddleFingerColor;
        private Brush ForefingerColor;
        private Brush ThumbColor;
        private Brush SideColor;

        public GameWindow(User user)

        {
            InitializeComponent();
            gamer = user;
            Setup();

        }
        private void Setup()
        {
            ColorPressed = (Brush)Application.Current.Resources["Button.Pressed.Background"];
            ColorNormal = (Brush)Application.Current.Resources["PrimaryBrush"];
            PinkieColor = (Brush)Application.Current.Resources["PinkieBrush"];
            RingFingerColor = (Brush)Application.Current.Resources["RingFingerBrush"];
            MiddleFingerColor = (Brush)Application.Current.Resources["MiddleFingerBrush"];
            ForefingerColor = (Brush)Application.Current.Resources["ForefingerBrush"];
            ThumbColor = (Brush)Application.Current.Resources["ThumbBrush"];
            SideColor = (Brush)Application.Current.Resources["SideColorBrush"];



            userNameTextBlock.Text = gamer.UserName;
            PopulateLevelsComboBox();
            SetupRichTextBox();
            startButton.Visibility = Visibility.Visible;
            stopButton.Visibility = Visibility.Collapsed;
            startButton.Focusable = true;
        }
        public void PopulateLevelsComboBox()
        {
            int userId = gamer.UserId;
            using var context = new KeyboardTrainerContext();

            levelsComboBox.Items.Clear();
            var taskIds = context.UserLevels
                                 .Where(ul => ul.UserId == userId && ul.Speed > 0)
                                 .Select(ul => ul.TaskId.Value)
                                  .OrderBy(id => id)
                                 .ToList();

            if (taskIds.Count == 0)
            {

                levelsComboBox.Items.Add("1");
                if (levelsComboBox.Items.Count > 0)
                {
                    levelsComboBox.SelectedIndex = levelsComboBox.Items.Count - 1;
                }
                return;
            }
            else
            {
                foreach (var taskId in taskIds)
                {
                    levelsComboBox.Items.Add(taskId.ToString());
                }
            }
            int maxTaskIdInTaskTable = context.Tasks.Max(t => t.Id);
            int maxTaskIdInComboBox = taskIds.Any() ? taskIds.Max() : 0;
            if (maxTaskIdInComboBox < maxTaskIdInTaskTable)
            {
                levelsComboBox.Items.Add((maxTaskIdInComboBox + 1).ToString());
            }
            if (levelsComboBox.Items.Count > 0)
            {
                levelsComboBox.SelectedIndex = levelsComboBox.Items.Count - 1;
            }

        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            changeUser = true;
            Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSourceMp3?.Cancel();

            var customMessageBox = new CustomMessageBox("Are you sure you want to exit?", "Exit", "Cancel");
            if (customMessageBox.ShowDialog() == false && !customMessageBox.IsConfirmed)
            {

                e.Cancel = true;
            }
        }

        private void resultButton_Click(object sender, RoutedEventArgs e)
        {
            Results results = new Results(gamer);

            results.ShowDialog();

        }
        private async void StartSound(CancellationToken token)
        {

            MediaPlayer mediaPlayer = new MediaPlayer();
            //var soundUri = new Uri("pack://application:,,,/res/sounds/metronom-schet-80.mp3");
            //var soundUri = new Uri(@"res/sounds/metronom-schet-80.mp3", UriKind.Relative);
            string musicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res\\sounds\\metronom-schet-80.mp3");
            var soundUri = new Uri(musicPath);
            //var soundUri = new Uri(@"C:\Alla\PROEKTS\wpf\KeybordTrainer\res\sounds\metronom-schet-80.mp3", UriKind.Absolute);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Завантажте аудіофайл
                    mediaPlayer.Open(soundUri);

                    // Програвайте файл
                    mediaPlayer.Play();

                    // Дочекайтесь завершення відтворення
                    while (mediaPlayer.NaturalDuration.HasTimeSpan && mediaPlayer.Position < mediaPlayer.NaturalDuration.TimeSpan)
                    {
                        // Перевірте чи токен скасований
                        if (token.IsCancellationRequested)
                        {
                            // Зупиніть програвання і вийдіть з циклу
                            mediaPlayer.Stop();
                            return;
                        }

                        // Затримка для перевірки статусу
                        Thread.Sleep(1000);
                    }

                    // Зупиніть програвання після закінчення треку
                    mediaPlayer.Stop();
                }
            }
            catch (OperationCanceledException)
            {
                // Обробка скасування
                mediaPlayer.Stop();
            }
            finally
            {
                // Закриття плеєра після завершення
                mediaPlayer.Close();
            }
        }
        private void startButton_Click(object sender, RoutedEventArgs e)
        {

            speedTextBox.Text = "0";
            failsTextBox.Text = "0";

            gamerTextBox.Document.Blocks.Clear();
            errorCount = 0;
            startButton.Visibility = Visibility.Collapsed;
            stopButton.Visibility = Visibility.Visible;
            gamerTextBox.IsReadOnly = false;

            levelsComboBox.IsEnabled = false;
            using var context = new KeyboardTrainerContext();

            currentTask = context.Tasks.Where(tsk => tsk.Id == Convert.ToInt32(levelsComboBox.SelectedValue.ToString()))
                                 .Select(tsk => tsk.Task1)
                                 .FirstOrDefault().Trim();


            SetupRichTextBox("", "", currentTask);
            gamerTextBox.Focus();



        }
        private void SetupRichTextBox(string first = "", string second = "", string third = "")
        {
            var flowDocument = new FlowDocument();
            var paragraph = new Paragraph();

            // Додаємо перший блок з фоном
            var firstBlock = new TextBlock
            {
                Text = first,
                Background = (Brush)this.FindResource("BackgroundBrush"),
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            paragraph.Inlines.Add(new InlineUIContainer(firstBlock));

            // Додаємо другий блок з червоним текстом
            var secondBlock = new TextBlock
            {
                Text = second,
                Foreground = Brushes.Red,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            paragraph.Inlines.Add(new InlineUIContainer(secondBlock));

            // Додаємо третій блок з звичайним текстом
            var thirdBlock = new TextBlock
            {
                Text = third,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            paragraph.Inlines.Add(new InlineUIContainer(thirdBlock));

            flowDocument.Blocks.Add(paragraph);
            taskTextBox.Document = flowDocument;

        }

        private async void gamerTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string userText = new TextRange(gamerTextBox.Document.ContentStart, gamerTextBox.Document.ContentEnd).Text.Trim();
            if (userText.Length == 0) return;

            if (!isTimerStarted)
            {
                startTime = DateTime.Now;
                isTimerStarted = true;
                cancellationTokenSource = new CancellationTokenSource();
                CancellationToken newCancellationToken = cancellationTokenSource.Token;
                StartTypingSpeedTracking(newCancellationToken);
            }




            if (currentTask.StartsWith(userText))
            {
                SetupRichTextBox(userText, "", currentTask.Substring(userText.Length));
                if (userText.Length == currentTask.Length)
                {
                    stopButton_Click(sender, e);
                    var userId = gamer.UserId;
                    var fails = errorCount;
                    var speed = Convert.ToInt32(speedTextBox.Text);
                    var taskId = Convert.ToInt32(levelsComboBox.SelectedValue);
                    UserLevel userLevel = new UserLevel { UserId = userId, Fails = fails, Speed = speed, TaskId = taskId };
                    SaveProgres form = new SaveProgres(userLevel);

                    form.ShowDialog();

                    if (form.saveResult)
                    {
                        using var context = new KeyboardTrainerContext();
                        var existingRecord = context.UserLevels
                                .FirstOrDefault(ul => ul.UserId == userId && ul.TaskId == taskId);

                        if (existingRecord != null)
                        {
                            // Оновлюємо існуючий запис
                            existingRecord.Fails = fails;
                            existingRecord.Speed = speed;
                        }
                        else
                        {
                            // Додаємо новий запис
                            var newUserLevel = new UserLevel
                            {
                                UserId = userId,
                                TaskId = taskId,
                                Fails = fails,
                                Speed = speed
                            };
                            context.UserLevels.Add(newUserLevel);
                        }
                        context.SaveChanges();
                        PopulateLevelsComboBox();
                    }
                }
            }
            else
            {
                // Обробка помилок
                int correctLength = currentTask.TakeWhile((c, i) => i < userText.Length && c == userText[i]).Count();
                string correctText = currentTask.Substring(0, correctLength);
                string incorrectText = userText.Substring(correctLength);
                string remainingText = currentTask.Substring(correctLength);

                // Оновлюємо текстові блоки
                SetupRichTextBox(correctText, incorrectText, remainingText);

                // Оновлюємо кількість помилок
                errorCount++;
                failsTextBox.Text = errorCount.ToString();
                if (errorCount >= maxNumberOfFails)
                {
                    stopButton_Click(sender, e);
                    UserLevel userLevel = new UserLevel { UserId = gamer.UserId, Fails = errorCount, Speed = Convert.ToInt32(speedTextBox.Text), TaskId = Convert.ToInt32(levelsComboBox.SelectedValue) };
                    SaveProgres form = new SaveProgres(userLevel);
                    form.ShowDialog();
                    if (form.saveResult)
                    {

                    }

                }
            }

        }
        private async void StartTypingSpeedTracking(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await System.Threading.Tasks.Task.Delay(1000, token); // Delay for 1 second

                    var elapsed = (DateTime.Now - startTime).TotalMinutes;
                    var textLength = gamerTextBox.Document.ContentEnd.GetPositionAtOffset(0, LogicalDirection.Forward).GetOffsetToPosition(gamerTextBox.Document.ContentStart);
                    var speed = -textLength / elapsed;
                    UpdateSpeedText(speed);
                }
            }
            catch (TaskCanceledException)
            {
                // Handle task cancellation
            }
        }
        private void UpdateSpeedText(double speed)
        {
            // Перевіряємо, чи потрібно оновлювати UI з іншого потоку
            if (Dispatcher.CheckAccess())
            {
                // Оновлюємо UI, якщо вже на основному потоці
                speedTextBox.Text = Math.Round(speed).ToString();
            }
            else
            {
                // Виконуємо оновлення через Dispatcher, якщо не на основному потоці
                Dispatcher.Invoke(() =>
                {
                    speedTextBox.Text = Math.Round(speed).ToString();
                });
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {

            cancellationTokenSource?.Cancel();
            isTimerStarted = false;



            stopButton.Visibility = Visibility.Collapsed;
            startButton.Visibility = Visibility.Visible;

            //gamerTextBox.Document.Blocks.Clear();
            SetupRichTextBox();
            gamerTextBox.IsReadOnly = true;

            levelsComboBox.IsEnabled = true;
            gamerTextBox.Document.Blocks.Clear();
        }
        private Button GetButtonForKey(Key key)
        {
            switch (key)
            {
                case Key.Oem3: // Клавіша `
                    return backtickButton;

                case Key.D1:
                    return Button1;

                case Key.D2:
                    return Button2;

                case Key.D3:
                    return Button3;

                case Key.D4:
                    return Button4;

                case Key.D5:
                    return Button5;

                case Key.D6:
                    return Button6;

                case Key.D7:
                    return Button7;

                case Key.D8:
                    return Button8;

                case Key.D9:
                    return Button9;

                case Key.D0:
                    return Button0;

                case Key.OemMinus: // Клавіша -
                    return MinusButton;

                case Key.OemPlus: // Клавіша =
                    return EqualsButton;

                case Key.Back:
                    return BackspaceButton;

                case Key.Tab:
                    return TabButton;

                case Key.Q:
                    return qButton;

                case Key.W:
                    return wButton;

                case Key.E:
                    return eButton;

                case Key.R:
                    return rButton;

                case Key.T:
                    return tButton;

                case Key.Y:
                    return yButton;

                case Key.U:
                    return uButton;

                case Key.I:
                    return iButton;

                case Key.O:
                    return oButton;

                case Key.P:
                    return pButton;

                case Key.Oem4: // Клавіша [
                    return LeftBracketButton;

                case Key.Oem6: // Клавіша ]
                    return RightBracketButton;

                case Key.Oem5: // Клавіша \
                    return BackslashButton;

                case Key.A:
                    return aButton;

                case Key.S:
                    return sButton;

                case Key.D:
                    return dButton;

                case Key.F:
                    return fButton;

                case Key.G:
                    return gButton;

                case Key.H:
                    return hButton;

                case Key.J:
                    return jButton;

                case Key.K:
                    return kButton;

                case Key.L:
                    return lButton;

                case Key.Z:
                    return zButton;

                case Key.X:
                    return xButton;

                case Key.C:
                    return cButton;

                case Key.V:
                    return vButton;

                case Key.B:
                    return bButton;

                case Key.N:
                    return nButton;

                case Key.M:
                    return mButton;

                case Key.OemComma: // Клавіша ,
                    return CommaButton;

                case Key.OemPeriod: // Клавіша .
                    return PeriodButton;

                case Key.Oem2: // Клавіша /
                    return SlashButton;

                case Key.Oem1: // Клавіша 
                    return SemicolonButton;

                case Key.OemQuotes: // Клавіша 
                    return ApostropheButton;

                case Key.Space: // Клавіша пробіл
                    return SpaceButton;
                case Key.LeftCtrl:
                    return LeftCtrlButton;
                case Key.RightCtrl:
                    return RightCtrlButton;
                case Key.LeftAlt:
                    return LeftAltButton;
                case Key.RightAlt:
                    return RightAltButton;
                case Key.Enter:
                    return EnterButton;

                case Key.LeftShift:
                    return ShiftButton;
                case Key.RightShift:
                    return RightShiftButton;
                case Key.Capital:
                    return CapsLockButton;



                default: return null;

            }
        }


        private void SimulateBacktickAction(Button btn)
        {
            btn.Background = ColorPressed;
        }
        private void SimulateBacktickActionUp(Button btn)
        {

            switch (btn)
            {
                case var _ when btn == backtickButton:
                case var _ when btn == Button1:
                case var _ when btn == Button2:
                case var _ when btn == Button9:
                case var _ when btn == qButton:
                case var _ when btn == aButton:
                case var _ when btn == zButton:
                case var _ when btn == iButton:
                case var _ when btn == kButton:
                case var _ when btn == CommaButton:

                    btn.Background = PinkieColor;
                    break;
                case var _ when btn == Button3:
                case var _ when btn == Button0:
                case var _ when btn == wButton:
                case var _ when btn == sButton:
                case var _ when btn == xButton:
                case var _ when btn == oButton:
                case var _ when btn == lButton:
                case var _ when btn == PeriodButton:

                    btn.Background = RingFingerColor;
                    break;

                case var _ when btn == Button4:
                case var _ when btn == pButton:
                case var _ when btn == eButton:
                case var _ when btn == dButton:
                case var _ when btn == cButton:
                case var _ when btn == EqualsButton:
                case var _ when btn == LeftBracketButton:
                case var _ when btn == MinusButton:
                case var _ when btn == RightBracketButton:
                case var _ when btn == BackslashButton:
                case var _ when btn == SemicolonButton:
                case var _ when btn == ApostropheButton:
                case var _ when btn == SlashButton:



                    btn.Background = MiddleFingerColor;
                    break;

                case var _ when btn == Button5:
                case var _ when btn == Button6:
                case var _ when btn == rButton:
                case var _ when btn == tButton:
                case var _ when btn == fButton:
                case var _ when btn == gButton:
                case var _ when btn == vButton:
                case var _ when btn == bButton:

                    btn.Background = ForefingerColor;
                    break;

                case var _ when btn == Button7:
                case var _ when btn == yButton:
                case var _ when btn == uButton:
                case var _ when btn == hButton:
                case var _ when btn == jButton:
                case var _ when btn == nButton:
                case var _ when btn == mButton:

                    btn.Background = ThumbColor;
                    break;

                case var _ when btn == SpaceButton:

                    btn.Background = ColorNormal;
                    break;

                default:


                    btn.Background = SideColor;
                    break;


            }



        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //MessageBox.Show(e.Key.ToString());
            // Перевірка на наявність натискання клавіші Shift для обробки великих літер
            bool isShiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            bool isCapsLockActive = Keyboard.IsKeyToggled(Key.CapsLock);

            if ((isShiftPressed && !isCapsLockActive) || (!isShiftPressed && isCapsLockActive))
            {
                qButton.Content = "Q";
                wButton.Content = "W";
                eButton.Content = "E";
                rButton.Content = "R";
                tButton.Content = "T";
                yButton.Content = "Y";
                uButton.Content = "U";
                iButton.Content = "I";
                oButton.Content = "O";
                pButton.Content = "P";
                aButton.Content = "A";
                sButton.Content = "S";
                dButton.Content = "D";
                fButton.Content = "F";
                gButton.Content = "G";
                hButton.Content = "H";
                jButton.Content = "J";
                kButton.Content = "K";
                lButton.Content = "L";
                zButton.Content = "Z";
                xButton.Content = "X";
                cButton.Content = "C";
                vButton.Content = "V";
                bButton.Content = "B";
                nButton.Content = "N";
                mButton.Content = "M";

            }
            else
            {
                qButton.Content = "q";
                wButton.Content = "w";
                eButton.Content = "e";
                rButton.Content = "r";
                tButton.Content = "t";
                yButton.Content = "y";
                uButton.Content = "u";
                iButton.Content = "i";
                oButton.Content = "o";
                pButton.Content = "p";
                aButton.Content = "a";
                sButton.Content = "s";
                dButton.Content = "d";
                fButton.Content = "f";
                gButton.Content = "g";
                hButton.Content = "h";
                jButton.Content = "j";
                kButton.Content = "k";
                lButton.Content = "l";
                zButton.Content = "z";
                xButton.Content = "x";
                cButton.Content = "c";
                vButton.Content = "v";
                bButton.Content = "b";
                nButton.Content = "n";
                mButton.Content = "m";


            }
            Button btn = GetButtonForKey(e.Key);
            if (btn != null)
            {
                SimulateBacktickAction(btn);
            }
        }



        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isShiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            bool isCapsLockActive = Keyboard.IsKeyToggled(Key.CapsLock);

            if ((isShiftPressed && !isCapsLockActive) || (!isShiftPressed && isCapsLockActive))
            {
                qButton.Content = "Q";
                wButton.Content = "W";
                eButton.Content = "E";
                rButton.Content = "R";
                tButton.Content = "T";
                yButton.Content = "Y";
                uButton.Content = "U";
                iButton.Content = "I";
                oButton.Content = "O";
                pButton.Content = "P";
                aButton.Content = "A";
                sButton.Content = "S";
                dButton.Content = "D";
                fButton.Content = "F";
                gButton.Content = "G";
                hButton.Content = "H";
                jButton.Content = "J";
                kButton.Content = "K";
                lButton.Content = "L";
                zButton.Content = "Z";
                xButton.Content = "X";
                cButton.Content = "C";
                vButton.Content = "V";
                bButton.Content = "B";
                nButton.Content = "N";
                mButton.Content = "M";

            }
            else
            {
                qButton.Content = "q";
                wButton.Content = "w";
                eButton.Content = "e";
                rButton.Content = "r";
                tButton.Content = "t";
                yButton.Content = "y";
                uButton.Content = "u";
                iButton.Content = "i";
                oButton.Content = "o";
                pButton.Content = "p";
                aButton.Content = "a";
                sButton.Content = "s";
                dButton.Content = "d";
                fButton.Content = "f";
                gButton.Content = "g";
                hButton.Content = "h";
                jButton.Content = "j";
                kButton.Content = "k";
                lButton.Content = "l";
                zButton.Content = "z";
                xButton.Content = "x";
                cButton.Content = "c";
                vButton.Content = "v";
                bButton.Content = "b";
                nButton.Content = "n";
                mButton.Content = "m";


            }

            Button btn = GetButtonForKey(e.Key);
            if (btn != null)
            {
                SimulateBacktickActionUp(btn);
            }
        }

        private void LeftWinButton_LostFocus(object sender, RoutedEventArgs e)
        {
            SimulateBacktickActionUp(LeftWinButton);
        }

        private void RighttWinButton_LostFocus(object sender, RoutedEventArgs e)
        {
            SimulateBacktickActionUp(RighttWinButton);
        }

        private async void soundCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (soundCheckBox.IsChecked == true)
            {
                cancellationTokenSourceMp3 = new CancellationTokenSource();
                tokenMp3 = cancellationTokenSourceMp3.Token;
                System.Threading.Tasks.Task.Run(() => StartSound(tokenMp3));
            }

        }

        private void soundCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            cancellationTokenSourceMp3?.Cancel();
        }
    }
}
