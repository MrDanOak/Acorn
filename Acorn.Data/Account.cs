namespace Acorn.Data;
public class Account
{
    public string Username { get; set; }
    public string? Password { get; set; }
    public string? Salt { get; set; }
    public string? FullName { get; set; }
    public string? Location { get; set; }
    public string? Email { get; set; }
    public string? Country { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastUsed { get; set; }
    public IList<Character> Characters { get; set; }
}