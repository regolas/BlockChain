// See https://aka.ms/new-console-template for more information

//reference block.cs in a using statement
using tBlockChain;

Console.WriteLine("Hello, World!");

var ran = new Random();
IBlock genesis = new Block(new byte[] {0x00,0x00,0x00,0x00,0x00});
byte[] difficulty = new byte[] {0x00,0x00};
BlockChain chain = new BlockChain(difficulty, genesis);
for (int i=0; i<200;i++)
{
    var data = Enumerable.Range(0,255).Select(p=>(byte)ran.Next());
    chain.Add(new Block(data.ToArray()));
    Console.WriteLine(chain.LastOrDefault()?.ToString());
    if(chain.IsValid())
        Console.WriteLine("BlockChain is valid");
    else
        Console.WriteLine("Chain is invalid");
}
Console.ReadLine();