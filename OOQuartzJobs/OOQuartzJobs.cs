using System;
using System.Threading.Tasks;
using Quartz;

namespace OOQuartzJobs
{
    public class OOTestJob : IJob
    {

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("OO Test Job Running");
            foreach (var jobdataStr in context.JobDetail.JobDataMap.Keys)
            {
                Console.WriteLine($"\t{jobdataStr} = {context.JobDetail.JobDataMap[jobdataStr]}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
            Console.WriteLine("OO Test Job Done Running");
        }
    }
}
