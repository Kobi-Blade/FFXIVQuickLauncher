using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using XIVLauncher.Common.Patching.IndexedZiPatch;

namespace XIVLauncher.PatchInstaller.Commands;

public class IndexVerifyCommand
{
    public static readonly Command Command = new("index-verify", "Verify and optionally repair a game installation.");

    private static readonly Argument<string> GameRootPathArgument = new("game-path")
    {
        Description = "Root folder of a game installation, such as \"C:\\Program Files (x86)\\SquareEnix\\FINAL FANTASY XIV - A Realm Reborn\""
    };

    private static readonly Argument<string[]> PatchIndexFilesArgument = new("patch-index-files")
    {
        Description = "Path to a patch index file. (*.patch.index)"
    };

    private static readonly Option<int> ThreadCountOption = new("--threads", "-t")
    {
        Description = "Number of threads. Specifying 0 will use all available cores."
    };

    static IndexVerifyCommand()
    {
        Command.Arguments.Add(GameRootPathArgument);
        Command.Arguments.Add(PatchIndexFilesArgument);
        ThreadCountOption.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<int>() < 0)
                result.AddError("Must be 0 or more");
        });
        Command.Options.Add(ThreadCountOption);
        Command.SetAction((parseResult, cancellationToken) => new IndexVerifyCommand(parseResult).Handle(cancellationToken));
    }

    private readonly string gameRootPath;
    private readonly string[] patchIndexFiles;
    private readonly int threadCount;

    private IndexVerifyCommand(ParseResult parseResult)
    {
        this.gameRootPath = parseResult.GetValue(GameRootPathArgument)!;
        this.patchIndexFiles = parseResult.GetValue(PatchIndexFilesArgument)!;
        this.threadCount = parseResult.GetValue(ThreadCountOption);
        if (this.threadCount == 0)
            this.threadCount = Environment.ProcessorCount;
        Debug.Assert(this.threadCount > 0);

        // Do we have a .patch.index as the first argument?
        if (File.Exists(this.gameRootPath) && this.gameRootPath.EndsWith(".patch.index", StringComparison.OrdinalIgnoreCase))
        {
            var lastArg = this.patchIndexFiles[this.patchIndexFiles.Length - 1];

            // Do we have a folder as the last argument?
            if (Directory.Exists(lastArg) || lastArg.EndsWith("/", StringComparison.Ordinal) || lastArg.EndsWith("\\", StringComparison.Ordinal))
            {
                Log.Information("Taking the first argument as the first patch file, and the last argument as the target directory.");
                this.patchIndexFiles = new[] { this.gameRootPath }.Concat(this.patchIndexFiles.Take(this.patchIndexFiles.Length - 1)).ToArray();
                this.gameRootPath = lastArg;
            }
        }
    }

    private async Task<int> Handle(CancellationToken cancellationToken)
    {
        foreach (var f in this.patchIndexFiles)
            await IndexedZiPatchOperations.VerifyFromZiPatchIndex(f, this.gameRootPath, this.threadCount, cancellationToken);
        return 0;
    }
}
