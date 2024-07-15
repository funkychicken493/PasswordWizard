using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TextCopy;
using static PasswordWizard.PasswordBank;

namespace PasswordWizard;

public class Main
{
    // Path of the folder where this program's data can be stored.
    public static string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PasswordWizard");
    // Primary password file.
    public static string passwordFile = Path.Combine(appDataPath, "passwords");

    public PasswordBank primaryBank;

    private string masterPassword = "";

    public void Run()
    {
        /*foreach(byte b in AesEncryptionHelper.GetKeyFromPassword(Console.ReadLine() ?? ""))
        {
            Console.WriteLine(b);
        }*//*
        //AesEncryptionHelper.key = Encoding.UTF8.GetBytes(Console.ReadLine() ?? "");
        AesEncryptionHelper.key = AesEncryptionHelper.GetKeyFromPassword(masterPassword);
        //AesEncryptionHelper.TestEncryption();
        await AesEncryptionHelper.TestDecryption();
        Console.WriteLine("ok are we still here or what");
        //AssertPrimaryPasswordFileExists();
        //LoadSavedPasswordBank();
        //UserLoop();
        //SavePasswordBank();*/
        Login();
        AssertPrimaryPasswordFileExists();
        UserLoop();
        SavePasswordBank();
    }

    public void UserLoop()
    {
        while (true)
        {
            int action = SelectNumberedMenuOption("What would you like to do?", false, ["Copy, delete, or edit a password", "Add a new password", "Exit program"]);
            switch (action)
            {
                case 1:
                    {
                        UserFind();
                        break;
                    }
                case 2:
                    {
                        AddNewPassword();
                        break;
                    }
                case 3:
                    {
                        return;
                    }
            }
            SavePasswordBank();
        }
    }

    public void AddNewPassword()
    {
        Console.WriteLine("Type \"cancel\" at any time to stop adding a new password.");
        string username = GetNonEmptyInput("What is the username? (required)");
        if (username == "cancel") { return; }
        string password = GetNonEmptyInput("What is the password? (required)");
        if (password == "cancel") { return; }
        string note = GetMaybeEmptyInput("Any notes on this password?");
        if (note == "cancel") { return; }
        List<string> websiteAddresses = [];
        string inputWebsite = GetMaybeEmptyInput("Enter one or more websites you can use this password on. Enter \"done\" when you are finished or \"none\" to skip this part.");
        while (inputWebsite != "done" && inputWebsite != "none" && inputWebsite != "")
        {
            if (inputWebsite == "cancel")
            {
                return;
            }
            websiteAddresses.Add(inputWebsite);
            inputWebsite = Console.ReadLine() ?? "";
        }
        PasswordEntry passwordEntry = PasswordEntry.Create(username, password);
        passwordEntry.note = note;
        passwordEntry.websiteAddresses = websiteAddresses;
        primaryBank.passwords.Add(passwordEntry);
        Console.WriteLine("Added new password!");
    }

    public void UserFind()
    {
        int searchType = SelectNumberedMenuOption("How would you like to search for your password?", true, ["Username", "Website", "Note", "Password"]);
        if (searchType == 0)
        {
            Console.WriteLine("Cancelled!");
            return;
        }
        QueryType searchOption = searchType switch
        {
            1 => QueryType.USERNAME,
            2 => QueryType.WEBSITE,
            3 => QueryType.NOTE,
            4 => QueryType.PASSWORD,
            // It gets mad at me if I don't put in a default case, even though it can never be outside 1-4, so here we are.
            _ => QueryType.USERNAME
        };
        Console.WriteLine("What would you like to search for?");
        string search = Console.ReadLine() ?? "";
        primaryBank.Search(search, searchOption);
        List<string> passwordSearchResults = [];
        // Prints up to ten entries that are relevant, or if there are less than ten entries it prints out every password.
        foreach (PasswordEntry entry in primaryBank.passwords.GetRange(0, primaryBank.passwords.Count > 10 ? 10 : primaryBank.passwords.Count))
        {
            passwordSearchResults.Add(
                "\n  Websites: " + entry.GetItemizedWebsiteList() +
                "\n  Username: " + entry.username
                );
        }
        int selection = SelectNumberedMenuOption("Select a password:", true, passwordSearchResults.ToArray());
        if (selection == 0) { return; }
        ActionPassword(selection - 1);
    }

