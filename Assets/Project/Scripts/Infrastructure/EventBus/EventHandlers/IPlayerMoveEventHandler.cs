using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Project.Scripts.Infrastructure.EventBus.EventHandlers
{
    public interface IPlayerMoveEventHandler : IGlobalSubscriber
    {
        void HandleMove(Vector2 direction);
    }
}
