
using Husty.RosBridge;

namespace CanRosbridgeAdaptor
{
    public class can_plugins2
    {
        public class msg
        {
            public record class Frame(std_msgs.msg.Header header, uint id, bool is_rtr, bool is_extended, bool is_error, byte dlc, byte[] data);
        }
    }

}