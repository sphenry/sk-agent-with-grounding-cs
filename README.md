# AgentWithInternet - AI Chat Agent with Bing Search Grounding

## Overview

This project is a console-based AI chat agent powered by **Azure OpenAI** and **Semantic Kernel**. The agent can answer user queries and is enhanced with **Bing search grounding** to provide up-to-date responses on current events.

## Features

- Uses **Azure OpenAI Chat Completion** for conversation.
- **Grounds responses using Bing search**, integrating real-time information.
- **Continuously engages in a conversation**, remembering previous exchanges in chat history.
- Supports **environment variable configuration** via `.env` files.
- **Handles agent-based execution** using `AIProjectClient` from `Azure.AI.Projects`.

## Prerequisites

Before running the project, ensure you have:

- **.NET SDK 8.0 or later** installed.
- An **Azure OpenAI deployment** with an API key and endpoint.
- **Azure AI Projects service** configured with an agent.
- A valid **Bing search integration** for grounding.

## Setup

1. **Clone the repository**:
   ```sh
   git clone <repo-url>
   cd <repo-directory>
   ```

2. **Install dependencies** (if needed):
   ```sh
   dotnet restore
   ```

3. **Create a `.env` file** in the project root with the following variables:
   ```sh
   AZURE_OPENAI_DEPLOYMENT=<your-deployment-name>
   AZURE_OPENAI_API_KEY=<your-api-key>
   AZURE_OPENAI_ENDPOINT=<your-endpoint-url>
   AZURE_AI_PROJECT_CONNECTION_STRING=<your-ai-project-connection-string>
   AZURE_AI_AGENT_ID=<your-agent-id>
   ```
4. **Log into Azure using Azure CLI**:
   ```sh
   az login
   ```

5. **Run the application**:
   ```sh
   dotnet run
   ```

## Usage

- Start the console app.
- Type messages to chat with the AI.
- Type `exit` to end the conversation.

### Example Interaction

```
Agent loaded. Start chatting!
User > What’s the latest news on AI advancements?
Agent > [Retrieves real-time information using Bing search]
```


## License

This project is licensed under the **MIT License**.