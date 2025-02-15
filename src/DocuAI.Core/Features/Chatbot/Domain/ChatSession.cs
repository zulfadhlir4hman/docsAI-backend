using System;
using System.Collections.Generic;

namespace DocuAI.Core.Features.Chatbot.Domain
{
    // Represents a chat session with exchanged messages
    public class ChatSession
    {
        public Guid SessionId { get; set; }
        public List<string> Messages { get; set; } = new();
    }
}
