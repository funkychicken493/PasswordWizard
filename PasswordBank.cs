using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PasswordWizard;

public class PasswordBank(List<PasswordEntry> passwords)
{

    public List<PasswordEntry> passwords = passwords;
    
    public PasswordEntry? UserSearch()
    {
        throw new NotImplementedException();
    }

    public string regexActivator = "reg:";

    public enum SearchOption
    {
        WEBSITE,
        USERNAME,
        PASSWORD,
        NOTE
    }

    public void Search(string query, SearchOption searchBy)
    {
        bool regex = query.StartsWith(regexActivator);
        if(regex)
        {
            query = query.Remove(0, regexActivator.Length);
            Console.WriteLine(query);
        }
        switch (searchBy)
        {
            case SearchOption.WEBSITE:
                if(regex)
                {
                    passwords = [.. passwords.OrderByDescending(e => e.websiteAddresses.Find(w => Regex.Match(w, query).Success) != null).ThenBy(e => e.username)];
                }
                else
                {
                    passwords = [.. passwords.OrderByDescending(e => e.websiteAddresses.Contains(query)).ThenBy(e => e.username)];
                }
                break;
            case SearchOption.USERNAME:
                if(regex)
                {
                    passwords = [.. passwords.OrderByDescending(e => Regex.Match(e.username, query).Success)];
                }
                else
                {
                    passwords = [.. passwords.OrderByDescending(e => e.username.Contains(query))];
                }
                break;
            case SearchOption.PASSWORD:
                if (regex)
                {
                    passwords = [.. passwords.OrderByDescending(e => Regex.Match(e.password, query).Success)];
                }
                else
                {
                    passwords = [.. passwords.OrderByDescending(e => e.username.Contains(query))];
                }
                break;
            case SearchOption.NOTE:
                if (regex)
                {
                    passwords = [.. passwords.OrderByDescending(e => Regex.Match(e.note, query).Success)];
                }
                else
                {
                    passwords = [.. passwords.OrderByDescending(e => e.username.Contains(query))];
                }
                break;
        }
    }

    public string ListUsernamesPasswords()
    {
        string output = "";
        foreach(PasswordEntry entry in passwords)
        {
            output += entry.username + ": " + entry.password + "\n";
        }
        return output;
    }
}
