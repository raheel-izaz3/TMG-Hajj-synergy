using Quartz;
using Quartz.Impl;
using TMG.Interface;
using TMG.Services;

namespace TMG.Services
{
    public class EmailJobScheduler
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EmailJobScheduler(ISchedulerFactory schedulerFactory, IServiceScopeFactory serviceScopeFactory)
        {
            _schedulerFactory = schedulerFactory;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ScheduleJobAsync()
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<DailyEmailJob>()
                .WithIdentity("dailyEmailJob", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("dailyEmailTrigger", "group1")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(9, 0))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }

        private IEmailService GetEmailService()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<IEmailService>();
            }
        }
    }

}
