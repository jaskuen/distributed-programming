using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Valuator.Pages
{
    public class AboutModel : PageModel
    {
        public string? Name { get; set; }
        public string? Group { get; set; }
        
        private readonly ILogger<AboutModel> _logger;

        public AboutModel(ILogger<AboutModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Name = "Михаил Шелеметев";
            Group = "ПС-33";
        }
    }
}
