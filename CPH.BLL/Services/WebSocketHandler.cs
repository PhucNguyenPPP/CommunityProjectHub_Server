using CPH.Common.DTO.Message;
using CPH.Common.DTO.WebSocket;
using CPH.DAL.Entities;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CPH.BLL.Services
{
    public class WebSocketHandler
    {
        private static List<WebSocket> _sockets = new List<WebSocket>();

        public static async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
        {
            _sockets.Add(webSocket);
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                // Broadcast the message to all connected clients
                foreach (var socket in _sockets.ToList())
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _sockets.Remove(webSocket);
        }

        public async Task BroadcastMessageAsync(MessageResponseDTO model)
        {
            var messageObject = new MessageWebSocketDTO()
            {
                MessageId = model.MessageId,
                SendAccountId = model.SendAccountId,
                SendAccountName = model.SendAccountName,
                ClassId = model.ClassId,
                Content = model.Content,
                CreatedDate = model.CreatedDate,
                Type = "Message", 
            };

            string message = JsonSerializer.Serialize(messageObject);
            var buffer = Encoding.UTF8.GetBytes(message);

            foreach (var socket in _sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }


        public async Task BroadcastNotificationAsync(Notification model)
        {
            var notificationObject = new NotificationWebSocketDTO()
            {
                NotificationId = model.NotificationId,
                AccountId = model.AccountId,
                CreatedDate = model.CreatedDate,
                IsRead = model.IsRead,
                MessageContent = model.MessageContent,
                Type = "Notification"
            };

            string notification = JsonSerializer.Serialize(notificationObject);
            var buffer = Encoding.UTF8.GetBytes(notification);

            foreach (var socket in _sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