    public void ActionPassword(int passwordIndex)
    {
        int selection = SelectNumberedMenuOption("Select what you want to do with this password:", true, ["Copy", "Edit", "Delete"]);
        if (selection == 0) { return; }
        switch (selection)
        {
            case 1:
                {
                    int copyWhat = SelectNumberedMenuOption("What would you like to copy to your clipboard?", true, ["Username", "Password", "Note"]);
                    if (copyWhat == 0) { ActionPassword(passwordIndex); return; }
                    PasswordEntry entry = primaryBank.passwords[passwordIndex];
                    string toCopy = copyWhat switch
                    {
                        1 => entry.username,
                        2 => entry.password,
                        3 => entry.note,
                        // Default case unreachable
                        _ => entry.password
                    };
                    ClipboardService.SetText(toCopy);
                    break;
                }
            case 2:
                {
                    EditPassword(passwordIndex);
                    break;
                }
            case 3:
                {
                    bool confirm = GetBooleanInput("Are you sure you want to delete this password? (type yes or no)");
                    if (!confirm)
                    {
                        return;
                    }
                    primaryBank.passwords.RemoveAt(passwordIndex);
                    Console.WriteLine("Password deleted!");
                    return;
                }
        }
    }

    public void EditPassword(int passwordIndex)
    {
        int selection = SelectNumberedMenuOption("Select what part you want to edit:", true, ["Username", "Website", "Note", "Password"]);
        if (selection == 0) { return; }
        switch (selection)
        {
            case 1:
                {
                    string newUsername = GetNonEmptyInput("What would you like to change your username to? (type cancel to cancel)");
                    if (newUsername == "cancel") { return; }
                    primaryBank.passwords[passwordIndex].username = newUsername;
                    break;
                }
            case 2:
                {
                    List<string> newWebsites = GetListOfInputs("Type a list of websites you can use this password on, press enter for each new entry. When you are done, simply type \"done\".");
                    primaryBank.passwords[passwordIndex].websiteAddresses = newWebsites;
                    break;
                }
            case 3:
                {
                    string newNote = GetNonEmptyInput("What would you like to change your note to? (type cancel to cancel)");
                    if (newNote == "cancel") { return; }
                    primaryBank.passwords[passwordIndex].note = newNote;
                    break;
                }
            case 4:
                {
                    string newPassword = GetNonEmptyInput("What would you like to change your password to? (type cancel to cancel)");
                    if (newPassword == "cancel") { return; }
                    primaryBank.passwords[passwordIndex].password = newPassword;
                    break;
                }
        }
    }

    /// <summary>
    /// Forces the user to select an option from a numbered menu of options. The integer returned is the index of the option in the options param that the user chose.
    /// </summary>
    /// <param name="prompt">The initial question which the user can answer by selecting a number.</param>
    /// <param name="canCancel">Specifies if there is an option with the number "0" that allows the user to cancel whatever they're doing.</param>
    /// <param name="options">The options the user can select from, displayed starting from 1.</param>
    /// <returns>The index in the options param that the user selected.</returns>
    public static int SelectNumberedMenuOption(string prompt, bool canCancel, string[] options)
    {
        Console.WriteLine(prompt);
        if (canCancel)
        {
            Console.WriteLine("0: Cancel");
        }
        for (int i = 0; i < options.Length; i++)
        {
            Console.WriteLine(i + 1 + ": " + options[i]);
        }
        while (true)
        {
            string input = Console.ReadLine() ?? "";
            if (!int.TryParse(input, out int numChoice))
            {
                Console.WriteLine("Input \"" + input + "\" is not an integer, please try again.");
            }
            // If the choice is less than the max number of choices, AND it is zero AND the user can cancel OR it is greater than 0.
            else if (numChoice <= options.Length && ((numChoice == 0 && canCancel) || numChoice > 0))
            {
                return numChoice;
            }
            else
            {
                Console.WriteLine("Specified number \"" + numChoice + "\" is outside the allowed range of choices, please try again.");
            }
        }
    }

