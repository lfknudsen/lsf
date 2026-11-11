module Program

open System
open System.IO
open lsf
open lsf.DirEntry
open lsf.Formatting
open lsf.Monad
open lsf.Sorting


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

let formatSubDirs bytesPerKB =
    computeSubDirSizes >> formatDirEntry bytesPerKB

let WriteFilesLast bytesPerKB dirs =
    Console.Write "\rComputing..."
    Array.fold (fun (acc: string) (f: string) -> acc + formatFile bytesPerKB f) "\r" dirs
    |> Console.Write

let WriteDirsLast bytesPerKB dirs =
    Console.Write "\rComputing..."
    Array.fold (fun acc f -> f |> formatSubDirs bytesPerKB |> ((+) acc)) "\r" dirs
    |> Console.Write

let WriteFile (bytesPerKB: int) =
    formatFile bytesPerKB >> Console.Write

let WriteDir (bytesPerKB: int) =
    formatSubDirs bytesPerKB >> Console.Write

let aside (f: 'a -> unit) (a: 'a) (v: 'v) :'v =
    f a
    v

let PrintHeader (printHeader: bool) (header: string) (files: 'a array): 'a array =
    if printHeader && files.Length > 0 then
        Console.WriteLine header
    files

let fileBanner = "┌─────────────────────────── Files: ───────────────────────────┐"
let dirBanner = "┌──────────────────────── Directories: ────────────────────────┐"

let WriteFiles (printContinuously: bool) (bytesPerKB: int) (files: string array) =
    if printContinuously then
        Array.iter (WriteFile bytesPerKB) files
    else
        WriteFilesLast bytesPerKB files

let WriteDirs (printContinuously: bool) (bytesPerKB: int) (dirs: string array) =
    if printContinuously then
        Array.iter (WriteDir bytesPerKB) dirs
    else
        WriteDirsLast bytesPerKB dirs

let printFiles (dir: string) (printContinuously: bool) (bytesPerKB: int) (printHeader: bool) (atTop: bool) =
    let headerString = 
        match atTop with
            | true -> "┌" + (formatBanner 78 "─" "Files:") + "┐"
            | false -> "├" + (formatBanner 78 "─" "Files:") + "┤"
    dir
    |> Directory.GetFiles
    |> PrintHeader printHeader headerString
    |> WriteFiles printContinuously bytesPerKB

let printFilesM (m: Monad) : Monad =
    if m.showFiles then
        printFiles m.dir m.printContinuously m.bytesPerKB m.printBanner true
    { m with printedFiles = true }

let printDirs (dir: string) (printContinuously: bool) (bytesPerKB: int) (printHeader: bool) (atTop: bool) =
    let headerString = 
        match atTop with
            | true -> "┌" + (formatBanner 78 "─" "Directories:") + "┐"
            | false -> "├" + (formatBanner 78 "─" "Directories:") + "┤"
    dir
    |> Directory.GetDirectories
    |> PrintHeader printHeader headerString
    |> WriteDirs printContinuously bytesPerKB

let printDirsM (m: Monad) : Monad =
    if m.showDirs then
        printDirs m.dir m.printContinuously m.bytesPerKB m.printBanner (not m.showFiles)
    m

let printBottom (m: Monad) : Monad =
    if m.showDirs || m.showFiles then
        Console.WriteLine ("└" + (String.replicate 78 "─" ) + "┘")
    m

[<EntryPoint>]
let main (argv: string array) =
    let contains (s: string) = Array.contains s argv
    let bytesPerKB = if contains "--1024" then 1024 else 1000
    let ordering: Ordering =
        match contains "-s", contains "-r" with
        | true, true -> OrderBy.Size, OrderDirection.Des
        | true, false -> OrderBy.Size, OrderDirection.Asc
        | false, true -> OrderBy.Name, OrderDirection.Des
        | false, false -> OrderBy.Name, OrderDirection.Asc
    let printContinuously = contains "-c" || ordering <> DefaultOrdering
    let showDirs = contains "-d" || not (contains "-f")
    let showFiles = contains "-f" || not (contains "-d")
    let printHeader = not (contains "-b")
    let dir = Directory.GetCurrentDirectory()
    newMonad ordering dir bytesPerKB printContinuously showDirs showFiles printHeader
    |> printFilesM
    |> printDirsM
    |> printBottom
    |> ignore

    0
