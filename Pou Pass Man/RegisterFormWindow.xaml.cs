using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Pou_Pass_Man.Helper;
using Pou_Pass_Man.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UIMessage;

namespace Pou_Pass_Man
{
    /// <summary>
    /// A class that regissters the a user in the application, implementing the Window class and extending the INotifyPropertyChancge
    /// </summary>
    public partial class RegisterFormWindow : Window, INotifyPropertyChanged
    {
        private string username = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// A constructor that binds data with the xaml and initialize the components
        /// </summary>
        public RegisterFormWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// a property cahgne listener method for the username field
        /// </summary>
        /// <param name="inputUsername">the username input field value</param>
        private void OnPropertyChanged([CallerMemberName] string? inputUsername = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(inputUsername));
        }

        /// <summary>
        /// Holds the value of the username input field, the setter callss the OnPropertyChange method
        /// </summary>
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A text change listener for the username field, checks if the filed is empty or null and dissables or enables the register button
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInputEmpty())
            {
                bRegister.IsEnabled = true;
            }
            else
            {
                bRegister.IsEnabled = false;

            }
        }

        /// <summary>
        /// A password change listener for the password field, checks if the filed is empty or null and dissables or enables the register button
        /// </summary>
        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInputEmpty())
            {
                bRegister.IsEnabled = true;
            }
            else
            {
                bRegister.IsEnabled = false;
            }
        }

        /// <summary>
        /// A password change listener for the re-enter password field, checks if the filed is empty or null and dissables or enables the register button
        /// </summary>
        private void tbRetypePassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInputEmpty())
            {
                bRegister.IsEnabled = true;
            }
            else
            {
                bRegister.IsEnabled = false;
            }
        }

        /// <summary>
        /// checks if any of the three fileds are empty or null
        /// </summary>
        /// <returns>True if the any is empty or null, False if all are NOT emty or null</returns>
        private bool IsInputEmpty()
        {
            return !(!String.IsNullOrEmpty(tbUsername.Text) &&
                (tbPassword.DataContext != null && tbPassword.Password.Length > 0) &&
                (tbRetypePassword.DataContext != null && tbPassword.Password == tbRetypePassword.Password));
        }

        /// <summary>
        /// Event listener of the login text-button opens the login window in the current location and closes the register window
        /// </summary>
        private void Login_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            openLoginWindow();
        }

        /// <summary>
        /// A async listener for register button, hushes the password the Argon2 and saves the username and the password in the database, displays the properly information to the user on success or on failure
        /// </summary>
        private async void bRegister_Click(object sender, RoutedEventArgs e)
        {
            User user = new User();
            user.Username = Username;

            Argon2Config config = Argon2Helper.config;
            config.Password = Encoding.Default.GetBytes((string)tbPassword.Password);

            Argon2 argon2 = new Argon2(config);


            using (SecureArray<byte> hash = argon2.Hash())
            {
                user.Password = config.EncodeString(hash.Buffer);
            }

            try
            {
                using DatabaseContext databaseContext = new DatabaseContext();
                await databaseContext.AddAsync(user);
                await databaseContext.SaveChangesAsync();
                clearInput();
                Notify.Show(ENotificationTypes.Success, message: "Acount created successfully");

                openLoginWindow();
            }
            catch
            {
                clearInput();
                Notify.Show(ENotificationTypes.Warning, message: "Username already exist");
            }
        }

        /// <summary>
        /// Opens the login window in the current location and closes the register window
        /// </summary>
        private void openLoginWindow()
        {
            UserAuthentication userAuthentication = new UserAuthentication();
            userAuthentication.Top = Top;
            userAuthentication.Left = Left;
            userAuthentication.Show();
            this.Close();
        }

        /// <summary>
        /// clears the text in the input field
        /// </summary>
        private void clearInput()
        {
            tbUsername.Text = "";
            tbPassword.Password = "";
            tbRetypePassword.Password = "";
        }
    }
}
