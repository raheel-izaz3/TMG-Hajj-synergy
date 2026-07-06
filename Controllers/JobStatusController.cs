using Microsoft.AspNetCore.Mvc;
using Quartz.Impl.Matchers;
using Quartz;

namespace TMG.Controllers
{
    public class JobStatusController : Controller
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public JobStatusController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetJobStatus()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            var jobStatuses = new List<object>();

            foreach (var jobKey in jobKeys)
            {
                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                foreach (var trigger in triggers)
                {
                    jobStatuses.Add(new
                    {
                        JobName = jobKey.Name,
                        GroupName = jobKey.Group,
                        TriggerState = await scheduler.GetTriggerState(trigger.Key)
                    });
                }
            }

            return Ok(jobStatuses);
        }
    }
}
