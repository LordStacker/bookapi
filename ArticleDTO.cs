namespace apiarticles;

public abstract class ArticleDTO
{
    public class ArticleCreateDto
    {
        public string Headline { get; set; }
        public string Author { get; set; }
        public string Body { get; set; }
        public int ArticleId { get; set; }
        
        public string ArticleImgUrl { get; set; }
    }

    public class ArticleUpdateDto
    {
        public int ArticleId { get; set; }
        public string Headline { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string ArticleImgUrl { get; set; }
    }

}