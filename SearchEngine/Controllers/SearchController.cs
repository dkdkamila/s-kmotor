
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;

using Microsoft.AspNetCore.Mvc;


public class SearchController : Controller
{
    private List<SearchResult> AllSearchResults
    {
        get
        {
            return HttpContext.Session.GetObject<List<SearchResult>>("AllSearchResults") ?? new List<SearchResult>();
        }
        set
        {
            HttpContext.Session.SetObject("AllSearchResults", value);
        }
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ActionName("Search")]
    public IActionResult Search(string query)
    {
        List<SearchResult> results = GetGoogleSearchResults(query);
        AllSearchResults = results;

        return View("/Views/Home/Search.cshtml", results);
    }

    public IActionResult SaveResult(int resultIndex)
    {
        AllSearchResults[resultIndex].IsSaved = true;
        AllSearchResults = AllSearchResults;
        return RedirectToAction("Search");
    }

    public IActionResult SavedResults()
    {
        var savedResults = AllSearchResults.Where(result => result.IsSaved).ToList();
        return View(savedResults);
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
}
