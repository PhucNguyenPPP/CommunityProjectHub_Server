using CPH.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ProjectStatusUpdater : BackgroundService
{
    private readonly ILogger<ProjectStatusUpdater> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ProjectStatusUpdater(ILogger<ProjectStatusUpdater> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProjectStatusUpdater service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();

                try
                {
                    await projectService.UpdateProjectsStatusToInProgress();
                    _logger.LogInformation("Cập nhật trạng thái dự án thành đang diễn ra thành công");
                    //          Console.WriteLine(" _logger.LogInformation(\"Project status updated successfully");
                    await projectService.UpdateProjectsStatusToCompleted();
                    _logger.LogInformation("Cập nhật trạng thái dự án thành đã kết thúc thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Xảy ra lỗi trong quá trình cập nhật trạng thái dự án");
              //      Console.WriteLine("An error occurred while updating project status.");
                }
            }

            // Chạy service mỗi 5 phút
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }

        _logger.LogInformation("ProjectStatusUpdater service stopped.");
    }
   
}