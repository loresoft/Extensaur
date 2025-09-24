using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Environments;

namespace LoreSoft.Extensions.Performance;

internal class Program
{
    static void Main(string[] args)
    {
        var benchmarkSwitcher = new BenchmarkSwitcher(typeof(Program).Assembly);

        var config = ManualConfig.CreateMinimumViable()
            .AddExporter(CsvExporter.Default)
            .AddExporter(MarkdownExporter.GitHub)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddJob(
                //CreateJob(ClrRuntime.Net48),
                //CreateJob(CoreRuntime.Core50),
                //CreateJob(CoreRuntime.Core60),
                //CreateJob(CoreRuntime.Core70),
                CreateJob(CoreRuntime.Core80),
                CreateJob(CoreRuntime.Core90)
            )
            .WithOption(ConfigOptions.JoinSummary, true)
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

        benchmarkSwitcher.Run(args, config);
    }

    private static Job CreateJob(Runtime runtime)
    {
        return Job.ShortRun
                .WithLaunchCount(1)
                .WithWarmupCount(2)
                .WithUnrollFactor(500)
                .WithIterationCount(10)
                .WithRuntime(runtime);
    }
}
