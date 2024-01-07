using Microsoft.EntityFrameworkCore;
using Pou_Pass_Man.Helper;
using Pou_Pass_Man.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using UIMessage;

namespace Pou_Pass_Man
{
    /// <summary>
    /// The main window of the application
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// an observable list of the user authentications
        /// </summary>
        private ObservableCollection<Authentication> authentication = new ObservableCollection<Authentication>();
        /// <summary>
        /// A getter of the obsrvble list
        /// is used to bind the list with the ListView in xaml
        /// </summary>
        public ObservableCollection<Authentication> Authentication
        {
            get { return authentication; }
        }

        /// <summary>
        /// stores a single authntication for editing purposes
        /// </summary>
        private Authentication? editAuthentication;
        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();
            PopulateData();
        }
        
        /// <summary>
        /// Populating the authentication list using the user id that just loggedin ordered by descending creation date
        /// </summary>
        private async void PopulateData()
        {
            int currentUserId = Properties.Settings.Default.UserId;
            using DatabaseContext databaseContext = new DatabaseContext();
            List<Authentication> list = new List<Authentication>(await databaseContext.Authentications
                .Where(i => i.UserId == currentUserId)
                .OrderByDescending(i => i.Date)
                .ToListAsync());
            list.ForEach(authentication.Add);
        }

        #region Listview item buttons click listeners
        /// <summary>
        /// Sets the clipboard with the website from the list item that the user clicked (Copy Website button)
        /// </summary>
        private void Button_Click_Copy_Website(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Authentication auth = (Authentication)button.DataContext;

            Clipboard.SetText(auth.Website);
            Notify.Show(ENotificationTypes.Notifications, message: "Website copied to clipboard");
        }

        /// <summary>
        /// Sets the clipboard with the username from the list item that the user clicked (Copy Username button)
        /// </summary>
        private void Button_Click_Copy_Username(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Authentication auth = (Authentication)button.DataContext;

            Clipboard.SetText(auth.UsernameOrEmail);
            Notify.Show(ENotificationTypes.Notifications, message: "Username copied to clipboard");
        }

        /// <summary>
        /// Decrypts the password and sets the clipboard with the password from the list item that the user clicked for 5 seconds (Copy Password button)
        /// </summary>
        private void Button_Click_Copy_Password(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Authentication auth = (Authentication)button.DataContext;

            try
            {
                string password = AesAlg.DecryptString(Cache.CurrentUser.Substring(Cache.CurrentUser.Length - 32), auth.Password);
                Notify.Show(ENotificationTypes.Notifications, message: "Password copied to clipboard for 5 sec.");
                Clipboard.SetText(password);

                DispatcherTimer timer = new DispatcherTimer();
                timer.Tick += (sender, e) =>
                {
                    Clipboard.Clear();
                    timer.Stop();
                };
                timer.Interval = TimeSpan.FromMilliseconds(5000);
                timer.Start();

            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        /// <summary>
        /// Copies the authentication item from the list in the editCurrentUser,
        /// populates the fields used to add and edit an authentication item,
        /// displays the delete btn and changes the Add btn to Save
        /// </summary>
        private void Button_Click_Edit(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Authentication auth = (Authentication)button.DataContext;

            tbxName.Text = auth.Name;
            tbxUsernameOrEmail.Text = auth.UsernameOrEmail;

            string password = AesAlg.DecryptString(Cache.CurrentUser.Substring(Cache.CurrentUser.Length - 32), auth.Password);
            tbxPassword.Password = password;
            tbxWebsite.Text = auth.Website;

            editAuthentication = auth;

            bAddOrEdit.Content = "Save";
            bDelete.Visibility = Visibility.Visible;
        }
        #endregion

        #region CUD Create Update Delete (and cansel) functionality
        /// <summary>
        /// if btn content is Add:
        ///     checks if the required fields are empty,
        ///         if it's true: display a notification to the usesr
        ///         else creates a new authentication object and saves it in the db asynchronously
        /// if the content is Save:
        ///     set the editAuthentication members from the input fields,
        ///     finds the authentication from the list with the id
        ///     gets the index of the authentication item
        ///     updates the item from the database
        ///     deletes the authentication item from the obsvble list and inserts the new authentication at the 0 index position of the list
        ///     displays a notification on success or failure accordingly
        /// </summary>
        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            if ((string)bAddOrEdit.Content == "Add")
            {
                if (areFiledsEmpty())
                {
                    clearInputFields();
                    Notify.Show(ENotificationTypes.Warning, message: "Name and Password required");
                }
                else
                {
                    Authentication auth = new Authentication()
                    {
                        UserId = Properties.Settings.Default.UserId,
                        Name = tbxName.Text,
                        UsernameOrEmail = tbxUsernameOrEmail.Text,
                        Website = tbxWebsite.Text ?? "",
                        Date = DateTime.Now.ToString()
                    };

                    try
                    {
                        auth.Password = AesAlg.EncryptString(Cache.CurrentUser.Substring(Cache.CurrentUser.Length - 32), tbxPassword.Password);

                        using (DatabaseContext cntxt = new DatabaseContext())
                        {
                            cntxt.Authentications.AddAsync(auth);
                            cntxt.SaveChangesAsync();
                        }

                        authentication.Insert(0, auth);

                        clearInputFields();
                        Notify.Show(ENotificationTypes.Success, message: "Added successfully");
                    }
                    catch (Exception) { Notify.Show(ENotificationTypes.Error, message: "Error while adding"); }
                }
            }
            else if ((string)bAddOrEdit.Content == "Save")
            {
                editAuthentication.UserId = Properties.Settings.Default.UserId;
                editAuthentication.Name = tbxName.Text;
                editAuthentication.UsernameOrEmail = tbxUsernameOrEmail.Text;
                editAuthentication.Website = tbxWebsite.Text ?? "";
                editAuthentication.Date = DateTime.Now.ToString();

                try
                {
                    Authentication currentAuth = authentication.Single(i => i.AuthId == editAuthentication.AuthId);
                    int index = authentication.IndexOf(currentAuth);

                    using (DatabaseContext cntxt = new DatabaseContext())
                    {
                        cntxt.Authentications.Update(editAuthentication);
                        cntxt.SaveChangesAsync();
                    }

                    authentication.RemoveAt(index);
                    authentication.Insert(0, editAuthentication);

                    clearInputFields();
                    bAddOrEdit.Content = "Add";
                    bDelete.Visibility = Visibility.Collapsed;
                    Notify.Show(ENotificationTypes.Success, message: "Saved successfully");
                }
                catch (Exception) { Notify.Show(ENotificationTypes.Error, message: "Error while adding"); }
            }
        }

        /// <summary>
        /// Cansels the add or edit function, clears the input fields, collapses the delete btn and turns the content of the bAddOrEdit to "Add"
        /// </summary>
        private void Button_Click_CanselUpdate(object sender, RoutedEventArgs e)
        {
            clearInputFields();
            bAddOrEdit.Content = "Add";
            bDelete.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// finds the authentication from the list with the id,
        /// asking for verification and saves the response,
        /// if the responcec is NOT EVerifyResults.YES return and do nothing,
        /// else remove the authentiation item from the database asynhronously
        /// reset the ui and display the correct notifications
        /// </summary>
        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                Authentication currentAuth = authentication.Single(i => i.AuthId == editAuthentication.AuthId);
                EVerifyResults res = Verify.Show(EVerifyTypes.YESNO,
                    "Warning",
                    $"Do you want to delete {currentAuth.Name} ?",
                    backgroundColor: (Brush?)this.FindResource("Background"),
                    textColor: (Brush?)this.FindResource("TextColorLight"),
                    confirmButtonColor: (Brush?)this.FindResource("MainColor"),
                    closeButtonColor: (Brush?)this.FindResource("MainColor"),
                    confirmBtnTextColor: (Brush?)this.FindResource("TextColorDark"));

                if (res != EVerifyResults.YES) return;

                using (DatabaseContext cntxt = new DatabaseContext())
                {
                    cntxt.Authentications.Remove(currentAuth);
                    cntxt.SaveChangesAsync();

                    authentication.Remove(currentAuth);
                }
                clearInputFields();
                bAddOrEdit.Content = "Add";
                bDelete.Visibility = Visibility.Collapsed;
                Notify.Show(ENotificationTypes.Success, message: $"{currentAuth.Name} deleted successfully");
            }
            catch (Exception) { Notify.Show(ENotificationTypes.Error, message: "Error while adding"); }
        }
        #endregion

        /// <summary>
        /// clears the input fields
        /// </summary>
        private void clearInputFields()
        {
            tbxName.Text = "";
            tbxUsernameOrEmail.Text = "";
            tbxPassword.Password = "";
            tbxWebsite.Text = "";
        }

        /// <summary>
        /// checks if the required fields are emty or null
        /// </summary>
        /// <returns>true if any is empty else false</returns>
        private bool areFiledsEmpty()
        {
            if (string.IsNullOrEmpty(tbxName.Text) || string.IsNullOrEmpty(tbxUsernameOrEmail.Text) || string.IsNullOrEmpty(tbxPassword.Password))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// text change event to dislay or hide the hint
        /// </summary>
        private void tbxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbxName.Text))
            {
                tbkName.Visibility = Visibility.Visible;
            }
            else
            {
                tbkName.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// text change event to dislay or hide the hint
        /// </summary>
        private void tbxUsernameOrEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbxName.Text))
            {
                tbkUsernameOrEmail.Visibility = Visibility.Visible;
            }
            else
            {
                tbkUsernameOrEmail.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// text change event to dislay or hide the hint
        /// </summary>
        private void tbxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbxPassword.Password))
            {
                tbkPassword.Visibility = Visibility.Visible;
            }
            else
            {
                tbkPassword.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// text change event to dislay or hide the hint
        /// </summary>
        private void tbxWebsite_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbxWebsite.Text))
            {
                tbkWebsite.Visibility = Visibility.Visible;
            }
            else
            {
                tbkWebsite.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// a button that removes the text from the password generator field and adds a new rundom password
        /// </summary>
        private void Button_Click_RNG(object sender, RoutedEventArgs e)
        {
            tbxRNG.Text = "";
            tbxRNG.Text = PasswordGenerator.Generate((int)sRNGLength.Value);
        }

        /// <summary>
        /// a value change listener, changes the length of the random password
        /// </summary>
        private void sRNGLength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbkRNGLength.Text = sRNGLength.Value.ToString();
        }

        /// <summary>
        /// a button listener
        /// generates a random password with the length based on the value of the slider, sets the password and random password fields the password
        /// </summary>
        private void Button_Click_Autofill_Password(object sender, RoutedEventArgs e)
        {
            string tempPass = PasswordGenerator.Generate((int)sRNGLength.Value);
            tbxPassword.Password = tempPass;
            tbxRNG.Text = tempPass;
        }

        /// <summary>
        /// logout function,
        /// removes the user id from the propertes settings
        /// opens the login window
        /// closes the current window
        /// </summary>
        private void Button_Click_LogOut(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UserId = -1;
            Properties.Settings.Default.Save();

            UserAuthentication userAuthentication = new UserAuthentication();
            userAuthentication.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            userAuthentication.Show();

            this.Close();
        }
    }

}
