module lsf.DirEntry

open System.IO
open lsf.Formatting

type FileProps =
    struct
        val exists: bool
        val is_dir: bool
        val hidden: bool
        val length: int64
    end

    new(exists, is_dir, hidden, length) =
        { exists = exists
          is_dir = is_dir
          hidden = hidden
          length = length }

    new(info: FileInfo) =
        { exists = info.Exists
          is_dir = info.Attributes.HasFlag(FileAttributes.Directory)
          hidden = info.Attributes.HasFlag(FileAttributes.Hidden)
          length = info.Length
        }

    new(path: string) =
        FileProps(FileInfo(path))

    static member get_Zero() : FileProps = FileProps(false, false, false, 0)


let getFileProperties (path: string) : FileProps =
    FileProps(path)


let getNameAndSize path =
    let props = getFileProperties path

    if not props.exists then
        $"{path}", props.length, props.is_dir
    elif props.hidden then
        $"{path}", props.length, props.is_dir
    else
        path, props.length, props.is_dir

type FileEntry =
    struct
        val name: string
        val size: int64

        new(path: string) =
            let info = FileInfo(path)
            { name = info.Name; size = info.Length }

        new(name: string, size: int64) =
            { name = name; size = size }

        static member op_Addition(left: FileEntry, right: FileEntry) : FileEntry =
            FileEntry(left.name, left.size + right.size)

        static member get_Zero() : FileEntry = FileEntry("", 0)

        override this.ToString() : string =
            let filename = Path.GetFileName(this.name) |> truncateFilename
            let size_string = formatSize 1000 this.size
            $"| {filename, -42} | {size_string, 15} |"

    end



type DirEntry =
    struct
        val path: string
        val size: int64

        new(path: string, size: int64) =
            { path = path; size = size }

        new(path: string) =
            { path = path; size = 0 }

        static member op_Addition(left: DirEntry, right: DirEntry) : DirEntry =
            DirEntry(left.path, left.size + right.size)

        static member get_Zero() : DirEntry = DirEntry("", 0)

        override this.ToString() : string =
            let filename = Path.GetFileName(this.path) |> truncateFilename
            let size_string = formatSize 1000 this.size
            formatEntryN (filename, size_string)

    end

let (.+.) (left: DirEntry) (right: DirEntry) =
    DirEntry(left.path, left.size + right.size)

let formatDirEntry (bytesPerKB: double) (entry: DirEntry) : string =
    let name = entry.path |> formatFilename
    let size = formatSize bytesPerKB entry.size
    formatEntryN (name, size)
