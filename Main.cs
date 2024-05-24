using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PasswordWizard;

public class Main
{
    public static string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PasswordWizard");
    public static string passwordFile = Path.Combine(appDataPath, "passwords.json");
    public void Run()
    {
        PasswordBank bank = new([
            PasswordEntry.Create("jim", "password123"),
            PasswordEntry.Create("amy", "90320749@&!8129412p04"),
            PasswordEntry.Create("aaaaa", "scream"),
            PasswordEntry.Create("zzzz", "sleep"),
            PasswordEntry.Create("mypasswordisa", "a"),
            PasswordEntry.Create("mypasswordisz", "z")
        ]);
        Console.WriteLine(bank.ListUsernamesPasswords());
        bank.Search("reg:a*", PasswordBank.SearchOption.PASSWORD);
        Console.WriteLine(bank.ListUsernamesPasswords());
        string json = JsonConvert.SerializeObject(bank);
        Console.WriteLine(passwordFile);
        if(!File.Exists(passwordFile))
        {
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            File.Create(passwordFile);
        }
        File.WriteAllText(passwordFile, json);
    }
}
