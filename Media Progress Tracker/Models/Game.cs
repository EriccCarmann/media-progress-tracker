namespace MediaProgressTracker.Models
{
    class Game
    {
        public int AppId { get; set; }
        public string Name { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string ScoreRank { get; set; }
        public int PositiveReviews { get; set; }
        public int NegativeReviews { get; set; }
        public int UserScore { get; set; }
        public string Owners { get; set; }
        public decimal AverageForever { get; set; }
        public decimal Average2Weeks { get; set; }
        public decimal MedianForever { get; set; }
        public decimal Median2Weeks { get; set; }
        public decimal Price { get; set; }
        public decimal InitialPrice { get; set; }
        public decimal Discount { get; set; }
        public int CCU { get; set; }
    }
}