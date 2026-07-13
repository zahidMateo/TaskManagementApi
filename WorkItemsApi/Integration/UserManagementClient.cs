using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkItemsApi.Models;

namespace WorkItemsApi.Integration
{
    public class UserManagementClient : IUserManagementClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserManagementClient> _logger;

        public UserManagementClient(HttpClient httpClient, ILogger<UserManagementClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            try
            {
                var users = await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>("api/users");
                return users ?? Enumerable.Empty<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users from UserManagementApi");
                return Enumerable.Empty<UserDto>();
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{id}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId} from UserManagementApi", id);
                return null;
            }
        }

        public async Task<UserSaturationDto?> GetUserSaturationAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{userId}/saturation");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<UserSaturationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user saturation for {UserId} from UserManagementApi", userId);
                return null;
            }
        }
    }
}
