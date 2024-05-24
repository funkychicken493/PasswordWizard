using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordWizard;

public class PasswordEntry(string username, string password)
{
    // The reason this is a list of strings is so that the user may reuse a password on multiple websites that use one account.
    public List<string> websiteAddresses = [];
    public string username = username;
    public string password = password;
    public string note = "";
    public DateTime timeCreated;
    public DateTime timeLastEdited;

    public PasswordEntry(List<string> websiteAddresses, string username, string password, string note, DateTime timeCreated, DateTime timeLastEdited) : this(username, password)
    {
        this.websiteAddresses = websiteAddresses;
        this.note = note;
        this.timeCreated = timeCreated;
        this.timeLastEdited = timeLastEdited;
    }

    /// <summary>
    /// Properly creates a new password and sets the time created.
    /// </summary>
    public static PasswordEntry Create(string username, string password)
    {
        PasswordEntry entry = new(username, password);
        entry.UpdateTimeCreated();
        return entry;
    }

    public void UpdateTimeCreated()
    {
        timeCreated = DateTime.Now;
        UpdateTimeLastEdited();
    }

    public void UpdateTimeLastEdited()
    {
        timeLastEdited = DateTime.Now;
    }
}
