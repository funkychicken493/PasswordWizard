namespace PasswordWizard;

public class PasswordEntry
{
    // The reason this is a list of strings is so that the user may reuse a password on multiple websites that use one account.
    public List<string> websiteAddresses = [];
    public string username;
    public string password;
    public string note = "";
    public DateTime timeCreated;
    public DateTime timeLastEdited;

    public PasswordEntry(string username, string password)
    {
        this.username = username;
        this.password = password;
    }

    public PasswordEntry(List<string> websiteAddresses, string username, string password, string note, DateTime timeCreated, DateTime timeLastEdited) : this(username, password)
    {
        this.websiteAddresses = websiteAddresses;
        this.note = note;
        this.timeCreated = timeCreated;
        this.timeLastEdited = timeLastEdited;
    }

    /// <summary>
    /// This constructor exists soley so that the json compiler can put it back together.
    /// </summary>
    public PasswordEntry()
    {

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

    public string GetItemizedWebsiteList()
    {
        if(websiteAddresses.Count == 0)
        {
            return "None";
        }
        string output = "";
        foreach(string website in websiteAddresses)
        {
            output += "\n   - " + website;
        }
        return output;
    }
}
