namespace apiarticles.Controllers;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using Npgsql;

public class ArticleController : ControllerBase
{
    private NpgsqlDataSource _dataSource;

    public ArticleController(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    [HttpGet]
    [Route("/api/feed")]
    public object GetFeed()
    {
        if (string.IsNullOrEmpty(_dataSource.OpenConnection().ToString()))
        {
            return BadRequest("Connection string is not configured.");
        }
        using (var conn = _dataSource.OpenConnection())
        {
            var query = "SELECT * FROM news.articles";
            var articles = conn.Query<Article>(query);
            return Ok(articles);
        }
    }
    [HttpGet]
    [Route("/api/articles")]
    public object GetArticles([FromHeader] string searchterm, int pagesize)
    {
        if (string.IsNullOrEmpty(_dataSource.OpenConnection().ToString()))
        {
            return BadRequest("Connection string is not configured.");
        }

        using (var conn = _dataSource.OpenConnection())
        {
            {
                var query = "SELECT * FROM news.articles WHERE body ILIKE @searchterm LIMIT @pagesize";
                var articles = conn.Query<Article>(query, new { SearchTerm = $"%{searchterm}%", PageSize = pagesize });

                return Ok(articles);
            }
        }
    }
    [HttpPost]
    [Route("/api/articles")]
    public object PostArticles([FromBody] ArticleDTO.ArticleCreateDto articleDto)
    {
        Console.WriteLine($"here: {articleDto}");
        if (string.IsNullOrEmpty(_dataSource.OpenConnection().ToString()))
        {
            return BadRequest("Connection string is not configured.");
        }

        using (var conn = _dataSource.OpenConnection())
        {
            var insertQuery = "INSERT INTO news.articles (headline, body, author, articleimgurl) " +
                              "VALUES (@Headline, @Body, @Author, @ArticleImgUrl) " +
                              "RETURNING articleid";

            var newArticleId = conn.ExecuteScalar<int>(insertQuery, new
            {
                articleDto.Headline,
                articleDto.Body,
                articleDto.Author,
                articleDto.ArticleImgUrl
            });

            if (newArticleId <= 0)
            {
                return BadRequest("Failed to create article.");
            }

            articleDto.ArticleId = newArticleId;

            return CreatedAtAction(nameof(PostArticles), new { articleId = newArticleId }, articleDto);
        }
    }
    [HttpGet]
    [Route("/api/articles/{articleid}")]
    public object GetArticleById(int articleid)
    {
        if (string.IsNullOrEmpty(_dataSource.OpenConnection().ToString()))
        {
            return BadRequest("Connection string is not configured.");
        }

        using (var conn = _dataSource.OpenConnection())
        {
            var query = "SELECT * FROM news.articles WHERE articleid = @articleid";
            var article = conn.QuerySingleOrDefault<Article>(query, new { ArticleId = articleid });

            if (article == null)
            {
                return NotFound($"Article with ID {articleid} not found.");
            }

            return Ok(article);

        }
    }
    [HttpPut]
    [Route("/api/articles/{articleid}")]
    public object UpdateArticleById(int articleid, [FromBody] ArticleDTO.ArticleUpdateDto articleDto)
    {
        Console.WriteLine($"HERE: {articleDto}");
        if (string.IsNullOrEmpty(_dataSource.OpenConnection().ToString()))
        {
            return BadRequest("Connection string is not configured.");
        }

        using (var conn = _dataSource.OpenConnection())
        {
            var updateQuery = "UPDATE news.articles " +
                              "SET headline = @Headline, body = @Body, " +
                              "author = @Author, articleimgurl = @ArticleImgUrl " +
                              "WHERE articleid = @ArticleId";

            articleDto.ArticleId = articleid;

            var rowsAffected = conn.Execute(updateQuery, articleDto);

            if (rowsAffected == 0)
            {
                return NotFound($"Article with ID {articleid} not found.");
            }

            return Ok(articleDto);
        }
    }


    [HttpDelete]
    [Route("/api/articles/{articleid}")]
    public IActionResult DeleteArticleById(int articleid)
    {
        if (string.IsNullOrEmpty(_dataSource.OpenConnection().ToString()))
        {
            return BadRequest("Connection string is not configured.");
        }

        if (articleid <= 0)
        {
            return BadRequest("Invalid ArticleId.");
        }

        using (var conn = _dataSource.OpenConnection())
        {
            var query = "DELETE FROM news.articles WHERE articleid = @articleid"; 
            var rowsAffected = conn.Execute(query, new { ArticleId = articleid });
            if (rowsAffected == 0)
            {
                return NotFound($"Article with ID {articleid} not found.");
            }

            return NoContent();
        }
    }

}
    
