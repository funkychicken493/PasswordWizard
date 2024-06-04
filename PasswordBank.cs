using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PasswordWizard;

public class PasswordBank()
{

    public List<PasswordEntry> passwords = [];
    
    public PasswordBank(List<PasswordEntry> passwordEntries) : this()
    {
        passwords = passwordEntries;
    }

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

    public enum SortOption
    {
        WEBSITE,
        USERNAME,
        TIME_CREATED,
        TIME_EDITED
    }

    public void Sort(SortOption sortOption)
    {
        switch(sortOption)
        {
            case SortOption.WEBSITE:
                passwords = [.. passwords.OrderByDescending(e => e.websiteAddresses.First())];
                break;
            case SortOption.USERNAME:
                passwords = [.. passwords.OrderByDescending(e => e.username)];
                break;
            case SortOption.TIME_CREATED:
                passwords = [.. passwords.OrderByDescending(e => e.timeCreated)];
                break;
            case SortOption.TIME_EDITED:
                passwords = [.. passwords.OrderByDescending(e => e.timeLastEdited)];
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
