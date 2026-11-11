module lsf.Monad

open lsf.Sorting

[<Struct>]
type Monad =
    {
        order: Ordering
        dir: string
        bytesPerKB: int
        printContinuously: bool
        showDirs: bool
        showFiles: bool
        printBanner: bool
        printedFiles: bool
        totalFileSize: int
        totalDirSize: int
    }

let newMonad(order: Ordering) (dir: string) (bytesPerKB: int) (printContinuously: bool) (showDirs: bool) (showFiles: bool) (printBanner: bool) : Monad =
        {
            order = order
            dir = dir
            bytesPerKB = bytesPerKB
            printContinuously = printContinuously
            showDirs = showDirs
            showFiles = showFiles
            printBanner = printBanner
            printedFiles = false
            totalFileSize = 0
            totalDirSize = 0
        }
