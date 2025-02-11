// For more information see https://aka.ms/fsharp-console-apps
open System.Security.Cryptography
open System.Linq
open System.IO
open System.Collections
open System

type IBlock =
    abstract member Data: byte[] with get
    abstract member Hash: byte[] with get, set
    abstract member Nonce: int with get, set
    abstract member PrevHash: byte[] with get, set
    abstract member TimeStamp: DateTime with get, set

type Block (data : byte[]) =
// Why is the setting hash to Array.empty failing

    let mutable hash = Array.empty<byte>
    let mutable nonce = 0
    let mutable prevHash = [|0uy|]
    let mutable timeStamp = DateTime.Now
    
    interface IBlock with
        member this.Data = 
            if data = null then raise (ArgumentNullException("data")) else data // ArgumentNullException handling
        member this.Hash
            with get() = hash
            and set(value) = hash <- value// Initialize to empty array
        member this.Nonce
            with get() = nonce
            and set(value) = nonce <- value
        member this.PrevHash
            with get() = prevHash
            and set(value) = prevHash <- value
        member this.TimeStamp
            with get() = timeStamp
            and set(value) = timeStamp <- value

    override this.ToString() =
        let hashString = BitConverter.ToString(hash).Replace("-", "")
        let prevHashString = BitConverter.ToString(prevHash).Replace("-", "")
        $"{hashString} :\n {prevHashString} :\n {nonce} {timeStamp}"

[<AbstractClass; Sealed>]
type BlockExtension =
    static member GenerateHash (data : byte[], prevHash : byte[], nonce:int,timeStamp) =
        let mutable sha512 = SHA512.Create()
        let ms = new MemoryStream()
        let bw = new BinaryWriter(ms)
        bw.Write(data)
        bw.Write(prevHash)
        bw.Write(nonce)
        bw.Write(timeStamp.ToString())
        let s = ms.ToArray()
        sha512.ComputeHash(s)

    static member MineHash (data : byte[], prevHash : byte[], nonce:int,timeStamp,difficulty:byte[]) =
        let mutable hash:byte[] = [|0uy|]
        let maxIterations = Int32.MaxValue
        let mutable iterations = 0
        let mutable currentNonce = nonce
        if difficulty = null then raise (ArgumentNullException(nameof(difficulty))) // ArgumentNullException handling
        if difficulty.Length > 32 then raise (ArgumentException("Difficulty is too long")) // ArgumentException handling
        
        while not (hash.Take(2).SequenceEqual(difficulty)) do
            if iterations >= maxIterations then
                raise (InvalidOperationException("Max iterations reached. Mining failed."))
            
            currentNonce <- currentNonce + 1
            let block = new Block(data)
            (block :> IBlock).PrevHash <- prevHash
            (block :> IBlock).Nonce <- currentNonce
            (block :> IBlock).TimeStamp <- timeStamp
            hash <- BlockExtension.GenerateHash(data, prevHash, currentNonce, timeStamp)
            iterations <- iterations + 1
        hash
    
    static member IsValid(data: byte[], prevHash: byte[], nonce: int, timeStamp: DateTime, expectedHash: byte[]) : bool =
        let generatedHash = BlockExtension.GenerateHash(data, prevHash, nonce, timeStamp)
        generatedHash.SequenceEqual(expectedHash)

    //static member CreateGenesisBlock () =
    //    let data = System.Text.Encoding.UTF8.GetBytes("Genesis Block")
    //    BlockExtension.CreateBlock(data)

[<EntryPoint>]
printfn "Hello from F#"
