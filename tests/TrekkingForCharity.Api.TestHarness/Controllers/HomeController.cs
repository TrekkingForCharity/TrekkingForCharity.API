using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrekkingForCharity.Api.Client;
using TrekkingForCharity.Api.Client.Executors.Commands;

namespace TrekkingForCharity.Api.TestHarness.Controllers
{

    public class CommandModel
    {
        public string CommandData { get; set; }
    }
    public class HomeController : Controller
    {
        private readonly IApiClient _apiClient;
        public HomeController(IApiClient apiClient)
        {
            this._apiClient = apiClient;
        }
        [Authorize]
        public IActionResult Index()
        {
            
            return this.View();
        }


        public async Task<IActionResult> ProcessCommand([FromQuery] string commandName, [FromBody] CommandModel model)
        {
            var commandType = Assembly.GetAssembly(typeof(CreateTrekCommand)).GetTypes().Single(x => x.Name == commandName);
            var command = JsonConvert.DeserializeObject(model.CommandData, commandType);
            var method = typeof(ApiClient).GetMethod("ExecuteCommand");
            var generic = method.MakeGenericMethod(commandType);
            var task = (Task)generic.Invoke(this._apiClient, new []{ command });

            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return this.Json(resultProperty.GetValue(task));

        }

        [Route("/Account/Login")]
        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be whitelisted in the
                // **Allowed Logout URLs** settings for the app.
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }            
    }
}