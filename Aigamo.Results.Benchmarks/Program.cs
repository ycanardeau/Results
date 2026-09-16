using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

// Run in-process: BenchmarkDotNet's default CsProj toolchain rejects the net11.0
// preview runtime moniker ("GetRuntimeVersion not implemented for NotRecognized").
// The in-process toolchain skips that SDK validation and runs the benchmarks in
// this host. For the same reason, do not pass `--job` on the command line — it
// re-adds a CsProj-toolchain job and reintroduces the crash. Drop all of this once
// BenchmarkDotNet recognizes net11.0.
var config = DefaultConfig.Instance.AddJob(
	Job.Default.WithToolchain(InProcessEmitToolchain.Instance)
);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
