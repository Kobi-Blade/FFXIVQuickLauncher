using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Threading.Tasks;
using XIVLauncher.Common.Patching.IndexedZiPatch;

namespace XIVLauncher.PatchInstaller.Commands;

public class IndexRepairCommand
{
    public static readonly Command Command = new("index-repair", "Repair a game installation.");

    private static readonly Argument<string> PatchIndexFileArgument = new("patch-index-file")
    {
        Description = "Path to a patch index file. (*.patch.index)"
    };

    private static readonly Argument<string> GameRootPathArgument = new("game-path")
    {
        Description = "Root folder of a game installation, such as \"C:\\Program Files (x86)\\SquareEnix\\FINAL FANTASY XIV - A Realm Reborn\\\""
    };

    private static readonly Argument<string> PatchRootPathArgument = new("patch-root-path")
    {
        Description = "Path to a folder containing relevant patch files."
    };

    private static readonly Option<int> ThreadCountOption = new("--threads", "-t")
    {
        Description = "Number of threads. Specifying 0 will use all available cores."
    };

    static IndexRepairCommand()
    {
        Command.Arguments.Add(PatchIndexFileArgument);
        Command.Arguments.Add(GameRootPathArgument);
        Command.Arguments.Add(PatchRootPathArgument);
        ThreadCountOption.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<int>() < 0)
                result.AddError("Must be 0 or more");
        });
        Command.Options.Add(ThreadCountOption);
        Command.SetAction(parseResult => new IndexRepairCommand(parseResult).Handle());
    }

    private readonly string patchIndexFile;
    private readonly string gameRootPath;
    private readonly string patchRootPath;
    private readonly int threadCount;

    private IndexRepairCommand(ParseResult parseResult)
    {
        this.patchIndexFile = parseResult.GetValue(PatchIndexFileArgument)!;
        this.gameRootPath = parseResult.GetValue(GameRootPathArgument)!;
        this.patchRootPath = parseResult.GetValue(PatchRootPathArgument)!;
        this.threadCount = parseResult.GetValue(ThreadCountOption);
        if (this.threadCount == 0)
            this.threadCount = Environment.ProcessorCount;
        Debug.Assert(this.threadCount > 0);
    }

    private async Task<int> Handle()
    {
        await IndexedZiPatchOperations.RepairFromPatchFileIndexFromFile(this.patchIndexFile, this.gameRootPath, this.patchRootPath, this.threadCount);
        return 0;
    }
}
