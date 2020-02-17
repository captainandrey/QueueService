﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueService.DAL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Runner
{
    public class QueueItemArchiver : BackgroundService
    {
        private readonly AppSettings config;
        private readonly ILogger<QueueItemArchiver> logger;
        private readonly IQueueItemRepository queueItemRepository;

        public QueueItemArchiver(IQueueItemRepository queueItemRepository, IOptions<AppSettings> config, ILogger<QueueItemArchiver> logger)
        {
            this.config = config.Value;
            this.logger = logger;
            this.queueItemRepository = queueItemRepository;

        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("QueueItemArchiver Service is starting.");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("QueueItemArchiver Service is stopping.");

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await BackgroundProcessing(cancellationToken);
        }

        private async Task BackgroundProcessing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation($"Archiving finished queue items older than {config.ArchiveQueueItemsAgeDays} days.");
                    await queueItemRepository.ArchiveQueueItems(config.ArchiveQueueItemsAgeDays);
                    logger.LogInformation($"QueueItemArchiver Service is going to sleep for {config.ArchiveQueueItemsSleepTimeMS} milliseconds.");
                    await Task.Delay(config.ArchiveQueueItemsSleepTimeMS, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error archiving queue items.");
                    await Task.Delay(config.ArchiveQueueItemsSleepTimeMS, cancellationToken);
                }
            }
            logger.LogInformation("QueueItemArchiver Service is cancelled.");
        }

    }
}
