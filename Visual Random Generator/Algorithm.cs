using Kalyna;

namespace Visual_Random_Generator
{
    public class Algorithm
    {
        public const int RandomValueSize = 100;

        public Block S { get; private set; } = new Block { Data = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
        public Block D { get; } = new Block();
        public Block K { get; } = new Block { Data = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };
        private Block R { get; set; } = new Block();
        private Block I { get; set; } = new Block();
        private Block X { get; set; } = new Block();
        private Block T { get; set; } = new Block();
        public Block RandomValue { get; set; } = new Block();

        public int CurrentByte { get; private set; } = 0;
        private int CurrentBit { get; set; } = 0;

        public bool IsCompleted { get; set; }

        private Kalyna.Algorithm Kalyna { get; } = new Kalyna.Algorithm();

        public Algorithm()
        {
            for (var i = 0; i < RandomValueSize; i++)
                RandomValue.Data.Add(0);
        }

        public void Stage3()
        {
            I = new Block(Kalyna.Encrypt(D, K));
        }

        public void GenerateRandomBit()
        {
            R = new Block(I);
            R.Xor(S);

            X = new Block(Kalyna.Encrypt(R, K));
            X.Xor(K);

            T = new Block(X);
            T.Xor(I);

            S = new Block(Kalyna.Encrypt(T, K));

            RandomValue.Data[CurrentByte] |= (byte)((X.Data[0] & 1) << CurrentBit);
            if (CurrentBit == 7)
                CurrentByte++;
            CurrentBit = (CurrentBit + 1) % 8;
            if (CurrentByte == RandomValueSize)
                IsCompleted = true;
        }
    }
}
