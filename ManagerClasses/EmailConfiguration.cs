namespace TMG.ManagerClasses;

public class EmailConfiguration
{

    private string SmtpServer { get; }
    private int Port { get; }
    private string Username { get; }
    private string Password { get; }
    private bool EnableSsl { get; }
    private string FromEmail { get; }

    public EmailConfiguration(string smtpServer, int port, string username, string password, bool enableSsl, string fromEmail)
    {
        SmtpServer = smtpServer;
        Port = port;
        Username = username;
        Password = password;
        EnableSsl = enableSsl;
        FromEmail = fromEmail;
    }



}
