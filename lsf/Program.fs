open System
open System.IO

let sizeUnits = [|" B";"KB";"MB";"GB";"TB"|]
let max_filename_length = 40
let truncated_filename_length = max_filename_length - 3
let max_size_length = 15
let truncated_size_length = max_size_length - 1
let truncatedSizeCache = (String.replicate truncated_size_length "9") + "+  "

let rec foldSizeToHigherUnit (size: double) (unitIndex: int) : double * int =
    if unitIndex < sizeUnits.Length && size >= 1000.0 then
        foldSizeToHigherUnit (size / 1000.0) (unitIndex + 1)
    else
        size, unitIndex

let truncateFilename (filename: string) : string =
    if filename.Length > max_filename_length then
            filename[..truncated_filename_length] + "..."
        else
            filename

let getFileName (path: string) : string =
    let idx = path.LastIndexOf(Path.PathSeparator)
    if idx < 0 then
        path
    else
        path[idx + 1..]

let formatFilename = getFileName >> truncateFilename

let truncateSize (size: double) : string =
    let szWhole = size |> floor |> string
    if szWhole.Length > max_size_length then
            truncatedSizeCache
        else
            string size

let formatSize (size: int64) =
    let sz, unitIdx = foldSizeToHigherUnit (double(size)) 0
    let szString = sz |> truncateSize
    let unitString = sizeUnits[unitIdx]
    $"%.2f{float szString} %s{unitString}"

let computeFileSize (path: string) : int64 =
    try
        FileInfo(path).Length
    with
    | _ -> 0

type DirEntry =
    struct
        val path: string
        val size: int64
        new(path: string, size: int64) = { path = path; size = size }
        new(path: string) = { path = path; size = computeFileSize(path) }

        static member op_Addition(left: DirEntry, right: DirEntry) : DirEntry =
            DirEntry(left.path, left.size + right.size)

        static member get_Zero () : DirEntry =
            DirEntry("", 0)

        override this.ToString() : string =
            let filename = Path.GetFileName(this.path)
                           |> truncateFilename
            let size_string = formatSize this.size
            $"| {filename,-42} | {size_string,15} |"

    end

let (.+.) (left: DirEntry) (right: DirEntry) = DirEntry(left.path, left.size + right.size)

let rec computeSubDirSizes (dir: string) : DirEntry =
    try
        Directory.GetDirectories(dir)
        |> Array.sumBy computeSubDirSizes
        |> ((.+.) (computeFileSizesInDir dir))
    with
    | _ -> DirEntry(dir, 0)

and computeFileSizesInDir path : DirEntry =
    let sumSize =
        Directory.GetFiles(path)
        |> Array.sumBy computeFileSize
    DirEntry(path, sumSize)

let printFile (path: string) (size: int64) =
    DirEntry(path, size) |> Console.WriteLine

let pair a b = (a,b)

[<EntryPoint>]
let main (_: string array) =
    let dir = Directory.GetCurrentDirectory()
    Directory.GetFiles(dir)
    |> Array.iter (fun filename -> filename |> (computeFileSize >> printFile filename))

    Directory.GetDirectories(dir)
    |> Array.iter (fun dirname -> dirname |> (computeSubDirSizes >> Console.WriteLine))

    0