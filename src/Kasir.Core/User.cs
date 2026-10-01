namespace Kasir.Core;

public enum UserRole
{
    Admin,
    Kasir
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Kasir;
    public bool IsActive { get; set; } = true;
}
