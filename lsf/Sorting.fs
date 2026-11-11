module lsf.Sorting

type OrderBy =
    | Name = 0
    | Size = 1

type OrderDirection =
    | Asc = 0
    | Des = 1

type Ordering = OrderBy * OrderDirection

let DefaultOrdering = OrderBy.Name, OrderDirection.Asc