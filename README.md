# LSF

Directory listing which computes and displays the size of files and subdirectories.

## Usage

```
lsf [OPTIONS]
    
    Options:
    -h, --help    Print this message and exit.
    --1024        Consider a kilobyte to be equal to 1024 bytes, as Microsoft does.
    -s            Sort the output by size instead of name.
    -r            Reverse the sort order.
    -c            Print each entry one at a time, instead of waiting for all computations to finish.
    -d            Only show directories. Mutually exclusive with -f.
    -f            Only show files. Mutually exclusive with -d.
    -b            Refrain from printing the header(s).
```

## Setup

```
dotnet publish lsf -o <output directory>
```

Note that, just like C#, F# programmes consist of many files - it's not just a single executable.
Thus, the output directory should be a directory exclusively for this programme's files.
It does not need to exist already.
You will then have to add the directory to your PATH.