module Program

open System
open System.IO
open lsf.DirEntry
open lsf.Formatting

let computeFileSize (path: string) : int64 =
    try
        FileInfo(path).Length
    with
    | _ -> -1

let rec computeSubDirSizes (dir: string) : DirEntry =
    try
        Directory.GetDirectories(dir)
        |> Array.sumBy computeSubDirSizes
        |> ((.+.) (computeFileSizesInDir dir))
    with
    | _ -> DirEntry(dir)

and computeFileSizesInDir path : DirEntry =
    let sumSize =
        Directory.GetFiles(path)
        |> Array.sumBy computeFileSize
    DirEntry(path, sumSize)

[<EntryPoint>]
let main (_: string array) =
    let bytesPerKB = 1000

    let dir = Directory.GetCurrentDirectory()
    Console.WriteLine "############################ Files: ############################"
    Directory.GetFiles(dir)
    |> Array.iter (formatFile bytesPerKB >> Console.WriteLine)

    Console.WriteLine "######################### Directories: #########################"
    Directory.GetDirectories(dir)
    |> Array.iter (computeSubDirSizes >> Console.WriteLine)

    0