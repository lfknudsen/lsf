module lsf.Formatting

open System.IO

let sizeUnits = [|" B";"KB";"MB";"GB";"TB"|]
let max_filename_length = 40
let truncated_filename_length = max_filename_length - 3
let max_size_length = 15
let truncated_size_length = max_size_length - 1
let truncatedSizeCache = (String.replicate truncated_size_length "9") + "+  "

type FileSize =
    struct
    val size: double
    val unitIndex: int
    val bytesPerKilo: double
    end

let foldSize (bytesPerKB: double) (size: int64) : double * int =
    let rec aux (size: double) (unitIndex: int) : double * int =
        if unitIndex < sizeUnits.Length && size >= bytesPerKB then
            aux (size / bytesPerKB) (unitIndex + 1)
        else
            size, unitIndex
    aux (double(size)) 0

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

let formatSize (bytesPerKB: double) (size: int64) =
    let sz, unitIdx = foldSize bytesPerKB size
    let szString = sz |> truncateSize
    let unitString = sizeUnits[unitIdx]
    $"%.2f{float szString} %s{unitString}"

let pair a b = (a,b)

let formatInfo (bytesPerKB: double) (info: FileInfo) =
    (info.Name |> formatFilename, info.Length |> formatSize bytesPerKB)

let formatEntry (name: string, size: string) =
    $"| {name, -42} | {size, 15} |"

let formatFile (bytesPerKB: double) =
    FileInfo >> formatInfo bytesPerKB >> formatEntry