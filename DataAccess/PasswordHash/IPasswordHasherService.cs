namespace DataAccess.PasswordHash
{
    public interface IPasswordHasherService
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string password, string hashedPassword);
    }
}
