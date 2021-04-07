module Tests.Types

open System
type Person = { Id: int; Name: string }
type LowerCaseId = { id: int; age:int }
type SimpleUnion = One | Two
type RecordWithSimpleUnion = { Id: int; Union: SimpleUnion }
type RecordWithList = { Id: int; List: int list }
type Maybe<'a> = Just of 'a | Nothing
type RecordWithGenericUnion<'t> = { Id: int; GenericUnion: Maybe<'t> }
type RecordWithDateTime = { id: int; created: DateTime }
type RecordWithMap = { id : int; map: Map<string, string> }
type RecordWithArray = { id: int; arr: int[] }
type RecordWithOptionalArray = { id: int; arr: int[] option }
type RecordWithResizeArray = { id: int; resizeArray: ResizeArray<int> }
type RecordWithDecimal = { id: int; number: decimal }
type RecordWithEnum = { id: int; color: ConsoleColor }
type RecordWithLong = { id: int; long: int64 }
type RecordWithFloat = { id: int; float: float }
type RecordWithGuid = { id: int; guid: Guid }
type RecordWithBytes = { id: int; data:byte[] }
type RecordWithObjectId = { id: LiteDB.ObjectId }
type RecordWithOptionOfValueType = { id:int; optionOfValueType: Option<int>  }
type RecordWithOptionOfReferenceType = { id:int; optionOfReferenceType : Option<Person>  }

type Shape =
    | Circle of float
    | Rect of float * float
    | Composite of Shape list


type Value = Num of int | String of string
type RecordWithMapDU = { Id: int; Properties: Map<string, Value> }

type RecordWithShape = { Id: int; Shape: Shape }

type ComplexUnion<'t> =
    | Any of 't
    | Int of int
    | String of string
    | Generic of Maybe<'t>


type SingleCaseDU = SingleCaseDU of int

type RecordWithSingleCaseId = { Id : SingleCaseDU; Value : string }

type IColor =
    abstract member Color : string

type IBarcode =
    abstract member Barcode : string

type ISize =
    abstract member Size : int

type IItem =
    abstract member Id : int
    abstract member Art : string
    abstract member Name : string
    abstract member Number : int


type RecWithMember = {
    Id: int
    Name: string
}
with member this.Ignored() = sprintf "%d %s" this.Id this.Name
     member this.IgnoredToo = sprintf "%d %s" this.Id this.Name


[<CLIMutable>]
type Company=
  { Id: int
    Name: string}

[<CLIMutable>]
type EOrder=
  { Id: int
    Items : IItem list
    OrderNumRange: string }

[<CLIMutable>]
type Order=
  { Id : int
    Company : Company
    EOrders : EOrder list}
