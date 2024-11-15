namespace Acorn.Database.Models;

public class Account
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Salt { get; set; }
    public required string FullName { get; set; }
    public required string Location { get; set; }
    public required string Email { get; set; }
    public required string Country { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastUsed { get; set; }
    public required IList<Character> Characters { get; set; }
}