    public static string GetNonEmptyInput(string prompt)
    {
        Console.WriteLine(prompt);
        string input = Console.ReadLine() ?? "";
        while (input == "")
        {
            Console.WriteLine("The requested input cannot be empty, please try again.");
            input = Console.ReadLine() ?? "";
        }
        return input;
    }

    public static string GetMaybeEmptyInput(string prompt)
    {
        Console.WriteLine(prompt);
        return Console.ReadLine() ?? "";
    }

    public static List<string> GetListOfInputs(string prompt)
    {
        List<string> output = [];
        string input = GetNonEmptyInput(prompt);
        while (input != "done" && input != "none" && input != "")
        {
            output.Add(input);
            input = Console.ReadLine() ?? "";
        }
        return output;
    }

    public static bool GetBooleanInput(string prompt)
    {
        string input = GetNonEmptyInput(prompt);
        while (!IsConfirm(input) && !IsDeny(input))
        {
            Console.WriteLine("Please enter \"yes\" or \"no\".");
            input = Console.ReadLine() ?? "";
        }
        if (IsConfirm(input))
        {
            return true;
        }
        return false;
    }

    public static string[] confirmStrings = ["y", "yes", "true"];
    public static bool IsConfirm(string input)
    {
        return confirmStrings.Contains(input.ToLower());
    }

    public static string[] denyStrings = ["n", "no", "false"];
    public static bool IsDeny(string input)
    {
        return denyStrings.Contains(input.ToLower());
    }

    public void LoadSavedPasswordBank()
    {
        string decryptedPasswordFile = AesEncryptionHelper.DecryptFromFile(passwordFile);
        primaryBank = JsonConvert.DeserializeObject<PasswordBank>(decryptedPasswordFile) ?? throw new Exception("Failed to decrypt primary password file!");
    }

    public void SavePasswordBank()
    {
        AesEncryptionHelper.EncryptIntoFile(JsonConvert.SerializeObject(primaryBank), passwordFile);
    }

    public static void AssertPrimaryPasswordFileExists()
    {
        if (!File.Exists(passwordFile))
        {
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            File.Create(passwordFile).Close();
        }
    }

    public static bool HasPrimaryPasswordFile()
    {
        return File.Exists(passwordFile);
    }

    public void Login()
    {
        if (!HasPrimaryPasswordFile())
        {
            string setPassword = GetNonEmptyInput("Please write a master password for your password bank, you will need this every time you start this program. It is recommended that you write it down somewhere. Your password must have at least 8 characters and a special symbol.");
            while (setPassword.Length < 8 || !Regex.Match(setPassword, "[^a-zA-Z0-9]").Success)
            {
                setPassword = GetNonEmptyInput("Please enter a valid password with at least 8 characters and a special symbol.");
            }
            GetMaybeEmptyInput("Your password has been set. There is no way to recover your passwords without the master password. Please write it down somewhere safe now. Press enter to once you are finished.");
            masterPassword = setPassword;
            AesEncryptionHelper.key = AesEncryptionHelper.GetKeyFromPassword(masterPassword);
            primaryBank = new();
        }
        else
        {
            string password = GetNonEmptyInput("Please enter your master password for the program now.");
            masterPassword = password;
            AesEncryptionHelper.key = AesEncryptionHelper.GetKeyFromPassword(masterPassword);
            LoadSavedPasswordBank();
        }
    }
}
