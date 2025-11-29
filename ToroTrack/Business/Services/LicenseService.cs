using ToroTrack.Data;
using ToroTrack.Data.Entities;
using ToroTrack.Data.Repositories;

namespace ToroTrack.Business.Services
{
    public class LicenseService
    {
        private readonly ILicenseRepository _repository;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(ILicenseRepository repository, ILogger<LicenseService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<License>> GetAllLicensesAsync()
        {
            try
            {
                return await _repository.GetAllLicensesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch licenses.");
                throw;
            }
        }

        public async Task<List<ApplicationUser>> GetClientsAsync()
        {
            return await _repository.GetClientsAsync();
        }

        public async Task SaveLicenseAsync(License license)
        {
            try
            {
                if (license.Id == 0)
                {
                    _logger.LogInformation($"Adding new license: {license.SoftwareName}");
                    await _repository.AddLicenseAsync(license);
                }
                else
                {
                    _logger.LogInformation($"Updating license ID: {license.Id}");
                    await _repository.UpdateLicenseAsync(license);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving license {license.SoftwareName}");
                throw;
            }
        }
    }
}