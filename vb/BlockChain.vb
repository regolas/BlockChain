Imports System.Security.Cryptography
Imports System.Collections
Imports System.IO

Namespace BlockChain

    Public Interface IBlock
        ReadOnly Property Data As Byte()
        Property Hash As Byte()
        Property Nonce As Integer
        Property PrevHash As Byte()
        Property TimeStamp As DateTime
    End Interface

    Public Class Block
        Implements IBlock

        Public Sub New(data As Byte())
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            Me.Data = data
            Me.Nonce = 0
            Me.PrevHash = New Byte() {&H0}
            Me.TimeStamp = DateTime.Now
        End Sub

        Public ReadOnly Property Data As Byte() Implements IBlock.Data
        Public Property Hash As Byte() Implements IBlock.Hash
        Public Property Nonce As Integer Implements IBlock.Nonce
        Public Property PrevHash As Byte() Implements IBlock.PrevHash
        Public Property TimeStamp As DateTime Implements IBlock.TimeStamp

        Public Overrides Function ToString() As String
            Return $"{BitConverter.ToString(Hash).Replace("-", "")} :{Environment.NewLine} {BitConverter.ToString(PrevHash).Replace("-", "")} :{Environment.NewLine} {Nonce} {TimeStamp}"
        End Function
    End Class

    Public Module BlockExtension
        <System.Runtime.CompilerServices.Extension>
        Public Function GenerateHash(block As IBlock) As Byte()
            Using sha512 As SHA512 = SHA512.Create()
                Using ms As New MemoryStream()
                    Using bw As New BinaryWriter(ms)
                        bw.Write(block.Data)
                        bw.Write(block.PrevHash)
                        bw.Write(block.Nonce)
                        bw.Write(block.TimeStamp.ToString())
                        Dim s = ms.ToArray()
                        Return sha512.ComputeHash(s)
                    End Using
                End Using
            End Using
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function MineHash(block As IBlock, difficulty As Byte()) As Byte()
            If difficulty Is Nothing Then Throw New ArgumentNullException(NameOf(difficulty))
            If difficulty.Length > 32 Then Throw New ArgumentException("Difficulty is too long")
            Dim hash As Byte() = New Byte() {}
            Dim maxIterations As Integer = Integer.MaxValue
            Dim iterations As Integer = 0
            While Not hash.Take(2).SequenceEqual(difficulty)
                If iterations >= maxIterations Then Throw New InvalidOperationException("Max iterations reached. Mining failed.")
                block.Nonce += 1
                hash = block.GenerateHash()
                iterations += 1
            End While
            Return hash
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function IsValid(block As IBlock) As Boolean
            Dim bk = block.GenerateHash()
            Return block.Hash.SequenceEqual(bk)
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function IsPrevBlock(block As IBlock, prevBlock As IBlock) As Boolean
            If prevBlock Is Nothing Then Throw New ArgumentNullException(NameOf(prevBlock))
            Return prevBlock.IsValid() AndAlso block.PrevHash.SequenceEqual(prevBlock.Hash)
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function IsValid(items As IEnumerable(Of IBlock)) As Boolean
            Dim enums = items.ToList()
            Return enums.Zip(enums.Skip(1), Function(a, b) Tuple.Create(a, b)).All(Function(block) block.Item2.IsValid() AndAlso block.Item2.IsPrevBlock(block.Item1))
        End Function
    End Module

    Public Class BlockChain
        Implements IEnumerable(Of IBlock)

        Private _items As New List(Of IBlock)()

        Public Sub New(difficulty As Byte(), genesis As IBlock)
            Me.Difficulty = difficulty
            genesis.Hash = genesis.MineHash(difficulty)
            _items.Add(genesis)
        End Sub

        Public ReadOnly Property Difficulty As Byte()

        Public Sub Add(item As IBlock)
            If _items.LastOrDefault() IsNot Nothing Then
                item.PrevHash = _items.LastOrDefault().Hash
            End If
            item.Hash = item.MineHash(Difficulty)
            Items.Add(item)
        End Sub

        Public Property Items As List(Of IBlock)
            Get
                Return _items
            End Get
            Set(value As List(Of IBlock))
                _items = value
            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return _items.Count
            End Get
        End Property

        Default Public Property Item(index As Integer) As IBlock
            Get
                Return Items(index)
            End Get
            Set(value As IBlock)
                Items(index) = value
            End Set
        End Property

        Public Function GetEnumerator() As IEnumerator(Of IBlock) Implements IEnumerable(Of IBlock).GetEnumerator
            Return Items.GetEnumerator()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Items.GetEnumerator()
        End Function
    End Class

End Namespace