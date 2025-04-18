using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Project.Scripts.Infrastructure.EventBus.EventHandlers
{
    public interface IPlayerMouseDeltaEventHandler : IGlobalSubscriber
    {
        void MouseDeltaHandle(Vector2 delta);
    }
}
