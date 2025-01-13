using System.Security.Cryptography;
using System.Collections;

namespace tBlockChain
{
    public interface IBlock
    {
        public byte[] Data { get; }
        public byte[] Hash { get; set; }
        public int Nonce { get; set; }
        public byte[] PrevHash { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class Block : IBlock
    {
        public Block(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Nonce = 0;
            PrevHash = new byte[]{0x00};
            TimeStamp = DateTime.Now;
        }
        public byte[] Data { get; }
        public byte[] Hash { get; set; }
        public int Nonce { get; set; }
        public byte[] PrevHash { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return $"{BitConverter.ToString(Hash).Replace("-", "")} :\n {BitConverter.ToString(PrevHash).Replace("-", "")} :\n {Nonce} {TimeStamp}";
        }
    }
    public static class BlockExtension
    {
        public static byte[] GenerateHash(this IBlock block)
        {

            using (SHA512 sha512 = SHA512.Create())
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
            {           
                bw.Write(block.Data);
                bw.Write(block.PrevHash);
                bw.Write(block.Nonce);
                bw.Write(block.TimeStamp.ToString());
                var s = ms.ToArray();
                return sha512.ComputeHash(s);
            }
        }
        public static byte[] MineHash(this IBlock block, byte[] difficulty)
        {
            if(difficulty == null)
                throw new ArgumentNullException(nameof(difficulty));
            if(difficulty.Length >32)
                throw new ArgumentException("Difficulty is too long");//added
            byte[] hash = new byte[0];//new byte[32] was recommended
            int maxIterations = int.MaxValue;//added
            int iterations = 0;//added
            while (!hash.Take(2).SequenceEqual(difficulty)) //changed from hash.Take(2).SequenceEqual(difficulty) to !hash.Take(difficulty.Length).SequenceEqual(difficulty)
            {
                if (iterations >= maxIterations)
                    throw new InvalidOperationException("Max iterations reached. Mining failed.");//added

                block.Nonce++;
                hash = block.GenerateHash();
                iterations++;//added
            }
            return hash;
        }
        public static bool IsValid(this IBlock block)
        {
            var bk = block.GenerateHash();
            return block.Hash.SequenceEqual(bk);
        }
        public static bool IsPrevBlock(this IBlock block, IBlock prevBlock)
        {
            if(prevBlock == null)
                throw new ArgumentNullException(nameof(prevBlock));
            return prevBlock.IsValid() && block.PrevHash.SequenceEqual(prevBlock.Hash);
        }
        public static bool IsValid(this IEnumerable<IBlock> items)
        {
            var enums = items.ToList();
            return enums.Zip(enums.Skip(1), Tuple.Create).All(block => block.Item2.IsValid()
            && block.Item2.IsPrevBlock(block.Item1));
        }
    }
    public class BlockChain:IEnumerable<IBlock>
    {
        private List<IBlock> _items = new List<IBlock>();
        public BlockChain(byte[] difficulty, IBlock genesis)
        {
            Difficulty = difficulty;
            genesis.Hash = genesis.MineHash(difficulty);
            _items.Add(genesis);
        }
        public byte[] Difficulty { get; }

        public void Add(IBlock item)
        {
            if(_items.LastOrDefault() != null)
            {
                item.PrevHash = _items.LastOrDefault().Hash;
            }
            item.Hash = item.MineHash(Difficulty);
            Items.Add(item);
        }
        public List<IBlock> Items
        {
            get => _items;
            set => _items=value;
        }
        public int Count => _items.Count;
        public IBlock this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }
        public IEnumerator<IBlock> GetEnumerator()
        {
            return Items.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}