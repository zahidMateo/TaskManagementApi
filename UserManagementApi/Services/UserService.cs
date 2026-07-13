using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagementApi.Data;
using UserManagementApi.Models;

namespace UserManagementApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly Integration.IWorkItemsClient _workItemsClient;

        public UserService(UserDbContext context, Integration.IWorkItemsClient workItemsClient)
        {
            _context = context;
            _workItemsClient = workItemsClient;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                return false;
            }

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Determina si un usuario está saturado (tiene más de 3 tareas altamente relevantes activas).
        /// </summary>
        public async Task<bool> IsUserSaturatedAsync(string id)
        {
            int count = await GetActiveHighlyRelevantTaskCountAsync(id);
            return count > 3;
        }

        /// <summary>
        /// Obtiene la cantidad de tareas altamente relevantes activas asignadas a un usuario.
        /// </summary>
        public async Task<int> GetActiveHighlyRelevantTaskCountAsync(string id)
        {
            var items = await _workItemsClient.GetWorkItemsByUserIdAsync(id);
            int activeHighlyRelevantCount = 0;
            foreach (var item in items)
            {
                if (item.IsRelevant && 
                    !string.Equals(item.Status, "Completed", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(item.Status, "Completado", StringComparison.OrdinalIgnoreCase))
                {
                    activeHighlyRelevantCount++;
                }
            }
            return activeHighlyRelevantCount;
        }
    }
}
