using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace RealTimeChat.Hubs
{
    public interface IChatHub {
        Task ListUsers(string [] users);

        Task StartedConversation(string frontUser, string toUser);

        Task RequestedPublicKey(string fromUser, string toUser);

        Task ReceivedPublicKey(string fromUser, string toUser, string publicKey);

        Task ReceivedMessage(string fromUser, string toUser, string message);
    }

    [SignalRHub]
    public class ChatHub: Hub<IChatHub> {
        
        public static Dictionary<string, string> Users = new Dictionary<string, string>();

        public async Task Init(string user) {
            Users.Add(user, Context.ConnectionId);

            await Clients.All.ListUsers(Users.Keys.ToArray());
        }

        public async Task StartConversation(string fromUser, string toUser) {
            await Clients.Client(Users[toUser]).RequestedPublicKey(fromUser, toUser);
            await Clients.Client(Users[fromUser]).RequestedPublicKey(toUser, fromUser);
        }

        public async Task SendPublicKey(string fromUser, string toUser, string publicKey) {
            await Clients.Client(Users[toUser]).ReceivedPublicKey(fromUser, toUser, publicKey);
        }

        public async Task SendMessage(string fromUser, string toUser, string message) {
            await Clients.Client(Users[toUser]).ReceivedMessage(fromUser, toUser, message);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Users.ContainsValue(Context.ConnectionId)) {
                Users.Remove(Users.First(user=> user.Value == Context.ConnectionId).Key);
                await Clients.All.ListUsers(Users.Keys.ToArray());
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}