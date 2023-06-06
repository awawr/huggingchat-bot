using Discord;
using Discord.WebSocket;

namespace huggingchat_bot
{
    internal class Program
    {
#pragma warning disable CS8618
        private HuggingChat _chat;
        private DiscordSocketClient _client;
#pragma warning restore CS8618
        private bool _processing = false;

        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            _chat = new HuggingChat(Config.Username, Config.Password, Config.UseSearch);
            _client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent });
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            await _client.LoginAsync(TokenType.Bot, Config.Token);
            await _client.StartAsync();
            await _client.SetGameAsync("you sleep", type: ActivityType.Watching);

            Console.ReadKey(true);
            _chat.Quit();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task MessageReceived(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return Task.CompletedTask;
            if (message.Channel.Id != Config.ChannelID)
                return Task.CompletedTask;
            if (string.IsNullOrWhiteSpace(message.CleanContent))
                return Task.CompletedTask;
            _ = Task.Run(async () =>
            {
                if (_processing)
                    return;
                _processing = true;
                var rmsg = await (message as SocketUserMessage).ReplyAsync("`Generating response...`");
                try
                {
                    string response = _chat.Ask(message.CleanContent);
                    if (response.Length > 1999)
                    {
                        response = new string(response.Take(1997).ToArray()) + "...";
                    }
                    await rmsg.ModifyAsync(m => m.Content = response);

                }
                catch (Exception ex)
                {
                    await rmsg.ModifyAsync(m => m.Content = ex.ToString());
                }
                _processing = false;
            });
            return Task.CompletedTask;
        }
    }
}