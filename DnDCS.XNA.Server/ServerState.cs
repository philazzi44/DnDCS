using DnDCS.Libs;
using DnDCS.XNA.Libs;

namespace DnDCS.XNA.Server
{
    public class ServerState : GameState
    {
        public ServerSocketConnection Connection { get; set; }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            if (this.Connection != null)
                this.Connection.Stop();

            base.Dispose();
        }
    }
}
