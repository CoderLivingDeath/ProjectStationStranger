using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Project.Scripts.Infrastructure.EventBus.EventHandlers
{
    public interface IPlayerAttackEventHandler : IGlobalSubscriber
    {
        void AttackHandle(bool isPressed);
    }
}
