using VideoLinks.Models.Abstract;

namespace VideoLinks.Models
{
    public class Vote : IEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// 1-10 and 0 = Broken
        /// </summary>
        public int Score { get; set; }

        public int? VideoId { get; set; }
        public virtual Video Video { get; set; }

        public int? LinkId { get; set; }
        public virtual Link Link { get; set; }
    }
}