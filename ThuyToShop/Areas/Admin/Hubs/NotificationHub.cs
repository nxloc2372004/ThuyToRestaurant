using Microsoft.AspNet.SignalR;
namespace ThuyTo.Areas.Admin.Hubs
{
    public class NotificationHub : Hub
    {
        public void Send(string message)
        {
            // Gửi tín hiệu đến tất cả client kết nối
            Clients.All.broadcastMessage(message);
        }
    }
}
