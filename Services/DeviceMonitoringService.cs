using System.Net.NetworkInformation;
using a_webapi.Configuration;
using a_webapi.Data;
using a_webapi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace a_webapi.Services;

public class DeviceMonitoringService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DeviceMonitoringService> _logger;
    private readonly MonitoringOptions _options;

    public DeviceMonitoringService(AppDbContext dbContext, IOptions<MonitoringOptions> options,
        ILogger<DeviceMonitoringService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _options = options.Value;
    }

    public async Task CheckDevicesAsync(CancellationToken cancellationToken)
    {
        var devices = await _dbContext.Devices
            .ToListAsync(cancellationToken);

        _logger.LogDevicesFound(devices.Count);

        var tasks = devices.Select(CheckDeviceAsync);

        var histories = await Task.WhenAll(tasks);
/*
        await _dbContext.DeviceStatusHistories
            .AddRangeAsync(histories, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
*/
        _logger.LogMonitoringCompleted(histories.Length);
    }

    private async Task<DeviceStatusHistory> CheckDeviceAsync(Device device)
    {
        var checkedAt = DateTime.UtcNow;

        using var ping = new Ping();

        try
        {
            var reply = await ping.SendPingAsync(
                device.IpAddress,
                _options.PingTimeoutMilliseconds);

            var isOnline = reply.Status == IPStatus.Success;

            device.IsOnline = isOnline;
            device.LastCheck = checkedAt;

            _logger.LogDeviceChecked(device.Name, device.IpAddress, isOnline, reply.RoundtripTime);

            return new DeviceStatusHistory
            {
                IsOnline = isOnline,
                LastUpdate = checkedAt,
                DeviceId = device.Id,
                ResponseTimeMs = isOnline
                    ? (int)reply.RoundtripTime
                    : null,
                ErrorMessage = isOnline
                    ? null
                    : $"Ping status: {reply.Status}"
            };
        }
        catch (Exception ex)
        {
            device.IsOnline = false;
            device.LastCheck = checkedAt;

            _logger.LogDeviceCheckError(ex, device.Name, device.IpAddress);

            return new DeviceStatusHistory
            {
                IsOnline = false,
                LastUpdate = checkedAt,
                DeviceId = device.Id,
                ResponseTimeMs = null,
                ErrorMessage = ex.Message
            };
        }
    }
}

internal static partial class DeviceMonitoringLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Getting devices done. Found: {DevicesCount}")]
    public static partial void LogDevicesFound(this ILogger logger, int devicesCount);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Added {HistoriesCount} devices histories. Monitoring cycle completed")]
    public static partial void LogMonitoringCompleted(this ILogger logger, int historiesCount);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Device {DeviceName} ({IpAddress}) checked. Online: {IsOnline}, Response: {ResponseTime} ms")]
    public static partial void LogDeviceChecked(
        this ILogger logger, string deviceName, string ipAddress, bool isOnline, long responseTime);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Error checking device {DeviceName} ({IpAddress})")]
    public static partial void LogDeviceCheckError(
        this ILogger logger, Exception ex, string deviceName, string ipAddress);
}