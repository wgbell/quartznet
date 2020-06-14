using System;

using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;

using OOQuartzJobs;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace OORemoteClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("------- Initializing -------------------");

                NameValueCollection properties = new NameValueCollection
                {
                    ["quartz.scheduler.instanceName"] = "RemoteClient",
                    ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                    ["quartz.threadPool.threadCount"] = "5",
                    ["quartz.scheduler.proxy"] = "true",
                    ["quartz.scheduler.proxy.address"] = "tcp://127.0.0.1:555/QuartzScheduler"
                };

                // First we must get a reference to a scheduler
                ISchedulerFactory sf = new StdSchedulerFactory(properties);
                IScheduler sched = await sf.GetScheduler();

                // First we must get a reference to a scheduler
                //ISchedulerFactory sf = new StdSchedulerFactory();
                //IScheduler sched = await sf.GetScheduler();

                Console.WriteLine("------- Initialization Complete --------");
                sched.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail oojob = JobBuilder.Create<OOTestJob>()
                    .WithIdentity("oojob1", "oogroup1")
                    .Build();

                oojob.JobDataMap.Put("OOTestJob1","Bla Bla");
                oojob.JobDataMap.Put("User", "Gary Bell");
                // Trigger the job to run on the next round minute
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("ootrigger1", "oogroup1")
                    .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Second))
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await sched.ScheduleJob(oojob, trigger);
                var keys = await sched.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                foreach (var key in keys)
                {
                    Console.WriteLine("\t " + key);
                    var sw = new Stopwatch();
                    sw.Start();
                    while (sw.ElapsedMilliseconds < 60000)
                    {
                        var jobs = await sched.GetCurrentlyExecutingJobs();
                        if (jobs.Count > 0)
                        {
                            Console.WriteLine($"---Waiting on {jobs.Count} to finish.");
                            foreach (var job in jobs)
                            {
                                Console.WriteLine($"\tJob Desc: {job.JobDetail.Description}");
                                foreach (var jobdataStr in job.MergedJobDataMap.Keys)
                                {
                                    Console.WriteLine($"\t{jobdataStr} = {job.MergedJobDataMap[jobdataStr]}");
                                }
                            }
                        }
                        else
                        {
                            var groups = await sched.GetJobGroupNames();
                            if (groups.Count > 0)
                            {
                                Console.WriteLine($"There are {groups.Count} scheduled");
                            }
                            else
                            {
                                Console.WriteLine($"***Press a key to exit");
                                break;
                            }

                        }

                        await Task.Delay(1500);
                    }
                }
                Console.Read();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
