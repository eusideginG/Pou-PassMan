using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.EntityFrameworkCore;
using Pou_Pass_Man.Helper;
using Pou_Pass_Man.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UIMessage;

namespace Pou_Pass_Man
{
    /// <summary>
    /// A user authentication class extenting Window class and implementing INotifyPropertyChanged interface
    /// </summary>
    public partial class UserAuthentication : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// attempts before locking the program for ten minutes from logingin
        /// </summary>
        private byte attempts = 4;
        private string? username;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? inputUsername = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(inputUsername));
        }

        /// <summary>
        /// Constructor, if the app just opened the window is in center of the screen and sets the initial start to false. Sets data context, initializaes components, sets ui
        /// </summary>
        public UserAuthentication()
        {
            if (((App)Application.Current).InitialStart)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                ((App)Application.Current).InitialStart = false;
            }
            DataContext = this;
            InitializeComponent();
            tbUsername.TextChanged += TextBox_TextChanged;
            SetUI();
        }

        /// <summary>
        /// gets the username from roperties settings, if there is a value the focus go to password field and the remember me chackbox is staing checked
        /// </summary>
        private void SetUI()
        {
            string name = Properties.Settings.Default.Username;
            if (!string.IsNullOrEmpty(name))
            {
                tbUsername.Text = name;
                tbPassword.Focus();
                cbRememberMe.IsChecked = true;
            }
            else
            {
                cbRememberMe.IsChecked = false;
            }
        }

        /// <summary>
        /// getter - setter for username, when setting trigges the propety change method
        /// </summary>
        public string? Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Text change event listener for the userrname, if the field is empty the login button is disabled else button is enabled
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!areInputEmpty()) bLogin.IsEnabled = true;
            else bLogin.IsEnabled = false;
        }
        /// <summary>
        /// Text change event listener for the password, if the field is empty the login button is disabled else button is enabled
        /// </summary>
        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!areInputEmpty()) bLogin.IsEnabled = true;
            else bLogin.IsEnabled = false;
        }

        /// <summary>
        /// Event listener for the login button, checks if there is a timeout, if true displays a verification popup with the remaining time else calls the userAuth() method
        /// </summary>
        private async void bLogin_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Timeout > DateTime.Now)
            {
                Verify.Show(EVerifyTypes.OK,
                    "Login Timeout",
                    String.Format("Time left until next try: {0} minutes.",
                    Properties.Settings.Default.Timeout.Subtract(DateTime.Now).ToString("mm").TrimStart('0')),
                    backgroundColor: (Brush?)this.FindResource("Background"),
                    textColor: (Brush?)this.FindResource("TextColorLight"),
                    confirmButtonColor: (Brush?)this.FindResource("MainColor"),
                    closeButtonColor: (Brush?)this.FindResource("MainColor"));
            }
            else
            {
                await UserAuth();
            }
        }

        /// <summary>
        /// Takes the user credenctials,
        /// checks if user is in database [if false = clear input decrease timeout (preventing brute force attacks) and retun false, else continues],
        /// the input password is hashed and compared with the one in the database [if it's not in the database clear input, decrease attemps (usesr entered wrong password)]
        /// if there is an exception display the correct mesage to the user
        /// </summary>
        /// <returns>false when can't authenticate the user, true when users provides the correct credentials</returns>
        private async Task<bool> UserAuth()
        {
            Username = (string)tbUsername.Text;
            string password = tbPassword.Password;
            RememberMe();

            try
            {
                using DatabaseContext databaseContext = new DatabaseContext();
                List<User> users = await databaseContext.Users.ToListAsync();

                User? user = users.FirstOrDefault(x => x.Username == Username);
                if (user == null)
                {
                    clearInput();
                    timeOut();
                    return false;
                }

                var argonConfig = new Argon2Config { Password = Encoding.Default.GetBytes(password), Threads = 1 };
                SecureArray<byte>? hash = null;

                if (argonConfig.DecodeString(user.Password, out hash) && hash != null)
                {
                    var argon2 = new Argon2(argonConfig);
                    using (var verify = argon2.Hash())
                    {
                        if (Argon2.FixedTimeEquals(hash, verify))
                        {
                            Properties.Settings.Default.UserId = user.UserId;
                            Properties.Settings.Default.Save();

                            Cache.CurrentUser = user.Password;

                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();
                            this.Close();
                            return true;
                        }
                        else
                        {
                            clearInput();
                            timeOut();
                            return false;
                        }
                    }
                }
                else
                {
                    clearInput();
                    timeOut();
                    return false;
                }
            }
            catch
            (Exception)
            {
                Notify.Show(ENotificationTypes.Error, "Cannot Login", "Something Went Wrong");
                return false;
            }
        }

        /// <summary>
        /// clears the input fields
        /// </summary>
        private void clearInput()
        {
            tbPassword.Password = "";
            tbPassword.Focus();    
        }

        /// <summary>
        /// decreases the attempes member, check if is 0, saves the current time plus ten minutess in properties settings and displays the proper messages
        /// </summary>
        private void timeOut()
        {
            attempts--;
            if (attempts == 0)
            {
                Properties.Settings.Default.Timeout = DateTime.Now.AddMinutes(10);
                Properties.Settings.Default.Save();

                Verify.Show(EVerifyTypes.OK,
                    "Loggin is locked",
                    "Try again in 10 minutes",
                    backgroundColor: (Brush?)this.FindResource("Background"),
                    textColor: (Brush?)this.FindResource("TextColorLight"),
                    confirmButtonColor: (Brush?)this.FindResource("MainColor"),
                    closeButtonColor: (Brush?)this.FindResource("MainColor"));
            }
            else
            {
                Notify.Show(ENotificationTypes.Warning, "Wrong Credencials", "Check your usename and password");
            }
        }

        /// <summary>
        /// Method That holds the remember me functionality, save or removes value from properties ssettings
        /// </summary>
        private void RememberMe()
        {
            if (cbRememberMe.IsChecked == true)
            {
                Properties.Settings.Default.Username = Username;
            }
            else
            {
                Properties.Settings.Default.Username = "";
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Event listener for clicking the register text-button, opens the register window in current location and closes this window
        /// </summary>
        private void Register(object sender, MouseButtonEventArgs e)
        {
            RegisterFormWindow registerFormWindow = new RegisterFormWindow();
            registerFormWindow.Top = this.Top;
            registerFormWindow.Left = this.Left;
            registerFormWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Event listener for remember me checkbox, if is checked save the usesrname in properties setting else delete from storage
        /// </summary>
        /// <param name="sender">The object that triggers the event (should be CheckBox in this case)</param>
        /// <param name="e">The event arguments</param>
        /// <exception cref="ArgumentNullException">If the sender converted as CheckBox is null</exception>
        private void cbRememberMe_Click(object sender, RoutedEventArgs e)
        {
            CheckBox? cb = sender as CheckBox;

            if (cb == null) throw new ArgumentNullException(nameof(cb));

            if (cb.IsChecked == true)
            {
                Properties.Settings.Default.Username = Username;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Username = "";
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Checks if the input fileds are empty
        /// </summary>
        /// <returns>True when any is emtpy, False when both have some value</returns>
        private bool areInputEmpty()
        {
            return !(!String.IsNullOrEmpty((string)tbUsername.Text) &&
                (tbPassword.DataContext != null && tbPassword.Password.Length > 0));
        }

        /// <summary>
        /// awaits for the databae creation when the window is loaded
        /// </summary>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using DatabaseContext databaseContext = new DatabaseContext();
            await databaseContext.Database.EnsureCreatedAsync();
            await databaseContext.Users.LoadAsync();
        }

        /// <summary>
        /// overriding the onclosing event and disposing the database asynchronously
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            using DatabaseContext databaseContext = new DatabaseContext();
            databaseContext.DisposeAsync();
            base.OnClosing(e);
        }
    }
}
