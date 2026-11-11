module lsf.Formatting

open System
open System.IO

let sizeUnits = [| " B"; "KB"; "MB"; "GB"; "TB" |]
let max_filename_length = 40
let truncated_filename_length = max_filename_length - 3
let max_size_length = 15
let truncated_size_length = max_size_length - 1
let truncatedSizeCache = (String.replicate truncated_size_length "9") + "+  "

type TextAlignment =
    | Left = -1
    | Right = 1

type Column =
    struct
        val header: string
        val width: int
        val align: TextAlignment
    end

    new(header, width, align) =
        { header = header
          width = width
          align = align
        }

    new(header, width) = Column(header, width, TextAlignment.Left)

type Table =
    struct
        val columns: Column seq
        val content: string seq seq
    end

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

    aux (double size) 0

let truncate (maxLength: int) (word: string) : string =
    if word.Length > maxLength then
        word[..maxLength - 3] + "..."
    else
        word

let truncateFilename (filename: string) : string =
    if filename.Length > max_filename_length then
        filename[..truncated_filename_length] + "..."
    else
        filename

let getFileName (path: string) : string =
    let idx = path.LastIndexOf(Path.DirectorySeparatorChar)
    if idx < 0 then path else path[idx + 1 ..]

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

let pair a b = (a, b)

let formatInfo (bytesPerKB: double) (info: FileInfo) =
    (info.Name |> formatFilename, info.Length |> formatSize bytesPerKB)

let formatEntry (name: string, size: string) = $"│ {size, 15} │ {name, -58} │"

let formatEntryN (nameAndSize: string * string) = formatEntry nameAndSize + "\n"

let formatFile (bytesPerKB: double) =
    FileInfo >> formatInfo bytesPerKB >> formatEntryN

let formatBanner (maxWidth: int) (bannerChar: string) (header: string) : string =
    if header.Length + 2 >= maxWidth then
        header
    else
        let bannerCharCount = maxWidth - (header.Length + 2)
        let leftCount, remainder = Math.DivRem(bannerCharCount, 2)
        let rightCount = leftCount + if remainder = 0 then 0 else 1
        let left = String.replicate leftCount bannerChar
        let right = String.replicate rightCount bannerChar
        $"{left} {header} {right}"

let padLeft (s: string) = s.PadLeft

let padL (width: int) = (fun (s: string) -> s.PadLeft(width))

let padRight (s: string) = s.PadRight

let padR (width: int) = (fun (s: string) -> s.PadRight(width))

let pad (width: int) =
    if width >= 0 then
        padR width
    else
        padL width

let formatColumn (col: Column) (text: string) =
    truncate col.width text
    |> pad (col.width * (int col.align))
