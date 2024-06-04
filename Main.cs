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
    // Path of the folder where this program's data can be stored.
    public static string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PasswordWizard");
    // Primary password file.
    public static string passwordFile = Path.Combine(appDataPath, "passwords.json");

    public PasswordBank primaryBank;

    public void Run()
    {
        AssertPrimaryPasswordFileExists();
        LoadSavedPasswordBank();
        primaryBank.passwords.Add(PasswordEntry.Create("eeee", "aaaeaeaeaeeeaaaaaaaaaaaaaaaa"));
        SavePasswordBank();
    }

    public void LoadSavedPasswordBank()
    {
        primaryBank = JsonConvert.DeserializeObject<PasswordBank>(File.ReadAllText(passwordFile));
        primaryBank ??= new([]);
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
