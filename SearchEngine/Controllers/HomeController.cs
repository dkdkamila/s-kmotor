using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SearchEngine.Models;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using System.Text.Json;
using SearchEngine.Extensions;





namespace SearchEngine.Controllers;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<HomeController> _logger;
    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    private List<SearchResult> AllSearchResults
    {
        get
        {
            var results = HttpContext.Session.GetObject<List<SearchResult>>("AllSearchResults");
            return results ?? new List<SearchResult>();
        }
        set
        {
            HttpContext.Session.SetObject("AllSearchResults", value);
        }
    }


    [HttpPost]
    [HttpGet]
    [ActionName("Search")]
    public IActionResult Search(string query)
    {
        List<SearchResult> results = GetGoogleSearchResults(query);
        AllSearchResults = results;

        return View("Search", results);
    }
    private List<SearchResult> GetGoogleSearchResults(string query)
    {
        var apiKey = "AIzaSyA1pFcTnhTpcbwJcegqE9rCHp_hIXFfHE8";
        var customSearchEngineId = "977d65868af22484e";

        var service = new CustomsearchService(new BaseClientService.Initializer { ApiKey = apiKey });

        var listRequest = service.Cse.List();
        listRequest.Q = query;
        listRequest.Cx = customSearchEngineId;

        var search = listRequest.Execute();

        var results = search.Items.Select(item => new SearchResult
        {
            Title = item.Title,
            Description = item.Snippet,
            Url = item.Link
        }).ToList();

        return results;
    }

    private List<CommentModel> AllComments
    {
        get
        {
            return HttpContext.Session.GetObject<List<CommentModel>>("AllComments") ?? new List<CommentModel>();
        }
        set
        {
            HttpContext.Session.SetObject("AllComments", value);
        }
    }

    [HttpPost]
    [ActionName("AddComment")]
    public IActionResult AddComment(string userName, string comment)
    {
        _logger.LogInformation($"AddComment called with userName: {userName}, comment: {comment}");

        try
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(comment))
            {
                // Läs befintliga kommentarer
                var existingComments = LoadCommentsFromJson();

                // Skapa ny kommentar
                var newComment = new CommentModel
                {
                    UserName = userName,
                    Comment = comment,
                    Timestamp = DateTime.Now
                };

                // Lägg till den nya kommentaren
                existingComments.Add(newComment);

                // Spara alla kommentarer till JSON
                SaveCommentsToJson(existingComments);
            }
            else
            {
                _logger.LogError("Invalid userName or comment.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding comment: {ex.Message}");
        }

        return RedirectToAction("Comments");
    }
    public IActionResult Comments()
    {
        var comments = LoadCommentsFromJson();
        return View("Comments", comments);
    }
    private void SaveCommentsToJson(List<CommentModel> comments)
    {
        try
        {
            var json = JsonSerializer.Serialize(comments);

            // Ange sökvägen till  JSON-fil i wwwroot-mappen
            var filePath = Path.Combine(_env.WebRootPath, "Comments.json");

            // Spara JSON till fil
            System.IO.File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving comments JSON: {ex.Message}");
        }
    }

    private List<CommentModel> LoadCommentsFromJson()
    {
        try
        {

            var filePath = Path.Combine(_env.WebRootPath, "Comments.json");

            // Kontrollera om filen existerar 
            if (System.IO.File.Exists(filePath))
            {
                // Läs JSON från fil
                var json = System.IO.File.ReadAllText(filePath);

                // Kontrollera om JSON-strängen är tom
                if (!string.IsNullOrEmpty(json))
                {
                    // Konvertera JSON till lista av kommentarer
                    var comments = JsonSerializer.Deserialize<List<CommentModel>>(json);

                    return comments ?? new List<CommentModel>();
                }
            }

            // Skicka  en tom lista om filen inte finns eller om JSON är ogiltig/tom
            return new List<CommentModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading comments JSON: {ex.Message}");
            return new List<CommentModel>();
        }
    }


}


