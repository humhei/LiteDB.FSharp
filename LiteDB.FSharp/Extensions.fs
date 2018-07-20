namespace LiteDB.FSharp

open LiteDB
open System.Linq.Expressions
open System
open Quotations.Patterns
open FSharp.Reflection
open System

module Extensions =
    open Microsoft.FSharp.Quotations

    type LiteCollection<'t> with
        /// Tries to find a document using the Id of the document. 
        member collection.TryFindById(id: BsonValue) = 
            let result : 't = collection.FindById(id)
            match box result with
            | null -> None
            | _ -> Some result

        /// Tries to find a document using the given query
        member collection.TryFind (query: Query) = 
            let skipped = 0
            let limit = 1
            collection.Find(query, skipped, limit)
            |> Seq.tryHead

        /// Tries to find a single document using a quoted query expression
        member collection.tryFindOne<'t> (expr: Expr<'t -> bool>) : Option<'t> =
            let query = Query.createQueryFromExpr expr 
            collection.TryFind(query)

        /// Tries to find a single document using a quoted query expression, if no document matches, an exception is thrown
        member collection.findOne<'t> (expr: Expr<'t -> bool>) : 't = 
            match collection.tryFindOne expr with 
            | Some item -> item 
            | None -> failwith "Could not find a single document that matches the given qeury"
        
        /// Searches the collection for documents that match the given query expression
        member collection.findMany<'t> (expr: Expr<'t -> bool>) : seq<'t> = 
            let query = Query.createQueryFromExpr expr
            collection.Find(query)

        /// Executes a full search using the Where query 
        member collection.fullSearch<'t, 'u> (expr: Expr<'t -> 'u>) (pred: 'u -> bool) = 
            match expr with 
            | Lambda(_, PropertyGet(_, propInfo, [])) -> 
                let propName = 
                    match propInfo.Name with 
                    | ("Id" | "id" | "ID") -> "_id"
                    | _ -> propInfo.Name
                let query = 
                    Query.Where(propName, fun bsonValue -> 
                        bsonValue
                        |> Bson.deserializeField<'u> 
                        |> pred)
                collection.Find(query) 
            | _ -> 
                let expression = sprintf "%A" expr
                failwithf "Could not recognize the given expression \n%s\n, it should a simple lambda to select a property, for example: <@ fun record -> record.property @>" expression
        
        /// Creates a Query for a full search using a selector expression like `<@ fun record -> record.Name @>` and predicate
        member collection.where<'t, 'u> (expr: Expr<'t -> 'u>) (pred: 'u -> bool) = 
            match expr with 
                | Lambda(_, PropertyGet(_, propInfo, [])) -> 
                    let propName = 
                        match propInfo.Name with 
                        | ("Id" | "id" | "ID") -> "_id"
                        | _ -> propInfo.Name 

                    Query.Where(propName, fun bsonValue -> 
                        bsonValue
                        |> Bson.deserializeField<'u> 
                        |> pred)
                | _ -> 
                    let expression = sprintf "%A" expr
                    failwithf "Could not recognize the given expression \n%s\n, it should a simple lambda to select a property, for example: <@ fun record -> record.property @>" expression
    
    type LiteRepository with 
        ///Create a new permanent index in all documents inside this collections if index not exists already.
        member this.EnsureIndex<'T1,'T2> (exp: Expression<Func<'T1,'T2>>) =
            this.Database.GetCollection<'T1>().EnsureIndex(exp,true) |> ignore

    [<RequireQualifiedAccess>]
    module LiteRepository =

        ///Insert an array of new documents into collection. Document Id must be a new value in collection. Can be set buffer size to commit at each N documents
        let insertItems<'a> (items: seq<'a>) (lr:LiteRepository) =
            lr.Insert<'a>(items) |> ignore 
            lr
            
        ///Insert a new document into collection. Document Id must be a new value in collection
        let insertItem<'a> (item: 'a) (lr:LiteRepository) =
            lr.Insert<'a>(item)
            lr

        ///Update a document into collection.
        let updateItem<'a> (item: 'a) (lr:LiteRepository) =
            if lr.Update<'a>(item) = false then failwithf "Failed updated item %A" item
            else
                lr
        ///Returns new instance of LiteQueryable that provides all method to query any entity inside collection. Use fluent API to apply filter/includes an than run any execute command, like ToList() or First()
        let query<'a> (lr:LiteRepository) =
            lr.Query<'a>()

    [<RequireQualifiedAccess>]
    type LiteQueryable =
        ///Include DBRef field in result query execution
        static member ``include`` (exp: Expression<Func<'a,'b>>) (query: LiteQueryable<'a>) =
            query.Include(exp)
       
       ///Include DBRef field in result query execution
        static member expand (exp: Expression<Func<'a,'b>>) (query: LiteQueryable<'a>) =
            query.Include(exp)
        static member tryFirst (query: LiteQueryable<'a>) =
            query.ToEnumerable() |> Seq.tryHead

        static member first (query: LiteQueryable<'a>) =
            query.First()

        static member toList (query: LiteQueryable<'a>) =
            query.ToEnumerable() |> List.ofSeq

        ///Add new Query filter when query will be executed. This filter use database index
        static member where (exp: Expression<Func<'a,bool>>) (query: LiteQueryable<'a>) =
            query.Where exp

        static member find (exp: Expression<Func<'a,bool>>) (query: LiteQueryable<'a>) =
            query |> LiteQueryable.where exp |> LiteQueryable.first

        static member tryFind (exp: Expression<Func<'a,bool>>) (query: LiteQueryable<'a>) =
            query |> LiteQueryable.where exp |> LiteQueryable.tryFirst