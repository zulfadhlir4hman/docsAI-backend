using Microsoft.AspNetCore.Mvc;
using DocuAI.Core.Features.Chatbot.Application;

namespace DocuAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        [HttpPost("query")]
        public IActionResult QueryChatbot([FromBody] string question)
        {
            // In a real app, the service would be injected via DI.
            var service = new ChatbotService();
            var response = service.GetResponse(question);
            return Ok(response);
        }
    }
}
