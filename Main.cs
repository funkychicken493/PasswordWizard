using Newtonsoft.Json;
using static PasswordWizard.PasswordBank;

namespace PasswordWizard;

public class Main
{
    // Path of the folder where this program's data can be stored.
    public static string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PasswordWizard");
    // Primary password file.
    public static string passwordFile = Path.Combine(appDataPath, "passwords.json");

    public PasswordBank primaryBank;

    public void Run()
    {
        AssertPrimaryPasswordFileExists();
        LoadSavedPasswordBank();
        UserLoop();
        SavePasswordBank();
    }

    public void UserLoop()
    {
        int action = SelectNumberedMenuOption("What would you like to do?", false, ["Copy, delete, or edit a password", "Add a new password"]);
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
                "\n  Username: " + entry.username +
                "\n  Password: " + entry.password
                );
        }
        int selection = SelectNumberedMenuOption("Select a password:", true, passwordSearchResults.ToArray());
        ActionPassword(selection);
    }

    public void ActionPassword(int passwordIndex)
    {

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

    public void LoadSavedPasswordBank()
    {
        primaryBank = JsonConvert.DeserializeObject<PasswordBank>(File.ReadAllText(passwordFile)) ?? new([]);
    }

    public void SavePasswordBank()
    {
        File.WriteAllText(passwordFile, JsonConvert.SerializeObject(primaryBank));
    }

    public static void AssertPrimaryPasswordFileExists()
    {
        if (!File.Exists(passwordFile))
        {
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            File.Create(passwordFile);
        }
    }
}
