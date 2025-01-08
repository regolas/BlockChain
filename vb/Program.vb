Imports System
Imports System.Linq
Imports BlockChain.BlockChain

Module Program
    Public Sub Main(args As String())
        Dim ran As New Random()
        Dim genesis As IBlock = New Block(New Byte() {&H0, &H0, &H0, &H0, &H0})
        Dim difficulty As Byte() = New Byte() {&H0, &H0}
        Dim chain As New BlockChain.BlockChain(difficulty, genesis)

        Console.WriteLine("Hello World!")

        For i As Integer = 0 To 199
            Dim data = Enumerable.Range(0, 255).Select(Function(p) CByte(ran.Next(0, 255)))
            chain.Add(New Block(data.ToArray()))
            Console.WriteLine(chain.LastOrDefault()?.ToString())

            If chain.IsValid() Then
                Console.WriteLine("BlockChain is valid")
            Else
                Console.WriteLine("Chain is invalid")
            End If
        Next

        Console.WriteLine("Press Enter to exit...")
        Console.ReadLine()
    End Sub
End Module
