using System;
using DataBase.Models;

namespace DataBase.Interfaces
{
    public interface IUserRepository
    {
        Task CreateUserAsync(string username, string passwordHash);
        Task<UserDto?> GetByUsernameAsync(string username);
    }
}
